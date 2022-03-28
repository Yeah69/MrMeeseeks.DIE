using System;
using MrMeeseeks.DIE.Test.Async.Awaited.FunctionCall;

internal class Program
{
    private static void Main()
    {
        Console.WriteLine("Hello, world!");
        var container = new Container();
        Console.WriteLine(container.CreateAsync());
    }
}