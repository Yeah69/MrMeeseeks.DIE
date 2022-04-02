using System;
using MrMeeseeks.DIE.Test.Decorator.SequenceEdgeCase;

internal class Program
{
    private static void Main()
    {
        Console.WriteLine("Hello, world!");
        var container = new Container();
        Console.WriteLine(container.Create0Async());
    }
}