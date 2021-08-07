using System;

namespace MrMeeseeks.StaticDelegate.Sample
{
    internal interface IContext
    {
        String Text { get; }
    }

    internal class Context : IContext
    {
        public String Text => "Hello, world";
        public Context()//(Child child)
        {

        }
    }
}
