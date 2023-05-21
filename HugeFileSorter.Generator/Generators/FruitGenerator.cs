using System.Text;
using HugeFileSorter.Generator.Absractions;

namespace HugeFileSorter.Generator.Generators;

public class FruitGenerator: ITextGenerator
{
    private readonly Random _random;
    private readonly int _maxFruits;

    private string[] _fruits = {
        "apple", "banana", "orange", "kiwi", "grape", "mango", "pear", "pineapple", "strawberry", "watermelon",
        "papaya", "peach", "plum", "blueberry", "raspberry", "blackberry", "cherry", "lemon", "lime", "coconut",
        "avocado", "fig", "grapefruit", "guava", "jackfruit", "lychee", "pomegranate", "tangerine", "apricot",
        "cranberry", "dragonfruit", "elderberry", "mulberry", "nectarine", "persimmon", "quince", "rhubarb"
    };
    
    public FruitGenerator(Random random, int maxFruits)
    {
        _random = random;
        _maxFruits = maxFruits;
    }
    
    public string Generate()
    {
        var length = _random.Next(_maxFruits) + 1;
        var sb = new StringBuilder();

        for (var i = 0; i < length; i++)
        {
            var index = _random.Next(_fruits.Length);
            var fruit = _fruits[index];
            if (i == 0)
                fruit = $"{fruit[..1].ToUpper()}{fruit[1..]}";
            else
                sb.Append(' ');
            sb.Append(fruit);
        }

        return sb.ToString();
    }
}