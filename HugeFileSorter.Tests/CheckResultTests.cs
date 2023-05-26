using HugeFileSorter.Tests.Helpers;

namespace HugeFileSorter.Tests;

public class CheckResultTests
{
    [Fact(Skip = "Manual test")]
    // [Fact]
    public void CheckOrder_Test()
    {
        var path = "../../../../output.txt";

        Row last = default;
        var counter = 0;

        var fileInfo = new FileInfo(path);
        
        Console.WriteLine($"{DateTime.UtcNow} Begin file {fileInfo.FullName} (size: {fileInfo.Length})");
        
        foreach (var line in File.ReadLines(path))
        {
            if(string.IsNullOrEmpty(line))
                break;
            
            var row = RowFactory.Parse(line).First();

            if (counter++ > 0)
            {
                var prevText = last.ToString();
                var currText = row.ToString();
                var diff = String
                    .Compare(prevText.Substring(last.FirstCharIndex), currText.Substring(row.FirstCharIndex),
                        StringComparison.InvariantCulture);

                if (diff == 0)
                    diff = last.Number.CompareTo(row.Number);
                
                Assert.True(diff <= 0, $"{DateTime.UtcNow} Wrong order at {counter} ({prevText} vs {currText})");
            }

            last = row;
        }
        
        Console.WriteLine($"{DateTime.UtcNow} Done (rows: {counter})");
    }
}