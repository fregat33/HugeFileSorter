using HugeFileSorter.Repositories;
using HugeFileSorter.Extensions;
using HugeFileSorter.Tests.Helpers;

namespace HugeFileSorter.Tests;

public class RepositoryTests
{
    [Theory]
    [InlineData(@"415. Apple
1. Apple
")]
    [InlineData(@"415. Apple
30432. Something something something
1. Apple
32. Cherry is the best
2. Banana is yellow
")]
    [InlineData(@"415. Apple
1. Apple")]
    public void ReadSource_Test(string text)
    {
        //given
        var config = new RepositoryConfig
        {
            SourceFile = $"{Guid.NewGuid().ToString()}.txt",
        };
        
        var repository = new FileRepository(config);
        
        File.WriteAllText(config.SourceFile, text);

        try
        {
            //when
            var rows = repository.GetSourceRows(1.ToMB()).First().Rows.ToArray();
            var expected = RowFactory.Parse(text);

            //then
            Assert.Equal(expected, rows);
        }
        finally
        {
            File.Delete(config.SourceFile);
        }
    }
    
    [Theory]
    [InlineData(@"415. Apple
1. Apple
")]
    [InlineData(@"415. Apple
30432. Something something something
1. Apple
32. Cherry is the best
2. Banana is yellow
")]
    [InlineData(@"415. Apple
30432. Something something something
1. Apple
32. Cherry is the best
2. Banana is yellow")]
    public void ReadSourceByChunks_Test(string text)
    {
        //given
        var stringEnd = (byte)'\n';
        var newLine = Environment.NewLine.Select(c => (byte)c).ToArray();
        
        var config = new RepositoryConfig
        {
            SourceFile = $"{Guid.NewGuid().ToString()}.txt",
            BuffersCount = 10
        };
        
        var repository = new FileRepository(config);
        
        File.WriteAllText(config.SourceFile, text);

        var maxRowLength = text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Max(r => r.Length) + newLine.Length;
        
        try
        {
            //when
            var rows = new List<Row>();

            foreach (var rowsChunk in repository.GetSourceRows(maxRowLength))
            {
                foreach (var row in rowsChunk.Rows)
                {
                    //create a copy of the record so as not to depend on the buffer in the chunk
                    var rowCopy = row.Copy();
                    rows.Add(rowCopy); 
                }

                rowsChunk.Release();
            }
            
            var expected = RowFactory.Parse(text);

            //then
            Assert.Equal(expected, rows);
        }
        finally
        {
            File.Delete(config.SourceFile);
        }
    }
    
    [Theory]
    [InlineData(@"415. Apple
1. Apple
", 2)]
    [InlineData(@"415. Apple
30432. Something something something
1. Apple
32. Cherry is the best
2. Banana is yellow
", 5)]
    public void WriteTarget_Test(string text, int expectedRows)
    {
        //given
        var config = new RepositoryConfig
        {
            TargetFile = $"{Guid.NewGuid().ToString()}.txt",
        };
        
        var repository = new FileRepository(config);
        var rows = RowFactory.Parse(text);
        
        try
        {
            //when
            var count = repository.WriteTarget(rows);

            //then
            Assert.Equal(expectedRows, count);
            var res = File.ReadAllText(config.TargetFile);
            Assert.Equal(text, res);
        }
        finally
        {
            File.Delete(config.TargetFile);
        }
    }
    
    [Theory]
    [InlineData(@"415. Apple
1. Apple
", 2)]
    [InlineData(@"415. Apple
30432. Something something something
1. Apple
32. Cherry is the best
2. Banana is yellow
", 5)]
    public void WriteChunk_ReadChunk_Test(string text, int rowsCount)
    {
        var config = new RepositoryConfig();
        try
        {
            var repository = new FileRepository(config);

            var id = new Random().Next(1, 100500);

            var expectedRows = RowFactory.Parse(text);
            var count = repository.WriteChunk(id, expectedRows);

            var actualRows = repository.ReadChunk(id);

            Assert.Equal(expectedRows, actualRows);
            Assert.Equal(rowsCount, count);
        }
        finally
        {
            Directory.Delete(config.TempDir, true);
        }
    }
}