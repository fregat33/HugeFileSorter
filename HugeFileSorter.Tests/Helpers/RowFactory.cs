using System.Text;
using HugeFileSorter.Extensions;

namespace HugeFileSorter.Tests.Helpers;

public class RowFactory
{
    private const byte StringEnd = (byte)'\n';

    private static readonly byte[] NewLine = Environment.NewLine.Select(c => (byte)c).ToArray();
    
    public static Row[] Parse(string data)
    {
        return data
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Select(r => Encoding.UTF8.GetBytes(r + Environment.NewLine))
            .Select(r => new RawRow(r).ToRow(StringEnd, NewLine))
            .ToArray();
    }
}