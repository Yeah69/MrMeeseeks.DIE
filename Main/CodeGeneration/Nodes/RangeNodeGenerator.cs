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
    private readonly ISingularDisposeFunctionUtility _singularDisposeFunctionUtility;
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly WellKnownTypesCollections _wellKnownTypesCollections;

    protected RangeNodeGenerator(
        IRangeNode rangeNode,
        IContainerNode containerNode,
        ISingularDisposeFunctionUtility singularDisposeFunctionUtility,
        WellKnownTypes wellKnownTypes,
        WellKnownTypesCollections wellKnownTypesCollections)
    {
        _rangeNode = rangeNode;
        _containerNode = containerNode;
        _singularDisposeFunctionUtility = singularDisposeFunctionUtility;
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

        code.AppendLine(
            $$"""
              {{ClassDeclaredAccessibility}}sealed partial class {{_rangeNode.Name}} : {{InterfaceAssignment}}
              {
              """);
        
        if (_rangeNode.GenerateEmptyConstructor) 
            code.AppendLine($"{DefaultConstructorDeclaredAccessibility}{_rangeNode.Name}() {{ }}");
        
        PreGeneralContent(code, visitor);
        
        code.AppendLine(
            $$"""
              private {{_wellKnownTypes.Int32.FullName()}} {{_rangeNode.ResolutionCounterReference}};
              private {{_wellKnownTypes.ListOfListOfObject.FullName()}} {{_rangeNode.DisposalHandling.CollectionReference}} = new {{_wellKnownTypes.ListOfListOfObject.FullName()}}();
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
                  }
                  }
                  """);
        }
    }
    private void GenerateDisposalFunction(
        StringBuilder code)
    {
        var disposalHandling = _rangeNode.DisposalHandling;
        
        if (_wellKnownTypes.ConcurrentBagOfAsyncDisposable is not null)
            code.AppendLine($"private {_wellKnownTypes.ConcurrentBagOfAsyncDisposable.FullName()} {disposalHandling.AsyncCollectionReference} = new {_wellKnownTypes.ConcurrentBagOfAsyncDisposable.FullName()}();");

        code.AppendLine($$"""
            private {{_wellKnownTypes.ConcurrentBagOfSyncDisposable.FullName()}} {{disposalHandling.SyncCollectionReference}} = new {{_wellKnownTypes.ConcurrentBagOfSyncDisposable.FullName()}}();
            private int {{disposalHandling.DisposedFieldReference}} = 0;
            private bool {{disposalHandling.DisposedPropertyReference}} => {{disposalHandling.DisposedFieldReference}} != 0;
            """);

        code.AppendLine(
            $$"""
              public void {{nameof(IDisposable.Dispose)}}()
              {
                  var {{disposalHandling.DisposedLocalReference}} = {{_wellKnownTypes.Interlocked.FullName()}}.{{nameof(Interlocked.Exchange)}}(ref {{disposalHandling.DisposedFieldReference}}, 1);
                  if ({{disposalHandling.DisposedLocalReference}} != 0) return;
                  {{_wellKnownTypes.SpinWait}}.{{nameof(SpinWait.SpinUntil)}}(() => {{_rangeNode.ResolutionCounterReference}} == 0);
                  if ({{_singularDisposeFunctionUtility.AggregateExceptionRoutineFullyQualified}}(Inner()) is {} aggregateException) throw aggregateException;
                  return;
                  
                  {{_wellKnownTypes.IEnumerableOfException}} Inner()
                  {
              """);
        
            switch (_rangeNode)
            {
                case IContainerNode container:
                    code.AppendLine(
                        $$"""
                          var temp = {{container.TransientScopeDisposalReference}}.ToArray();
                          foreach (var transientScope in temp)
                          {
                          """);
                    
                    if (_wellKnownTypes.IAsyncDisposable is not null && _wellKnownTypes.ValueTask is not null)
                        code.AppendLine(
                            $$"""
                              if (transientScope is {{_wellKnownTypes.IAsyncDisposable.FullName()}} asyncDisposable && {{_singularDisposeFunctionUtility.DisposeAsyncSyncedFullyQualified}}(asyncDisposable) is {{_wellKnownTypes.Exception.FullName()}} exception)
                              yield return exception;
                              """);
                    else
                        code.AppendLine(
                            $$"""
                              if (transientScope is {{_wellKnownTypes.IDisposable.FullName()}} disposable && {{_singularDisposeFunctionUtility.DisposeFullyQualified}}(disposable) is {{_wellKnownTypes.Exception.FullName()}} exception)
                              yield return exception;
                              """);
                    
                    code.AppendLine(
                        $$"""
                          }
                          {{container.TransientScopeDisposalReference}}.Clear();
                          """);
                    break;
                case ITransientScopeNode transientScope:
                    code.AppendLine(
                        $$"""
                          {{transientScope.ContainerReference}}.{{transientScope.TransientScopeDisposalReference}}.{{nameof(List<object>.Remove)}}(this);
                          {{transientScope.ContainerReference}}.{{transientScope.TransientScopeDisposalReference}}.{{nameof(List<object>.TrimExcess)}}();
                          """);
                    break;
            }

        
        code.AppendLine(
            $$"""
                      for (var i = {{_rangeNode.DisposalHandling.CollectionReference}}.{{nameof(List<List<object>>.Count)}} - 1; i >= 0; i--)
                      {
                          foreach (var exception in {{_rangeNode.DisposeChunkMethodName}}({{_rangeNode.DisposalHandling.CollectionReference}}[i]))
                          {
                              yield return exception;
                          }
                      }
              """);
        
            if (_wellKnownTypes.IAsyncDisposable is not null && _wellKnownTypes.ValueTask is not null)
            {
                code.AppendLine(
                    $$"""
                      foreach (var disposable in {{_rangeNode.DisposalHandling.AsyncCollectionReference}})
                      {
                          if ({{_singularDisposeFunctionUtility.DisposeAsyncSyncedFullyQualified}}(disposable) is {{_wellKnownTypes.Exception.FullName()}} exception)
                          {
                              yield return exception;
                          }
                      }
                      """);
            }
            code.AppendLine(
                $$"""
                  
                          foreach (var disposable in {{_rangeNode.DisposalHandling.SyncCollectionReference}})
                          {
                              if ({{_singularDisposeFunctionUtility.DisposeFullyQualified}}(disposable) is {{_wellKnownTypes.Exception.FullName()}} exception)
                              {
                                  yield return exception;
                              }
                          }
                      }
                  }
                  """);
        
        if (_wellKnownTypes.ValueTask is not null
            && _wellKnownTypes.IAsyncDisposable is not null
            && _wellKnownTypesCollections.IAsyncEnumerable1 is not null
            && _wellKnownTypes.IAsyncEnumerableOfException is not null)
        {
            code.AppendLine(
                $$"""
                  public async {{_wellKnownTypes.ValueTask.FullName()}} {{Constants.IAsyncDisposableDisposeAsync}}()
                  {
                  var {{disposalHandling.DisposedLocalReference}} = {{_wellKnownTypes.Interlocked.FullName()}}.{{nameof(Interlocked.Exchange)}}(ref {{disposalHandling.DisposedFieldReference}}, 1);
                  if ({{disposalHandling.DisposedLocalReference}} != 0) return;
                  {{_wellKnownTypes.SpinWait}}.{{nameof(SpinWait.SpinUntil)}}(() => {{_rangeNode.ResolutionCounterReference}} == 0);
                  if (await {{_singularDisposeFunctionUtility.AggregateExceptionRoutineAsyncFullyQualified}}(Inner()) is {} aggregateException) throw aggregateException;
                  return;

                  async {{_wellKnownTypes.IAsyncEnumerableOfException}} Inner()
                  {
                  """);
            
            
        
            switch (_rangeNode)
            {
                case IContainerNode container:
                    code.AppendLine(
                        $$"""
                          var temp = {{container.TransientScopeDisposalReference}}.ToArray();
                          foreach (var transientScope in temp)
                          {
                          if (transientScope is {{_wellKnownTypes.IAsyncDisposable.FullName()}} asyncDisposable && await {{_singularDisposeFunctionUtility.DisposeAsyncFullyQualified}}(asyncDisposable) is {{_wellKnownTypes.Exception.FullName()}} exception)
                          yield return exception;
                          }
                          {{container.TransientScopeDisposalReference}}.Clear();
                          """);
                    break;
                case ITransientScopeNode transientScope:
                    code.AppendLine(
                        $$"""
                          {{transientScope.ContainerReference}}.{{transientScope.TransientScopeDisposalReference}}.{{nameof(List<object>.Remove)}}(this);
                          {{transientScope.ContainerReference}}.{{transientScope.TransientScopeDisposalReference}}.{{nameof(List<object>.TrimExcess)}}();
                          """);
                    break;
            }

            code.AppendLine(
                $$"""
                            for (var i = {{_rangeNode.DisposalHandling.CollectionReference}}.{{nameof(List<List<object>>.Count)}} - 1; i >= 0; i--)
                            {
                                await foreach (var exception in {{_rangeNode.DisposeChunkAsyncMethodName}}({{_rangeNode.DisposalHandling.CollectionReference}}[i]))
                                {
                                    yield return exception;
                                }
                            }
                            foreach (var disposable in {{_rangeNode.DisposalHandling.AsyncCollectionReference}})
                            {
                                if ({{_singularDisposeFunctionUtility.DisposeAsyncSyncedFullyQualified}}(disposable) is {{_wellKnownTypes.Exception.FullName()}} exception)
                                {
                                    yield return exception;
                                }
                            }
                            foreach (var disposable in {{_rangeNode.DisposalHandling.SyncCollectionReference}})
                            {
                                if ({{_singularDisposeFunctionUtility.DisposeFullyQualified}}(disposable) is {{_wellKnownTypes.Exception.FullName()}} exception)
                                {
                                    yield return exception;
                                }
                            }
                        }
                    }
                  """);
        }

        var disposalMap = _rangeNode.GetDisposalTypeToTypeFullNames();
        var asyncDisposal = disposalMap.TryGetValue(DisposalType.Async, out var listAsync) ? listAsync : [];
        var syncDisposal = disposalMap.TryGetValue(DisposalType.Sync, out var listSync) ? listSync : [];
        
        GenerateDisposeExceptionHandling();
        GenerateDisposeExceptionHandlingAsync();

        GenerateDisposeChunk(isAsync: false);

        if (_wellKnownTypes.ValueTask is not null
            && _wellKnownTypes.IAsyncDisposable is not null
            && _wellKnownTypesCollections.IAsyncEnumerable1 is not null
            && _wellKnownTypes.IAsyncEnumerableOfException is not null)
        {
            GenerateDisposeChunk(isAsync: true);
        }
        
 
        
        return;

        void GenerateDisposeExceptionHandling()
        {
            var transientScopeDisposableType = _wellKnownTypes.IAsyncDisposable is not null && _wellKnownTypes.ValueTask is not null
                ? _wellKnownTypes.IAsyncDisposable.FullName()
                : _wellKnownTypes.IDisposable.FullName();
            var transientScopeDisposeMethod = _wellKnownTypes.IAsyncDisposable is not null && _wellKnownTypes.ValueTask is not null
                ? _singularDisposeFunctionUtility.DisposeAsyncSyncedFullyQualified
                : _singularDisposeFunctionUtility.DisposeFullyQualified;
            
            code.AppendLine(
                $$"""
                  private {{_wellKnownTypes.Exception.FullName()}} {{_rangeNode.DisposeExceptionHandlingMethodName}}({{_wellKnownTypes.Exception.FullName()}} exception, {{_wellKnownTypes.ListOfObject.FullName()}} subDisposal, {{_wellKnownTypes.ListOfObject.FullName()}}? transientScopeDisposal = null)
                  {
                      if ({{_singularDisposeFunctionUtility.AggregateExceptionRoutineFullyQualified}}(Inner()) is { } aggregateException)
                          return aggregateException;
                      else
                          return exception;
                          
                      {{_wellKnownTypes.IEnumerableOfException}} Inner()
                      {
                         yield return exception;
                         if (transientScopeDisposal is not null)
                         {
                             foreach (var transientScope in transientScopeDisposal)
                             {
                                if (transientScope is {{transientScopeDisposableType}} disposable && {{transientScopeDisposeMethod}}(disposable) is {{_wellKnownTypes.Exception.FullName()}} transientException)
                                {
                                    yield return transientException;
                                }
                             }
                         }
                         foreach (var subException in {{_rangeNode.DisposeChunkMethodName}}(subDisposal))
                         {
                             yield return subException;
                         }
                      }
                  }
                  """);
        }

        void GenerateDisposeExceptionHandlingAsync()
        {
            if (_wellKnownTypes.ValueTask is null
                || _wellKnownTypes.IAsyncDisposable is null
                || _wellKnownTypesCollections.IAsyncEnumerable1 is null)
                return;
            
            code.AppendLine(
                $$"""
                  private async {{_wellKnownTypes.TaskOfException.FullName()}} {{_rangeNode.DisposeExceptionHandlingAsyncMethodName}}({{_wellKnownTypes.Exception.FullName()}} exception, {{_wellKnownTypes.ListOfObject.FullName()}} subDisposal, {{_wellKnownTypes.ListOfObject.FullName()}}? transientScopeDisposal = null)
                  {
                      if (await {{_singularDisposeFunctionUtility.AggregateExceptionRoutineAsyncFullyQualified}}(Inner()) is { } aggregateException && aggregateException.InnerExceptions.Count > 1)
                          return aggregateException;
                      else
                          return exception;
                      
                      async {{_wellKnownTypes.IAsyncEnumerableOfException}} Inner()
                      {
                         yield return exception;
                         if (transientScopeDisposal is not null)
                         {
                             foreach (var transientScope in transientScopeDisposal)
                             {
                                 if (transientScope is {{_wellKnownTypes.IAsyncDisposable.FullName()}} disposable && await {{_singularDisposeFunctionUtility.DisposeAsyncFullyQualified}}(disposable) is {{_wellKnownTypes.Exception.FullName()}} transientException)
                                 {
                                     yield return transientException;
                                 }
                             }
                         }
                         await foreach (var subException in {{_rangeNode.DisposeChunkAsyncMethodName}}(subDisposal))
                         {
                             yield return subException;
                         }
                      }
                  }
                  """);
        }

        void GenerateDisposeChunk(bool isAsync)
        {
            var (asyncModifier, returnType, functionName, taskYieldLine, treatScopesAsync, asyncDisposableCall) = isAsync switch
            {
                true when _wellKnownTypes.IAsyncEnumerableOfException is not null => 
                    ("async ", 
                        _wellKnownTypes.IAsyncEnumerableOfException.FullName(), 
                        _rangeNode.DisposeChunkAsyncMethodName,
                        $"await {_wellKnownTypes.Task.FullName()}.{nameof(Task.Yield)}();",
                        true,
                        $"await {_singularDisposeFunctionUtility.DisposeAsyncFullyQualified}"),
                false => 
                    ("", 
                        _wellKnownTypes.IEnumerableOfException.FullName(), 
                        _rangeNode.DisposeChunkMethodName, 
                        "",
                        _wellKnownTypes.IAsyncDisposable is not null && _singularDisposeFunctionUtility.DisposeAsyncSyncedFullyQualified is not null,
                        _singularDisposeFunctionUtility.DisposeAsyncSyncedFullyQualified ?? ""),
                _ => throw new InvalidOperationException("DisposeChunkAsync shouldn't be generated if async types aren't available.")
            };
            
            code.AppendLine(
                $$"""
                  private static {{asyncModifier}}{{returnType}} {{functionName}}({{_wellKnownTypes.ListOfObject.FullName()}} disposables)
                  {
                      {{taskYieldLine}}
                      for (var i = disposables.{{nameof(List<object>.Count)}} - 1; i >= 0; i--)
                      {
                  """);
            
            if (asyncDisposal.Concat(syncDisposal).Any(t => t.IsUnboundGenericType))
                code.AppendLine($"{_wellKnownTypes.Type.FullName()} genericTypeDefinition = disposables[i].{nameof(Type.GetType)}().{nameof(Type.GetGenericTypeDefinition)}();");
            
            if (_wellKnownTypes.IAsyncDisposable is not null
                && _singularDisposeFunctionUtility.DisposeAsyncSyncedFullyQualified is not null
                && _singularDisposeFunctionUtility.DisposeAsyncFullyQualified is not null)
                IfClause(asyncDisposal, treatScopesAsync, _wellKnownTypes.IAsyncDisposable.FullName(), asyncDisposableCall);
            
            IfClause(syncDisposal, !treatScopesAsync, _wellKnownTypes.IDisposable.FullName(), _singularDisposeFunctionUtility.DisposeFullyQualified);

            code.AppendLine(
                """
                    }
                }
                """);
            return;

            void IfClause(IReadOnlyList<INamedTypeSymbol> disposedTypes, bool includeScopes, string disposableType, string disposableCall)
            {
                var clause = string.Join(" || ", disposedTypes
                    .Select(d => d.IsUnboundGenericType 
                        ? $"genericTypeDefinition == typeof({d.FullName()})" 
                        : $"disposables[i] is {d.FullName()}")
                    .AppendIf($"disposables[i] is {_containerNode.ScopeInterface}", includeScopes));
                if (!string.IsNullOrWhiteSpace(clause))
                    code.AppendLine(
                        $$"""
                          if ({{clause}})
                          {
                              if (disposables[i] is {{disposableType}} disposable && {{disposableCall}}(disposable) is {{_wellKnownTypes.Exception}} exception)
                              {
                                  yield return exception;
                              }
                          }
                          """);
            }
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