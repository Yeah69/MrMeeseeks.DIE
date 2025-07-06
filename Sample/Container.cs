using System;
using System.Collections.Generic;
using System.Linq;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;

internal interface IInterface;

internal sealed class Proxy(IInterface decorated) : IInterface
{
    public IInterface Decorated => decorated;
}

internal sealed class Decorator(IInterface boobies) : IInterface, IDecorator<IInterface>
{
    public IInterface Decorated => boobies;
}

internal enum Key 
{
    Zero,
    A,
    B,
    C
}

[InjectionKey(Key.Zero)]
internal sealed class Implementation : IInterface;

[InjectionKey(Key.A)]
internal sealed class ImplementationA : IInterface;

[InjectionKey(Key.B)]
internal sealed class ImplementationB : IInterface;

[InjectionKey(Key.C)]
internal sealed class ImplementationC : IInterface;

internal class Parent(IEnumerable<KeyValuePair<Key, Lazy<IInterface>>> children) 
{
    public List<KeyValuePair<Key, IInterface>> Children { get; } = children.Select(kvp => new KeyValuePair<Key, IInterface>(kvp.Key, kvp.Value.Value)).ToList();
}

[ImplementationCollectionChoice(typeof(IInterface),
    typeof(Implementation), typeof(ImplementationA), typeof(ImplementationB), typeof(ImplementationC),
    typeof(Implementation), typeof(ImplementationA), typeof(ImplementationB), typeof(ImplementationC))]
[CreateFunction(typeof(Parent), "Create")]
internal sealed partial class Container;
