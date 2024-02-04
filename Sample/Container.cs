using System;
using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;

internal class Dependency : IContainerInstance
{
    // ReSharper disable once UnusedParameter.Local
    internal Dependency(Func<Dependency> inner) {}
}

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container
{
    private Container() {}
}

/*internal class Class<T> : IContainerInstance
{
}

internal class Class0<T>
{
    internal Class0() {}
}

internal class Parent<T>
{
    public required Func<Class<IList<T>>> Class00 { get; init; }
    public required Class<IList<int>> Class { get; init; }
    public required Class<string> Class0 { get; init; }
    public required Class<IList<T>> Class1 { get; init; }
    
}

[CreateFunction(typeof(Parent<>), "Create")]
internal sealed partial class Container
{
    private Container() {}
}
//*/