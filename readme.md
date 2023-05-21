HugeFileSorter
=
Requirements 
* .net7
Quick start
-
build solution
```bash
dotnet build --configuration Release
```
generate file input.txt
```bash
dotnet HugeFileSorter.Generator\bin\Release\net7.0\HugeFileSorter.Generator.dll 
```
run app 
```bash
dotnet HugeFileSorter.Console\bin\Release\net7.0\HugeFileSorter.Console.dll
```
⚠️ check log and generated output.txt

clear artefacts
```bash
del input.txt
del output.txt
rmdir temp 
```

HugeFileSorter.Generator
-
generate file for tests, args:

```txt
Expected arguments: [maxSize [maxStringLength [fileName]]]
1. maxSize - output file size (int), default (MB): 1000
2. maxStringLength - max row length symbol/words (int), default: 10
3. fileName - output file name (string), default: input.txt
```

HugeFileSorter.Console
-

entry point, args:

```txt
Expected arguments: [chunkSize [inputFile [outputFile]]]
1. chunkSize - output file size (int), default (MB): 200
2. inputFile - output file name (string), default: input.txt
3. outputFile - max row length symbol/words (string), default: output.txt
```

HugeFileSorter
-
sort lib
