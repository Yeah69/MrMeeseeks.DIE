using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Test.Composite.ScopeRoot;

internal class Program
{
    private static async Task Main()
    {
        try
        {
            await using var container = new Container();
            var asdf = container.CreateDep();
            Console.WriteLine("Hello, World!");
        }
        catch (Exception)
        {
            // ignored
        }
    }
}