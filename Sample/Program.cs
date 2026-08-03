using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Sample;

internal class Program
{
    private static async Task Main()
    {
        try
        {
            var container = MixedSynchronicityScopedInstance.Container.DIE_CreateContainer(); 
            var parent = container.Create();

            var sync = parent.Sync;
            var async = await parent.Async;
            
            Console.WriteLine("Hello, World!");
        }
        catch (Exception)
        {
            // ignored
        }
    }
}