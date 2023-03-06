using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Test.Abstraction.ArrayType;

internal interface IInterface {}

internal class DependencyA : IInterface {}

internal class DependencyB : IInterface {}

internal class DependencyC : IInterface {}

[CreateFunction(typeof(IInterface[]), "Create")]
internal sealed partial class Container {}