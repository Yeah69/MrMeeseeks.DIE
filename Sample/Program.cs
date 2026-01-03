using System;
using MrMeeseeks.DIE.Sample;

internal class Program
{
    private static void Main()
    {
        try
        {
            var container = Container.DIE_CreateContainer(); 
            var parent = container.Create();

            Console.WriteLine("Hello, World!");
        }
        catch (Exception)
        {
            // ignored
        }
    }
}