namespace HugeFileSorter.Extensions;

public static class RowExtensions
{
    public static Row Copy(this Row row)
    {
        var rowTextCopy = row.Text.ToArray();
        return row with { Text = rowTextCopy };
    }
    
    public static Row ToRow(this RawRow row, byte stringEnd, byte[] newLine)
    {
        var span = row.Text.Span;
        var separatorIndex = span.IndexOf((byte)'.');
        
        var num = ConvertAsciiBytesToLong(span, separatorIndex);

        var skip = separatorIndex + 2; // ". " skipped
        
        if (span.LastIndexOf(stringEnd) > 0)
            return new Row(row.Text, num, skip);
        
        var text = new byte[row.Text.Length + newLine.Length];
        
        span.CopyTo(text);
        newLine.CopyTo(text, row.Text.Length);

        return new Row(text, num, skip);
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