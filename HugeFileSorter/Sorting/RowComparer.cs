namespace HugeFileSorter.Sorting;

public class RowComparer : IComparer<Row>
{
    public int Compare(Row left, Row right)
    {
        var leftSpan = left.Text.Span[left.FirstCharIndex..];
        var rightSpan = right.Text.Span[right.FirstCharIndex..];
        
        var res = leftSpan.SequenceCompareTo(rightSpan);

        if (res != 0)
            return res;
        
        return left.Number.CompareTo(right.Number);
    }
}