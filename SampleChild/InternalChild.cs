using System;

namespace MrMeeseeks.DIE.SampleChild;

public interface IInternalChild
{}
internal class InternalChild : IInternalChild, IDisposable, IScopedInstance
{
    public InternalChild(
        Lazy<IYetAnotherInternalChild> yetAnotherInternalChild,
        IAndThenAnotherScope andThenAnotherScope)
    {
            
    }

    public void Dispose()
    {
    }
}
    
public interface IAndThenAnotherScope
{}
internal class AndThenAnotherScope : IAndThenAnotherScope, IDisposable, IScopedInstance, IScopeRoot
{
    public void Dispose()
    {
    }
}
    
public interface IYetAnotherInternalChild
{}
internal class YetAnotherInternalChild : IYetAnotherInternalChild, IDisposable, IScopedInstance
{
    public YetAnotherInternalChild(
        Func<IA, IAndThenSingleInstance> andThenSingleInstanceA,
        Func<IB, IAndThenSingleInstance> andThenSingleInstanceB)
    {}
    public void Dispose()
    {
    }
}
    
public interface IAndThenSingleInstance
{}
internal class AndThenSingleInstance : IAndThenSingleInstance, IDisposable, ISingleInstance
{
    public AndThenSingleInstance(
        IA a,
        IB b){}
    public void Dispose()
    {
    }
}

public interface IA {}
public interface IB {}

public class A : IA {}
public class B : IB {}