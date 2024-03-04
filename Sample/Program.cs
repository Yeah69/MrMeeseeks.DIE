using System;
using System.Runtime.CompilerServices;

internal class Program
{
    private static void Main()
    {
        try
        {
            //using var container = Container.DIE_CreateContainer(); 
            //var asdf = container.Create();
            Console.WriteLine("Hello, World!");
        }
        catch (Exception)
        {
            // ignored
        }
    }
}