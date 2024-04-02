using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Decorator.SequenceEdgeCase;

internal interface IInterface
{
    IInterface Decorated { get; }
}

internal sealed class Dependency : IInterface, IContainerInstance
{
    public IInterface Decorated => this;
}

internal sealed class DecoratorA : IInterface, IDecorator<IInterface>
{
    public IInterface Decorated { get; }
    
    internal DecoratorA(IInterface decorated) => Decorated = decorated;
}

internal sealed class DecoratorB : IInterface, IDecorator<IInterface>
{
    public IInterface Decorated { get; }
    
    internal DecoratorB(IInterface decorated) => Decorated = decorated;
}

internal sealed class ScopeRoot0 : IScopeRoot
{
    public IInterface Decorated { get; }

    internal ScopeRoot0(IInterface decorated) => Decorated = decorated;
}

internal sealed class ScopeRoot1 : IScopeRoot
{
    public IInterface Decorated { get; }

    internal ScopeRoot1(IInterface decorated) => Decorated = decorated;
}

[CreateFunction(typeof(ScopeRoot0), "Create0")]
[CreateFunction(typeof(ScopeRoot1), "Create1")]
[CreateFunction(typeof(IInterface), "CreateFromContainerAsSanityCheck")]
[DecoratorSequenceChoice(typeof(IInterface), typeof(IInterface))]
internal sealed partial class Container
{
    
    [DecoratorSequenceChoice(typeof(IInterface), typeof(IInterface), typeof(DecoratorA), typeof(DecoratorB))]
    [CustomScopeForRootTypes(typeof(ScopeRoot0))]
    // ReSharper disable once InconsistentNaming
    private sealed partial class DIE_Scope_0;
    
    [DecoratorSequenceChoice(typeof(IInterface), typeof(IInterface), typeof(DecoratorA))]
    [CustomScopeForRootTypes(typeof(ScopeRoot1))]
    // ReSharper disable once InconsistentNaming
    private sealed partial class DIE_Scope_1;
}

public sealed class Tests
{
    [Fact]
    public void Test()
    {
        using var container0Then1 = Container.DIE_CreateContainer();

        var _0Then1_0_2 = container0Then1.Create0().Decorated;
        var _0Then1_0_1 = _0Then1_0_2.Decorated;
        var _0Then1_0_0 = _0Then1_0_1.Decorated;

        var _0Then1_1_1 = container0Then1.Create1().Decorated;
        var _0Then1_1_0 = _0Then1_1_1.Decorated;

        var _0Then1_SanityCheck = container0Then1.CreateFromContainerAsSanityCheck();

        Assert.IsType<DecoratorB>(_0Then1_0_2);
        Assert.IsType<DecoratorA>(_0Then1_0_1);
        Assert.IsType<Dependency>(_0Then1_0_0);

        Assert.IsType<DecoratorA>(_0Then1_1_1);
        Assert.IsType<Dependency>(_0Then1_1_0);

        Assert.IsType<Dependency>(_0Then1_SanityCheck);

        Assert.Same(_0Then1_1_0, _0Then1_0_0);
        Assert.Same(_0Then1_1_0, _0Then1_SanityCheck);
        

        using var container1Then0 = Container.DIE_CreateContainer();

        var _1Then0_1_1 = container1Then0.Create1().Decorated;
        var _1Then0_1_0 = _1Then0_1_1.Decorated;

        var _1Then0_0_2 = container1Then0.Create0().Decorated;
        var _1Then0_0_1 = _1Then0_0_2.Decorated;
        var _1Then0_0_0 = _1Then0_0_1.Decorated;

        var _1Then0_SanityCheck = container1Then0.CreateFromContainerAsSanityCheck();

        Assert.IsType<DecoratorB>(_1Then0_0_2);
        Assert.IsType<DecoratorA>(_1Then0_0_1);
        Assert.IsType<Dependency>(_1Then0_0_0);

        Assert.IsType<DecoratorA>(_1Then0_1_1);
        Assert.IsType<Dependency>(_1Then0_1_0);

        Assert.IsType<Dependency>(_1Then0_SanityCheck);

        Assert.Same(_1Then0_1_0, _1Then0_0_0);
        Assert.Same(_1Then0_1_0, _1Then0_SanityCheck);
    }
}