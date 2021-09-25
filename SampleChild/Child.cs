namespace MrMeeseeks.DIE.SampleChild
{
    public interface IChild
    { }

    public class Child : IChild
    {
        internal Child(
            IInternalChild innerChild){}
    }
}
