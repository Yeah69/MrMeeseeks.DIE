using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Test.Collection.Injection.IAsyncEnumerable;

internal class Program
{
    private static async Task Main()
    {
        try
        {
            await using var container = new Container();
            var asdf = container.Create();
            Console.WriteLine("Hello, World!");
        }
        catch (Exception)
        {
            // ignored
        }
    }
}