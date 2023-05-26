using System.Threading.Channels;

namespace HugeFileSorter.Tests.Helpers;

public static class Consumer
{
    public static async Task<Row[]> ReadAll(ChannelReader<Row> consumer)
    {
        var res = new List<Row>();
        
        await foreach (var row in consumer.ReadAllAsync())
        {
            res.Add(row);
        }

        return res.ToArray();
    }
}