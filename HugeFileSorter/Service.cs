using System.Diagnostics;
using System.Threading.Channels;
using HugeFileSorter.Repositories;
using HugeFileSorter.Sorting;

namespace HugeFileSorter;

public class Service
{
    private const int DefaultChannelCapacity = 1;
    private readonly int _chunkSize;
    private int _lastChunkId;

    private readonly FileRepository _repository;

    private readonly BucketStrategy _buckets;
    private readonly MergeStrategy _mergeStrategy;

    public Service(int chunkSize, FileRepository repository)
    {
        var comparer = new RowComparer();
        _buckets = new BucketStrategy(comparer);
        _mergeStrategy = new MergeStrategy(comparer);

        _chunkSize = chunkSize;
        _repository = repository;
    }

    public async Task ExecuteAsync()
    {
        Console.WriteLine($"{DateTime.UtcNow} Started");
        var sw = Stopwatch.StartNew();

        await SourceToSortedChunksAsync();
        await MergeChunksToTargetAsync();

        sw.Stop();
        Console.WriteLine($"{DateTime.UtcNow} Finished (elapsed: {sw.Elapsed})");
    }

    private async Task SourceToSortedChunksAsync()
    {
        var channelOptions = new BoundedChannelOptions(DefaultChannelCapacity)
        {
            SingleWriter = true,
            SingleReader = true
        };
        
        var workerChannel = Channel.CreateBounded<RowsChunk>(channelOptions);
        var main = ReadSourceToByChunks(workerChannel.Writer);
        var works = Worker(workerChannel.Reader);
        await Task.WhenAll(main, works);
    }

    private async Task ReadSourceToByChunks(ChannelWriter<RowsChunk> producer)
    {
        foreach (var rowsChunk in _repository.GetSourceRows(_chunkSize))
        {
            _lastChunkId = rowsChunk.Id;

            var rowsCounter = 0;
            foreach (var row in rowsChunk.Rows)
            {
                ++rowsCounter;
                _buckets.Add(row);
            }

            var sortedRows = _buckets.Sort();

            Console.WriteLine(
                $"{DateTime.UtcNow} ReadSourceToByChunks chunk created (chunkId: {rowsChunk.Id}, rows: {rowsCounter})");

            var sortedChunk = rowsChunk with { Rows = sortedRows };

            await producer.WriteAsync(sortedChunk);

            _buckets.Clear();
        }

        producer.Complete();
    }

    private async Task Worker(ChannelReader<RowsChunk> consumer)
    {
        await foreach (var rowsChunk in consumer.ReadAllAsync())
        {
            try
            {
                var count = _repository.WriteChunk(rowsChunk.Id, rowsChunk.Rows);
                Console.WriteLine(
                    $"{DateTime.UtcNow} ReadSourceToByChunks chunk saved (chunkId: {rowsChunk.Id}, rows: {count})");
            }
            finally
            {
                rowsChunk.Release();
            }
        }
    }

    private async Task MergeChunksToTargetAsync()
    {
        var channelOptions = new BoundedChannelOptions(DefaultChannelCapacity)
        {
            SingleWriter = true,
            SingleReader = true
        };
        var chunksToSource = Channel.CreateBounded<Row>(channelOptions);

        await Task.WhenAll(MergeChunksAsync(chunksToSource), WriteTargetAsync(chunksToSource));
    }

    private async Task WriteTargetAsync(ChannelReader<Row> channel)
    {
        Console.WriteLine($"{DateTime.UtcNow} WriteTargetAsync started");

        var counter = await _repository.WriteTargetAsync(channel.ReadAllAsync());

        Console.WriteLine($"{DateTime.UtcNow} WriteTargetAsync finished (rows: {counter})");
    }

    private async Task MergeChunksAsync(ChannelWriter<Row> channel)
    {
        var sortedChunks = Enumerable.Range(1, _lastChunkId)
            .Select(id => _repository.ReadChunk(id))
            .ToArray();

        Console.WriteLine($"{DateTime.UtcNow} MergeChunksAsync (chunks: {sortedChunks.Length})");

        await _mergeStrategy.MergeAsync(channel, sortedChunks);
    }
}