using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test;

internal interface IScopeSpecificAttributesInterface
{
    int Number { get; }
}

internal class ScopeSpecificAttributesImplementation : IScopeSpecificAttributesInterface
{
    public int Number => 1;
}

internal class ScopeSpecificAttributesContai : IScopeSpecificAttributesInterface, IDecorator<IScopeSpecificAttributesInterface>
{
    public int Number => 69;
    
    internal ScopeSpecificAttributesContai(IScopeSpecificAttributesInterface _) {}
}

internal class ScopeSpecificAttributesScope : IScopeSpecificAttributesInterface, IDecorator<IScopeSpecificAttributesInterface>
{
    public int Number => 3;
    
    internal ScopeSpecificAttributesScope(IScopeSpecificAttributesInterface _) {}
}

internal class ScopeSpecificAttributesTransientScope : IScopeSpecificAttributesInterface, IDecorator<IScopeSpecificAttributesInterface>
{
    public int Number => 23;
    
    internal ScopeSpecificAttributesTransientScope(IScopeSpecificAttributesInterface _) {}
}

internal class ScopeSpecificAttributesDep
{
    public int Number { get; }
    
    internal ScopeSpecificAttributesDep(IScopeSpecificAttributesInterface scopeSpecificAttributesInterface)
    {
        Number = scopeSpecificAttributesInterface.Number;
    }
}

internal class ScopeSpecificAttributesTransientScopeRoot : ITransientScopeRoot
{
    public ScopeSpecificAttributesDep Dep { get; }
    
    internal ScopeSpecificAttributesTransientScopeRoot(ScopeSpecificAttributesDep dep)
    {
        Dep = dep;
    }
}

internal class ScopeSpecificAttributesScopeRoot : IScopeRoot
{
    public ScopeSpecificAttributesDep Dep { get; }
    
    internal ScopeSpecificAttributesScopeRoot(ScopeSpecificAttributesDep dep)
    {
        Dep = dep;
    }
}


[DecoratorSequenceChoice(typeof(IScopeSpecificAttributesInterface), typeof(ScopeSpecificAttributesContai))]
internal partial class ScopeSpecificAttributesContainer : IContainer<IScopeSpecificAttributesInterface>, IContainer<ScopeSpecificAttributesTransientScopeRoot>, IContainer<ScopeSpecificAttributesScopeRoot>
{
    [DecoratorSequenceChoice(typeof(IScopeSpecificAttributesInterface), typeof(ScopeSpecificAttributesTransientScope))]
    internal partial class DIE_DefaultTransientScope
    {
        
    }

    [DecoratorSequenceChoice(typeof(IScopeSpecificAttributesInterface), typeof(ScopeSpecificAttributesScope))]
    internal partial class DIE_DefaultScope
    {
        
    }
}

public partial class ScopeSpecificAttributesTests
{
    [Fact]
    public void Container()
    {
        using var container = new ScopeSpecificAttributesContainer();
        var instance = ((IContainer<IScopeSpecificAttributesInterface>) container).Resolve();
        Assert.Equal(69, instance.Number);
    }
    [Fact]
    public void TransientScope()
    {
        using var container = new ScopeSpecificAttributesContainer();
        var instance = ((IContainer<ScopeSpecificAttributesTransientScopeRoot>) container).Resolve();
        Assert.Equal(23, instance.Dep.Number);
    }
    [Fact]
    public void Scope()
    {
        using var container = new ScopeSpecificAttributesContainer();
        var instance = ((IContainer<ScopeSpecificAttributesScopeRoot>) container).Resolve();
        Assert.Equal(3, instance.Dep.Number);
    }
}