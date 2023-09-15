using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

internal abstract class Class {}

internal class SubClassA : Class {}

internal class SubClassB : Class {}

internal class SubClassC : Class {}

[ImplementationCollectionChoice(typeof(Class), typeof(SubClassA), typeof(SubClassB))]
[CreateFunction(typeof(IReadOnlyList<Class>), "Create")]
internal sealed partial class Container
{
    private Container() {}
}

/*
internal enum Keys
{
    A,
    B,
    C
}

internal interface IInterface { }

[Key(Keys.A)]
internal class DependencyA : IInterface { }

[Key(Keys.B)]
internal class DependencyB : IInterface { }

[Key(Keys.C)]
internal class DependencyC : IInterface { }

[CreateFunction(typeof(IReadOnlyDictionary<Keys, IInterface>), "Create")]
internal sealed partial class Container
{
    private Container() {}
}*/