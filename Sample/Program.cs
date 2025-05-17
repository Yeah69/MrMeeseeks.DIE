using System;
using System.Collections.Generic;
using System.Linq;
using MrMeeseeks.DIE.Sample;

internal class Program
{
    private static void Main()
    {
        try
        {
            var doSomething = CreateDoSomething(new Context(0, 0));
            var doSomethings = CreateDoSomethingEnumerable().ToArray();
            //var container = Container.DIE_CreateContainer(); 
            //var dialog = container.Create();
            
            Console.WriteLine("Hello, World!");
        }
        catch (Exception)
        {
            // ignored
        }
    }
    
    internal interface IDoSomething
    {
        void Do();
    }
    
    internal class DoSomething : IDoSomething
    {
        public void Do()
        {
            Console.WriteLine("Doing something...");
        }
    }
    
    internal class DoSomethingElse : IDoSomething
    {
        public void Do()
        {
            Console.WriteLine("Doing something else...");
        }
    }
    
    internal class DoSomethingDecorator(IDoSomething doSomething) : IDoSomething
    {
        public void Do()
        {
            Console.WriteLine("Decorating...");
            doSomething.Do();
        }
    }

    internal record Context(int Interface, int Implementation);
    
    internal static IDoSomething CreateDoSomething(Context context)
    {
        IDoSomething ret;
        if (context.Interface != 1)
            ret = CreateDoSomething(new Context(1, 3));
        else
            switch (context.Implementation)
            {
                case 1:
                    ret = new DoSomething();
                    break;
                case 2:
                    ret = new DoSomethingElse();
                    break;
                case 3:
                    var inner = CreateDoSomething(new Context(1, 1));
                    ret = new DoSomethingDecorator(inner);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(context.Implementation), "Invalid implementation");
            }
        return ret;
    }
    
    internal static IEnumerable<IDoSomething> CreateDoSomethingEnumerable()
    {
        yield return CreateDoSomething(new Context(1, 3));
        yield return CreateDoSomething(new Context(1, 2));
    }
}