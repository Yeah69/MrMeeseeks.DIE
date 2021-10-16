using System;

namespace MrMeeseeks.DIE.SampleChild
{
    public interface IInternalChild
    {}
    internal class InternalChild : IInternalChild, IDisposable
    {
        public InternalChild(Lazy<IYetAnotherInternalChild> yetAnotherInternalChild)
        {
            
        }

        public void Dispose()
        {
        }
    }
    
    public interface IYetAnotherInternalChild
    {}
    internal class YetAnotherInternalChild : IYetAnotherInternalChild, IDisposable
    {
        public void Dispose()
        {
        }
    }
}