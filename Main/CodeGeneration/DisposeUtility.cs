using System.Threading;
using System.Threading.Tasks;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.CodeGeneration;

internal sealed record DisposalUtilityInterfaceData(
    string InterfaceNameFullyQualified,
    string SyncClauseFunctionName,
    string? AsyncClauseFunctionName,
    string TransientScopesPropertyName,
    string DisposablesPropertyName,
    string UserDefinedSyncDisposablesPropertyName,
    string? UserDefinedAsyncDisposablesPropertyName);

internal interface IDisposeUtility
{
    string ClassName { get; }
    DisposalUtilityInterfaceData DisposableRangeInterfaceData { get; }
    string DisposeFullyQualified { get; }
    string? DisposeSyncOnlyFullyQualified { get; }
    string? DisposeAsyncFullyQualified { get; }
    string? DisposeChunkAsyncFullyQualified { get; }
    string DisposeExceptionHandlingFullyQualified { get; }
    string? DisposeExceptionHandlingSyncOnlyFullyQualified { get; }
    string? DisposeExceptionHandlingAsyncFullyQualified { get; }
    string? DisposeSingularAsyncFullyQualified { get; }
    string? DisposeSingularAsyncSyncedFullyQualified { get; }
    string GenerateSingularDisposeFunctionsFile();
}

internal sealed class DisposeUtility : IDisposeUtility, IContainerInstance
{
    private readonly IReferenceGenerator _referenceGenerator;
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly WellKnownTypesCollections _wellKnownTypesCollections;
    private readonly string _disposeName;
    private readonly string? _disposeSyncOnlyName;
    private readonly string? _disposeAsyncName;
    private readonly string _disposeSingularName;
    private readonly string? _disposeSingularAsyncName;
    private readonly string? _disposeSingularAsyncSyncedName;
    private readonly string _aggregateExceptionRoutine;
    private readonly string? _aggregateExceptionRoutineAsync;
    private readonly string _disposableRangeInterfaceName;
    private readonly string _disposeChunkName;
    private readonly string? _disposeChunkAsyncName;
    private readonly string _disposeExceptionHandlingName;
    private readonly string? _disposeExceptionHandlingSyncOnlyName;
    private readonly string? _disposeExceptionHandlingAsyncName;
    private readonly string _syncDisposalTriggeredExceptionName;

    internal DisposeUtility(
        IReferenceGenerator referenceGenerator,
        WellKnownTypes wellKnownTypes,
        WellKnownTypesCollections wellKnownTypesCollections)
    {
        _referenceGenerator = referenceGenerator;
        _wellKnownTypes = wellKnownTypes;
        _wellKnownTypesCollections = wellKnownTypesCollections;
        ClassName = referenceGenerator.Generate("DisposeUtility");
        
        _disposeName = referenceGenerator.Generate("Dispose");
        DisposeFullyQualified = $"{Constants.NamespaceForGeneratedUtilities}.{ClassName}.{_disposeName}";
        
        _disposeSyncOnlyName = wellKnownTypes.IAsyncDisposable is not null && wellKnownTypes.ValueTask is not null
            ? referenceGenerator.Generate("DisposeSyncOnly")
            : null;
        DisposeSyncOnlyFullyQualified = _disposeSyncOnlyName is not null
            ? $"{Constants.NamespaceForGeneratedUtilities}.{ClassName}.{_disposeSyncOnlyName}"
            : null;
        
        _disposeAsyncName = wellKnownTypes.IAsyncDisposable is not null && wellKnownTypes.ValueTask is not null
            ? referenceGenerator.Generate("DisposeAsync")
            : null;
        DisposeAsyncFullyQualified = _disposeAsyncName is not null
            ? $"{Constants.NamespaceForGeneratedUtilities}.{ClassName}.{_disposeAsyncName}"
            : null;
        
        _disposeSingularName = referenceGenerator.Generate("DisposeSingular");
        DisposeSingularFullyQualified = $"{Constants.NamespaceForGeneratedUtilities}.{ClassName}.{_disposeSingularName}";
        
        _disposeSingularAsyncName = wellKnownTypes.IAsyncDisposable is not null && _wellKnownTypes.ValueTask is not null
            ? referenceGenerator.Generate("DisposeSingularAsync")
            : null;
        DisposeSingularAsyncFullyQualified = _disposeSingularAsyncName is not null
            ? $"{Constants.NamespaceForGeneratedUtilities}.{ClassName}.{_disposeSingularAsyncName}"
            : null;
        
        _disposeSingularAsyncSyncedName = wellKnownTypes.IAsyncDisposable is not null && _wellKnownTypes.ValueTask is not null
            ? referenceGenerator.Generate("DisposeSingularAsyncSynced")
            : null;
        DisposeSingularAsyncSyncedFullyQualified = _disposeSingularAsyncSyncedName is not null
            ? $"{Constants.NamespaceForGeneratedUtilities}.{ClassName}.{_disposeSingularAsyncSyncedName}"
            : null;
        
        _aggregateExceptionRoutine = referenceGenerator.Generate("AggregateExceptionRoutine");
        AggregateExceptionRoutineFullyQualified = $"{Constants.NamespaceForGeneratedUtilities}.{ClassName}.{_aggregateExceptionRoutine}";
        
        _aggregateExceptionRoutineAsync = wellKnownTypes.IAsyncEnumerableOfException is not null && _wellKnownTypes.ValueTask is not null
            ? referenceGenerator.Generate("AggregateExceptionRoutineAsync")
            : null;
        AggregateExceptionRoutineAsyncFullyQualified = _aggregateExceptionRoutineAsync is not null 
            ? $"{Constants.NamespaceForGeneratedUtilities}.{ClassName}.{_aggregateExceptionRoutineAsync}"
            : null;
        
        _disposableRangeInterfaceName = referenceGenerator.Generate("IDisposableRange");
        
        DisposableRangeInterfaceData = new DisposalUtilityInterfaceData(
            $"{Constants.NamespaceForGeneratedUtilities}.{_disposableRangeInterfaceName}",
            referenceGenerator.Generate("ShouldBeDisposed"),
            wellKnownTypes.IAsyncDisposable is not null && _wellKnownTypes.ValueTask is not null
                ? referenceGenerator.Generate("ShouldBeDisposedAsync")
                : null,
            referenceGenerator.Generate("TransientScopes"),
            referenceGenerator.Generate("Disposables"),
            referenceGenerator.Generate("UserDefinedSyncDisposables"),
            wellKnownTypes.IAsyncDisposable is not null
                ? referenceGenerator.Generate("UserDefinedAsyncDisposables")
                : null);
        
        _disposeChunkName = referenceGenerator.Generate("DisposeChunk");
        _disposeChunkAsyncName = wellKnownTypes.IAsyncEnumerableOfException is not null && _wellKnownTypes.ValueTask is not null
            ? referenceGenerator.Generate("DisposeChunkAsync")
            : null;
        DisposeChunkFullyQualified = $"{Constants.NamespaceForGeneratedUtilities}.{ClassName}.{_disposeChunkName}";
        DisposeChunkAsyncFullyQualified = _disposeChunkAsyncName is not null
            ? $"{Constants.NamespaceForGeneratedUtilities}.{ClassName}.{_disposeChunkAsyncName}"
            : null;
        
        _disposeExceptionHandlingName = referenceGenerator.Generate("DisposeExceptionHandling");
        _disposeExceptionHandlingSyncOnlyName = wellKnownTypes.IAsyncDisposable is not null && wellKnownTypes.ValueTask is not null
            ? referenceGenerator.Generate("DisposeExceptionHandlingSyncOnly")
            : null;
        _disposeExceptionHandlingAsyncName = wellKnownTypes.ValueTask is not null && wellKnownTypes.IAsyncDisposable is not null
            ? referenceGenerator.Generate("DisposeExceptionHandlingAsync")
            : null;
        
        DisposeExceptionHandlingFullyQualified = $"{Constants.NamespaceForGeneratedUtilities}.{ClassName}.{_disposeExceptionHandlingName}";
        DisposeExceptionHandlingSyncOnlyFullyQualified = _disposeExceptionHandlingSyncOnlyName is not null
            ? $"{Constants.NamespaceForGeneratedUtilities}.{ClassName}.{_disposeExceptionHandlingSyncOnlyName}"
            : null;
        DisposeExceptionHandlingAsyncFullyQualified = _disposeExceptionHandlingAsyncName is not null
            ? $"{Constants.NamespaceForGeneratedUtilities}.{ClassName}.{_disposeExceptionHandlingAsyncName}"
            : null;
        
        _syncDisposalTriggeredExceptionName = "SyncDisposalTriggeredException";
    }

    public DisposalUtilityInterfaceData DisposableRangeInterfaceData { get; }
    public string DisposeFullyQualified { get; }
    public string? DisposeSyncOnlyFullyQualified { get; }
    public string? DisposeAsyncFullyQualified { get; }
    private string DisposeChunkFullyQualified { get; }
    public string? DisposeChunkAsyncFullyQualified { get; }
    public string DisposeExceptionHandlingFullyQualified { get; }
    public string? DisposeExceptionHandlingSyncOnlyFullyQualified { get; }
    public string? DisposeExceptionHandlingAsyncFullyQualified { get; }
    private string AggregateExceptionRoutineFullyQualified { get; }
    public string? AggregateExceptionRoutineAsyncFullyQualified { get; }
    private string DisposeSingularFullyQualified { get; }
    public string? DisposeSingularAsyncFullyQualified { get; }
    public string? DisposeSingularAsyncSyncedFullyQualified { get; }
    public string ClassName { get; }

    public string GenerateSingularDisposeFunctionsFile()
    {
        var code = new StringBuilder();
        code.AppendLine(
            $$"""
              #nullable enable
              namespace {{Constants.NamespaceForGeneratedUtilities}}
              {
              """);
        
        GenerateDisposableRangeInterface();
        
        GenerateSyncDisposalTriggeredException();

        code.AppendLine(
            $$"""
                  internal static class {{ClassName}}
                  {
              """);
        
        var disposeParamReference = _referenceGenerator.Generate("disposableRange");
        
        if (_disposeAsyncName is not null && _wellKnownTypes.IAsyncDisposable is not null && _wellKnownTypes.ValueTask is not null)
        {
            GenerateSyncDisposeToException();
            GenerateSyncDispose(syncOnlyMode: true);
        }
        else
            GenerateSyncDispose(syncOnlyMode: false);

        GenerateAsyncDispose();
        
        GenerateDisposeChunk(isAsync: false);

        if (_wellKnownTypes.IAsyncDisposable is not null && _wellKnownTypes.ValueTask is not null && _wellKnownTypes.IAsyncEnumerableOfException is not null)
            GenerateDisposeChunk(isAsync: true);
        
        if (_disposeExceptionHandlingAsyncName is not null && _wellKnownTypes.IAsyncDisposable is not null && _wellKnownTypes.ValueTask is not null)
        {
            GenerateDisposeExceptionHandlingToException();
            GenerateDisposeExceptionHandling(syncOnlyMode: true);
        }
        else
            GenerateDisposeExceptionHandling(syncOnlyMode: false);
        
        if (_wellKnownTypes.ValueTask is not null && _wellKnownTypes.IAsyncDisposable is not null)
            GenerateDisposeExceptionHandlingAsync();
        
        code.AppendLine(
            $$"""
                      private static {{_wellKnownTypes.Exception.FullName()}}? {{_disposeSingularName}}({{_wellKnownTypes.IDisposable.FullName()}} disposable)
                      {
                          try
                          {
                              disposable.{{nameof(IDisposable.Dispose)}}();
                          }
                          catch ({{_wellKnownTypes.Exception.FullName()}} e)
                          {
                              return e;
                          }
              
                          return null;
                      }
              """);
        
        if (_disposeSingularAsyncName is not null && _wellKnownTypes.IAsyncDisposable is not null && _wellKnownTypes.ValueTask is not null)
        {
            code.AppendLine(
                $$"""
                  private static async {{_wellKnownTypes.ValueTask.FullName()}}<{{_wellKnownTypes.Exception.FullName()}}?> {{_disposeSingularAsyncName}}({{_wellKnownTypes.IAsyncDisposable.FullName()}} disposable)
                  {
                      try
                      {
                          await disposable.{{Constants.IAsyncDisposableDisposeAsync}}();
                      }
                      catch ({{_wellKnownTypes.Exception.FullName()}} e)
                      {
                          return e;
                      }
                  
                      return null;
                  }
                  """);
        }

        if (_disposeSingularAsyncSyncedName is not null && _wellKnownTypes.IAsyncDisposable is not null && _wellKnownTypes.ValueTask is not null)
        {
            code.AppendLine(
                $$"""
                  private static {{_wellKnownTypes.Exception.FullName()}}? {{_disposeSingularAsyncSyncedName}}({{_wellKnownTypes.IAsyncDisposable.FullName()}} disposable)
                  {
                      try
                      {
                          var valueTask = disposable.{{Constants.IAsyncDisposableDisposeAsync}}();
                          {{_wellKnownTypes.SpinWait.FullName()}}.{{nameof(SpinWait.SpinUntil)}}(() => valueTask.IsCompleted);
                          if (valueTask.IsFaulted)
                              return valueTask.AsTask().{{nameof(Task.Exception)}};
                      }
                      catch ({{_wellKnownTypes.Exception.FullName()}} e)
                      {
                          return e;
                      }
                  
                      return null;
                  }
                  """);
        }
        
        code.AppendLine(
            $$"""
              private static {{_wellKnownTypes.AggregateException.FullName()}}? {{_aggregateExceptionRoutine}}({{_wellKnownTypes.IEnumerableOfException.FullName()}} exceptions)
              {
                  {{_wellKnownTypes.AggregateException.FullName()}} aggregateException = new {{_wellKnownTypes.AggregateException.FullName()}}(exceptions);
                  if (aggregateException.{{nameof(AggregateException.InnerExceptions)}}.{{nameof(ReadOnlyCollection<Exception>.Count)}} > 0) return aggregateException;
                  return null;
              }
              """);
        
        if (_aggregateExceptionRoutineAsync is not null && _wellKnownTypes.IAsyncEnumerableOfException is not null && _wellKnownTypes.ValueTask is not null)
        {
            code.AppendLine(
                $$"""
                  private static async {{_wellKnownTypes.TaskOfNullableAggregateException.FullName()}} {{_aggregateExceptionRoutineAsync}}({{_wellKnownTypes.IAsyncEnumerableOfException.FullName()}} exceptions)
                  {
                      {{_wellKnownTypes.ListOfException.FullName()}} aggregate = new {{_wellKnownTypes.ListOfException.FullName()}}();
                      await foreach (var exception in exceptions)
                      {
                          aggregate.{{nameof(List<Exception>.Add)}}(exception);
                      }
                      if (aggregate.{{nameof(List<Exception>.Count)}} > 0) return new {{_wellKnownTypes.AggregateException.FullName()}}(aggregate);
                      return null;
                  }
                  """);
        }

        code.AppendLine(
            """
                }
            }
            #nullable disable
            """);
        
        return code.ToString();
        
        void GenerateDisposeChunk(bool isAsync)
        {
            var (asyncModifier, returnType, functionName, taskYieldLine, asyncDisposableCall) = isAsync switch
            {
                true when _wellKnownTypes.IAsyncEnumerableOfException is not null => 
                    ("async ", 
                        _wellKnownTypes.IAsyncEnumerableOfException.FullName(), 
                        _disposeChunkAsyncName,
                        $"await {_wellKnownTypes.Task.FullName()}.{nameof(Task.Yield)}();",
                        $"await {DisposeSingularAsyncFullyQualified}"),
                false => 
                    ("", 
                        _wellKnownTypes.IEnumerableOfException.FullName(), 
                        _disposeChunkName, 
                        "",
                        DisposeSingularAsyncSyncedFullyQualified ?? ""),
                _ => throw new InvalidOperationException("DisposeChunkAsync shouldn't be generated if async types aren't available.")
            };
            
            var disposableElementParameterReference = _referenceGenerator.Generate("disposableElement");
            
            code.AppendLine(
                $$"""
                  private static {{asyncModifier}}{{returnType}} {{functionName}}({{DisposableRangeInterfaceData.InterfaceNameFullyQualified}} {{disposableElementParameterReference}}, {{_wellKnownTypes.ListOfObject.FullName()}} disposables)
                  {
                      {{taskYieldLine}}
                      for (var i = disposables.{{nameof(List<object>.Count)}} - 1; i >= 0; i--)
                      {
                  """);
            
            if (_wellKnownTypes.IAsyncDisposable is not null
                && DisposeSingularAsyncSyncedFullyQualified is not null
                && DisposeSingularAsyncFullyQualified is not null)
                IfClause(DisposableRangeInterfaceData.AsyncClauseFunctionName ?? "", _wellKnownTypes.IAsyncDisposable.FullName(), asyncDisposableCall);
            
            IfClause(DisposableRangeInterfaceData.SyncClauseFunctionName, _wellKnownTypes.IDisposable.FullName(), _disposeSingularName);

            code.AppendLine(
                """
                    }
                }
                """);
            return;

            void IfClause(string clauseFunctionName, string disposableType, string disposableCall)
            {
                var disposableReference = _referenceGenerator.Generate("disposable");
                var exceptionReference = _referenceGenerator.Generate("exception");
                code.AppendLine(
                    $$"""
                      if ({{disposableElementParameterReference}}.{{clauseFunctionName}}(disposables[i]) && disposables[i] is {{disposableType}} {{disposableReference}} && {{disposableCall}}({{disposableReference}}) is {{_wellKnownTypes.Exception}} {{exceptionReference}})
                      {
                          yield return {{exceptionReference}};
                      }
                      """);
            }
        }

        void GenerateDisposeExceptionHandling(bool syncOnlyMode)
        {
            var name = syncOnlyMode ? _disposeExceptionHandlingSyncOnlyName : _disposeExceptionHandlingName;
            
            var disposableElementParameterReference = _referenceGenerator.Generate("disposableElement");
            
            var transientScopeDisposableType = !syncOnlyMode && _wellKnownTypes.IAsyncDisposable is not null && _wellKnownTypes.ValueTask is not null
                ? _wellKnownTypes.IAsyncDisposable.FullName()
                : _wellKnownTypes.IDisposable.FullName();
            var transientScopeDisposeMethod = !syncOnlyMode && _wellKnownTypes.IAsyncDisposable is not null && _wellKnownTypes.ValueTask is not null
                ? DisposeSingularAsyncSyncedFullyQualified
                : DisposeSingularFullyQualified;
            
            code.AppendLine(
                $$"""
                  internal static {{_wellKnownTypes.Exception.FullName()}} {{name}}({{DisposableRangeInterfaceData.InterfaceNameFullyQualified}} {{disposableElementParameterReference}}, {{_wellKnownTypes.Exception.FullName()}} exception, {{_wellKnownTypes.ListOfObject.FullName()}} subDisposal, {{_wellKnownTypes.ListOfObject.FullName()}}? transientScopeDisposal = null)
                  {
                      if ({{AggregateExceptionRoutineFullyQualified}}(Inner()) is { } aggregateException)
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
                         foreach (var subException in {{DisposeChunkFullyQualified}}({{disposableElementParameterReference}}, subDisposal))
                         {
                             yield return subException;
                         }
                      }
                  }
                  """);
        }

        void GenerateDisposeExceptionHandlingToException()
        {
            if (_disposeExceptionHandlingAsyncName is null) return;
            
            var disposableElementParameterReference = _referenceGenerator.Generate("disposableElement");
            
            code.AppendLine(
                $$"""
                  internal static {{_wellKnownTypes.Exception.FullName()}} {{_disposeExceptionHandlingName}}({{DisposableRangeInterfaceData.InterfaceNameFullyQualified}} {{disposableElementParameterReference}}, {{_wellKnownTypes.Exception.FullName()}} exception, {{_wellKnownTypes.ListOfObject.FullName()}} subDisposal, {{_wellKnownTypes.ListOfObject.FullName()}}? transientScopeDisposal = null) =>
                        throw new {{_syncDisposalTriggeredExceptionName}}({{_disposeExceptionHandlingAsyncName}}({{disposableElementParameterReference}}, exception, subDisposal, transientScopeDisposal), exception);
                  """);
        }

        void GenerateDisposeExceptionHandlingAsync()
        {
            var disposableElementParameterReference = _referenceGenerator.Generate("disposableElement");

            if (_wellKnownTypes.ValueTask is null
                || _wellKnownTypes.IAsyncDisposable is null
                || _wellKnownTypesCollections.IAsyncEnumerable1 is null)
                return;
            
            code.AppendLine(
                $$"""
                  internal static async {{_wellKnownTypes.TaskOfException.FullName()}} {{_disposeExceptionHandlingAsyncName}}({{DisposableRangeInterfaceData.InterfaceNameFullyQualified}} {{disposableElementParameterReference}}, {{_wellKnownTypes.Exception.FullName()}} exception, {{_wellKnownTypes.ListOfObject.FullName()}} subDisposal, {{_wellKnownTypes.ListOfObject.FullName()}}? transientScopeDisposal = null)
                  {
                      if (await {{AggregateExceptionRoutineAsyncFullyQualified}}(Inner()) is { } aggregateException && aggregateException.InnerExceptions.Count > 1)
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
                                 if (transientScope is {{_wellKnownTypes.IAsyncDisposable.FullName()}} disposable && await {{DisposeSingularAsyncFullyQualified}}(disposable) is {{_wellKnownTypes.Exception.FullName()}} transientException)
                                 {
                                     yield return transientException;
                                 }
                             }
                         }
                         await foreach (var subException in {{DisposeChunkAsyncFullyQualified}}({{disposableElementParameterReference}}, subDisposal))
                         {
                             yield return subException;
                         }
                      }
                  }
                  """);
        }

        void GenerateDisposableRangeInterface()
        {
            code.AppendLine(
                $$"""
                  internal interface {{_disposableRangeInterfaceName}}
                  {
                      internal {{_wellKnownTypes.Object.FullName()}}[] {{DisposableRangeInterfaceData.TransientScopesPropertyName}} { get; }
                      internal {{_wellKnownTypes.ListOfListOfObject.FullName()}} {{DisposableRangeInterfaceData.DisposablesPropertyName}} { get; }
                      internal {{_wellKnownTypes.ConcurrentBagOfSyncDisposable.FullName()}} {{DisposableRangeInterfaceData.UserDefinedSyncDisposablesPropertyName}} { get; }
                      internal bool {{DisposableRangeInterfaceData.SyncClauseFunctionName}}({{_wellKnownTypes.Object.FullName()}} disposable);
                  """);
            if (DisposableRangeInterfaceData.UserDefinedAsyncDisposablesPropertyName is not null)
                code.AppendLine(
                    $"    internal {_wellKnownTypes.ConcurrentBagOfAsyncDisposable?.FullName()} {DisposableRangeInterfaceData.UserDefinedAsyncDisposablesPropertyName} {{ get; }}");
        
            if (DisposableRangeInterfaceData.AsyncClauseFunctionName is not null)
                code.AppendLine(
                    $"internal bool {DisposableRangeInterfaceData.AsyncClauseFunctionName}({_wellKnownTypes.Object.FullName()} disposable);");
        
            code.AppendLine("}");
        }

        void GenerateSyncDisposalTriggeredException()
        {
            if (_wellKnownTypes.IAsyncDisposable is null || _wellKnownTypes.ValueTask is null)
                return;
            
            code.AppendLine(
                $$"""
                  internal class {{_syncDisposalTriggeredExceptionName}} : {{_wellKnownTypes.Exception.FullName()}}
                  {
                      internal {{_wellKnownTypes.Task.FullName()}} AsyncDisposal { get; }
                      internal {{_wellKnownTypes.Exception.FullName()}}? Exception { get; }
                  
                      internal {{_syncDisposalTriggeredExceptionName}}({{_wellKnownTypes.Task.FullName()}} asyncDisposal)
                          : base("MrMeeseeks.DIE: Sync disposal triggered where the async disposal may be necessary. Await the AsyncDisposal property to make sure the async disposal is completed.")
                      {
                          AsyncDisposal = asyncDisposal;
                      }
                  
                      internal {{_syncDisposalTriggeredExceptionName}}({{_wellKnownTypes.Task.FullName()}} asyncDisposal, {{_wellKnownTypes.Exception.FullName()}} exception) 
                          : base("MrMeeseeks.DIE: Sync disposal triggered where the async disposal may be necessary. Await the AsyncDisposal property to make sure the async disposal is completed.", exception)
                      {
                          AsyncDisposal = asyncDisposal;
                          Exception = exception;
                      }
                  }
                  """);
        }

        void GenerateSyncDispose(bool syncOnlyMode)
        {
            var name = syncOnlyMode ? _disposeSyncOnlyName : _disposeName;
            code.AppendLine(
                $$"""
                  internal static void {{name}}({{DisposableRangeInterfaceData.InterfaceNameFullyQualified}} {{disposeParamReference}})
                  {
                      if ({{AggregateExceptionRoutineFullyQualified}}(Inner()) is {} aggregateException) throw aggregateException;
                      return;
                      
                      {{_wellKnownTypes.IEnumerableOfException}} Inner()
                      {
                  """);
            code.AppendLine(
                $$"""
                  foreach (var transientScope in {{disposeParamReference}}.{{DisposableRangeInterfaceData.TransientScopesPropertyName}})
                  {
                  """);
                    
            if (!syncOnlyMode && _wellKnownTypes.IAsyncDisposable is not null && _wellKnownTypes.ValueTask is not null)
                code.AppendLine(
                    $$"""
                      if (transientScope is {{_wellKnownTypes.IAsyncDisposable.FullName()}} asyncDisposable && {{DisposeSingularAsyncSyncedFullyQualified}}(asyncDisposable) is {{_wellKnownTypes.Exception.FullName()}} exception)
                      yield return exception;
                      """);
            else
                code.AppendLine(
                    $$"""
                      if (transientScope is {{_wellKnownTypes.IDisposable.FullName()}} disposable && {{DisposeSingularFullyQualified}}(disposable) is {{_wellKnownTypes.Exception.FullName()}} exception)
                      yield return exception;
                      """);
                    
            code.AppendLine("}");

        
            code.AppendLine(
                $$"""
                          for (var i = {{disposeParamReference}}.{{DisposableRangeInterfaceData.DisposablesPropertyName}}.{{nameof(List<List<object>>.Count)}} - 1; i >= 0; i--)
                          {
                              foreach (var exception in {{DisposeChunkFullyQualified}}(({{DisposableRangeInterfaceData.InterfaceNameFullyQualified}}) {{disposeParamReference}}, {{disposeParamReference}}.{{DisposableRangeInterfaceData.DisposablesPropertyName}}[i]))
                              {
                                  yield return exception;
                              }
                          }
                  """);
        
            if (!syncOnlyMode && _wellKnownTypes.IAsyncDisposable is not null && _wellKnownTypes.ValueTask is not null)
            {
                code.AppendLine(
                    $$"""
                      foreach (var disposable in {{disposeParamReference}}.{{DisposableRangeInterfaceData.UserDefinedAsyncDisposablesPropertyName}})
                      {
                          if ({{DisposeSingularAsyncSyncedFullyQualified}}(disposable) is {{_wellKnownTypes.Exception.FullName()}} exception)
                          {
                              yield return exception;
                          }
                      }
                      """);
            }
            code.AppendLine(
                $$"""
                  
                          foreach (var disposable in {{disposeParamReference}}.{{DisposableRangeInterfaceData.UserDefinedSyncDisposablesPropertyName}})
                          {
                              if ({{DisposeSingularFullyQualified}}(disposable) is {{_wellKnownTypes.Exception.FullName()}} exception)
                              {
                                  yield return exception;
                              }
                          }
                      }
                  }
                  """);
        }

        void GenerateSyncDisposeToException()
        {
            if (_disposeAsyncName is null) return;
            
            code.AppendLine(
                $$"""
                  internal static void {{_disposeName}}({{DisposableRangeInterfaceData.InterfaceNameFullyQualified}} {{disposeParamReference}}) =>
                      throw new {{_syncDisposalTriggeredExceptionName}}({{_disposeAsyncName}}({{disposeParamReference}}).AsTask());
                  """);
        }

        void GenerateAsyncDispose()
        {
            if (_wellKnownTypes.ValueTask is not null
                && _wellKnownTypes.IAsyncDisposable is not null
                && _wellKnownTypesCollections.IAsyncEnumerable1 is not null
                && _wellKnownTypes.IAsyncEnumerableOfException is not null
                && DisposeChunkAsyncFullyQualified is not null)
            {
                code.AppendLine(
                    $$"""
                      internal static async {{_wellKnownTypes.ValueTask.FullName()}} {{_disposeAsyncName}}({{DisposableRangeInterfaceData.InterfaceNameFullyQualified}} {{disposeParamReference}})
                      {
                      if (await {{AggregateExceptionRoutineAsyncFullyQualified}}(Inner()) is {} aggregateException) throw aggregateException;
                      return;

                      async {{_wellKnownTypes.IAsyncEnumerableOfException}} Inner()
                      {
                      foreach (var transientScope in {{disposeParamReference}}.{{DisposableRangeInterfaceData.TransientScopesPropertyName}})
                      {
                      if (transientScope is {{_wellKnownTypes.IAsyncDisposable.FullName()}} asyncDisposable && await {{DisposeSingularAsyncFullyQualified}}(asyncDisposable) is {{_wellKnownTypes.Exception.FullName()}} exception)
                      yield return exception;
                      }
                                for (var i = {{disposeParamReference}}.{{DisposableRangeInterfaceData.DisposablesPropertyName}}.{{nameof(List<List<object>>.Count)}} - 1; i >= 0; i--)
                                {
                                    await foreach (var exception in {{DisposeChunkAsyncFullyQualified}}({{disposeParamReference}}, {{disposeParamReference}}.{{DisposableRangeInterfaceData.DisposablesPropertyName}}[i]))
                                    {
                                        yield return exception;
                                    }
                                }
                                foreach (var disposable in {{disposeParamReference}}.{{DisposableRangeInterfaceData.UserDefinedAsyncDisposablesPropertyName}})
                                {
                                    if (await {{DisposeSingularAsyncFullyQualified}}(disposable) is {{_wellKnownTypes.Exception.FullName()}} exception)
                                    {
                                        yield return exception;
                                    }
                                }
                                foreach (var disposable in {{disposeParamReference}}.{{DisposableRangeInterfaceData.UserDefinedSyncDisposablesPropertyName}})
                                {
                                    if ({{DisposeSingularFullyQualified}}(disposable) is {{_wellKnownTypes.Exception.FullName()}} exception)
                                    {
                                        yield return exception;
                                    }
                                }
                            }
                        }
                      """);
            }
        }
    }
}