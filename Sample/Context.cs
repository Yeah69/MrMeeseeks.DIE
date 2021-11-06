using System;
using System.Collections.Generic;
using MrMeeseeks.DIE.SampleChild;

namespace MrMeeseeks.DIE.Sample
{
    internal interface IContext
    {
        string Text { get; }
    }

    internal class Context : IContext, IDisposable, ISingleInstance
    {
        public string Text => "Hello, world!";
        public Context(IReadOnlyList<IChild> child)
        {

        }

        public void Dispose()
        {
        }
    }
}
