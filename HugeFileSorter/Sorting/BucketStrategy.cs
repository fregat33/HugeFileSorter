using System.Buffers;

namespace HugeFileSorter.Sorting;

public class BucketStrategy
{
    private const int BucketsCount = 28;
    private readonly List<Row>[] _buckets = new List<Row>[BucketsCount];
    private readonly IComparer<Row> _comparer;

    public BucketStrategy(IComparer<Row> comparer)
    {
        _comparer = comparer;
        
        for (var i = 0; i < BucketsCount; ++i)
        {
            _buckets[i] = new List<Row>();
        }
    }

    public void Add(Row row)
    {
        var firstChar = row.FirstChar;
        var first = firstChar - 96;
        if (first < 0)
            first = firstChar - 64;

        var idx = Math.Clamp(first, 0, BucketsCount);
        var bucket = _buckets[idx];
        
        bucket.Add(row);
    }

    public IEnumerable<Row> Sort()
    {
        var count = _buckets.Sum(e => e.Count);

        var sortedArray = ArrayPool<Row>.Shared.Rent(count);

        var positionSum = 0;
        var positionBuckets = _buckets
            .Select((rows, idx) => (idx, rows, currentPosition: positionSum += rows.Count))
            .ToArray();

        var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount - 1 };
        
        Parallel.ForEach(positionBuckets, options, (bucket, _) =>
        {
            var currentCount = bucket.rows.Count;
            if (currentCount == 0)
                return;

            var idx = 0;
            if (bucket.idx > 0)
            {
                idx = positionBuckets[bucket.idx - 1].currentPosition;
            }

            bucket.rows.CopyTo(sortedArray, idx);

            Array.Sort(sortedArray, idx, currentCount, _comparer);
        });

        return GetSorted(sortedArray, count);
    }
    
    public void Clear()
    {
        for (var i = 0; i < BucketsCount; ++i)
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