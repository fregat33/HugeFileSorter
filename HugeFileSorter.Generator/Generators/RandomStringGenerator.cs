using System.Text;
using HugeFileSorter.Generator.Absractions;

namespace HugeFileSorter.Generator.Generators;

public class RandomStringGenerator : ITextGenerator
{
    private readonly Random _random;
    private readonly int _maxLength;
    
    public RandomStringGenerator(Random random, int maxLength)
    {
        _random = random;
        _maxLength = maxLength;
    }
    
    public string Generate()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        var length = _random.Next(_maxLength) + 1;
        var sb = new StringBuilder(length);

        for (var i = 0; i < length; i++)
        {
            var index = _random.Next(chars.Length);
            sb.Append(chars[index]);
        }

        return sb.ToString();
    }
}