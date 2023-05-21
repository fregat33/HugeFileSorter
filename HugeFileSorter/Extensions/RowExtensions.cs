namespace HugeFileSorter.Extensions;

public static class RowExtensions
{
    public static RawRow ToRaw(this Row preparedRow)
    {
        return new RawRow(preparedRow.Text);
    }

    public static Row ToRow(this RawRow row)
    {
        var span = row.Text.Span;
        var separatorIndex = span.IndexOf((byte)'.');
        
        var num = ConvertAsciiBytesToLong(span, separatorIndex);

        var skip = separatorIndex + 2; // ". " skipped
        
        return new Row(row.Text, num, skip);
    }
    
    private static long ConvertAsciiBytesToLong(ReadOnlySpan<byte> bytes, int separatorIndex)
    {
        long result = 0;

        for (int i = 0; i < separatorIndex; i++)
        {
            byte b = bytes[i];

            if (b < 48 || b > 57) // ASCII digits range from 48 to 57
            {
                throw new ArgumentException("Input byte array must contain ASCII digits only.");
            }

            result = result * 10 + (b - 48);
        }

        return result;
    }
}