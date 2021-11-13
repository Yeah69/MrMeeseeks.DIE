using MrMeeseeks.DIE;
using MrMeeseeks.DIE.SampleChild;
using SampleChild;

[assembly:Spy(typeof(IPublicTypeReport), typeof(IInternalTypeReport))]
[assembly:SingleInstance(typeof(ISingleInstance))]
[assembly:ScopedInstance(typeof(IScopedInstance))]
[assembly:ScopeRoot(typeof(IScopeRoot))]
[assembly:Transient(typeof(ITransient))]

namespace MrMeeseeks.DIE.Sample;

internal partial class Container : IContainer<IContext>
{
}