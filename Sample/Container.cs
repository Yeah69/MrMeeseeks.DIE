using System;
using System.Linq;
using System.Reflection;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.Descriptions;

namespace MrMeeseeks.DIE.Sample;

//*
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
    int Dont<T, TA>(T t, string s, TA ta);
    int Prop { get; set; }
    int PropGet { get; }
    int PropSet { set; }
    event EventHandler Event;
    int this[int index] { get; set; }
}

internal sealed class IInterface_Interception_Decorator // : IInterface
{
    private readonly IInterface _decorated;
    private readonly Interceptor _interceptor;

    public IInterface_Interception_Decorator(IInterface decorated, Interceptor interceptor)
    {
        _decorated = decorated;
        _interceptor = interceptor;
    }

    public void Do()
    {
        var arguments = Array.Empty<object>();
        var invocation = new InvocationDescription_0_21
        {
            Arguments = arguments,
            GenericArguments = Array.Empty<Type>(),
            InvocationTarget = _decorated,
            Method = typeof(IInterface).GetMethod(nameof(IInterface.Do))!,
            MethodInvocationTarget = typeof(IInterface).GetMethod(nameof(IInterface.Do))!,
            Proxy = this,
            ReturnValue = null!,
            TargetType = typeof(IInterface),
            ProceedAction = () => _decorated.Do()
        };
        _interceptor.Intercept(invocation);
    }

    public int Dont<T, TA>(T t, string s, TA ta)
    {
        int returnValue = 0;
        var arguments = new object[] { t!, s, ta! };
        
        var method = typeof(IInterface)
            .GetMethods()
            .First(m =>
            {
                if (m.Name != nameof(IInterface.Dont) || m.GetGenericArguments().Length != 2 || m.GetParameters().Length != 3) return false;
                var parameters = m.GetParameters();
                var genericArguments = m.GetGenericArguments();
                return genericArguments[0].Name == "T" 
                       && genericArguments[1].Name == "TA" 
                       && parameters[0].ParameterType.Name == "T" 
                       && parameters[1].ParameterType == typeof(string) 
                       && parameters[2].ParameterType.Name == "TA";
            });
        var invocation = new InvocationDescription_0_21
        {
            Arguments = arguments,
            GenericArguments = new Type[] { typeof(T), typeof(TA) },
            InvocationTarget = _decorated,
            Method = method,
            MethodInvocationTarget = method,
            Proxy = this,
            ReturnValue = returnValue,
            TargetType = typeof(IInterface),
        };
        invocation.ProceedAction = () => invocation.ReturnValue = _decorated.Dont<T, TA>((T)arguments[0], (string)arguments[1], (TA)arguments[2]);
        _interceptor.Intercept(invocation);
        return (int)invocation.ReturnValue;
    }

    public int Prop
    {
        get
        {
            var returnValue = default(int);
            var invocation = new InvocationDescription_0_21
            {
                Arguments = Array.Empty<object>(),
                GenericArguments = Array.Empty<Type>(),
                InvocationTarget = _decorated,
                Method = typeof(IInterface).GetProperty(nameof(Prop))!.GetMethod!,
                MethodInvocationTarget = typeof(IInterface).GetProperty(nameof(Prop))!.GetMethod!,
                Proxy = this,
                ReturnValue = null!,
                TargetType = typeof(IInterface),
                ProceedAction = () => returnValue = _decorated.Prop
            };
            return invocation.ReturnValue is int overriddenReturnValue ? overriddenReturnValue : returnValue;
        }
        set
        {
            var arguments = new object[] { value };
            var invocation = new InvocationDescription_0_21
            {
                Arguments = arguments,
                GenericArguments = Array.Empty<Type>(),
                InvocationTarget = _decorated,
                Method = typeof(IInterface).GetProperty(nameof(Prop))!.SetMethod!,
                MethodInvocationTarget = typeof(IInterface).GetProperty(nameof(Prop))!.SetMethod!,
                Proxy = this,
                ReturnValue = null!,
                TargetType = typeof(IInterface),
                ProceedAction = () => _decorated.Prop = (int)arguments[0]
            };
            _interceptor.Intercept(invocation);
        }
    }

    public int PropGet
    {
        get
        {
            var returnValue = default(int);
            var invocation = new InvocationDescription_0_21
            {
                Arguments = Array.Empty<object>(),
                GenericArguments = Array.Empty<Type>(),
                InvocationTarget = _decorated,
                Method = typeof(IInterface).GetProperty(nameof(PropGet))!.GetMethod!,
                MethodInvocationTarget = typeof(IInterface).GetProperty(nameof(PropGet))!.GetMethod!,
                Proxy = this,
                ReturnValue = null!,
                TargetType = typeof(IInterface),
                ProceedAction = () => returnValue = _decorated.PropGet
            };
            return invocation.ReturnValue is int overriddenReturnValue ? overriddenReturnValue : returnValue;
        }
    }

    public int PropSet
    {
        set
        {
            var arguments = new object[] { value };
            var invocation = new InvocationDescription_0_21
            {
                Arguments = arguments,
                GenericArguments = Array.Empty<Type>(),
                InvocationTarget = _decorated,
                Method = typeof(IInterface).GetProperty(nameof(PropSet))!.SetMethod!,
                MethodInvocationTarget = typeof(IInterface).GetProperty(nameof(PropSet))!.SetMethod!,
                Proxy = this,
                ReturnValue = null!,
                TargetType = typeof(IInterface),
                ProceedAction = () => _decorated.PropSet = (int)arguments[0]
            };
            _interceptor.Intercept(invocation);
        }
    }

    public event EventHandler? Event
    {
        add
        {
            var arguments = new object[] { value! };
            var @event = typeof(IInterface).GetEvent(nameof(Event))!.AddMethod!;
            var invocation = new InvocationDescription_0_21
            {
                Arguments = arguments,
                GenericArguments = Array.Empty<Type>(),
                InvocationTarget = _decorated,
                Method = typeof(IInterface).GetEvent(nameof(Event))!.AddMethod!,
                MethodInvocationTarget = typeof(IInterface).GetEvent(nameof(Event))!.AddMethod!,
                Proxy = this,
                ReturnValue = null!,
                TargetType = typeof(IInterface),
                ProceedAction = () => _decorated.Event += (EventHandler)arguments[0]
            };
            _interceptor.Intercept(invocation);
        }
        remove
        {
            var arguments = new object[] { value! };
            var invocation = new InvocationDescription_0_21
            {
                Arguments = arguments,
                GenericArguments = Array.Empty<Type>(),
                InvocationTarget = _decorated,
                Method = typeof(IInterface).GetEvent(nameof(Event))!.RemoveMethod!,
                MethodInvocationTarget = typeof(IInterface).GetEvent(nameof(Event))!.RemoveMethod!,
                Proxy = this,
                ReturnValue = null!,
                TargetType = typeof(IInterface),
                ProceedAction = () => _decorated.Event -= (EventHandler)arguments[0]
            };
            _interceptor.Intercept(invocation);
        }
    }

    public int this[int index]
    {
        get 
        {
            var returnValue = default(int);
            var invocation = new InvocationDescription_0_21
            {
                Arguments = new object[] { index },
                GenericArguments = Array.Empty<Type>(),
                InvocationTarget = _decorated,
                Method = typeof(IInterface).GetProperty("Item")!.GetMethod!,
                MethodInvocationTarget = typeof(IInterface).GetProperty("Item")!.GetMethod!,
                Proxy = this,
                ReturnValue = null!,
                TargetType = typeof(IInterface),
                ProceedAction = () => returnValue = _decorated[index]
            };
            return invocation.ReturnValue is int overriddenReturnValue ? overriddenReturnValue : returnValue;
        }
        set
        {
            var arguments = new object[] { index, value };
            typeof(IInterface).GetProperty("Item")!.GetIndexParameters();
            var invocation = new InvocationDescription_0_21
            {
                Arguments = arguments,
                GenericArguments = Array.Empty<Type>(),
                InvocationTarget = _decorated,
                Method = typeof(IInterface).GetProperty("Item")!.SetMethod!,
                MethodInvocationTarget = typeof(IInterface).GetProperty("Item")!.SetMethod!,
                Proxy = this,
                ReturnValue = null!,
                TargetType = typeof(IInterface),
                ProceedAction = () => _decorated[index] = (int)arguments[1]
            };
            _interceptor.Intercept(invocation);
        }
    }
}

internal sealed class Class : IInterface
{
    public void Do()
    {
        Console.WriteLine("Do");
    }

    public int Dont<T, TA>(T t, string s, TA ta)
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
