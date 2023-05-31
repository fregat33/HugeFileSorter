using System.Threading.Channels;

namespace HugeFileSorter.Sorting;

public class MergeStrategy
{
    private readonly IComparer<Row> _comparer;

    public MergeStrategy(IComparer<Row> comparer)
    {
        _comparer = comparer;
    }

    public async Task MergeAsync(ChannelWriter<Row> producer, IReadOnlyCollection<IEnumerable<Row>> sortedCollections)
    {
        var heap = new List<IEnumerator<Row>>(sortedCollections.Count);
        try
        {
            for (var i = 0; i < sortedCollections.Count; i++)
            {
                var enumerator = sortedCollections.ElementAt(i).GetEnumerator();
                if (enumerator.MoveNext())
                {
                    heap.Add(enumerator);
                }
            }
            
            while (heap.Count > 0)
            {
                heap.Sort((x, y) => _comparer.Compare(x.Current, y.Current));
                
                var reader = heap[0];
                await producer.WriteAsync(reader.Current);

                if (!reader.MoveNext())
                {
                    reader.Dispose();
                    heap.RemoveAt(0);
                }
            }
        }
        finally
        {
            foreach (var item in heap)
            {
                item.Dispose();
            }
            producer.Complete();
        }
    }
}