namespace HugeFileSorter.Repositories;

public class RepositoryConfig
{
    public string SourceFile { get; set; }
    public string TargetFile { get; set; }
    public string TempDir { get; set; } = "temp";
    public int BuffersCount { get; set; } = 2;
}