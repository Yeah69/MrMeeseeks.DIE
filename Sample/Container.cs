using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Test.Bugs.UngenericImplementationGenericInterface;

internal interface IInterface<T> {}

internal class DependencyA : IInterface<int> {}

internal class DependencyB : IInterface<string> {}

internal class DependencyC : IInterface<long> {}

[CreateFunction(typeof(IInterface<int>), "Create")]
internal sealed partial class Container {}