using Xunit;

namespace MrMeeseeks.DIE.Test;

internal interface ITransientScopeInstanceInner {}
internal class TransientScopeInstance : ITransientScopeInstanceInner, ITransientScopeInstance {}

internal partial class TransientScopeInstanceContainer : IContainer<ITransientScopeInstanceInner>
{
    
}

public partial class TransientScopeInstanceTests
{
    [Fact]
    public void InContainer()
    {
        using var container = new TransientScopeInstanceContainer();
        var _ = ((IContainer<ITransientScopeInstanceInner>) container).Resolve();
    }
}

internal interface IScopeWithTransientScopeInstance {}

internal class ScopeWithTransientScopeInstance : IScopeWithTransientScopeInstance, IScopeRoot
{
    public ScopeWithTransientScopeInstance(ITransientScopeInstanceInner _) {}
}

internal partial class TransientScopeInstanceContainer : IContainer<IScopeWithTransientScopeInstance>
{
    
}

public partial class TransientScopeInstanceTests
{
    [Fact]
    public void InScope()
    {
        using var container = new TransientScopeInstanceContainer();
        var _ = ((IContainer<IScopeWithTransientScopeInstance>) container).Resolve();
    }
}

internal interface IScopeWithTransientScopeInstanceAbove {}

internal class ScopeWithTransientScopeInstanceAbove : IScopeWithTransientScopeInstanceAbove, IScopeRoot
{
    public ScopeWithTransientScopeInstanceAbove(IScopeWithTransientScopeInstance _) {}
}

internal partial class TransientScopeInstanceContainer : IContainer<IScopeWithTransientScopeInstanceAbove>
{
    
}

public partial class TransientScopeInstanceTests
{
    [Fact]
    public void InScopeInScope()
    {
        using var container = new TransientScopeInstanceContainer();
        var _ = ((IContainer<IScopeWithTransientScopeInstanceAbove>) container).Resolve();
    }
}