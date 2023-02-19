using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Collection.Injection.All;

internal interface IInterface
{
}

internal class DependencyA : IInterface
{
}

internal class DependencyB : IInterface
{
}

internal class DependencyC : IInterface
{
}

internal interface IAsyncInterface
{
}

internal class AsyncDependencyA : IAsyncInterface, ITaskInitializer
{
    public Task InitializeAsync() => Task.CompletedTask;
}

internal class AsyncDependencyB : IAsyncInterface, IValueTaskInitializer
{
    public async ValueTask InitializeAsync() => await Task.Yield();
}

internal class AsyncDependencyC : IAsyncInterface, ITaskInitializer
{
    public Task InitializeAsync() => Task.CompletedTask;
}

internal class Root
{
    public required IEnumerable<IInterface> Dependency { get; init; }
    public required IAsyncEnumerable<IAsyncInterface> DependencyIAsyncEnumerable { get; init; }
    public required ValueTask<IEnumerable<IAsyncInterface>> DependencyValueTaskIEnumerable { get; init; }
    public required Task<IEnumerable<IAsyncInterface>> DependencyTaskIEnumerable { get; init; }
    public required IEnumerable<IAsyncInterface> DependencyAsyncIEnumerable { get; init; }
    public required IInterface[] DependencyArray { get; init; }
    public required IList<IInterface> DependencyIList { get; init; }
    public required ICollection<IInterface> DependencyICollection { get; init; }
    public required ReadOnlyCollection<IInterface> DependencyReadOnlyCollection { get; init; }
    public required IReadOnlyCollection<IInterface> DependencyIReadOnlyCollection { get; init; }
    public required IReadOnlyList<IInterface> DependencyIReadOnlyList { get; init; }
    public required ArraySegment<IInterface> DependencyArraySegment { get; init; }
    public required ConcurrentBag<IInterface> DependencyConcurrentBag { get; init; }
    public required ConcurrentQueue<IInterface> DependencyConcurrentQueue { get; init; }
    public required ConcurrentStack<IInterface> DependencyConcurrentStack { get; init; }
    public required HashSet<IInterface> DependencyHashSet { get; init; }
    public required LinkedList<IInterface> DependencyLinkedList { get; init; }
    public required List<IInterface> DependencyList { get; init; }
    public required Queue<IInterface> DependencyQueue { get; init; }
    public required Stack<IInterface> DependencyStack { get; init; }
    public required SortedSet<IInterface> DependencySortedSet { get; init; }
    public required ImmutableArray<IInterface> DependencyImmutableArray { get; init; }
    public required ImmutableHashSet<IInterface> DependencyImmutableHashSet { get; init; }
    public required ImmutableList<IInterface> DependencyImmutableList { get; init; }
    public required ImmutableQueue<IInterface> DependencyImmutableQueue { get; init; }
    public required ImmutableSortedSet<IInterface> DependencyImmutableSortedSet { get; init; }
    public required ImmutableStack<IInterface> DependencyImmutableStack { get; init; }
    public required ValueTask<IAsyncInterface[]> DependencyValueTaskArray { get; init; }
    public required ValueTask<IList<IAsyncInterface>> DependencyValueTaskIList { get; init; }
    public required ValueTask<ICollection<IAsyncInterface>> DependencyValueTaskICollection { get; init; }
    public required ValueTask<ReadOnlyCollection<IAsyncInterface>> DependencyValueTaskReadOnlyCollection { get; init; }
    public required ValueTask<IReadOnlyCollection<IAsyncInterface>> DependencyValueTaskIReadOnlyCollection { get; init; }
    public required ValueTask<IReadOnlyList<IAsyncInterface>> DependencyValueTaskIReadOnlyList { get; init; }
    public required ValueTask<ArraySegment<IAsyncInterface>> DependencyValueTaskArraySegment { get; init; }
    public required ValueTask<ConcurrentBag<IAsyncInterface>> DependencyValueTaskConcurrentBag { get; init; }
    public required ValueTask<ConcurrentQueue<IAsyncInterface>> DependencyValueTaskConcurrentQueue { get; init; }
    public required ValueTask<ConcurrentStack<IAsyncInterface>> DependencyValueTaskConcurrentStack { get; init; }
    public required ValueTask<HashSet<IAsyncInterface>> DependencyValueTaskHashSet { get; init; }
    public required ValueTask<LinkedList<IAsyncInterface>> DependencyValueTaskLinkedList { get; init; }
    public required ValueTask<List<IAsyncInterface>> DependencyValueTaskList { get; init; }
    public required ValueTask<Queue<IAsyncInterface>> DependencyValueTaskQueue { get; init; }
    public required ValueTask<Stack<IAsyncInterface>> DependencyValueTaskStack { get; init; }
    public required ValueTask<SortedSet<IAsyncInterface>> DependencyValueTaskSortedSet { get; init; }
    public required ValueTask<ImmutableArray<IAsyncInterface>> DependencyValueTaskImmutableArray { get; init; }
    public required ValueTask<ImmutableHashSet<IAsyncInterface>> DependencyValueTaskImmutableHashSet { get; init; }
    public required ValueTask<ImmutableList<IAsyncInterface>> DependencyValueTaskImmutableList { get; init; }
    public required ValueTask<ImmutableQueue<IAsyncInterface>> DependencyValueTaskImmutableQueue { get; init; }
    public required ValueTask<ImmutableSortedSet<IAsyncInterface>> DependencyValueTaskImmutableSortedSet { get; init; }
    public required ValueTask<ImmutableStack<IAsyncInterface>> DependencyValueTaskImmutableStack { get; init; }
    public required Task<IAsyncInterface[]> DependencyTaskArray { get; init; }
    public required Task<IList<IAsyncInterface>> DependencyTaskIList { get; init; }
    public required Task<ICollection<IAsyncInterface>> DependencyTaskICollection { get; init; }
    public required Task<ReadOnlyCollection<IAsyncInterface>> DependencyTaskReadOnlyCollection { get; init; }
    public required Task<IReadOnlyCollection<IAsyncInterface>> DependencyTaskIReadOnlyCollection { get; init; }
    public required Task<IReadOnlyList<IAsyncInterface>> DependencyTaskIReadOnlyList { get; init; }
    public required Task<ArraySegment<IAsyncInterface>> DependencyTaskArraySegment { get; init; }
    public required Task<ConcurrentBag<IAsyncInterface>> DependencyTaskConcurrentBag { get; init; }
    public required Task<ConcurrentQueue<IAsyncInterface>> DependencyTaskConcurrentQueue { get; init; }
    public required Task<ConcurrentStack<IAsyncInterface>> DependencyTaskConcurrentStack { get; init; }
    public required Task<HashSet<IAsyncInterface>> DependencyTaskHashSet { get; init; }
    public required Task<LinkedList<IAsyncInterface>> DependencyTaskLinkedList { get; init; }
    public required Task<List<IAsyncInterface>> DependencyTaskList { get; init; }
    public required Task<Queue<IAsyncInterface>> DependencyTaskQueue { get; init; }
    public required Task<Stack<IAsyncInterface>> DependencyTaskStack { get; init; }
    public required Task<SortedSet<IAsyncInterface>> DependencyTaskSortedSet { get; init; }
    public required Task<ImmutableArray<IAsyncInterface>> DependencyTaskImmutableArray { get; init; }
    public required Task<ImmutableHashSet<IAsyncInterface>> DependencyTaskImmutableHashSet { get; init; }
    public required Task<ImmutableList<IAsyncInterface>> DependencyTaskImmutableList { get; init; }
    public required Task<ImmutableQueue<IAsyncInterface>> DependencyTaskImmutableQueue { get; init; }
    public required Task<ImmutableSortedSet<IAsyncInterface>> DependencyTaskImmutableSortedSet { get; init; }
    public required Task<ImmutableStack<IAsyncInterface>> DependencyTaskImmutableStack { get; init; }
    public required IAsyncInterface[] DependencyAsyncArray { get; init; }
    public required IList<IAsyncInterface> DependencyAsyncIList { get; init; }
    public required ICollection<IAsyncInterface> DependencyAsyncICollection { get; init; }
    public required ReadOnlyCollection<IAsyncInterface> DependencyAsyncReadOnlyCollection { get; init; }
    public required IReadOnlyCollection<IAsyncInterface> DependencyAsyncIReadOnlyCollection { get; init; }
    public required IReadOnlyList<IAsyncInterface> DependencyAsyncIReadOnlyList { get; init; }
    public required ArraySegment<IAsyncInterface> DependencyAsyncArraySegment { get; init; }
    public required ConcurrentBag<IAsyncInterface> DependencyAsyncConcurrentBag { get; init; }
    public required ConcurrentQueue<IAsyncInterface> DependencyAsyncConcurrentQueue { get; init; }
    public required ConcurrentStack<IAsyncInterface> DependencyAsyncConcurrentStack { get; init; }
    public required HashSet<IAsyncInterface> DependencyAsyncHashSet { get; init; }
    public required LinkedList<IAsyncInterface> DependencyAsyncLinkedList { get; init; }
    public required List<IAsyncInterface> DependencyAsyncList { get; init; }
    public required Queue<IAsyncInterface> DependencyAsyncQueue { get; init; }
    public required Stack<IAsyncInterface> DependencyAsyncStack { get; init; }
    public required SortedSet<IAsyncInterface> DependencyAsyncSortedSet { get; init; }
    public required ImmutableArray<IAsyncInterface> DependencyAsyncImmutableArray { get; init; }
    public required ImmutableHashSet<IAsyncInterface> DependencyAsyncImmutableHashSet { get; init; }
    public required ImmutableList<IAsyncInterface> DependencyAsyncImmutableList { get; init; }
    public required ImmutableQueue<IAsyncInterface> DependencyAsyncImmutableQueue { get; init; }
    public required ImmutableSortedSet<IAsyncInterface> DependencyAsyncImmutableSortedSet { get; init; }
    public required ImmutableStack<IAsyncInterface> DependencyAsyncImmutableStack { get; init; }
}

[CreateFunction(typeof(Root), "Create")]
internal partial class Container
{
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
    }
}