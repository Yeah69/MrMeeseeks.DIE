using System;
using System.Collections.Generic;
using System.Linq;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;

internal interface IInterface;

internal sealed class Decorator(IInterface decorated) : IInterface, IDecorator<IInterface>
{
    public IInterface Decorated => decorated;
}

internal sealed class Implementation : IInterface;

internal sealed class ImplementationA : IInterface;

internal sealed class ImplementationB : IInterface;

internal sealed class ImplementationC : IInterface;

internal sealed class Composite(List<Func<Func<Lazy<IInterface>>>> children) : IInterface, IComposite<IInterface>
{
    public IReadOnlyList<IInterface> Children { get; } = children.Select(c => c()().Value).ToList();
}

internal class Parent(Lazy<IInterface> child) 
{
    public IInterface Child { get; } = child.Value;
}

[ImplementationChoice(typeof(IInterface), typeof(Implementation))]
[ImplementationCollectionChoice(typeof(IInterface), 
    typeof(Implementation), typeof(ImplementationA), typeof(ImplementationB), typeof(ImplementationC), 
    typeof(Implementation), typeof(ImplementationA), typeof(ImplementationB), typeof(ImplementationC))]
[CreateFunction(typeof(Parent), "Create")]
internal sealed partial class Container;
