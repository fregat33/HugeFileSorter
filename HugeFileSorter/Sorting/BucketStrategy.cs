using System.Buffers;

namespace HugeFileSorter.Sorting;

public class BucketStrategy
{
    private readonly int _bucketsCount;
    private readonly List<Row>[] _buckets;
    private readonly IComparer<Row> _comparer;

    public BucketStrategy(IComparer<Row> comparer)
    {
        _comparer = comparer;
        _bucketsCount = 28;
        var buckets = new List<Row>[_bucketsCount];

        for (var i = 0; i < _bucketsCount; ++i)
        {
            buckets[i] = new List<Row>();
        }

        _buckets = buckets;
    }

    public void Add(Row row)
    {
        // _bag.Add(row);
        var firstChar = row.FirstChar;
        var first = firstChar - 96;
        if (first < 0)
            first = firstChar - 64;

        var idx = Math.Clamp(first, 0, _bucketsCount);
        var bucket = _buckets[idx];
        lock (bucket)
        {
            bucket.Add(row);
        }
    }

    public async Task<IEnumerable<Row>> Sort()
    {
        var count = _buckets.Sum(e => e.Count);

        var sortedArray = ArrayPool<Row>.Shared.Rent(count);

        var accum = 0;
        var positionBuckets = _buckets.Select((b, i) => (i, b, accum += b.Count)).ToArray();

        var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount - 1 };
        
        await Parallel.ForEachAsync(positionBuckets, options, (b, c) =>
        {
            var currentCount = b.b.Count;
            if (currentCount == 0)
                return default;

            var idx = 0;
            if (b.i > 0)
            {
                idx = positionBuckets[b.i - 1].Item3;
            }

            b.b.CopyTo(sortedArray, idx);

            Array.Sort(sortedArray, idx, currentCount, _comparer);

            return default;
        });

        return GetSorted(sortedArray, count);
    }
    
    public void Clear()
    {
        for (var i = 0; i < _bucketsCount; ++i)
        {
            _buckets[i].Clear();
        }
    }
    
    private static IEnumerable<Row> GetSorted(Row[] sortedArray, int count)
    {
        for (var i = 0; i < count; ++i)
        {
            yield return sortedArray[i];
        }
    
        ArrayPool<Row>.Shared.Return(sortedArray);
    }
}