using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;

internal interface IInterface;

internal sealed class Decorator(IInterface decorated) : IInterface, IDecorator<IInterface>;

internal sealed class Implementation : IInterface;

internal sealed class ImplementationA : IInterface;

internal class Parent(IInterface child);

[ImplementationChoice(typeof(IInterface), typeof(ImplementationA))]
[CreateFunction(typeof(Parent), "Create")]
internal sealed partial class Container;
