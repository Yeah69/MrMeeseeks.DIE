using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

namespace MrMeeseeks.DIE.Test.Interceptor.MixedDecoration;

[InvocationDescription]
internal interface IInvocation
{
    void Proceed();
}

internal class TrackUsedDecorationTypes : IContainerInstance
{
    internal List<Type> _decorationTypes = [];
    internal IReadOnlyList<Type> DecorationTypes => _decorationTypes;
    internal void RegisterDecoration(Type decorationType) => _decorationTypes.Add(decorationType);
}

internal class Interceptor
{
    private readonly TrackUsedDecorationTypes _trackUsedDecorationTypes;

    internal Interceptor(TrackUsedDecorationTypes trackUsedDecorationTypes)
    {
        _trackUsedDecorationTypes = trackUsedDecorationTypes;
    }
    
    internal void Intercept(IInvocation invocation)
    {
        _trackUsedDecorationTypes.RegisterDecoration(GetType());
        invocation.Proceed();
    }
}

internal interface IInterface
{
    void Procedure();
}

internal class DecoratorA(IInterface decorated, TrackUsedDecorationTypes trackUsedDecorationTypes) : IDecorator<IInterface>, IInterface
{
    public void Procedure()
    {
        trackUsedDecorationTypes.RegisterDecoration(GetType());
        decorated.Procedure();
    }
}

internal class DecoratorB(IInterface decorated, TrackUsedDecorationTypes trackUsedDecorationTypes) : IDecorator<IInterface>, IInterface
{
    public void Procedure()
    {
        trackUsedDecorationTypes.RegisterDecoration(GetType());
        decorated.Procedure();
    }
}

internal sealed class Class : IInterface
{
    public void Procedure() { }
}

[InterceptorChoice(typeof(Interceptor), typeof(IInterface))]
[DecoratorSequenceChoice(typeof(IInterface), typeof(IInterface), typeof(DecoratorA), typeof(Interceptor), typeof(DecoratorB))]
[CreateFunction(typeof(IInterface), "Create")]
[CreateFunction(typeof(TrackUsedDecorationTypes), "CreateTrackUsedDecorationTypes")]
internal sealed partial class Container;


public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var trackLastInvocation = container.CreateTrackUsedDecorationTypes();
        var instance = container.Create();
        
        instance.Procedure();
        trackLastInvocation.DecorationTypes.SequenceEqual(new [] { typeof(DecoratorB), typeof(Interceptor), typeof(DecoratorA) });
    }
}//*/