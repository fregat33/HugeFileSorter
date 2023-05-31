// See https://aka.ms/new-console-template for more information

using System.Text;
using HugeFileSorter.Generator.Generators;

namespace HugeFileSorter.Generator;

class Program
{
    private static void Main(string[] args)
    {
        var config = GetConfig(args);

        var random = new Random();

        var currentSize = 0L;
        var generator = new FruitGenerator(random, config.maxStringLength);
        using var writer = new StreamWriter(config.fileName);
        var counter = 0;
        
        do
        {
            var number = random.Next(10000);
            var text = generator.Generate();
            
            var line = $"{number}. {text}";
            
            currentSize += Encoding.UTF8.GetByteCount(line) + Encoding.UTF8.GetByteCount(Environment.NewLine);

            writer.WriteLine(line);
            ++counter;
        } while (currentSize < config.maxSize);
        
        Console.WriteLine($"File {config.fileName} generated (rows: {counter})");
    }

    private static (long maxSize, int maxStringLength, string fileName) GetConfig(string[] args)
    {
        var fileName = "input.txt";
        long maxSize = 1000L * 1024 * 1024;
        var maxStringLength = 13;
        
        try
        {
            for (var i = 0; i < args?.Length; ++i)
            {
                var currentArg = args[i];
                switch (i)
                {
                    case 0:
                        maxSize = long.Parse(currentArg) * 1024 * 1024;
                        break;
                    case 1:
                        maxStringLength = int.Parse(currentArg);
                        break;
                    case 2:
                        fileName = currentArg;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        catch
        {
            throw new ArgumentException(@$"Expected arguments: [{nameof(maxSize)} [{nameof(maxStringLength)} [{nameof(fileName)}]]]  
1. {nameof(maxSize)} - output file size (long), default (MB): 1000           
2. {nameof(maxStringLength)} - max row length symbol/words (int), default: 10
3. {nameof(fileName)} - output file name (string), default: input.txt");
        }

        return (maxSize, maxStringLength, fileName);
    }
}