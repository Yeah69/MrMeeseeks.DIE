using System;
using MrMeeseeks.DIE.Test.Async.AwaitedDependency.Dependency;

internal class Program
{
    private static void Main()
    {
        Console.WriteLine("Hello, world!");
        var container = new Container();
        Console.WriteLine(container.CreateDepAsync().ConfigureAwait(false));
    }
}