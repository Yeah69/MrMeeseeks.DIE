using System;
using MrMeeseeks.DIE.Sample;

internal class Program
{
    private static void Main()
    {
        var asdf = new Container().Create();
        asdf.Clean();
        Console.WriteLine("Hello, World!");
    }
}