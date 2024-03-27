using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;

internal class Dependency<T> : IDisposable, IAsyncDisposable
{
    public void Dispose() { }

    public async ValueTask DisposeAsync() => await Task.CompletedTask;
}

internal class Parent : IScopeRoot
{
    internal required Dependency<int> Dependency { get; init; }
}

[CreateFunction(typeof(Parent), "Create")]
internal sealed partial class Container;//*/
