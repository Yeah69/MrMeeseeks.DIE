using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE;
using MrMeeseeks.DIE.Test.Async.TaskCollection;

internal class Program
{
    private static void Main()
    {
        Console.WriteLine("Hello, world!");
        var container = new Container();
        Console.WriteLine(((IContainer<IReadOnlyList<Task<IInterface>>>) container).Resolve());
    }
}