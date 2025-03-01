using System;
using System.Threading;
using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

internal class HelloSayer
{
    internal void SayHello() => Console.WriteLine("Hello!");
}

internal class Dialog
{
    internal Dialog(ThreadLocal<HelloSayer> helloSayer) => HelloSayer = helloSayer.Value!;
    public HelloSayer HelloSayer { get; set; }
    
    internal void StartConversation() => HelloSayer.SayHello();
}

[CreateFunction(typeof(Dialog), "Create")]
internal sealed partial class Container;
