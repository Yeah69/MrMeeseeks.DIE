using System;
using MrMeeseeks.DIE.Sample;

internal class Program
{
    private static void Main()
    {
        var asdf = new Container().Create("Hello, Earth!");
        Console.WriteLine(asdf.Dependency.Parameter);
        Console.WriteLine("Hello, World!");
    }
}