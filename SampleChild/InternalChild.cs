namespace MrMeeseeks.DIE.SampleChild
{
    public interface IInternalChild
    {}
    internal class InternalChild : IInternalChild
    {
        public InternalChild(IYetAnotherInternalChild yetAnotherInternalChild)
        {
            
        }
    }
    
    public interface IYetAnotherInternalChild
    {}
    internal class YetAnotherInternalChild : IYetAnotherInternalChild
    {
        
    }
}