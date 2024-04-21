using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;

internal sealed class Dependency
{
    internal Dependency() => throw new Exception("Yikes!");
}

internal class Dependency<T> : IDisposable, IAsyncDisposable
{
    internal required Dependency DependencyDependency { get; init; }
    
    public void Dispose() { }

    public async ValueTask DisposeAsync() => await Task.CompletedTask;
}

internal class Parent : ITransientScopeRoot
{
    internal required Dependency<int> Dependency { get; init; }
    internal required Dependency<int> Dependency0 { get; init; }
    internal required Dependency<int> Dependency1 { get; init; }
    internal required Dependency<int> Dependency2 { get; init; }
    internal required Dependency<int> Dependency3 { get; init; }
    internal required Dependency<int> Dependency4 { get; init; }
}

[CreateFunction(typeof(Task<Parent>), "Create")]
internal sealed partial class Container;//*/
