using System;

namespace MrMeeseeks.DIE.SampleChild;

public interface IChild
{ }

public class Child : IChild, IDisposable, IScopeRoot
{
    public Child(
        IInternalChild innerChild){}

    public void Dispose()
    {
    }
}