using System;
using MrMeeseeks.DIE.Sample;

internal class Program
{
    private static void Main()
    {
        try
        {
            var container = Container.DIE_CreateContainer(); 
            var asdf = container.Create("asdf0", 3, "asdf1", 23, 69);
            
            Console.WriteLine("Hello, World!");
        }
        catch (Exception)
        {
            // ignored
        }
    }
}