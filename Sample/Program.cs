﻿using System;

internal class Program
{
    private static void Main()
    {
        try
        {
            //using var container = Container.DIE_CreateContainer(); 
            //var asdf = container.Create()(3);
            Console.WriteLine("Hello, World!");
        }
        catch (Exception)
        {
            // ignored
        }
    }
}