using System;

namespace MrMeeseeks.DIE.SampleChild
{
    public interface IChild
    { }

    public class Child : IChild, IDisposable
    {
        public Child(
            IInternalChild innerChild){}

        public void Dispose()
        {
        }
    }
}
