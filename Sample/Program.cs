using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Sample;

internal class Program
{
    private static async Task Main()
    {
        try
        {
            var container = Container.DIE_CreateContainer(); 
            var asdf = container.Create();
            await container.DisposeAsync();
            Console.WriteLine("Hello, World!");
        }
        catch (Exception)
        {
            // ignored
        }
    }
}