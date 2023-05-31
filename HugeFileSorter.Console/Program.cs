using HugeFileSorter;
using HugeFileSorter.Extensions;
using HugeFileSorter.Repositories;

var config = GetConfig();
var repositoryConfig = new RepositoryConfig
{
    SourceFile = config.inputFile,
    TargetFile = config.outputFile
};

var repository = new FileRepository(repositoryConfig);
var service = new Service(config.chunkSize, repository);

await service.ExecuteAsync();

static (int chunkSize, string inputFile, string outputFile) GetConfig()
{
    var chunkSize = 200.ToMB();
    var inputFile = "input.txt";
    var outputFile = "output.txt";

    var args = Environment.GetCommandLineArgs().Skip(1).ToArray();

    try
    {
        for (var i = 0; i < args.Length; ++i)
        {
            var currentArg = args[i];
            switch (i)
            {
                case 0:
                    chunkSize = int.Parse(currentArg).ToMB();
                    break;
                case 1:
                    outputFile = currentArg;
                    break;
                case 2:
                    inputFile = currentArg;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    catch
    {
        throw new ArgumentException(
            @$"Expected arguments: [{nameof(chunkSize)} [{nameof(outputFile)} [{nameof(inputFile)}]]]
1. {nameof(chunkSize)} - chunk file/memory size (int), default (MB): 200     
2. {nameof(outputFile)} - max row length symbol/words (string), default: output.txt
3. {nameof(inputFile)} - output file name (string), default: input.txt");
    }

    return (chunkSize, inputFile, outputFile);
}
