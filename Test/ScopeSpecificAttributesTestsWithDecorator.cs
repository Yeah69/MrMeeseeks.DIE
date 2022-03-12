using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.ScopeSpecificAttributesTestsWithDecorator;

internal interface IInterface
{
    int CheckNumber { get; }
}

internal class Implementation : IInterface
{
    public int CheckNumber => 1;
}

internal class ContainerDecorator : IInterface, IDecorator<IInterface>
{
    public int CheckNumber => 69;
    
    internal ContainerDecorator(IInterface _) {}
}

internal class ScopeDecorator : IInterface, IDecorator<IInterface>
{
    public int CheckNumber => 3;
    
    internal ScopeDecorator(IInterface _) {}
}

internal class ScopeSpecificDecorator : IInterface, IDecorator<IInterface>
{
    public int CheckNumber => 13;
    
    internal ScopeSpecificDecorator(IInterface _) {}
}

internal class TransientScopeDecorator : IInterface, IDecorator<IInterface>
{
    public int CheckNumber => 23;
    
    internal TransientScopeDecorator(IInterface _) {}
}

internal class TransientScopeSpecificDecorator : IInterface, IDecorator<IInterface>
{
    public int CheckNumber => 7;
    
    internal TransientScopeSpecificDecorator(IInterface _) {}
}

internal class TransientScopeRoot : ITransientScopeRoot
{
    public IInterface Dep { get; }
    
    internal TransientScopeRoot(IInterface dep)
    {
        Dep = dep;
    }
}

internal class TransientScopeRootSpecific : ITransientScopeRoot
{
    public IInterface Dep { get; }
    
    internal TransientScopeRootSpecific(IInterface dep)
    {
        Dep = dep;
    }
}

internal class ScopeRootSpecific  : IScopeRoot
{
    public IInterface Dep { get; }
    
    internal ScopeRootSpecific (IInterface dep)
    {
        Dep = dep;
    }
}

internal class ScopeRoot : IScopeRoot
{
    public IInterface Dep { get; }
    
    internal ScopeRoot(IInterface dep)
    {
        Dep = dep;
    }
}


[MultiContainer(typeof(IInterface), typeof(TransientScopeRoot), typeof(TransientScopeRootSpecific), typeof(ScopeRoot), typeof(ScopeRootSpecific))]
[DecoratorSequenceChoice(typeof(IInterface), typeof(ContainerDecorator))]
internal partial class Container
{
    [DecoratorSequenceChoice(typeof(IInterface), typeof(TransientScopeDecorator))]
    partial class DIE_DefaultTransientScope
    {
        
    }
    
    [DecoratorSequenceChoice(typeof(IInterface), typeof(TransientScopeSpecificDecorator))]
    [CustomScopeForRootTypes(typeof(TransientScopeRootSpecific))]
    partial class DIE_TransientScope_A
    {
        
    }

    [DecoratorSequenceChoice(typeof(IInterface), typeof(ScopeDecorator))]
    partial class DIE_DefaultScope
    {
        
    }
    
    [DecoratorSequenceChoice(typeof(IInterface), typeof(ScopeSpecificDecorator))]
    [CustomScopeForRootTypes(typeof(ScopeRootSpecific))]
    partial class DIE_Scope_A
    {
        
    }
}

public class ScopeSpecificAttributesTests
{
    [Fact]
    public void Container()
    {
        using var container = new Container();
        var instance = container.Create0();
        Assert.Equal(69, instance.CheckNumber);
    }
    [Fact]
    public void TransientScope()
    {
        using var container = new Container();
        var instance = container.Create1();
        Assert.Equal(23, instance.Dep.CheckNumber);
    }
    [Fact]
    public void TransientScopeSpecific()
    {
        using var container = new Container();
        var instance = container.Create2();
        Assert.Equal(7, instance.Dep.CheckNumber);
    }
    [Fact]
    public void Scope()
    {
        using var container = new Container();
        var instance = container.Create3();
        Assert.Equal(3, instance.Dep.CheckNumber);
    }
    [Fact]
    public void ScopeSpecific()
    {
        using var container = new Container();
        var instance = container.Create4();
        Assert.Equal(13, instance.Dep.CheckNumber);
    }
}