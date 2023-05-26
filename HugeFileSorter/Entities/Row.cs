using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

[StructLayout(LayoutKind.Sequential)]
public record struct Row(ReadOnlyMemory<byte> Text, long Number, int FirstCharIndex)
{
    public char FirstChar => (char) Text.Span[FirstCharIndex];

    public bool Equals(Row other)
    {
        if (ReferenceEquals(null, other))
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Text.Span.SequenceEqual(other.Text.Span);
    }

    public override int GetHashCode()
    {
        return Text.Equals(default) ? StructuralComparisons.StructuralEqualityComparer.GetHashCode(Text.Span.ToArray()) : 0;
    }

    public override string ToString()
    {
        return Encoding.UTF8.GetString(Text.Span);
    }
}

public readonly record struct RawRow(ReadOnlyMemory<byte> Text)
{
    public ReadOnlyMemory<byte> Text { get; } = Text;
    
    public override string ToString()
    {
        return Encoding.UTF8.GetString(Text.Span);
    }
}