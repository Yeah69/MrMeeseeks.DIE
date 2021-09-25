using System;
using MrMeeseeks.DIE.SampleChild;

namespace MrMeeseeks.DIE.Sample
{
    internal interface IContext
    {
        String Text { get; }
    }

    internal class Context : IContext
    {
        public String Text => "Hello, world";
        public Context(IChild child)
        {

        }
    }
}
