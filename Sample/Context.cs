using MrMeeseeks.DIE.Configuration;

namespace MrMeeseeks.DIE.Test.Struct.RecordNoExplicitConstructor;

internal record struct Dependency;

[CreateFunction(typeof(Dependency), "Create")]
internal partial class Container { }