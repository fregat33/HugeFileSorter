using System.IO.Compression;
using System.IO.MemoryMappedFiles;
using HugeFileSorter.Extensions;

namespace HugeFileSorter.Repositories;

public class FileRepository
{
    private readonly string _sourceFile;
    private readonly string _targetFile;

    private readonly string _tempDir = "temp/";
    private readonly long _fileSize;
    
    private const byte StringEnd = (byte)'\n';
    
    private static readonly int BufferSize = 1024 * 1024;
    private static readonly FileStreamOptions WriteFileStreamOptions = new()
    {
        Options = FileOptions.Asynchronous | FileOptions.SequentialScan, 
        Access = FileAccess.Write, 
        Share = FileShare.Write,
        Mode = FileMode.Create,
        BufferSize = BufferSize
    };
    
    private static readonly FileStreamOptions ReadFileStreamOptions = new()
    {
        Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
        Access = FileAccess.Read,
        Share = FileShare.Read,
        Mode = FileMode.Open,
        BufferSize = BufferSize
    };

    public FileRepository(string sourceFile, string targetFile)
    {
        if (!Directory.Exists(_tempDir))
            Directory.CreateDirectory(_tempDir);
        
        _fileSize = new FileInfo(sourceFile).Length;
        _sourceFile = sourceFile;
        _targetFile = targetFile;
    }

    public IEnumerable<IEnumerable<Row>> GetSourceRows(int chunkSize)
    {
        using var memoryMappedFile = MemoryMappedFile.CreateFromFile(_sourceFile, FileMode.Open);

        var totalChunks = (int)Math.Ceiling((double)_fileSize / chunkSize);
        
        var offset = 0;
        
        

        for (var chunkIndex = 0; chunkIndex < totalChunks; chunkIndex++)
        {
            var chunkBuffer = new byte[chunkSize];
            var remainingBytes = (int)Math.Min(chunkSize, _fileSize - offset);
            
            var buffer = remainingBytes == chunkSize ? chunkBuffer : new byte[remainingBytes];
            
            using var accessor = memoryMappedFile.CreateViewAccessor(offset, remainingBytes);
            
            accessor.ReadArray(0, buffer, 0, remainingBytes);

            var lastInBuffer = Array.LastIndexOf(buffer, StringEnd);
            if (lastInBuffer > 0)
            {
                offset += remainingBytes - (remainingBytes - lastInBuffer) + 1;
            }

            yield return GetRows(buffer);
        }
    }

    private IEnumerable<Row> GetRows(byte[] buffer)
    {
        var rowStart = 0;
        var currentPosition = 0;
            
        while(currentPosition < buffer.Length)
        {
            var currentByte = buffer[currentPosition++];

            if (currentByte != StringEnd) 
                continue;
            
            var len = currentPosition - rowStart;
            var row = new RawRow(buffer.AsMemory(rowStart, len));

            yield return row.ToRow();
            rowStart = currentPosition;
        }
    }
    
    public int WriteChunk(int chunkId, IEnumerable<Row> rows)
    {
        var path = $"{_tempDir}chunk_{chunkId:0000000000}.txt";

        using var fileStream = File.Open(path, WriteFileStreamOptions);
        using var bufferedStream = new BufferedStream(fileStream, BufferSize);

        var counter = 0;
        foreach (var row in rows)
        {
            ++counter;
            bufferedStream.Write(row.Text.Span);
        }

        return counter;
    }

    public IEnumerable<Row> ReadChunk(int chunkId)
    {
        var path = $"{_tempDir}chunk_{chunkId:0000000000}.txt";

        using var fileStream = File.Open(path, ReadFileStreamOptions);
        using var bufferedStream = new BufferedStream(fileStream, BufferSize);
        
        var bufferSize = 2048;
        var buffer = new byte[bufferSize];

        var read = -1;
        var stringEnd = (byte)'\n';
        var i = 0;
        
        while ((read = bufferedStream.ReadByte()) != -1)
        {
            var currentByte = (byte) read;
            buffer[i++] = currentByte;

            if (currentByte != stringEnd) 
                continue;
            
            var row = new RawRow(buffer[..i].ToArray());
            yield return row.ToRow();
            i = 0;
        }
    }

    public int WriteTarget(IEnumerable<Row> rows)
    {
        var path = _targetFile;
        
        using var fileStream = File.Open(path, WriteFileStreamOptions);
        var counter = 0;
        foreach (var row in rows)
        {
            ++counter;
            fileStream.Write(row.Text.Span);
        }

        return counter;
    }

    public async Task<int> WriteTargetAsync(IAsyncEnumerable<Row> rows)
    {
        var path = _targetFile;
        
        using var fileStream = File.Open(path, WriteFileStreamOptions);
        var counter = 0;
        await foreach (var row in rows)
        {
            ++counter;
            fileStream.Write(row.Text.Span);
        }

        return counter;
    }
}