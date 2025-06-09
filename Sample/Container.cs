using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;

internal interface IInterface;

internal sealed class Proxy(IInterface decorated) : IInterface
{
    public IInterface Decorated => decorated;
}

internal sealed class Decorator(Func<string, Func<string, Lazy<IInterface>>> boobies, Proxy proxy) : IInterface, IDecorator<IInterface>
{
    public IInterface Decorated => boobies(".")(".").Value;
    public Proxy Proxy => proxy;
}

internal sealed class Implementation : IInterface;

internal sealed class ImplementationA : IInterface;

internal sealed class ImplementationB : IInterface;

internal sealed class ImplementationC : IInterface;

internal class Parent(IInterface[] children) 
{
    public ICollection<IInterface> Children { get; } = children;
}

[ImplementationChoice(typeof(IInterface), typeof(Implementation))]
[ImplementationCollectionChoice(typeof(IInterface),
    typeof(Implementation), typeof(ImplementationA), typeof(ImplementationB), typeof(ImplementationC),
    typeof(Implementation), typeof(ImplementationA), typeof(ImplementationB), typeof(ImplementationC))]
[CreateFunction(typeof(Parent), "Create")]
internal sealed partial class Container;
