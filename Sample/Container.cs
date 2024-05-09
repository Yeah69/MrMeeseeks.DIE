using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

internal class Dependency<T>;

[CreateFunction(typeof(Dependency<>), "Create")]
internal sealed partial class Container<[GenericParameterMapping(typeof(Dependency<>), "T")] T>;//*/
