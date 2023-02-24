using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Test.Bugs.ReuseOfFieldFactory;

internal class Program
{
    private static async Task Main()
    {
        try
        {
            await using var container = new Container(new Dependency());
            var asdf = container.CreateHolder();
            Console.WriteLine("Hello, World!");
        }
        catch (Exception)
        {
            // ignored
        }
    }
}