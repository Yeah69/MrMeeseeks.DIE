using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Sample;

internal class Program
{
    private static async Task Main()
    {
        try
        {
            var container = MixedSynchronicityScopes.Container.DIE_CreateContainer(); 
            var parent = container.Create();

            //var sync = parent.Sync;
            //var async = await parent.Async;
            
            Console.WriteLine("Hello, World!");
        }
        catch (Exception)
        {
            // ignored
        }
    }
}