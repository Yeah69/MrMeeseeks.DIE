﻿using System;
using MrMeeseeks.DIE.Sample;

internal class Program
{
    private static void Main()
    {
        try
        {
            using var container = Container.DIE_CreateContainer(); 
            var asdf = container.Create<InnerClass<InnerInterface<int>>, int>();
            Console.WriteLine("Hello, World!");
        }
        catch (Exception)
        {
            // ignored
        }
    }
}