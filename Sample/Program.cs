using System;
using System.ComponentModel;

internal class Program
{
    private static void Main()
    {
        try
        {
            //using var container = Container.DIE_CreateContainer();
            //var asdf = container.Create<int, string, double>();
            Console.WriteLine("Hello, World!");
        }
        catch (Exception)
        {
            // ignored
        }
    }
}