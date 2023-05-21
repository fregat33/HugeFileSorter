﻿using HugeFileSorter;
using HugeFileSorter.Repositories;

var config = GetConfig();

var repository = new FileRepository(config.inputFile, config.outputFile);
var service = new Service(config.chunkSize, repository);

await service.ExecuteAsync();

static (int chunkSize, string inputFile, string outputFile) GetConfig()
{
    var chunkSize = 200 * 1024 * 1024;
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
                    chunkSize = int.Parse(currentArg) * 1024 * 1024;
                    break;
                case 1:
                    outputFile = currentArg;
                    break;
                case 3:
                    inputFile = currentArg;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    catch
    {
        throw new ArgumentException(@$"Expected arguments: [{nameof(chunkSize)} [{nameof(inputFile)} [{nameof(outputFile)}]]]
1. {nameof(chunkSize)} - output file size (int), default (MB): 200     
2. {nameof(inputFile)} - output file name (string), default: input.txt 
3. {nameof(outputFile)} - max row length symbol/words (string), default: output.txt");
    }

    return (chunkSize, inputFile, outputFile);
}
