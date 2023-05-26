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
        var heap = new List<(IEnumerator<Row> reader, int fileIndex)>();
        try
        {
            for (var i = 0; i < sortedCollections.Count; i++)
            {
                var enumerator = sortedCollections.ElementAt(i).GetEnumerator();
                if (enumerator.MoveNext())
                {
                    heap.Add((enumerator, i));
                }
            }

            heap.Sort((x, y) => _comparer.Compare(x.reader.Current, y.reader.Current));

            while (heap.Count > 0)
            {
                var (reader, _) = heap[0];
                await producer.WriteAsync(reader.Current);

                if (!reader.MoveNext())
                {
                    reader.Dispose();
                    heap.RemoveAt(0);
                }
                Heapify(heap);
            }
        }
        finally
        {
            foreach (var item in heap)
            {
                item.reader?.Dispose();
            }
            producer.Complete();
        }
    }

    private void Heapify(List<(IEnumerator<Row> reader, int fileIndex)> heap)
    {
        var i = 0;
        while (true)
        {
            
            var left = 2 * i + 1;
            
            var right = 2 * i + 2;
            
            var smallestIndex = i;
            
            if (left < heap.Count && _comparer.Compare(heap[left].reader.Current, heap[smallestIndex].reader.Current) < 0)
                smallestIndex = left;
            if (right < heap.Count && _comparer.Compare(heap[right].reader.Current, heap[smallestIndex].reader.Current) < 0)
                smallestIndex = right;
            if (smallestIndex == i)
                break;
            (heap[i], heap[smallestIndex]) = (heap[smallestIndex], heap[i]);
            i = smallestIndex;
        }
    }
}