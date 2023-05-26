using System.Threading.Channels;
using HugeFileSorter.Sorting;
using HugeFileSorter.Tests.Helpers;

namespace HugeFileSorter.Tests;

public class BucketSortTests
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
    public async Task Test(string unsortedText)
    {
        //given
        var comparer = new RowComparer();
        var sorter = new BucketStrategy(comparer);
        var unsortedRows = RowFactory.Parse(unsortedText);

        foreach (var row in unsortedRows)
        {
            sorter.Add(row);
        }

        //when
        var sortedRows = sorter.Sort().ToArray();
        
        //then
        var expectedOrder = unsortedRows.ToArray();
        Array.Sort(expectedOrder, comparer);
        
        Assert.Equal(expectedOrder, sortedRows);
    }
}