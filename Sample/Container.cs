using System;
using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;

internal interface IInterface;

/*internal sealed class Decorator(IInterface decorated) : IInterface, IDecorator<IInterface>
{
    public IInterface Decorated => decorated;
}*/

internal sealed class Implementation : IInterface;

internal sealed class ImplementationA : IInterface;

internal sealed class ImplementationB : IInterface;

internal sealed class ImplementationC : IInterface;

internal class Parent(IEnumerable<Lazy<IInterface>> children) 
{
    public IEnumerable<Lazy<IInterface>> Children { get; } = children;
}

[CreateFunction(typeof(Parent), "Create")]
internal sealed partial class Container;
