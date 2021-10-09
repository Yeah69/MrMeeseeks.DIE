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

    public class Child0 : IChild, IDisposable
    {
        public Child0(
            IInternalChild innerChild){}

        public void Dispose()
        {
        }
    }

    public class Child1 : IChild
    {
        public Child1(
            IInternalChild innerChild){}
    }
}
