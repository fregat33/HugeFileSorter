using System.Runtime.InteropServices;
using System.Text;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Row(ReadOnlyMemory<byte> Text, long Number, int FirstCharIndex)
{
    public char FirstChar => (char) Text.Span[FirstCharIndex];
    
    public override string ToString()
    {
        return Encoding.UTF8.GetString(Text.Span);
    }
}

public readonly record struct RawRow(ReadOnlyMemory<byte> Text)
{
    public override string ToString()
    {
        return Encoding.UTF8.GetString(Text.Span);
    }
}