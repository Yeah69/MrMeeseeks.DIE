using System;
using MrMeeseeks.DIE.SampleChild;

namespace MrMeeseeks.DIE.Sample
{
    internal interface IContext
    {
        string Text { get; }
    }

    internal class Context : IContext
    {
        public string Text => "Hello, world!";
        public Context(Func<IYetAnotherInternalChild, IChild> child)
        {

        }
    }
}
