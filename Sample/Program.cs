using System;
using System.Collections.Generic;

internal class Foo<T0, T1> where T1 : IList<T0>
{
    
}

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