using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Sample;

internal class Program
{
    private static async Task Main()
    {
        try
        {
            var container = MixedSynchronicityScopedInstanceMore.Container.DIE_CreateContainer(); 
            var parent = container.Create();

            //var sync = parent.Sync;
            //var async = await parent.Async;
            //await foreach (var dep in parent.Dependencies)
            //    _ = dep;
            
            Console.WriteLine("Hello, World!");
        }
        catch (Exception)
        {
            // ignored
        }
    }
}