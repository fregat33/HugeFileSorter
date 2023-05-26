using HugeFileSorter.Sorting;
using HugeFileSorter.Tests.Helpers;

namespace HugeFileSorter.Tests;

public class RowComparerTests
{
    [Theory]
    [InlineData("415. Apple", "415. Apple", 0)]
    [InlineData("416. Apple", "415. Apple", 1)]
    [InlineData("415. Apple", "416. Apple", -1)]
    [InlineData("415. Apple", "416. Banana", -1)]
    [InlineData("415. Banana", "416. Apple", 1)]
    public void Line_Comparison_Test(string leftRow, string rightRow, int expected)
    {
        var left = RowFactory.Parse(leftRow).First();
        var right = RowFactory.Parse(rightRow).First();
        var comparer = new RowComparer();

        var diff = comparer.Compare(left, right);
        Assert.Equal(expected, diff);
    }

    [Theory]
    [InlineData("415. Apple", "415. Apple", true)]
    [InlineData("416. Apple", "416. Apple", true)]
    [InlineData("415. Apple", "416. Apple", false)]
    [InlineData("416. Banana", "416. Banana", true)]
    [InlineData("415. Banana", "416. Apple", false)]
    public void Row_Equals_Test(string leftRow, string rightRow, bool expected)
    {
        var left = RowFactory.Parse(leftRow).First();
        var right = RowFactory.Parse(rightRow).First();

        Assert.Equal(expected, left.Equals(right));
    }

    [Theory]
    [InlineData(@"415. Apple
1. Apple",
        @"1. Apple
415. Apple
")]
    [InlineData(@"415. Apple
30432. Something something something
1. Apple
32. Cherry is the best
2. Banana is yellow",
        @"1. Apple
415. Apple
2. Banana is yellow
32. Cherry is the best
30432. Something something something")]
    [InlineData(@"6666662. Something something something
6666661. Something something something",
        @"6666661. Something something something
6666662. Something something something")]
    public void Sort_Test(string input, string expected)
    {
        var inputRows = RowFactory.Parse(input);
        var comparer = new RowComparer();

        Array.Sort(inputRows, comparer);

        var expectedRows = RowFactory.Parse(expected);

        Assert.Equal(expectedRows as IEnumerable<Row>, inputRows as IEnumerable<Row>);
    }
    
    [Theory]  
    [InlineData(@"6666662. Something something something
6666661. Something  something something",
        @"6666661. Something  something something
6666662. Something something something")]
    [InlineData(@"6666662.  Something something something
6666661. Something something something",
        @"6666662.  Something something something
6666661. Something something something")]
    [InlineData(@"6666662. Something something something
6666661. Something something something ",
        @"6666662. Something something something
6666661. Something something something ")]
    public void Sort_With_Spaces_Test(string input, string expected)
    {
        var inputRows = RowFactory.Parse(input);
        var comparer = new RowComparer();

        Array.Sort(inputRows, comparer);

        var expectedRows = RowFactory.Parse(expected);

        Assert.Equal(expectedRows, inputRows);
    }
}