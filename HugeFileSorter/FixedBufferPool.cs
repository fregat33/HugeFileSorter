using System.Collections.Concurrent;

namespace HugeFileSorter;

public class FixedBufferPool
{
    private readonly long _bufferSize;
    private readonly SemaphoreSlim _semaphoreSlim;
    private readonly ConcurrentQueue<byte[]> _buffers;

    public FixedBufferPool(long bufferSize, int buffersCount = 2)
    {
        _bufferSize = bufferSize;
        _semaphoreSlim = new SemaphoreSlim(buffersCount);
        _buffers = new ConcurrentQueue<byte[]>();

        for (var i = 0; i < buffersCount; ++i)
        {
            _buffers.Enqueue(new byte[bufferSize]);
        }
    }

    public byte[] GetOrWait()
    {
        _semaphoreSlim.Wait();

        byte[] buffer;

        while (!_buffers.TryDequeue(out buffer))
        {
            Thread.Sleep(50);
        }
        return buffer;
    }

    public void Return(byte[] item)
    {
        if(item.Length != _bufferSize)
            return;
        
        _buffers.Enqueue(item);
        _semaphoreSlim.Release();
    }
}