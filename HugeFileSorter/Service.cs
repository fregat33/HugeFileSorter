using System.Diagnostics;
using System.Threading.Channels;
using HugeFileSorter.Repositories;
using HugeFileSorter.Sorting;

namespace HugeFileSorter;

public class Service
{
    private readonly int _chunkSize;
    private readonly FileRepository _repository;
    private int _chunksCounter;
    private readonly BucketStrategy _buckets;
    private readonly RowComparer _comparer;

    public Service(int chunkSize, FileRepository repository)
    {
        _comparer = new RowComparer();
        _buckets = new BucketStrategy(_comparer);
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
        var channelOptions = new BoundedChannelOptions(1)
        {
            SingleWriter = true,
            SingleReader = true
        };
        var workerChannel = Channel.CreateBounded<WorkerTask>(channelOptions);
        var main = ReadSourceToByChunks(workerChannel);
        var works = Worker(workerChannel);
        await Task.WhenAll(main, works);
    }
    
    private async Task ReadSourceToByChunks(ChannelWriter<WorkerTask> channelWriter)
    {
        foreach (var rowsChunk in _repository.GetSourceRows(_chunkSize))
        {
            var chunkId = ++_chunksCounter;

            foreach (var row in rowsChunk)
            {
                _buckets.Add(row);
            }

            var sortedChunk = await _buckets.Sort();

            Console.WriteLine($"{DateTime.UtcNow} ReadSourceToByChunks chunk created (chunkId: {chunkId})");
            var writeChunkTask = new WorkerTask { Id = chunkId, Rows = sortedChunk };
            await channelWriter.WriteAsync(writeChunkTask);

            _buckets.Clear();
        }

        channelWriter.Complete();
    }

    private async Task Worker(ChannelReader<WorkerTask> channelReader)
    {
        await foreach (var workerTask in channelReader.ReadAllAsync())
        {
            var count = _repository.WriteChunk(workerTask.Id, workerTask.Rows);
            Console.WriteLine($"{DateTime.UtcNow} ReadSourceToByChunks chunk saved (chunkId: {workerTask.Id}, rows: {count})");
        }
    }

    private async Task MergeChunksToTargetAsync()
    {
        var channelOptions = new BoundedChannelOptions(5)
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
        var merge = new MergeStrategy(channel, _comparer);

        var sortedChunks = Enumerable.Range(1, _chunksCounter)
            .Select(id => _repository.ReadChunk(id))
            .ToArray();

        Console.WriteLine($"{DateTime.UtcNow} MergeChunksAsync (chunks: {sortedChunks.Length})");
        await merge.MergeAsync(sortedChunks);

        channel.Complete();
    }

    private record struct WorkerTask
    {
        public int Id { get; set; }
        public IEnumerable<Row> Rows { get; set; }
    }
}