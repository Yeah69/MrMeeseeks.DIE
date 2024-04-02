using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;
// ReSharper disable UnusedParameter.Local
// ReSharper disable InconsistentNaming

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Scoping.ScopeSpecificAttributes.Decorator;

internal interface IInterface
{
    int CheckNumber { get; }
}

internal sealed class Implementation : IInterface
{
    public int CheckNumber => 1;
}

internal sealed class ContainerDecorator : IInterface, IDecorator<IInterface>
{
    public int CheckNumber => 69;
    
    internal ContainerDecorator(IInterface _) {}
}

internal sealed class ScopeDecorator : IInterface, IDecorator<IInterface>
{
    public int CheckNumber => 3;
    
    internal ScopeDecorator(IInterface _) {}
}

internal sealed class ScopeSpecificDecorator : IInterface, IDecorator<IInterface>
{
    public int CheckNumber => 13;
    
    internal ScopeSpecificDecorator(IInterface _) {}
}

internal sealed class TransientScopeDecorator : IInterface, IDecorator<IInterface>
{
    public int CheckNumber => 23;
    
    internal TransientScopeDecorator(IInterface _) {}
}

internal sealed class TransientScopeSpecificDecorator : IInterface, IDecorator<IInterface>
{
    public int CheckNumber => 7;
    
    internal TransientScopeSpecificDecorator(IInterface _) {}
}

internal sealed class TransientScopeRoot : ITransientScopeRoot
{
    public IInterface Dep { get; }
    
    internal TransientScopeRoot(IInterface dep)
    {
        Dep = dep;
    }
}

internal sealed class TransientScopeRootSpecific : ITransientScopeRoot
{
    public IInterface Dep { get; }
    
    internal TransientScopeRootSpecific(IInterface dep)
    {
        Dep = dep;
    }
}

internal sealed class ScopeRootSpecific  : IScopeRoot
{
    public IInterface Dep { get; }
    
    internal ScopeRootSpecific (IInterface dep)
    {
        Dep = dep;
    }
}

internal sealed class ScopeRoot : IScopeRoot
{
    public IInterface Dep { get; }
    
    internal ScopeRoot(IInterface dep)
    {
        Dep = dep;
    }
}

[CreateFunction(typeof(IInterface), "Create0")]
[CreateFunction(typeof(TransientScopeRoot), "Create1")]
[CreateFunction(typeof(TransientScopeRootSpecific), "Create2")]
[CreateFunction(typeof(ScopeRoot), "Create3")]
[CreateFunction(typeof(ScopeRootSpecific), "Create4")]
[DecoratorSequenceChoice(typeof(IInterface), typeof(IInterface), typeof(ContainerDecorator))]
internal sealed partial class Container
{
    
    
    [DecoratorSequenceChoice(typeof(IInterface), typeof(IInterface), typeof(TransientScopeDecorator))]
    private sealed partial class DIE_DefaultTransientScope;
    
    [DecoratorSequenceChoice(typeof(IInterface), typeof(IInterface), typeof(TransientScopeSpecificDecorator))]
    [CustomScopeForRootTypes(typeof(TransientScopeRootSpecific))]
    private sealed partial class DIE_TransientScope_A;

    [DecoratorSequenceChoice(typeof(IInterface), typeof(IInterface), typeof(ScopeDecorator))]
    private sealed partial class DIE_DefaultScope;
    
    [DecoratorSequenceChoice(typeof(IInterface), typeof(IInterface), typeof(ScopeSpecificDecorator))]
    [CustomScopeForRootTypes(typeof(ScopeRootSpecific))]
    private sealed partial class DIE_Scope_A;
}

public sealed class Tests
{
    [Fact]
    public void Container()
    {
        using var container = Decorator.Container.DIE_CreateContainer();
        var instance = container.Create0();
        Assert.Equal(69, instance.CheckNumber);
    }
    [Fact]
    public void TransientScope()
    {
        using var container = Decorator.Container.DIE_CreateContainer();
        var instance = container.Create1();
        Assert.Equal(23, instance.Dep.CheckNumber);
    }
    [Fact]
    public void TransientScopeSpecific()
    {
        using var container = Decorator.Container.DIE_CreateContainer();
        var instance = container.Create2();
        Assert.Equal(7, instance.Dep.CheckNumber);
    }
    [Fact]
    public void Scope()
    {
        using var container = Decorator.Container.DIE_CreateContainer();
        var instance = container.Create3();
        Assert.Equal(3, instance.Dep.CheckNumber);
    }
    [Fact]
    public void ScopeSpecific()
    {
        using var container = Decorator.Container.DIE_CreateContainer();
        var instance = container.Create4();
        Assert.Equal(13, instance.Dep.CheckNumber);
    }
}