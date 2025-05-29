using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.CodeGeneration.Nodes;

internal interface IRangeNodeGenerator : INodeGenerator;

internal abstract class RangeNodeGenerator : IRangeNodeGenerator
{
    private readonly IRangeNode _rangeNode;
    private readonly IContainerNode _containerNode;
    private readonly IDisposeUtility _disposeUtility;
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly WellKnownTypesCollections _wellKnownTypesCollections;

    protected RangeNodeGenerator(
        IRangeNode rangeNode,
        IContainerNode containerNode,
        IDisposeUtility disposeUtility,
        WellKnownTypes wellKnownTypes,
        WellKnownTypesCollections wellKnownTypesCollections)
    {
        _rangeNode = rangeNode;
        _containerNode = containerNode;
        _disposeUtility = disposeUtility;
        _wellKnownTypes = wellKnownTypes;
        _wellKnownTypesCollections = wellKnownTypesCollections;
    }

    protected virtual void PreClass(StringBuilder code) {}

    protected virtual string ClassDeclaredAccessibility => "";

    protected abstract string InterfaceAssignment { get; }

    protected virtual string DefaultConstructorDeclaredAccessibility => "private ";

    protected abstract void PreGeneralContent(StringBuilder code, ICodeGenerationVisitor visitor);
    
    protected virtual void PostGeneralContent(StringBuilder code, ICodeGenerationVisitor visitor) {}
    
    protected virtual void PostClass(StringBuilder code) {}
    
    public void Generate(StringBuilder code, ICodeGenerationVisitor visitor)
    {
        PreClass(code);
        
        var genericParameters = _rangeNode is IContainerNode containerNode && containerNode.TypeParameters.Any()
            ? $"<{string.Join(", ", containerNode.TypeParameters.Select(p => p.Name))}>"
            : "";

        code.AppendLine(
            $$"""
              {{ClassDeclaredAccessibility}}sealed partial class {{_rangeNode.Name}}{{genericParameters}} : {{_disposeUtility.DisposableRangeInterfaceData.InterfaceNameFullyQualified}}, {{InterfaceAssignment}}
              {
              """);
        
        if (_rangeNode.GenerateEmptyConstructor) 
            code.AppendLine($"{DefaultConstructorDeclaredAccessibility}{_rangeNode.Name}() {{ }}");
        
        PreGeneralContent(code, visitor);

        if (_wellKnownTypes.ValueTask is not null && _wellKnownTypes.IAsyncDisposable is not null)
        {
            code.AppendLine(
                $"private readonly {_wellKnownTypes.TaskCompletionSourceOfInt.FullName()} {_rangeNode.ReleaseDisposeAsyncReference} = new {_wellKnownTypes.TaskCompletionSourceOfInt.FullName()}();");
        }
        
        code.AppendLine(
            $$"""
              private {{_wellKnownTypes.Int32.FullName()}} {{_rangeNode.ResolutionCounterReference}} = 0;
              private {{_wellKnownTypes.ConcurrentStackOfConcurrentStackOfObject.FullName()}} {{_rangeNode.DisposalHandling.CollectionReference}} = new {{_wellKnownTypes.ConcurrentStackOfConcurrentStackOfObject.FullName()}}();
              """);
        foreach (var initializedInstance in _rangeNode.InitializedInstances)
            visitor.VisitIInitializedInstanceNode(initializedInstance);
        
        foreach (var initializationFunction in _rangeNode.InitializationFunctions)
            visitor.VisitIVoidFunctionNode(initializationFunction);
        
        foreach (var createFunctionNode in _rangeNode.CreateFunctions)
            visitor.VisitICreateFunctionNodeBase(createFunctionNode);
        
        if (_rangeNode.HasGenericRangeInstanceFunctionGroups)
            code.AppendLine($"private readonly {_wellKnownTypes.ConcurrentDictionaryOfRuntimeTypeHandleToObject.FullName()} {_rangeNode.RangedInstanceStorageFieldName} = new {_wellKnownTypes.ConcurrentDictionaryOfRuntimeTypeHandleToObject.FullName()}();");

        foreach (var rangedInstanceFunctionGroup in _rangeNode.RangedInstanceFunctionGroups)
            visitor.VisitIRangedInstanceFunctionGroupNode(rangedInstanceFunctionGroup);

        foreach (var multiFunctionNodeBase in _rangeNode.MultiFunctions)
            switch (multiFunctionNodeBase)
            {
                case IMultiFunctionNode multiFunctionNode:
                    visitor.VisitIMultiFunctionNode(multiFunctionNode);
                    break;
                case IMultiKeyValueFunctionNode multiKeyValueFunctionNode:
                    visitor.VisitIMultiKeyValueFunctionNode(multiKeyValueFunctionNode);
                    break;
                case IMultiKeyValueMultiFunctionNode multiKeyValueMultiFunctionNode:
                    visitor.VisitIMultiKeyValueMultiFunctionNode(multiKeyValueMultiFunctionNode);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown multi function node type: {multiFunctionNodeBase.GetType().FullName}");
            }
        
        if (_rangeNode is { AddForDisposal: { } addForDisposal, DisposalHandling.SyncCollectionReference: { } syncCollectionReference })
            GenerateAddForDisposal(
                addForDisposal,
                Constants.UserDefinedAddForDisposal,
                _wellKnownTypes.IDisposable.FullName(),
                "disposable",
                syncCollectionReference);

        if (_rangeNode is { AddForDisposalAsync: { } addForDisposalAsync, DisposalHandling.AsyncCollectionReference: { } asyncCollectionReference }
            && _wellKnownTypes.IAsyncDisposable is not null)
            GenerateAddForDisposal(
                addForDisposalAsync,
                Constants.UserDefinedAddForDisposalAsync,
                _wellKnownTypes.IAsyncDisposable.FullName(),
                "asyncDisposable",
                asyncCollectionReference);
        
        GenerateDisposalFunction(code);
        
        PostGeneralContent(code, visitor);

        code.AppendLine("}");
        
        PostClass(code);
        return;

        void GenerateAddForDisposal(
            IMethodSymbol addForDisposalMethod, 
            string methodName,
            string disposableFullName, 
            string disposableParameterName, 
            string collectionReference)
        {
            var declaredAccessibility =
                addForDisposalMethod.DeclaredAccessibility == Accessibility.Private ? "private" : "protected";
            var virtualModifier = addForDisposalMethod.IsVirtual ? "virtual " : "";
            var overrideModifier = addForDisposalMethod.IsOverride ? "override " : "";
            var sealedModifier = addForDisposalMethod.IsSealed ? "sealed " : "";
            code.AppendLine(
                $$"""
                  {{declaredAccessibility}} {{sealedModifier}}{{virtualModifier}}{{overrideModifier}}partial void {{methodName}}({{disposableFullName}} {{disposableParameterName}})
                  {
                  {{_wellKnownTypes.Interlocked.FullName()}}.{{nameof(Interlocked.Increment)}}(ref {{_rangeNode.ResolutionCounterReference}});
                  try
                  {
                  if ({{_rangeNode.DisposalHandling.DisposedPropertyReference}}) throw new {{_wellKnownTypes.ObjectDisposedException}}("{{_rangeNode.FullName}}", $"[DIE] This scope \"{{_rangeNode.FullName}}\" is already disposed, so it can't manage another disposable.");
                  {{collectionReference}}.Add(({{disposableFullName}}) {{disposableParameterName}});
                  }
                  finally
                  {
                  {{_wellKnownTypes.Interlocked.FullName()}}.{{nameof(Interlocked.Decrement)}}(ref {{_rangeNode.ResolutionCounterReference}});

                  """);
            if (_wellKnownTypes.ValueTask is not null && _wellKnownTypes.IAsyncDisposable is not null && _disposeUtility.ReleaseDisposeAsyncFullyQualified is {} releaseDisposeAsyncFullyQualified)
            {
                code.AppendLine(
                    $"{releaseDisposeAsyncFullyQualified}(ref {_rangeNode.DisposalHandling.DisposedFieldReference}, ref {_rangeNode.ResolutionCounterReference}, {_rangeNode.ReleaseDisposeAsyncReference});");
            }
            code.AppendLine(
                $$"""
                  }
                  }
                  """);
        }
    }
    private void GenerateDisposalFunction(
        StringBuilder code)
    {
        var disposalHandling = _rangeNode.DisposalHandling;

        var disposalMap = _rangeNode.GetDisposalTypeToTypeFullNames();
        var asyncDisposal = disposalMap.TryGetValue(DisposalType.Async, out var listAsync0) ? listAsync0 : [];
        var syncDisposal = disposalMap.TryGetValue(DisposalType.Sync, out var listSync0) ? listSync0 : [];

        var asyncDisposablesPossible = _containerNode.AsyncDisposablesPossible;

        var isAsyncClauseFunction = 
            _wellKnownTypes.IAsyncDisposable is not null 
            && _disposeUtility.DisposeSingularAsyncSyncedFullyQualified is not null
            && _disposeUtility.DisposeSingularAsyncFullyQualified is not null;
        
        switch (_rangeNode)
        {
            case IContainerNode container:
                code.AppendLine(
                    $$"""
                      {{_wellKnownTypes.Object.FullName()}}[] {{_disposeUtility.DisposableRangeInterfaceData.InterfaceNameFullyQualified}}.{{_disposeUtility.DisposableRangeInterfaceData.TransientScopesPropertyName}}
                      {
                      get
                      {
                      {{container.TransientScopeDisposalSemaphoreReference}}.{{nameof(SemaphoreSlim.Wait)}}();
                      try
                      {
                      return {{_wellKnownTypesCollections.Enumerable}}.{{nameof(Enumerable.ToArray)}}({{container.TransientScopeDisposalReference}});
                      }
                      finally
                      {
                      {{container.TransientScopeDisposalSemaphoreReference}}.{{nameof(SemaphoreSlim.Release)}}();
                      }
                      }
                      }
                      """);
                break;
            default:
                code.AppendLine(
                    $"{_wellKnownTypes.Object.FullName()}[] {_disposeUtility.DisposableRangeInterfaceData.InterfaceNameFullyQualified}.{_disposeUtility.DisposableRangeInterfaceData.TransientScopesPropertyName} => new {_wellKnownTypes.Object}[0];");
                break;
        }
            
        code.AppendLine(
            $$"""
              {{_wellKnownTypes.ConcurrentStackOfConcurrentStackOfObject.FullName()}} {{_disposeUtility.DisposableRangeInterfaceData.InterfaceNameFullyQualified}}.{{_disposeUtility.DisposableRangeInterfaceData.DisposablesPropertyName}} => {{_rangeNode.DisposalHandling.CollectionReference}};
              {{_wellKnownTypes.ConcurrentBagOfSyncDisposable.FullName()}} {{_disposeUtility.DisposableRangeInterfaceData.InterfaceNameFullyQualified}}.{{_disposeUtility.DisposableRangeInterfaceData.UserDefinedSyncDisposablesPropertyName}} => {{_rangeNode.DisposalHandling.SyncCollectionReference}};
              """);

        if (_wellKnownTypes.IAsyncDisposable is not null)
            code.AppendLine(
                $$"""
                  {{_wellKnownTypes.ConcurrentBagOfAsyncDisposable?.FullName()}} {{_disposeUtility.DisposableRangeInterfaceData.InterfaceNameFullyQualified}}.{{_disposeUtility.DisposableRangeInterfaceData.UserDefinedAsyncDisposablesPropertyName}} => {{_rangeNode.DisposalHandling.AsyncCollectionReference}};
                  """);
        
        GenerateClauseFunction(_disposeUtility.DisposableRangeInterfaceData.SyncClauseFunctionName, syncDisposal, !asyncDisposablesPossible);
        
        if (isAsyncClauseFunction && _disposeUtility.DisposableRangeInterfaceData.AsyncClauseFunctionName is not null)
            GenerateClauseFunction(_disposeUtility.DisposableRangeInterfaceData.AsyncClauseFunctionName, asyncDisposal, asyncDisposablesPossible);


        if (_wellKnownTypes.ConcurrentBagOfAsyncDisposable is not null)
            code.AppendLine($"private {_wellKnownTypes.ConcurrentBagOfAsyncDisposable.FullName()} {disposalHandling.AsyncCollectionReference} = new {_wellKnownTypes.ConcurrentBagOfAsyncDisposable.FullName()}();");

        code.AppendLine(
            $$"""
              private {{_wellKnownTypes.ConcurrentBagOfSyncDisposable.FullName()}} {{disposalHandling.SyncCollectionReference}} = new {{_wellKnownTypes.ConcurrentBagOfSyncDisposable.FullName()}}();
              private int {{disposalHandling.DisposedFieldReference}} = 0;
              private bool {{disposalHandling.DisposedPropertyReference}} => {{disposalHandling.DisposedFieldReference}} != 0;
              """);

        GenerateDisposeFunction(false);
        
        if (_wellKnownTypes.ValueTask is not null
            && _wellKnownTypes.IAsyncDisposable is not null
            && _wellKnownTypesCollections.IAsyncEnumerable1 is not null
            && _wellKnownTypes.IAsyncEnumerableOfException is not null
            && _disposeUtility.DisposeChunkAsyncFullyQualified is not null)
        {
            GenerateDisposeFunction(true);
        }
        
        return;

        void GenerateDisposeFunction(bool isAsync)
        {
            var returnTypeAndName = isAsync && _wellKnownTypes.ValueTask is not null
                ? $"async {_wellKnownTypes.ValueTask.FullName()} {Constants.IAsyncDisposableDisposeAsync}"
                : $"void {nameof(IDisposable.Dispose)}";
            var awaitPrefix = isAsync ? "await " : "";
            var utilityCallName = (isAsync, asyncDisposablesPossible) switch
            {
                (true, _) => _disposeUtility.DisposeAsyncFullyQualified,
                (_, false) when _disposeUtility.DisposeSyncOnlyFullyQualified is {} syncOnlyName => syncOnlyName,
                _ => _disposeUtility.DisposeFullyQualified
            };
            
            code.AppendLine(
                $$"""
                  public {{returnTypeAndName}}()
                  {
                      var {{disposalHandling.DisposedLocalReference}} = {{_wellKnownTypes.Interlocked.FullName()}}.{{nameof(Interlocked.Exchange)}}(ref {{disposalHandling.DisposedFieldReference}}, 1);
                      if ({{disposalHandling.DisposedLocalReference}} != 0) return;
                  """);
            if (_disposeUtility.ReleaseDisposeAsyncFullyQualified is {} releaseDisposeAsyncFullyQualified)
            {
                code.AppendLine(
                    $"{releaseDisposeAsyncFullyQualified}(ref {_rangeNode.DisposalHandling.DisposedFieldReference}, ref {_rangeNode.ResolutionCounterReference}, {_rangeNode.ReleaseDisposeAsyncReference});");
            }

            code.AppendLine(isAsync && _disposeUtility.ReleaseDisposeAsyncFullyQualified is not null
                ? $"await {_rangeNode.ReleaseDisposeAsyncReference}.{nameof(TaskCompletionSource<int>.Task)};"
                : $"{_wellKnownTypes.SpinWait}.{nameof(SpinWait.SpinUntil)}(() => {_rangeNode.ResolutionCounterReference} == 0);");

            switch (_rangeNode)
            {
                case ITransientScopeNode transientScope:
                    var waitMethod = isAsync ? nameof(SemaphoreSlim.WaitAsync) : nameof(SemaphoreSlim.Wait);
                    code.AppendLine(
                        $$"""
                          {{awaitPrefix}}{{transientScope.ContainerReference}}.{{_containerNode.TransientScopeDisposalSemaphoreReference}}.{{waitMethod}}();
                          try 
                          {
                          {{transientScope.ContainerReference}}.{{_containerNode.TransientScopeDisposalReference}}.{{nameof(List<object>.Remove)}}(this);
                          {{transientScope.ContainerReference}}.{{_containerNode.TransientScopeDisposalReference}}.{{nameof(List<object>.TrimExcess)}}();
                          }
                          finally
                          {
                          {{transientScope.ContainerReference}}.{{_containerNode.TransientScopeDisposalSemaphoreReference}}.{{nameof(SemaphoreSlim.Release)}}();
                          }
                          """);
                    break;
            }
        
            code.AppendLine(
                $$"""
                      {{awaitPrefix}}{{utilityCallName}}(({{_disposeUtility.DisposableRangeInterfaceData.InterfaceNameFullyQualified}}) {{Constants.ThisKeyword}});
                  }
                  """);
        }

        void GenerateClauseFunction(string clauseFunctionName, IReadOnlyList<INamedTypeSymbol> disposables, bool includeScopes)
        {
            var clause = string.Join(" || ", disposables
                .Select(d => d.IsUnboundGenericType 
                    ? $"genericTypeDefinition == typeof({d.FullName()})" 
                    : $"disposable is {d.FullName()}")
                .AppendIf($"disposable is {_containerNode.ScopeInterface}", includeScopes));

            code.AppendLine(
                $$"""
                  bool {{_disposeUtility.DisposableRangeInterfaceData.InterfaceNameFullyQualified}}.{{clauseFunctionName}}({{_wellKnownTypes.Object.FullName()}} disposable)
                  {
                      var genericTypeDefinition = disposable.GetType().IsGenericType ? disposable.GetType().GetGenericTypeDefinition() : null;
                      return {{(string.IsNullOrWhiteSpace(clause) ? "false" : clause)}};
                  }
                  """);
        }
    }
    
    protected string GenerateDisposalInterfaceAssignments() =>
        GetGeneratedDisposalTypes() switch
        {
            DisposalType.Sync | DisposalType.Async when _wellKnownTypes.IAsyncDisposable is not null => 
                $"{_wellKnownTypes.IAsyncDisposable.FullName()}, {_wellKnownTypes.IDisposable.FullName()}",
            DisposalType.Async when _wellKnownTypes.IAsyncDisposable is not null => 
                $"{_wellKnownTypes.IAsyncDisposable.FullName()}",
            DisposalType.Sync => $"{_wellKnownTypes.IDisposable.FullName()}",
            _ => ""
        };
    
    // The generated disposal handling is only depending on the availability of the IAsyncDisposable interface.
    private DisposalType GetGeneratedDisposalTypes() =>
        _wellKnownTypes.IAsyncDisposable is null || _wellKnownTypes.ValueTask is null
            ? DisposalType.Sync
            : DisposalType.Sync | DisposalType.Async;

}