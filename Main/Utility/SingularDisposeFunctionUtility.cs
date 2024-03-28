using System.Threading;
using System.Threading.Tasks;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Utility;

internal interface ISingularDisposeFunctionUtility
{
    string AggregateExceptionRoutineFullyQualified { get; }
    string? AggregateExceptionRoutineAsyncFullyQualified { get; }
    string DisposeFullyQualified { get; }
    string? DisposeAsyncFullyQualified { get; }
    string? DisposeAsyncSyncedFullyQualified { get; }
    string ClassName { get; }
    string GenerateSingularDisposeFunctionsFile();
}

internal sealed class SingularDisposeFunctionUtility : ISingularDisposeFunctionUtility, IContainerInstance
{
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly string _disposeName;
    private readonly string? _disposeAsyncName;
    private readonly string? _disposeAsyncSyncedName;
    private readonly string _aggregateExceptionRoutine;
    private readonly string? _aggregateExceptionRoutineAsync;

    internal SingularDisposeFunctionUtility(
        IReferenceGenerator referenceGenerator,
        WellKnownTypes wellKnownTypes)
    {
        _wellKnownTypes = wellKnownTypes;
        ClassName = referenceGenerator.Generate("DisposeUtility");
        
        _disposeName = referenceGenerator.Generate("Dispose");
        DisposeFullyQualified = $"{Constants.NamespaceForGeneratedStatics}.{ClassName}.{_disposeName}";
        
        _disposeAsyncName = wellKnownTypes.IAsyncDisposable is not null && _wellKnownTypes.ValueTask is not null
            ? referenceGenerator.Generate("DisposeAsync")
            : null;
        DisposeAsyncFullyQualified = _disposeAsyncName is not null
            ? $"{Constants.NamespaceForGeneratedStatics}.{ClassName}.{_disposeAsyncName}"
            : null;
        
        _disposeAsyncSyncedName = wellKnownTypes.IAsyncDisposable is not null && _wellKnownTypes.ValueTask is not null
            ? referenceGenerator.Generate("DisposeAsyncSynced")
            : null;
        DisposeAsyncSyncedFullyQualified = _disposeAsyncSyncedName is not null
            ? $"{Constants.NamespaceForGeneratedStatics}.{ClassName}.{_disposeAsyncSyncedName}"
            : null;
        
        _aggregateExceptionRoutine = referenceGenerator.Generate("AggregateExceptionRoutine");
        AggregateExceptionRoutineFullyQualified = $"{Constants.NamespaceForGeneratedStatics}.{ClassName}.{_aggregateExceptionRoutine}";
        
        _aggregateExceptionRoutineAsync = wellKnownTypes.IAsyncEnumerableOfException is not null && _wellKnownTypes.ValueTask is not null
            ? referenceGenerator.Generate("AggregateExceptionRoutineAsync")
            : null;
        AggregateExceptionRoutineAsyncFullyQualified = _aggregateExceptionRoutineAsync is not null 
            ? $"{Constants.NamespaceForGeneratedStatics}.{ClassName}.{_aggregateExceptionRoutineAsync}"
            : null;
    }

    public string AggregateExceptionRoutineFullyQualified { get; }
    public string? AggregateExceptionRoutineAsyncFullyQualified { get; }
    public string DisposeFullyQualified { get; }
    public string? DisposeAsyncFullyQualified { get; }
    public string? DisposeAsyncSyncedFullyQualified { get; }
    public string ClassName { get; }

    public string GenerateSingularDisposeFunctionsFile()
    {
        var code = new StringBuilder();
        code.AppendLine(
            $$"""
              #nullable enable
              namespace {{Constants.NamespaceForGeneratedStatics}}
              {
                  internal static class {{ClassName}}
                  {
                      public static {{_wellKnownTypes.Exception.FullName()}}? {{_disposeName}}({{_wellKnownTypes.IDisposable.FullName()}} disposable)
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
        
        if (_disposeAsyncName is not null && _wellKnownTypes.IAsyncDisposable is not null && _wellKnownTypes.ValueTask is not null)
        {
            code.AppendLine(
                $$"""
                  public static async {{_wellKnownTypes.ValueTask.FullName()}}<{{_wellKnownTypes.Exception.FullName()}}?> {{_disposeAsyncName}}({{_wellKnownTypes.IAsyncDisposable.FullName()}} disposable)
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

        if (_disposeAsyncSyncedName is not null && _wellKnownTypes.IAsyncDisposable is not null && _wellKnownTypes.ValueTask is not null)
        {
            code.AppendLine(
                $$"""
                  public static {{_wellKnownTypes.Exception.FullName()}}? {{_disposeAsyncSyncedName}}({{_wellKnownTypes.IAsyncDisposable.FullName()}} disposable)
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
              public static {{_wellKnownTypes.AggregateException.FullName()}}? {{_aggregateExceptionRoutine}}({{_wellKnownTypes.IEnumerableOfException.FullName()}} exceptions)
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
                  public static async {{_wellKnownTypes.TaskOfNullableAggregateException.FullName()}} {{_aggregateExceptionRoutineAsync}}({{_wellKnownTypes.IAsyncEnumerableOfException.FullName()}} exceptions)
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
    }
}