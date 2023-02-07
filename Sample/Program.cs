using System;
using MrMeeseeks.DIE.Sample;

internal class Program
{
    private static void Main()
    {
        var asdf = new Container().Create();
        Console.WriteLine("Hello, World!");
    }
}