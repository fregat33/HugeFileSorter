using System.Threading.Channels;
using HugeFileSorter.Sorting;
using HugeFileSorter.Tests.Helpers;

namespace HugeFileSorter.Tests;

public class MergeTests
{
    [Theory]
    [InlineData(@"1. Apple
415. Apple")]
    [InlineData(@"1. Apple
1. Banana")]
    [InlineData(@"1. Apple
415. Apple
2. Banana is yellow
32. Cherry is the best
30432. Something something something
")]
    public async Task Merge_Test(string sortedText)
    {
        //given 
        var comparer = new RowComparer();
        var merge = new MergeStrategy(comparer);

        var channel = Channel.CreateBounded<Row>(new BoundedChannelOptions(capacity: 1)
        {
            SingleWriter = true,
            SingleReader = true,
            AllowSynchronousContinuations = true,
        });

        var sortedCollections = new List<IEnumerable<Row>>();

        var chunks = new Random().Next(2, 10);
        var totalRows = 0;

        for (var i = 0; i < chunks; ++i)
        {
            var sortedRows = RowFactory.Parse(sortedText);
            totalRows += sortedRows.Length;
            sortedCollections.Add(sortedRows);
        }

        //when
        var mergeTask = Task.Run(() => merge.MergeAsync(channel.Writer, sortedCollections));
        var readTask = Consumer.ReadAll(channel.Reader);
        
        await Task.WhenAll(mergeTask, readTask);

        var actual = readTask.Result;
        
        //then
        Assert.Equal(totalRows, actual.Length);
        
        for(var i = 1; i < actual.Length; ++i)
        {
            var prev = actual[i - 1];
            var curr = actual[i];
            var prevText = prev.ToString();
            var currText = curr.ToString();
            var diff = String
                .Compare(prevText.Substring(prev.FirstCharIndex), currText.Substring(curr.FirstCharIndex), StringComparison.InvariantCulture);
            
            if (diff == 0)
                diff = prev.Number.CompareTo(curr.Number);
            
            Assert.True(diff <= 0, $"{prev.ToString()} compared with {curr.ToString()}");
        }
    }
}