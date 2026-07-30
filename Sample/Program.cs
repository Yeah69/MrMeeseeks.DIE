using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Sample;

internal class Program
{
    private static async Task Main()
    {
        try
        {
            var container = MixedSynchronicity.Container.DIE_CreateContainer(); 
            var parent = await container.Create();
            
            Console.WriteLine("Hello, World!");
        }
        catch (Exception)
        {
            // ignored
        }
    }
}