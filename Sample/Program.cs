﻿using System;
using MrMeeseeks.DIE.Sample;

internal class Program
{
    private static void Main()
    {
        try
        {
            //using var container = Container<int>.DIE_CreateContainer(); 
            //var asdf = container.Create();
            
            Console.WriteLine("Hello, World!");
        }
        catch (Exception)
        {
            // ignored
        }
    }
}