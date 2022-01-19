namespace MrMeeseeks.DIE.Sample;

internal interface ITransientScopeInstanceInner {}
internal class TransientScopeInstance : ITransientScopeInstanceInner, ITransientScopeInstance {}

internal interface ITransientScopeWithTransientScopeInstance {}

internal class TransientScopeWithTransientScopeInstance : ITransientScopeWithTransientScopeInstance, ITransientScopeRoot
{
    public TransientScopeWithTransientScopeInstance(ITransientScopeInstanceInner _) {}
}

internal partial class TransientScopeInstanceContainer : IContainer<ITransientScopeWithTransientScopeInstance>
{
    
}