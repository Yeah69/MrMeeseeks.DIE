using System;
using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;

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

//*
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
}//*/

internal sealed class Class : IInterface
{
    public void Procedure() { }
}

[InterceptorChoice(typeof(Interceptor), typeof(IInterface))]
[DecoratorSequenceChoice(typeof(IInterface), typeof(IInterface), typeof(DecoratorA), typeof(Interceptor), typeof(DecoratorB))]
[CreateFunction(typeof(IInterface), "Create")]
[CreateFunction(typeof(TrackUsedDecorationTypes), "CreateTrackUsedDecorationTypes")]
internal sealed partial class Container;

/*
[InvocationDescription]
internal interface IInvocation
{
    object[] Arguments { get; }
    Type[] GenericArguments { get; }
    object InvocationTarget { get; }
    MethodInfo Method { get; }
    MethodInfo MethodInvocationTarget { get; }
    object Proxy { get; }
    object ReturnValue { get; }
    Type TargetType { get; }
    void Proceed();
}

internal class TrackLastInvocation : IContainerInstance
{
    internal IInvocation? LastInvocation { get; set; }
}

internal class Interceptor
{
    private readonly TrackLastInvocation _trackLastInvocation;

    internal Interceptor(TrackLastInvocation trackLastInvocation)
    {
        _trackLastInvocation = trackLastInvocation;
    }
    
    internal void Intercept(IInvocation invocation)
    {
        _trackLastInvocation.LastInvocation = invocation;
        invocation.Proceed();
    }
}

internal interface IInterface
{
    void Procedure();
    void Procedure(int valueType);
    void Procedure(object referenceType, int valueType);
    void Procedure<TA, TB>(object referenceType, int valueType, TA ta, TB tb);
    void Procedure<TA, TB>(object referenceType, int valueType, TB tb, TA ta) where TA : class where TB : struct;
    int Function();
    object Function(int valueType);
    int Function(object referenceType, int valueType);
    object Function<TA, TB>(object referenceType, int valueType, TA ta, TB tb);
    int Function<TA, TB>(object referenceType, int valueType, TB tb, TA ta) where TA : class where TB : struct;
    int Prop { get; set; }
    int PropGet { get; }
    int PropSet { set; }
    event EventHandler Event;
    int this[int index] { get; set; }
}

internal sealed class Class : IInterface
{
    public void Procedure() { }
    public void Procedure(int valueType) { }
    public void Procedure(object referenceType, int valueType) { }
    public void Procedure<TA, TB>(object referenceType, int valueType, TA ta, TB tb) { }
    public void Procedure<TA, TB>(object referenceType, int valueType, TB tb, TA ta) where TA : class where TB : struct { }
    public int Function() => 0;
    public object Function(int valueType) => valueType;
    public int Function(object referenceType, int valueType) => valueType;
    public object Function<TA, TB>(object referenceType, int valueType, TA ta, TB tb) => valueType;
    public int Function<TA, TB>(object referenceType, int valueType, TB tb, TA ta) where TA : class where TB : struct => valueType;
    public int Prop { get; set; }
    public int PropGet { get; }
    public int PropSet { set{} }
    public event EventHandler? Event;
    public int this[int index] { get => index; set { } }
}

[InterceptorChoice(typeof(Interceptor), typeof(IInterface))]
[CreateFunction(typeof(IInterface), "Create")]
[CreateFunction(typeof(TrackLastInvocation), "CreateTrackLastInvocation")]
internal sealed partial class Container;

/*

[InvocationDescription]
internal interface IInvocation
{
    object[] Arguments { get; }
    Type[] GenericArguments { get; }
    object InvocationTarget { get; }
    MethodInfo Method { get; }
    MethodInfo MethodInvocationTarget { get; }
    object Proxy { get; }
    object ReturnValue { get; }
    Type TargetType { get; }
    void Proceed();
}

internal class Interceptor
{
    internal void Intercept(IInvocation invocation)
    {
        Console.WriteLine($"Type: {invocation.TargetType.FullName} Method: {invocation.Method.Name}");
        invocation.Proceed();
    }
}

internal interface IInterface
{
    void Do();
    int Dont<T, TA>(T t, string s, TA ta) where T : class where TA : struct;
    int Prop { get; set; }
    int PropGet { get; }
    int PropSet { set; }
    event EventHandler Event;
    int this[int index] { get; set; }
}

internal sealed class Class : IInterface
{
    public void Do()
    {
        Console.WriteLine("Do");
    }

    public int Dont<T, TA>(T t, string s, TA ta) where T : class where TA : struct
    {
        return 69;
    }

    public int Prop { get; set; } = 69;
    public int PropGet { get; } = 69;
    public int PropSet { get; set; } = 69;
    public event EventHandler? Event;

    public int this[int index]
    {
        get { return index; }
        set { value = 69; }
    }
}

[InterceptorChoice(typeof(Interceptor), typeof(IInterface))]
[CreateFunction(typeof(IInterface), "Create")]
internal sealed partial class Container; //*/
