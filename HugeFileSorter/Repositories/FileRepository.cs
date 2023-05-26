using System.IO.MemoryMappedFiles;
using HugeFileSorter.Extensions;

namespace HugeFileSorter.Repositories;

public class FileRepository
{
    private readonly RepositoryConfig _config;

    private const byte StringEnd = (byte)'\n';

    private static readonly byte[] NewLine = Environment.NewLine.Select(c => (byte)c).ToArray();
    private static readonly int BufferSize = 1.ToMB(); 

    public FileRepository(RepositoryConfig config)
    {
        _config = config;
    }

    public IEnumerable<RowsChunk> GetSourceRows(int bufferSize)
    {
        var fileInfo = new FileInfo(_config.SourceFile);
        var fileSize = fileInfo.Length;

        var fixedPool = new FixedBufferPool(bufferSize, _config.BuffersCount);

        using var memoryMappedFile = MemoryMappedFile.CreateFromFile(fileInfo.FullName, FileMode.Open);
        
        var offset = 0;
        var chunkCounter = 0;
        
        while (true)
        {
            var remainingBytes = (int)Math.Min(bufferSize, fileSize - offset);

            if (remainingBytes == 0)
                break;

            var isLastBuffer = remainingBytes != bufferSize;
            var buffer = isLastBuffer
                ? new byte[remainingBytes]
                : fixedPool.GetOrWait();

            using var accessor = memoryMappedFile.CreateViewAccessor(offset, remainingBytes);

            accessor.ReadArray(0, buffer, 0, remainingBytes);

            int lastInBuffer;
            if (isLastBuffer)
            {
                lastInBuffer = remainingBytes;
                offset += remainingBytes;
            } 
            else
            {
                lastInBuffer = Array.LastIndexOf(buffer, StringEnd) + 1;
                offset += remainingBytes - (remainingBytes - lastInBuffer);
            }

            yield return new RowsChunk(++chunkCounter, GetRows(buffer, lastInBuffer), () => fixedPool.Return(buffer));

            if (bufferSize != remainingBytes)
                break;
        }
    }

    private IEnumerable<Row> GetRows(byte[] buffer, int length)
    {
        var rowStart = 0;
        var currentPosition = 0;

        for (; currentPosition < length; ++currentPosition)
        {
            var currentByte = buffer[currentPosition];

            if (currentByte != StringEnd)
                continue;

            var len = currentPosition - rowStart + 1;
            var row = new RawRow(buffer.AsMemory(rowStart, len));
            yield return row.ToRow(StringEnd, NewLine);
            
            rowStart = currentPosition + 1;
        }
        
        var lastLen = currentPosition - rowStart;
        if (buffer.Length == length && lastLen > 0)
        {
            var row = new RawRow(buffer.AsMemory(rowStart, lastLen));
            yield return row.ToRow(StringEnd, NewLine);
        }
    }

    public int WriteChunk(int chunkId, IEnumerable<Row> rows)
    {
        var path = BuildChunkPath(chunkId);

        using var fileStream = File.Open(path, FileMode.Create);
        using var bufferedStream = new BufferedStream(fileStream, BufferSize);

        var counter = 0;
        foreach (var row in rows)
        {
            ++counter;
            bufferedStream.Write(row.Text.Span);
        }

        return counter;
    }

    public async Task<int> WriteChunkAsync(int chunkId, IAsyncEnumerable<Row> rows)
    {
        var path = BuildChunkPath(chunkId);

        await using var fileStream = File.Open(path, FileMode.Create);
        await using var bufferedStream = new BufferedStream(fileStream, BufferSize);

        var counter = 0;
        await foreach (var row in rows)
        {
            ++counter;
            bufferedStream.Write(row.Text.Span);
        }

        return counter;
    }

    public IEnumerable<Row> ReadChunk(int chunkId)
    {
        var bufferSize = 1.ToMB();
        var fixedPool = new FixedBufferPool(bufferSize);
        var path = BuildChunkPath(chunkId);
        var fileInfo = new FileInfo(path);
        var fileSize = fileInfo.Length;

        using var memoryMappedFile = MemoryMappedFile.CreateFromFile(path, FileMode.Open);

        var offset = 0;

        while (true)
        {
            var remainingBytes = (int)Math.Min(bufferSize, fileSize - offset);

            if (remainingBytes == 0)
                break;

            var isLast = remainingBytes != bufferSize;
            var buffer = isLast
                ? new byte[remainingBytes]
                : fixedPool.GetOrWait();

            using var accessor = memoryMappedFile.CreateViewAccessor(offset, remainingBytes);

            accessor.ReadArray(0, buffer, 0, remainingBytes);
            
            int lastInBuffer;
            if (isLast)
            {
                lastInBuffer = remainingBytes;
                offset += remainingBytes;
            }
            else
            {
                lastInBuffer = Array.LastIndexOf(buffer, StringEnd) + 1;
                offset += remainingBytes - (remainingBytes - lastInBuffer);
            }

            foreach (var row in GetRows(buffer, lastInBuffer))
            {
                yield return row;
            }

            fixedPool.Return(buffer);
            
            if (bufferSize != remainingBytes)
                break;
        }
    }

    private string BuildChunkPath(int chunkId)
    {
        var tempDir = _config.TempDir;

        if (!string.IsNullOrWhiteSpace(tempDir) && !Directory.Exists(tempDir))
            Directory.CreateDirectory(tempDir);

        return Path.Combine(tempDir, $"chunk_{chunkId:0000000000}.txt");
    }

    public int WriteTarget(IEnumerable<Row> rows)
    {
        var path = _config.TargetFile;
        using var fileStream = File.Open(path, FileMode.Create);
        using var bufferedStream = new BufferedStream(fileStream, BufferSize);
   
        var counter = 0;
        foreach (var row in rows)
        {
            ++counter;
            bufferedStream.Write(row.Text.Span);
        }

        return counter;
    }

    public async Task<int> WriteTargetAsync(IAsyncEnumerable<Row> rows)
    {
        var path = _config.TargetFile;

        await using var fileStream = File.Open(path, FileMode.Create);
        await using var bufferedStream = new BufferedStream(fileStream, BufferSize);
        
        var counter = 0;
        await foreach (var row in rows)
        {
            ++counter;
            bufferedStream.Write(row.Text.Span);
        }

        return counter;
    }
}