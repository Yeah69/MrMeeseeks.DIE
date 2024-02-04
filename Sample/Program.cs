﻿using System;
using MrMeeseeks.DIE.Sample;

internal interface IInterface<T0> {}

internal class Class<T0> : IInterface<T0> {}

internal class Program
{
    private static void Main()
    {
        try
        {
            using var container = Container.DIE_CreateContainer(); 
            var asdf = container.Create();
            Console.WriteLine("Hello, World!");
        }
        catch (Exception)
        {
            // ignored
        }
    }
}