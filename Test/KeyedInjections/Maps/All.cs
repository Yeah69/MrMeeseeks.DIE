using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.KeyedInjections.Maps.All;

internal enum Key
{
    A,
    B,
    C
}

internal interface IInterface
{
}

[Key(Key.A)]
internal class DependencyA : IInterface
{
}

[Key(Key.B)]
internal class DependencyB : IInterface
{
}

[Key(Key.C)]
internal class DependencyC : IInterface
{
}

internal interface IAsyncInterface
{
}

[Key(Key.A)]
internal class AsyncDependencyA : IAsyncInterface, ITaskInitializer
{
    public Task InitializeAsync() => Task.CompletedTask;
}

[Key(Key.B)]
internal class AsyncDependencyB : IAsyncInterface, IValueTaskInitializer
{
    public async ValueTask InitializeAsync() => await Task.Yield();
}

[Key(Key.C)]
internal class AsyncDependencyC : IAsyncInterface, ITaskInitializer
{
    public Task InitializeAsync() => Task.CompletedTask;
}

internal class Root
{
    public required IEnumerable<KeyValuePair<Key, IInterface>> Dependency { get; init; }
    public required IAsyncEnumerable<KeyValuePair<Key, IAsyncInterface>> DependencyIAsyncEnumerable { get; init; }
    public required ValueTask<IEnumerable<KeyValuePair<Key, IInterface>>> DependencyValueTaskIEnumerable { get; init; }
    public required Task<IEnumerable<KeyValuePair<Key, IInterface>>> DependencyTaskIEnumerable { get; init; }
    public required IEnumerable<KeyValuePair<Key, IAsyncInterface>> DependencyAsyncIEnumerable { get; init; }
    public required IDictionary<Key, IInterface> DependencyIDictionary { get; init; }
    public required IReadOnlyDictionary<Key, IInterface> DependencyIReadOnlyDictionary { get; init; }
    public required Dictionary<Key, IInterface> DependencyDictionary { get; init; }
    public required ReadOnlyDictionary<Key, IInterface> DependencyReadOnlyDictionary { get; init; }
    public required SortedDictionary<Key, IInterface> DependencySortedDictionary { get; init; }
    public required SortedList<Key, IInterface> DependencySortedList { get; init; }
    public required ImmutableDictionary<Key, IInterface> DependencyImmutableDictionary { get; init; }
    public required ImmutableSortedDictionary<Key, IInterface> DependencyImmutableSortedDictionary { get; init; }
    public required ValueTask<IDictionary<Key, IInterface>> DependencyValueTaskIDictionary { get; init; }
    public required ValueTask<IReadOnlyDictionary<Key, IInterface>> DependencyValueTaskIReadOnlyDictionary { get; init; }
    public required ValueTask<Dictionary<Key, IInterface>> DependencyValueTaskDictionary { get; init; }
    public required ValueTask<ReadOnlyDictionary<Key, IInterface>> DependencyValueTaskReadOnlyDictionary { get; init; }
    public required ValueTask<SortedDictionary<Key, IInterface>> DependencyValueTaskSortedDictionary { get; init; }
    public required ValueTask<SortedList<Key, IInterface>> DependencyValueTaskSortedList { get; init; }
    public required ValueTask<ImmutableDictionary<Key, IInterface>> DependencyValueTaskImmutableDictionary { get; init; }
    public required ValueTask<ImmutableSortedDictionary<Key, IInterface>> DependencyValueTaskImmutableSortedDictionary { get; init; }
    public required Task<IDictionary<Key, IInterface>> DependencyTaskIDictionary { get; init; }
    public required Task<IReadOnlyDictionary<Key, IInterface>> DependencyTaskIReadOnlyDictionary { get; init; }
    public required Task<Dictionary<Key, IInterface>> DependencyTaskDictionary { get; init; }
    public required Task<ReadOnlyDictionary<Key, IInterface>> DependencyTaskReadOnlyDictionary { get; init; }
    public required Task<SortedDictionary<Key, IInterface>> DependencyTaskSortedDictionary { get; init; }
    public required Task<SortedList<Key, IInterface>> DependencyTaskSortedList { get; init; }
    public required Task<ImmutableDictionary<Key, IInterface>> DependencyTaskImmutableDictionary { get; init; }
    public required Task<ImmutableSortedDictionary<Key, IInterface>> DependencyTaskImmutableSortedDictionary { get; init; }
    public required IDictionary<Key, IAsyncInterface> DependencyAsyncIDictionary { get; init; }
    public required IReadOnlyDictionary<Key, IAsyncInterface> DependencyAsyncIReadOnlyDictionary { get; init; }
    public required Dictionary<Key, IAsyncInterface> DependencyAsyncDictionary { get; init; }
    public required ReadOnlyDictionary<Key, IAsyncInterface> DependencyAsyncReadOnlyDictionary { get; init; }
    public required SortedDictionary<Key, IAsyncInterface> DependencyAsyncSortedDictionary { get; init; }
    public required SortedList<Key, IAsyncInterface> DependencyAsyncSortedList { get; init; }
    public required ImmutableDictionary<Key, IAsyncInterface> DependencyAsyncImmutableDictionary { get; init; }
    public required ImmutableSortedDictionary<Key, IAsyncInterface> DependencyAsyncImmutableSortedDictionary { get; init; }
}

[CreateFunction(typeof(Root), "Create")]
internal sealed partial class Container
{
    private Container() {}
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var _ = container.Create();
    }
}