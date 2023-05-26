namespace HugeFileSorter.Extensions;

public static class IntExtensions
{
    public static int ToMB(this int number)
    {
        return number * 1024 * 1024;
    }
}