using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

namespace MrMeeseeks.DIE.Test.InterfaceInterception.Interceptor.Vanilla;

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


public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var trackLastInvocation = container.CreateTrackLastInvocation();
        var instance = container.Create();
        
        instance.Procedure();
        Assert.NotNull(trackLastInvocation.LastInvocation);
        if (trackLastInvocation.LastInvocation is {} procedureInvocation)
        {
            var procedureMethod = typeof(IInterface).GetMethods().First(m => m.GetParameters().Length == 0 && m.Name == nameof(IInterface.Procedure));
            Assert.Equal(procedureMethod, procedureInvocation.Method);
        }

        _ = instance.Function();
        Assert.NotNull(trackLastInvocation.LastInvocation);
        if (trackLastInvocation.LastInvocation is {} functionInvocation)
        {
            var functionMethod = typeof(IInterface).GetMethods().First(m => m.GetParameters().Length == 0 && m.Name == nameof(IInterface.Function));
            Assert.Equal(functionMethod, functionInvocation.Method);
        }
        
        instance.Prop = 42;
        Assert.NotNull(trackLastInvocation.LastInvocation);
        if (trackLastInvocation.LastInvocation is {} propInvocation)
        {
            var propSetMethod = typeof(IInterface).GetMethods().First(m => m.Name == "set_Prop");
            Assert.Equal(propSetMethod, propInvocation.Method);
        }
        
        _ = instance.Prop;
        Assert.NotNull(trackLastInvocation.LastInvocation);
        if (trackLastInvocation.LastInvocation is {} propGetInvocation)
        {
            var propGetMethod = typeof(IInterface).GetMethods().First(m => m.Name == "get_Prop");
            Assert.Equal(propGetMethod, propGetInvocation.Method);
        }
        
        instance.Event += (_, _) => { };
        Assert.NotNull(trackLastInvocation.LastInvocation);
        if (trackLastInvocation.LastInvocation is {} eventInvocation)
        {
            var eventAddMethod = typeof(IInterface).GetMethods().First(m => m.Name == "add_Event");
            Assert.Equal(eventAddMethod, eventInvocation.Method);
        }
        
        instance[42] = 42;
        Assert.NotNull(trackLastInvocation.LastInvocation);
        if (trackLastInvocation.LastInvocation is {} indexerInvocation)
        {
            var indexerSetMethod = typeof(IInterface).GetMethods().First(m => m.Name == "set_Item");
            Assert.Equal(indexerSetMethod, indexerInvocation.Method);
        }
        
        _ = instance[42];
        Assert.NotNull(trackLastInvocation.LastInvocation);
        if (trackLastInvocation.LastInvocation is {} indexerGetInvocation)
        {
            var indexerGetMethod = typeof(IInterface).GetMethods().First(m => m.Name == "get_Item");
            Assert.Equal(indexerGetMethod, indexerGetInvocation.Method);
        }
        //*/
    }
}//*/