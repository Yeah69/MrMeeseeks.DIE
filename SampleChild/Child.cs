namespace MrMeeseeks.DIE.SampleChild
{
    public interface IChild
    { }

    public class Child : IChild
    {
        public Child(
            IInternalChild innerChild){}
    }
}
