using System;
using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

//*
[InvocationDescription]
internal interface IInvocationDescription
{
    ITypeDescription TargetType { get; }
    IMethodDescription TargetMethod { get; }
}

[MethodDescription]
internal interface IMethodDescription
{
    string Name { get; }
    ITypeDescription ReturnType { get; }
}

[TypeDescription]
internal interface ITypeDescription
{
    string FullName { get; }
    string Name { get; }
}

internal class Interceptor
{
    internal void Intercept(IInvocationDescription invocationDescription)
    {
        Console.WriteLine($"Type: {invocationDescription.TargetType.FullName} Method: {invocationDescription.TargetMethod.Name}");
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
