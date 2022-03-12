using System;
using MrMeeseeks.DIE.Test;

internal class Program
{
    private static void Main()
    {
        Console.WriteLine("Hello, world!");
        var container = new DecoratorScopeRootContainer();
        Console.WriteLine(container.Create0());
    }
}