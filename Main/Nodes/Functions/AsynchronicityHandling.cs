using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface IAsynchronicityHandling
{
    ReturnTypeStatus ReturnTypeStatus { get; }
    AsyncAwaitStatus AsyncAwaitStatus { get; }
    string NameMiddlePart(ReturnTypeStatus returnTypeStatus);
    void MakeTaskBasedOnly();
    void MakeTaskBasedToo();
    void MakeAsyncYes();
    string ReturnedTypeFullName(ReturnTypeStatus returnTypeStatus);
    AsyncSingleReturnStrategy SelectAsyncSingleReturnStrategy(ReturnTypeStatus returnTypeStatus, bool isAsyncAwait);
}

internal enum AsyncSingleReturnStrategy
{
    Pass,
    Await,
    ValueTaskFromResult,
    TaskFromResult,
    ValueTaskCompletedTask,
    TaskCompletedTask
}

internal class AsynchronicityHandlingFactory
{
    private readonly Func<VoidAsynchronicityHandling> _voidAsynchronicityHandlingFactory;
    private readonly Func<ITypeSymbol, TypedAsynchronicityHandling> _typedAsynchronicityHandlingFactory;
    private readonly Func<INamedTypeSymbol, SomeTaskAsynchronicityHandling> _someTaskAsynchronicityHandlingFactory;
    private readonly Func<INamedTypeSymbol, bool, AsyncEnumerableAsynchronicityHandling> _asyncEnumerableAsynchronicityHandlingFactory;
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly WellKnownTypesCollections _wellKnownTypesCollections;

    internal AsynchronicityHandlingFactory(
        Func<VoidAsynchronicityHandling> voidAsynchronicityHandlingFactory,
        Func<ITypeSymbol, TypedAsynchronicityHandling> typedAsynchronicityHandlingFactory,
        Func<INamedTypeSymbol, SomeTaskAsynchronicityHandling> someTaskAsynchronicityHandlingFactory,
        Func<INamedTypeSymbol, bool, AsyncEnumerableAsynchronicityHandling> asyncEnumerableAsynchronicityHandlingFactory,
        WellKnownTypes wellKnownTypes,
        WellKnownTypesCollections wellKnownTypesCollections)
    {
        _voidAsynchronicityHandlingFactory = voidAsynchronicityHandlingFactory;
        _typedAsynchronicityHandlingFactory = typedAsynchronicityHandlingFactory;
        _someTaskAsynchronicityHandlingFactory = someTaskAsynchronicityHandlingFactory;
        _asyncEnumerableAsynchronicityHandlingFactory = asyncEnumerableAsynchronicityHandlingFactory;
        _wellKnownTypes = wellKnownTypes;
        _wellKnownTypesCollections = wellKnownTypesCollections;
    }
    
    internal IAsynchronicityHandling Void() => _voidAsynchronicityHandlingFactory();
    internal IAsynchronicityHandling Typed(ITypeSymbol type, bool isIteratorFunction)
    {
        if (type is INamedTypeSymbol namedTypeSymbol)
        {
            if (CustomSymbolEqualityComparer.IncludeNullability.Equals(namedTypeSymbol.OriginalDefinition, _wellKnownTypes.Task1))
                return _someTaskAsynchronicityHandlingFactory(namedTypeSymbol);
            if (_wellKnownTypes.ValueTask1 is not null &&
                CustomSymbolEqualityComparer.IncludeNullability.Equals(namedTypeSymbol.OriginalDefinition, _wellKnownTypes.ValueTask1))
                return _someTaskAsynchronicityHandlingFactory(namedTypeSymbol);
            if (_wellKnownTypesCollections.IAsyncEnumerable1 is not null &&
                CustomSymbolEqualityComparer.IncludeNullability.Equals(namedTypeSymbol.OriginalDefinition, _wellKnownTypesCollections.IAsyncEnumerable1))
                return _asyncEnumerableAsynchronicityHandlingFactory(namedTypeSymbol, isIteratorFunction);
        }
        return _typedAsynchronicityHandlingFactory(type);
    }
}

internal abstract class AsynchronicityHandlingBase : IAsynchronicityHandling
{
    internal AsynchronicityHandlingBase(
        // parameters
        ReturnTypeStatus returnTypeStatus,
        AsyncAwaitStatus asyncAwaitStatus)
    {
        ReturnTypeStatus = returnTypeStatus;
        AsyncAwaitStatus = asyncAwaitStatus;
    }
    public ReturnTypeStatus ReturnTypeStatus { get; protected set; }
    public AsyncAwaitStatus AsyncAwaitStatus { get; protected set; }
    
    public string NameMiddlePart(ReturnTypeStatus returnTypeStatus)
    {
        return returnTypeStatus switch
        {
            ReturnTypeStatus.IAsyncEnumerable => "Async",
            ReturnTypeStatus.ValueTask => "ValueAsync",
            ReturnTypeStatus.Task => "Async",
            ReturnTypeStatus.Ordinary => "",
            _ => throw new ArgumentOutOfRangeException(nameof(returnTypeStatus), returnTypeStatus, null)
        };
    }

    public abstract void MakeTaskBasedOnly();

    public abstract void MakeTaskBasedToo();
    public virtual void MakeAsyncYes() => AsyncAwaitStatus = AsyncAwaitStatus.Yes;

    public abstract string ReturnedTypeFullName(ReturnTypeStatus returnTypeStatus);
    public abstract AsyncSingleReturnStrategy SelectAsyncSingleReturnStrategy(ReturnTypeStatus returnTypeStatus, bool isAsyncAwait);
}

internal sealed class TypedAsynchronicityHandling : AsynchronicityHandlingBase
{
    private readonly ITypeSymbol _type;
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly bool _valueTask1Existing;

    internal TypedAsynchronicityHandling(
        // parameters
        ITypeSymbol type,
        
        // dependencies
        WellKnownTypes wellKnownTypes) : base(ReturnTypeStatus.Ordinary, AsyncAwaitStatus.No)
    {
        _type = type;
        _wellKnownTypes = wellKnownTypes;
        _valueTask1Existing = wellKnownTypes.ValueTask1 is not null;
    }

    public override void MakeTaskBasedOnly()
    {
        AsyncAwaitStatus = AsyncAwaitStatus.Yes;
        if (ReturnTypeStatus.HasFlag(ReturnTypeStatus.Task) || ReturnTypeStatus.HasFlag(ReturnTypeStatus.ValueTask))
        {
            ReturnTypeStatus = ReturnTypeStatus.HasFlag(ReturnTypeStatus.ValueTask) 
                ? ReturnTypeStatus.ValueTask 
                : ReturnTypeStatus.Task;
            return;
        }
        ReturnTypeStatus = _valueTask1Existing ? ReturnTypeStatus.ValueTask : ReturnTypeStatus.Task;
    }

    public override void MakeTaskBasedToo()
    {
        if (ReturnTypeStatus.HasFlag(ReturnTypeStatus.Task) || ReturnTypeStatus.HasFlag(ReturnTypeStatus.ValueTask))
            return;
        ReturnTypeStatus |= _valueTask1Existing ? ReturnTypeStatus.ValueTask : ReturnTypeStatus.Task;
    }
    
    public override string ReturnedTypeFullName(ReturnTypeStatus returnTypeStatus) =>
        returnTypeStatus switch
        {
            ReturnTypeStatus.ValueTask when _wellKnownTypes.ValueTask1 is not null => 
                _wellKnownTypes.ValueTask1.Construct(_type).FullName(),
            ReturnTypeStatus.Task => _wellKnownTypes.Task1.Construct(_type).FullName(),
            ReturnTypeStatus.Ordinary => _type.FullName(),
            _ => throw new InvalidOperationException($"Invalid return type status: {returnTypeStatus}")
        };

    public override AsyncSingleReturnStrategy SelectAsyncSingleReturnStrategy(ReturnTypeStatus returnTypeStatus, bool isAsyncAwait)
    {
        if (!isAsyncAwait)
        {
            if (returnTypeStatus.HasFlag(ReturnTypeStatus.ValueTask))
                return AsyncSingleReturnStrategy.ValueTaskFromResult;
            if (returnTypeStatus.HasFlag(ReturnTypeStatus.Task))
                return AsyncSingleReturnStrategy.TaskFromResult;
            if (returnTypeStatus.HasFlag(ReturnTypeStatus.Ordinary))
                return AsyncSingleReturnStrategy.Pass;
        }
        
        if (returnTypeStatus.HasFlag(ReturnTypeStatus.ValueTask) || returnTypeStatus.HasFlag(ReturnTypeStatus.Task))
            return AsyncSingleReturnStrategy.Pass;
        throw new ArgumentOutOfRangeException(nameof(returnTypeStatus), returnTypeStatus, null);
    }
}

internal sealed class VoidAsynchronicityHandling : AsynchronicityHandlingBase
{
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly bool _valueTaskExisting;

    internal VoidAsynchronicityHandling(
        // dependencies
        WellKnownTypes wellKnownTypes) : base(ReturnTypeStatus.Ordinary, AsyncAwaitStatus.No)
    {
        _wellKnownTypes = wellKnownTypes;
        _valueTaskExisting = wellKnownTypes.ValueTask is not null;
    }

    public override void MakeTaskBasedOnly()
    {
        AsyncAwaitStatus = AsyncAwaitStatus.Yes;
        if (ReturnTypeStatus.HasFlag(ReturnTypeStatus.Task) || ReturnTypeStatus.HasFlag(ReturnTypeStatus.ValueTask))
        {
            ReturnTypeStatus = ReturnTypeStatus.HasFlag(ReturnTypeStatus.ValueTask) 
                ? ReturnTypeStatus.ValueTask 
                : ReturnTypeStatus.Task;
            return;
        }
        ReturnTypeStatus = _valueTaskExisting ? ReturnTypeStatus.ValueTask : ReturnTypeStatus.Task;
    }

    public override void MakeTaskBasedToo()
    {
        if (ReturnTypeStatus.HasFlag(ReturnTypeStatus.Task) || ReturnTypeStatus.HasFlag(ReturnTypeStatus.ValueTask))
            return;
        ReturnTypeStatus |= _valueTaskExisting ? ReturnTypeStatus.ValueTask : ReturnTypeStatus.Task;
    }
    
    public override string ReturnedTypeFullName(ReturnTypeStatus returnTypeStatus) =>
        returnTypeStatus switch
        {
            ReturnTypeStatus.ValueTask when _wellKnownTypes.ValueTask is not null => 
                _wellKnownTypes.ValueTask.FullName(),
            ReturnTypeStatus.Task => _wellKnownTypes.Task.FullName(),
            ReturnTypeStatus.Ordinary => "void",
            _ => throw new InvalidOperationException($"Invalid return type status: {returnTypeStatus}")
        };

    public override AsyncSingleReturnStrategy SelectAsyncSingleReturnStrategy(ReturnTypeStatus returnTypeStatus, bool isAsyncAwait) =>
        !isAsyncAwait && !returnTypeStatus.HasFlag(ReturnTypeStatus.Ordinary)
            ? returnTypeStatus.HasFlag(ReturnTypeStatus.ValueTask)
                ? AsyncSingleReturnStrategy.ValueTaskCompletedTask
                : returnTypeStatus.HasFlag(ReturnTypeStatus.Task)
                    ? AsyncSingleReturnStrategy.TaskCompletedTask
                    : throw new ArgumentOutOfRangeException(nameof(returnTypeStatus), returnTypeStatus, null)
            : AsyncSingleReturnStrategy.Pass;
}

internal sealed class SomeTaskAsynchronicityHandling : AsynchronicityHandlingBase
{
    private readonly INamedTypeSymbol _someTaskType;

    internal SomeTaskAsynchronicityHandling(
        // parameters
        INamedTypeSymbol someTaskType,
        
        // dependencies
        WellKnownTypes wellKnownTypes) : base(ReturnTypeStatus.Ordinary, AsyncAwaitStatus.No)
    {
        _someTaskType = someTaskType;
        ReturnTypeStatus = wellKnownTypes.ValueTask1 is { } valueTaskType &&
                           CustomSymbolEqualityComparer.IncludeNullability.Equals(someTaskType.OriginalDefinition, valueTaskType)
            ? ReturnTypeStatus.ValueTask
            : CustomSymbolEqualityComparer.IncludeNullability.Equals(someTaskType.OriginalDefinition, wellKnownTypes.Task1) 
                ? ReturnTypeStatus.Task
                : throw new ArgumentOutOfRangeException(nameof(someTaskType), someTaskType, null);
    }

    public override void MakeTaskBasedOnly() => 
        AsyncAwaitStatus = AsyncAwaitStatus.Yes;

    public override void MakeTaskBasedToo() { }
    
    public override string ReturnedTypeFullName(ReturnTypeStatus returnTypeStatus) => _someTaskType.FullName();
    
    public override AsyncSingleReturnStrategy SelectAsyncSingleReturnStrategy(ReturnTypeStatus returnTypeStatus, bool isAsyncAwait) => 
        isAsyncAwait 
            ? AsyncSingleReturnStrategy.Await 
            : AsyncSingleReturnStrategy.Pass;
}

internal sealed class AsyncEnumerableAsynchronicityHandling : AsynchronicityHandlingBase
{
    private readonly INamedTypeSymbol _asyncEnumerableType;

    internal AsyncEnumerableAsynchronicityHandling(
        // parameters
        INamedTypeSymbol asyncEnumerableType,
        bool isIteratorFunction) 
        : base(ReturnTypeStatus.IAsyncEnumerable, isIteratorFunction ? AsyncAwaitStatus.Yes : AsyncAwaitStatus.No)
    {
        _asyncEnumerableType = asyncEnumerableType;
    }

    public override void MakeTaskBasedOnly() { }

    public override void MakeTaskBasedToo() { }
    
    public override void MakeAsyncYes()
    {
        // Do nothing in case of IAsyncEnumerable
    }

    public override string ReturnedTypeFullName(ReturnTypeStatus returnTypeStatus) => 
        _asyncEnumerableType.FullName();
    
    public override AsyncSingleReturnStrategy SelectAsyncSingleReturnStrategy(ReturnTypeStatus returnTypeStatus, bool isAsyncAwait) => 
        AsyncSingleReturnStrategy.Pass;
}