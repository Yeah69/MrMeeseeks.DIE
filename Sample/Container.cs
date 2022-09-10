using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Test.Implementation.ArrayType;

internal class Dependency {}

[CreateFunction(typeof(Dependency[]), "Create")]
internal sealed partial class Container {}