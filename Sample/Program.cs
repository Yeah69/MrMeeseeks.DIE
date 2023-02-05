using System;
using System.Linq;
using MrMeeseeks.DIE.Sample;

internal class Program
{
    private static void Main()
    {
        var asdf = new Container().Create("Hello, Earth!");
        Console.WriteLine(string.Join(", ", asdf.Result.DependencyValueTaskList.Result.Select(d => d.Parameter)));
        Console.WriteLine("Hello, World!");
    }
}