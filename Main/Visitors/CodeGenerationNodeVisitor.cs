using Microsoft.CodeAnalysis.CSharp;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.Delegates;
using MrMeeseeks.DIE.Nodes.Elements.Factories;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Elements.Tasks;
using MrMeeseeks.DIE.Nodes.Elements.Tuples;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.ResolutionBuilding.Function;

namespace MrMeeseeks.DIE.Visitors;

internal interface ICodeGenerationVisitor : INodeVisitor
{
    string GenerateContainerFile();
}

internal class CodeGenerationVisitor : ICodeGenerationVisitor
{
    private readonly StringBuilder _code = new();
    private readonly WellKnownTypes _wellKnownTypes;

    public CodeGenerationVisitor(WellKnownTypes wellKnownTypes)
    {
        _wellKnownTypes = wellKnownTypes;
    }

    public void VisitContainerNode(IContainerNode container)
    {
        var disposableImplementation = container.DisposalType.HasFlag(DisposalType.Async)
            ? $" , {_wellKnownTypes.AsyncDisposable.FullName()}"
            : $" , {_wellKnownTypes.AsyncDisposable.FullName()}, {_wellKnownTypes.Disposable.FullName()}";

        _code.AppendLine($$"""
#nullable enable
namespace {{container.Namespace}}
{
sealed partial class {{container.Name}} : {{container.TransientScopeInterface.FullName}}{{disposableImplementation}}
{
""");

        foreach (var entryFunctionNode in container.RootFunctions)
            VisitEntryFunctionNode(entryFunctionNode);

        GenerateRangeNodeContent(container);
        
        var dictionaryTypeName = container.DisposalType.HasFlag(DisposalType.Async)
            ? _wellKnownTypes.ConcurrentDictionaryOfAsyncDisposable.FullName()
            : _wellKnownTypes.ConcurrentDictionaryOfSyncDisposable.FullName();

        _code.AppendLine(
            $"private {dictionaryTypeName} {container.TransientScopeDisposalReference} = new {dictionaryTypeName}();");
        
        VisitTransientScopeInterfaceNode(container.TransientScopeInterface);
        
        foreach (var scope in container.Scopes)
            VisitScopeNode(scope);
        
        foreach (var transientScope in container.TransientScopes)
            VisitTransientScopeNode(transientScope);
        
        _code.AppendLine("""
}
}
#nullable disable
""");
    }

    public void VisitTransientScopeInterfaceNode(ITransientScopeInterfaceNode transientScopeInterface)
    {
        _code.AppendLine($$"""
private interface {{transientScopeInterface.Name}}
{
""");
        foreach (var rangedInstanceInterfaceFunctionNode in transientScopeInterface.Functions)
            VisitRangedInstanceInterfaceFunctionNode(rangedInstanceInterfaceFunctionNode);
        
        _code.AppendLine("}");
    }

    public void VisitScopeNode(IScopeNode scope)
    {
        var disposableImplementation = scope.DisposalType.HasFlag(DisposalType.Async) 
            ? $" : {_wellKnownTypes.AsyncDisposable.FullName()}" 
            : $" : {_wellKnownTypes.AsyncDisposable.FullName()}, {_wellKnownTypes.Disposable.FullName()}";
        
        _code.AppendLine($$"""
private sealed partial class {{scope.Name}}{{disposableImplementation}}
{
private readonly {{scope.ContainerFullName}} {{scope.ContainerReference}};
private readonly {{scope.TransientScopeInterfaceFullName}} {{scope.TransientScopeInterfaceReference}};
internal {{scope.Name}}({{scope.ContainerFullName}} {{scope.ContainerParameterReference}}, {{scope.TransientScopeInterfaceFullName}} {{scope.TransientScopeInterfaceParameterReference}})
{
{{scope.ContainerReference}} = {{scope.ContainerParameterReference}};
{{scope.TransientScopeInterfaceReference}} = {{scope.TransientScopeInterfaceParameterReference}};
}
""");

        GenerateRangeNodeContent(scope);

        _code.AppendLine("}");
    }

    public void VisitTransientScopeNode(ITransientScopeNode transientScope)
    {
        var disposableImplementation = transientScope.DisposalType.HasFlag(DisposalType.Async) 
            ? $", {_wellKnownTypes.AsyncDisposable.FullName()}" 
            : $", {_wellKnownTypes.AsyncDisposable.FullName()}, {_wellKnownTypes.Disposable.FullName()}";
        
        _code.AppendLine($$"""
private sealed partial class {{transientScope.Name}} : {{transientScope.TransientScopeInterfaceName}}{{disposableImplementation}}
{
private readonly {{transientScope.ContainerFullName}} {{transientScope.ContainerReference}};
internal {{transientScope.Name}}({{transientScope.ContainerFullName}} {{transientScope.ContainerParameterReference}})
{
{{transientScope.ContainerReference}} = {{transientScope.ContainerParameterReference}};
}
""");

        GenerateRangeNodeContent(transientScope);

        _code.AppendLine("}");
    }

    public void VisitScopeCallNode(IScopeCallNode scopeCall)
    {
        _code.AppendLine(
            $"{scopeCall.ScopeFullName} {scopeCall.ScopeReference} = new {scopeCall.ScopeFullName}({scopeCall.ContainerParameter}, {scopeCall.TransientScopeInterfaceParameter});");
        if (scopeCall.DisposalType.HasFlag(DisposalType.Async))
            _code.AppendLine($"{scopeCall.DisposableCollectionReference}.Add(({_wellKnownTypes.AsyncDisposable.FullName()}) {scopeCall.ScopeReference});");
        else if (scopeCall.DisposalType.HasFlag(DisposalType.Sync))
            _code.AppendLine($"{scopeCall.DisposableCollectionReference}.Add(({_wellKnownTypes.Disposable.FullName()}) {scopeCall.ScopeReference});");
        VisitFunctionCallNode(scopeCall);
    }

    public void VisitTransientScopeCallNode(ITransientScopeCallNode transientScopeCall)
    {
        _code.AppendLine(
            $"{transientScopeCall.TransientScopeFullName} {transientScopeCall.TransientScopeReference} = new {transientScopeCall.TransientScopeFullName}({transientScopeCall.ContainerParameter});");
        if (transientScopeCall.DisposalType is not DisposalType.None)
        {
            var disposalType = transientScopeCall.DisposalType.HasFlag(DisposalType.Async) 
                ? _wellKnownTypes.AsyncDisposable.FullName()
                : _wellKnownTypes.Disposable.FullName();
            var owner = transientScopeCall.ContainerReference is { } containerReference
                ? $"{containerReference}."
                : "";
            _code
                .AppendLine($"{owner}{transientScopeCall.TransientScopeDisposalReference}[{transientScopeCall.TransientScopeReference}] = ({disposalType}) {transientScopeCall.TransientScopeReference};");
        }
        VisitFunctionCallNode(transientScopeCall);
    }

    private void GenerateRangeNodeContent(IRangeNode rangeNode)
    {
        foreach (var createFunctionNode in rangeNode.CreateFunctions)
            VisitCreateFunctionNode(createFunctionNode);

        foreach (var rangedInstanceFunctionGroup in rangeNode.RangedInstanceFunctionGroups)
            VisitRangedInstanceFunctionGroupNode(rangedInstanceFunctionGroup);
        
        if (rangeNode is { AddForDisposal: true, DisposalHandling.SyncCollectionReference: { } syncCollectionReference })
            _code.AppendLine($$"""
private partial void {{Constants.UserDefinedAddForDisposal}}({{_wellKnownTypes.Disposable.FullName()}} disposable) =>
{{syncCollectionReference}}.Add(({{_wellKnownTypes.Disposable.FullName()}}) disposable);
""");

        if (rangeNode is { AddForDisposalAsync: true, DisposalHandling.AsyncCollectionReference: { } asyncCollectionReference })
            _code.AppendLine($$"""
private partial void {{Constants.UserDefinedAddForDisposal}}({{_wellKnownTypes.AsyncDisposable.FullName()}} asyncDisposable) =>
{{asyncCollectionReference}}.Add(({{_wellKnownTypes.AsyncDisposable.FullName()}}) asyncDisposable);
""");
        GenerateDisposalFunction(rangeNode);
    }

    private void GenerateDisposalFunction(
        IRangeNode range)
    {
        var disposalHandling = range.DisposalHandling;

        _code.AppendLine($$"""
private {{_wellKnownTypes.ConcurrentBagOfAsyncDisposable.FullName()}} {{disposalHandling.AsyncCollectionReference}} = new {{_wellKnownTypes.ConcurrentBagOfAsyncDisposable.FullName()}}();
private {{_wellKnownTypes.ConcurrentBagOfSyncDisposable.FullName()}} {{disposalHandling.SyncCollectionReference}} = new {{_wellKnownTypes.ConcurrentBagOfSyncDisposable.FullName()}}();
private int {{disposalHandling.DisposedFieldReference}} = 0;
private bool {{disposalHandling.DisposedPropertyReference}} => {{disposalHandling.DisposedFieldReference}} != 0;
""");
        
        // Async part

        _code.AppendLine($$"""
public async {{_wellKnownTypes.ValueTask.FullName()}} DisposeAsync()
{
var {{disposalHandling.DisposedLocalReference}} = global::System.Threading.Interlocked.Exchange(ref {{disposalHandling.DisposedFieldReference}}, 1);
if ({{disposalHandling.DisposedLocalReference}} != 0) return;
""");
        
        foreach (var rangedInstanceFunctionGroup in range.RangedInstanceFunctionGroups)
            _code.AppendLine($"await {rangedInstanceFunctionGroup.LockReference}.WaitAsync();");

        _code.AppendLine("""
try
{
""");

        TransientScopeDisposal();
        
        _code.AppendLine($$"""
foreach(var {{disposalHandling.DisposableLocalReference}} in {{disposalHandling.AsyncCollectionReference}})
{
try
{
await {{disposalHandling.DisposableLocalReference}}.DisposeAsync();
}
catch({{_wellKnownTypes.Exception.FullName()}})
{
// catch and ignore exceptions of individual disposals so the other disposals are triggered
}
}
{{disposalHandling.AsyncCollectionReference}}.Clear();
foreach(var {{disposalHandling.DisposableLocalReference}} in {{disposalHandling.SyncCollectionReference}})
{
try
{
{{disposalHandling.DisposableLocalReference}}.Dispose();
}
catch({{_wellKnownTypes.Exception.FullName()}})
{
// catch and ignore exceptions of individual disposals so the other disposals are triggered
}
}
{{disposalHandling.SyncCollectionReference}}.Clear();
}
finally
{
""");

        foreach (var rangedInstanceFunctionGroup in range.RangedInstanceFunctionGroups)
            _code.AppendLine($"{rangedInstanceFunctionGroup.LockReference}.Release();");

        _code.AppendLine("""
}
}
""");
        
        // Sync part

        if (!range.DisposalType.HasFlag(DisposalType.Async))
        {
            _code.AppendLine($$"""
public void Dispose()
{
var {{disposalHandling.DisposedLocalReference}} = global::System.Threading.Interlocked.Exchange(ref {{disposalHandling.DisposedFieldReference}}, 1);
if ({{disposalHandling.DisposedLocalReference}} != 0) return;
""");

            foreach (var rangedInstanceFunctionGroup in range.RangedInstanceFunctionGroups)
                _code.AppendLine($"{rangedInstanceFunctionGroup.LockReference}.Wait();");

            _code.AppendLine("""
try
{
""");

            TransientScopeDisposal();

            _code.AppendLine($$"""
foreach(var {{disposalHandling.DisposableLocalReference}} in {{disposalHandling.AsyncCollectionReference}})
{
try
{
({{disposalHandling.DisposableLocalReference}} as {{_wellKnownTypes.Disposable.FullName()}})?.Dispose();
}
catch({{_wellKnownTypes.Exception.FullName()}})
{
// catch and ignore exceptions of individual disposals so the other disposals are triggered
}
}
{{disposalHandling.AsyncCollectionReference}}.Clear();
foreach(var {{disposalHandling.DisposableLocalReference}} in {{disposalHandling.SyncCollectionReference}})
{
try
{
{{disposalHandling.DisposableLocalReference}}.Dispose();
}
catch({{_wellKnownTypes.Exception.FullName()}})
{
// catch and ignore exceptions of individual disposals so the other disposals are triggered
}
}
{{disposalHandling.SyncCollectionReference}}.Clear();
}
finally
{
""");

            foreach (var rangedInstanceFunctionGroup in range.RangedInstanceFunctionGroups)
                _code.AppendLine($"{rangedInstanceFunctionGroup.LockReference}.Release();");

            _code.AppendLine("""
}
}
""");
        }

        void TransientScopeDisposal()
        {
            switch (range)
            {
                case IContainerNode container:
                    string asyncSuffix = container.DisposalType.HasFlag(DisposalType.Async) ? "Async" : "";
                    string awaitPrefix = container.DisposalType.HasFlag(DisposalType.Async) ? "await " : "";
                    var elementName = container.TransientScopeDisposalElement;
                    _code.AppendLine($$"""
while ({{container.TransientScopeDisposalReference}}.Count > 0)
{
var {{elementName}} = global::System.Linq.Enumerable.FirstOrDefault({{container.TransientScopeDisposalReference}}.Keys);
if ({{elementName}} is not null && {{container.TransientScopeDisposalReference}}.TryRemove({{elementName}}, out _))
{
{{awaitPrefix}}{{elementName}}.Dispose{{asyncSuffix}}();
}
}
{{container.TransientScopeDisposalReference}}.Clear();
""");
                    break;
                case ITransientScopeNode transientScope:
                    var disposalType = transientScope.DisposalType.HasFlag(DisposalType.Async) 
                        ? _wellKnownTypes.AsyncDisposable.FullName() 
                        : _wellKnownTypes.Disposable.FullName();

                    _code.AppendLine(
                        $"{transientScope.ContainerReference}.{transientScope.TransientScopeDisposalReference}.TryRemove(({disposalType}) this, out _);");
                    break;
            }
        }
    }

    private void VisitSingleFunctionNode(ISingleFunctionNode singleFunction)
    {
        var accessibility = singleFunction is { Accessibility: { } acc, ExplicitInterfaceFullName: null }
            ? $"{SyntaxFacts.GetText(acc)} "  
            : "";
        var asyncModifier = singleFunction.SynchronicityDecision is SynchronicityDecision.AsyncTask or SynchronicityDecision.AsyncValueTask
            ? "async "
            : "";
        var explicitInterfaceFullName = singleFunction.ExplicitInterfaceFullName is { } interfaceName
            ? $"{interfaceName}."
            : "";
        var parameter = string.Join(",", singleFunction.Parameters.Select(r => $"{r.Item3.TypeFullName} {r.Item3.Reference}"));
        _code.AppendLine($$"""
{{accessibility}}{{asyncModifier}}{{explicitInterfaceFullName}}{{singleFunction.ReturnedTypeFullName}} {{singleFunction.Name}}({{parameter}})
{
""");
        ObjectDisposedCheck(
            singleFunction.DisposedPropertyReference, 
            singleFunction.RangeFullName, 
            singleFunction.ReturnedTypeFullName);
        VisitElementNode(singleFunction.ReturnedElement);
        ObjectDisposedCheck(
            singleFunction.DisposedPropertyReference, 
            singleFunction.RangeFullName, 
            singleFunction.ReturnedTypeFullName);
        _code.AppendLine($"return {singleFunction.ReturnedElement.Reference};");
            
        foreach (var localFunction in singleFunction.LocalFunctions)
            VisitSingleFunctionNode(localFunction);
        
        _code.AppendLine("}");
    }

    public void VisitCreateFunctionNode(ICreateFunctionNode createFunction) => VisitSingleFunctionNode(createFunction);
    public void VisitEntryFunctionNode(IEntryFunctionNode entryFunction) => VisitSingleFunctionNode(entryFunction);
    public void VisitLocalFunctionNode(ILocalFunctionNode localFunction) => VisitSingleFunctionNode(localFunction);
    public void VisitRangedInstanceFunctionNode(IRangedInstanceFunctionNode rangedInstanceFunctionNode)
    {
        // Nothing to do here. It's generated in "VisitRangedInstanceFunctionGroupNode"
    }

    public void VisitRangedInstanceInterfaceFunctionNode(IRangedInstanceInterfaceFunctionNode rangedInstanceInterfaceFunctionNode)
    {
        var parameter = string.Join(",", rangedInstanceInterfaceFunctionNode.Parameters.Select(r => $"{r.Item3.TypeFullName} {r.Item3.Reference}"));
        _code.AppendLine($"{rangedInstanceInterfaceFunctionNode.ReturnedTypeFullName} {rangedInstanceInterfaceFunctionNode.Name}({parameter});");
    }

    public void VisitRangedInstanceFunctionGroupNode(IRangedInstanceFunctionGroupNode rangedInstanceFunctionGroupNode)
    {
        var isRefType = rangedInstanceFunctionGroupNode.IsCreatedForStructs is null;
        _code.AppendLine($$"""
private {{rangedInstanceFunctionGroupNode.TypeFullName}}{{(isRefType ? "?" : "")}} {{rangedInstanceFunctionGroupNode.FieldReference}};
private {{_wellKnownTypes.SemaphoreSlim.FullName()}} {{rangedInstanceFunctionGroupNode.LockReference}} = new {{_wellKnownTypes.SemaphoreSlim.FullName()}}(1);
""");

        if (!isRefType) _code.AppendLine($"private bool {rangedInstanceFunctionGroupNode.IsCreatedForStructs};");

        foreach (var overload in rangedInstanceFunctionGroupNode.Overloads)
        {
            var isAsync =
                overload.SynchronicityDecision is SynchronicityDecision.AsyncTask or SynchronicityDecision.AsyncValueTask;
            var parameters = string.Join(", ",
                overload.Parameters.Select(p => $"{p.Item1} {p.Item3.Reference}"));
            _code.AppendLine(rangedInstanceFunctionGroupNode.Level == ScopeLevel.TransientScope && overload.ExplicitInterfaceFullName is {} explicitInterfaceFullName
                ? $"{(isAsync ? "async " : "")}{overload.ReturnedTypeFullName} {explicitInterfaceFullName}.{overload.Name}({parameters})"
                : $"{Constants.PrivateKeyword} {(isAsync ? "async " : "")}{overload.ReturnedTypeFullName} {overload.Name}({parameters})");

            var checkAndReturnAlreadyCreatedInstance = isRefType
                ? $"if (!object.ReferenceEquals({rangedInstanceFunctionGroupNode.FieldReference}, null)) return {rangedInstanceFunctionGroupNode.FieldReference};"
                : $"if ({rangedInstanceFunctionGroupNode.IsCreatedForStructs}) return {rangedInstanceFunctionGroupNode.FieldReference};";
            
            _code.AppendLine($$"""
{
{{checkAndReturnAlreadyCreatedInstance}}
{{(isAsync ? "await " : "")}}{{Constants.ThisKeyword}}.{{rangedInstanceFunctionGroupNode.LockReference}}.Wait{{(isAsync ? "Async" : "")}}();
try
{
""");
            
            ObjectDisposedCheck(
                overload.DisposedPropertyReference, 
                overload.RangeFullName, 
                overload.ReturnedTypeFullName);
            _code.AppendLine(checkAndReturnAlreadyCreatedInstance);
            
            VisitElementNode(overload.ReturnedElement);

            _code.AppendLine($"{rangedInstanceFunctionGroupNode.FieldReference} = {overload.ReturnedElement.Reference};");
            if (!isRefType) _code.AppendLine($"{rangedInstanceFunctionGroupNode.IsCreatedForStructs} = true;");
            _code.AppendLine($$"""
}
finally
{
{{Constants.ThisKeyword}}.{{rangedInstanceFunctionGroupNode.LockReference}}.Release();
}
""");
            
            ObjectDisposedCheck(
                overload.DisposedPropertyReference, 
                overload.RangeFullName, 
                overload.ReturnedTypeFullName); 
            _code.AppendLine($$"""
return {{Constants.ThisKeyword}}.{{rangedInstanceFunctionGroupNode.FieldReference}};
}
""");
        }
    }

    private void VisitFunctionCallNode(IFunctionCallNode functionCallNode)
    {
        var owner = functionCallNode.OwnerReference is { } ownerReference ? $"{ownerReference}." : ""; 
        var typeFullName = functionCallNode.Awaited
            ? functionCallNode.AsyncTypeFullName
            : functionCallNode.TypeFullName;
        var call = $"{owner}{functionCallNode.FunctionName}({string.Join(", ", functionCallNode.Parameters.Select(p => $"{p.Item1.Reference}: {p.Item2.Reference}"))})";
        call = functionCallNode.Awaited ? $"(await {call})" : call;
        _code.AppendLine($"{typeFullName} {functionCallNode.Reference} = ({typeFullName}){call};");
    }

    public void VisitPlainFunctionCallNode(IPlainFunctionCallNode plainFunctionCallNode) => VisitFunctionCallNode(plainFunctionCallNode);

    private void VisitFactoryNodeBase(IFactoryNodeBase factoryNode, string optionalParameters)
    {
        var typeFullName = factoryNode.Awaited
            ? factoryNode.AsyncTypeFullName
            : factoryNode.TypeFullName;
        var awaitPrefix = factoryNode.Awaited ? "await " : "";
        _code.AppendLine($"{typeFullName} {factoryNode.Reference} = ({typeFullName}){awaitPrefix}{factoryNode.Name}{optionalParameters};");
    }

    public void VisitFactoryFieldNode(IFactoryFieldNode factoryFieldNode)
    {
        VisitFactoryNodeBase(factoryFieldNode, "");
    }

    public void VisitFactoryPropertyNode(IFactoryPropertyNode factoryPropertyNode)
    {
        VisitFactoryNodeBase(factoryPropertyNode, "");
    }

    public void VisitFactoryFunctionNode(IFactoryFunctionNode factoryFunctionNode)
    {
        foreach (var (_, element) in factoryFunctionNode.Parameters)
            VisitElementNode(element);
        VisitFactoryNodeBase(factoryFunctionNode, $"({string.Join(", ", factoryFunctionNode.Parameters.Select(t => $"{t.Name}: {t.Element.Reference}"))})");
    }

    public void VisitFuncNode(IFuncNode funcNode) =>
        _code.AppendLine($"{funcNode.TypeFullName} {funcNode.Reference} = {funcNode.MethodGroup};");

    public void VisitLazyNode(ILazyNode lazyNode) => 
        _code.AppendLine($"{lazyNode.TypeFullName} {lazyNode.Reference} = new {lazyNode.TypeFullName}({lazyNode.MethodGroup});");

    public void VisitValueTaskNode(IValueTaskNode valueTaskNode)
    {
        VisitElementNode(valueTaskNode.WrappedElement);
        switch (valueTaskNode.Strategy)
        {
            case AsyncWrappingStrategy.VanillaFromResult:
                _code.AppendLine($"{valueTaskNode.TypeFullName} {valueTaskNode.Reference} = new {valueTaskNode.TypeFullName}({valueTaskNode.WrappedElement.Reference});");    
                break;
            case AsyncWrappingStrategy.ImplementationFromValueTask:
                _code.AppendLine($$"""
{{valueTaskNode.TypeFullName}} {{valueTaskNode.Reference}} = new {{valueTaskNode.TypeFullName}}({{valueTaskNode.AsyncReference}}.AsTask().ContinueWith(t =>
{
if (t.IsCompletedSuccessfully) return {{valueTaskNode.WrappedElement.Reference}};
if (t.IsFaulted && t.Exception is { }) throw t.Exception;
if (t.IsCanceled) throw new {{_wellKnownTypes.TaskCanceledException.FullName()}}(t);
throw new {{_wellKnownTypes.Exception.FullName()}}("[DIE] Something unexpected.");
}));
"""); // todo mark last exception as from DIE and give it unique GUID
                break;
            case AsyncWrappingStrategy.ImplementationFromTask:
                _code
                    .AppendLine($$"""
{{valueTaskNode.TypeFullName}} {{valueTaskNode.Reference}} = new {{valueTaskNode.TypeFullName}}({{valueTaskNode.AsyncReference}}.ContinueWith(t =>
{
if (t.IsCompletedSuccessfully) return {{valueTaskNode.WrappedElement.Reference}};
if (t.IsFaulted && t.Exception is { }) throw t.Exception;
if (t.IsCanceled) throw new {{_wellKnownTypes.TaskCanceledException.FullName()}}(t);
throw new {{_wellKnownTypes.Exception.FullName()}}("[DIE] Something unexpected.");
}));
"""); // todo mark last exception as from DIE and give it unique GUID
                break;
            case AsyncWrappingStrategy.FactoryFromValueTask:
            case AsyncWrappingStrategy.CallFromValueTask:
                _code.AppendLine($"{valueTaskNode.TypeFullName} {valueTaskNode.Reference} = {valueTaskNode.WrappedElement.Reference};");
                break;
            case AsyncWrappingStrategy.FactoryFromTask:
            case AsyncWrappingStrategy.CallFromTask:
                _code.AppendLine($"{valueTaskNode.TypeFullName} {valueTaskNode.Reference} = new {valueTaskNode.TypeFullName}({valueTaskNode.WrappedElement.Reference});");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void VisitTaskNode(ITaskNode taskNode)
    {
        VisitElementNode(taskNode.WrappedElement);
        switch (taskNode.Strategy)
        {
            case AsyncWrappingStrategy.VanillaFromResult:
                _code.AppendLine($"{taskNode.TypeFullName} {taskNode.Reference} = {_wellKnownTypes.Task.FullName()}.FromResult({taskNode.WrappedElement.Reference});");   
                break;
            case AsyncWrappingStrategy.ImplementationFromValueTask:
                _code.AppendLine($$"""
{{taskNode.TypeFullName}} {{taskNode.Reference}} = {{taskNode.AsyncReference}}.AsTask().ContinueWith(t =>
{
if (t.IsCompletedSuccessfully) return {{taskNode.WrappedElement.Reference}};
if (t.IsFaulted && t.Exception is { }) throw t.Exception;
if (t.IsCanceled) throw new {{_wellKnownTypes.TaskCanceledException.FullName()}}(t);
throw new {{_wellKnownTypes.Exception.FullName()}}("[DIE] Something unexpected.");
});
"""); // todo mark last exception as from DIE and give it unique GUID
                break;
            case AsyncWrappingStrategy.ImplementationFromTask:
                _code
                    .AppendLine($$"""
{{taskNode.TypeFullName}} {{taskNode.Reference}} = {{taskNode.AsyncReference}}.ContinueWith(t =>
{
if (t.IsCompletedSuccessfully) return {{taskNode.WrappedElement.Reference}};
if (t.IsFaulted && t.Exception is { }) throw t.Exception;
if (t.IsCanceled) throw new {{_wellKnownTypes.TaskCanceledException.FullName()}}(t);
throw new {{_wellKnownTypes.Exception.FullName()}}("[DIE] Something unexpected.");
});
"""); // todo mark last exception as from DIE and give it unique GUID
                break;
            case AsyncWrappingStrategy.FactoryFromValueTask:
            case AsyncWrappingStrategy.CallFromValueTask:
                _code.AppendLine($"{taskNode.TypeFullName} {taskNode.Reference} = {taskNode.WrappedElement.Reference}.AsTask();");
                break;
            case AsyncWrappingStrategy.FactoryFromTask:
            case AsyncWrappingStrategy.CallFromTask:
                _code.AppendLine($"{taskNode.TypeFullName} {taskNode.Reference} = {taskNode.WrappedElement.Reference};");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void VisitTupleNode(ITupleNode tupleNode)
    {
        foreach (var parameter in tupleNode.Parameters)
            VisitElementNode(parameter.Node);
        _code.AppendLine(
            $"{tupleNode.TypeFullName} {tupleNode.Reference} = new {tupleNode.TypeFullName}({string.Join(", ", tupleNode.Parameters.Select(p => $"{p.Name}: {p.Node.Reference}"))});");
    }

    public void VisitValueTupleNode(IValueTupleNode valueTupleNode)
    {
        foreach (var parameter in valueTupleNode.Parameters)
            VisitElementNode(parameter.Node);
        _code.AppendLine(
            $"{valueTupleNode.TypeFullName} {valueTupleNode.Reference} = new {valueTupleNode.TypeFullName}({string.Join(", ", valueTupleNode.Parameters.Select(p => $"{p.Name}: {p.Node.Reference}"))});");
    }

    public void VisitValueTupleSyntaxNode(IValueTupleSyntaxNode valueTupleSyntaxNode)
    {
        foreach (var item in valueTupleSyntaxNode.Items)
        {
            VisitElementNode(item);
        }
        _code.AppendLine($"{valueTupleSyntaxNode.TypeFullName} {valueTupleSyntaxNode.Reference} = ({string.Join(", ", valueTupleSyntaxNode.Items.Select(d => d.Reference))});");
    }

    private void VisitElementNode(IElementNode elementNode)
    {
        if (elementNode is IPlainFunctionCallNode createCallNode)
            VisitPlainFunctionCallNode(createCallNode);
        if (elementNode is IScopeCallNode scopeCallNode)
            VisitScopeCallNode(scopeCallNode);
        if (elementNode is ITransientScopeCallNode transientScopeCallNode)
            VisitTransientScopeCallNode(transientScopeCallNode);
        if (elementNode is IParameterNode parameterNode)
            VisitParameterNode(parameterNode);
        if (elementNode is IOutParameterNode outParameterNode)
            VisitOutParameterNode(outParameterNode);
        if (elementNode is IFactoryFieldNode factoryFieldNode)
            VisitFactoryFieldNode(factoryFieldNode);
        if (elementNode is IFactoryFunctionNode factoryFunctionNode)
            VisitFactoryFunctionNode(factoryFunctionNode);
        if (elementNode is IFactoryPropertyNode factoryPropertyNode)
            VisitFactoryPropertyNode(factoryPropertyNode);
        if (elementNode is IFuncNode funcNode)
            VisitFuncNode(funcNode);
        if (elementNode is ILazyNode lazyNode)
            VisitLazyNode(lazyNode);
        if (elementNode is IValueTaskNode valueTaskNode)
            VisitValueTaskNode(valueTaskNode);
        if (elementNode is ITaskNode taskNode)
            VisitTaskNode(taskNode);
        if (elementNode is ITupleNode tupleNode)
            VisitTupleNode(tupleNode);
        if (elementNode is IValueTupleNode valueTupleNode)
            VisitValueTupleNode(valueTupleNode);
        if (elementNode is IValueTupleSyntaxNode valueTupleSyntaxNode)
            VisitValueTupleSyntaxNode(valueTupleSyntaxNode);
        if (elementNode is ICollectionNode collectionNode)
            VisitCollectionNode(collectionNode);
        if (elementNode is IAbstractionNode abstractionNode)
            VisitAbstractionNode(abstractionNode);
        if (elementNode is IImplementationNode implementationNode)
            VisitImplementationNode(implementationNode);
        if (elementNode is ITransientScopeDisposalTriggerNode transientScopeDisposalTriggerNode)
            VisitTransientScopeDisposalTriggerNode(transientScopeDisposalTriggerNode);
    }

    public void VisitCollectionNode(ICollectionNode collectionNode)
    {
        foreach (var collectionNodeItem in collectionNode.Items)
        {
            VisitElementNode(collectionNodeItem);
        }
        _code.AppendLine(
            $"{collectionNode.TypeFullName} {collectionNode.Reference} = new {collectionNode.ItemTypeFullName}[]{{{string.Join(", ", collectionNode.Items.Select(i => $"({collectionNode.ItemTypeFullName}) {i.Reference}"))}}};");
    }

    public void VisitImplementationNode(IImplementationNode implementationNode)
    {
        if (implementationNode.UserDefinedInjectionConstructor is {})
            ProcessUserDefinedInjection(implementationNode.UserDefinedInjectionConstructor);
        if (implementationNode.UserDefinedInjectionProperties is {})
            ProcessUserDefinedInjection(implementationNode.UserDefinedInjectionProperties);
        foreach (var (_, element) in implementationNode.ConstructorParameters)
            VisitElementNode(element);
        foreach (var (_, element)  in implementationNode.Properties)
            VisitElementNode(element);
        var objectInitializerParameter = implementationNode.Properties.Any()
            ? $" {{ {string.Join(", ", implementationNode.Properties.Select(p => $"{p.Name} = {p.Element.Reference}"))} }}"
            : "";
        var constructorParameters =
            string.Join(", ", implementationNode.ConstructorParameters.Select(d => $"{d.Name}: {d.Element.Reference}"));
        _code.AppendLine(
            $"{implementationNode.TypeFullName} {implementationNode.Reference} = new {implementationNode.ConstructorCallName}({constructorParameters}){objectInitializerParameter};");
        
        if (implementationNode.SyncDisposalCollectionReference is {} syncDisposalCollectionReference)
            _code.AppendLine(
                $"{syncDisposalCollectionReference}.Add(({_wellKnownTypes.Disposable.FullName()}) {implementationNode.Reference});");
        if (implementationNode.AsyncDisposalCollectionReference is {} asyncDisposalCollectionReference)
            _code.AppendLine(
                $"{asyncDisposalCollectionReference}.Add(({_wellKnownTypes.AsyncDisposable.FullName()}) {implementationNode.Reference});");

        if (implementationNode.Initializer is {} init)
        {
            if (init.UserDefinedInjection is {})
                ProcessUserDefinedInjection(init.UserDefinedInjection);
            foreach (var (_, element) in init.Parameters)
                VisitElementNode(element);
            var initializerParameters =
                string.Join(", ", init.Parameters.Select(d => $"{d.Name}: {d.Element.Reference}"));

            var prefix = implementationNode.Awaited
                ? "await "
                : implementationNode is { AsyncReference: { } asyncReference, AsyncTypeFullName: { } asyncTypeFullName }
                    ? $"{asyncTypeFullName} {asyncReference} = "
                    : "";

            _code.AppendLine($"{prefix}(({init.TypeFullName}) {implementationNode.Reference}).{init.MethodName}({initializerParameters});");
        }

        void ProcessUserDefinedInjection(ImplementationNode.UserDefinedInjection userDefinedInjection)
        {
            foreach (var (_, element, _) in userDefinedInjection.Parameters)
                VisitElementNode(element);
            _code.AppendLine(
                $"{userDefinedInjection.Name}({string.Join(", ", userDefinedInjection.Parameters.Select(p => $"{p.Name}: {(p.IsOut ? "out var " : "")} {p.Element.Reference}"))});");
        }
    }

    public void VisitParameterNode(IParameterNode parameterNode)
    {
        // Processing is done in associated function node
    }

    public void VisitOutParameterNode(IOutParameterNode outParameterNode)
    {
        // Processing is done in associated implementation node
    }

    public void VisitAbstractionNode(IAbstractionNode abstractionNode)
    {
        VisitElementNode(abstractionNode.Implementation);
        _code.AppendLine($"{abstractionNode.TypeFullName} {abstractionNode.Reference} = ({abstractionNode.TypeFullName}) {abstractionNode.Implementation.Reference};");
    }

    public void VisitTransientScopeDisposalTriggerNode(ITransientScopeDisposalTriggerNode transientScopeDisposalTriggerNode) => _code.AppendLine(
        $"{transientScopeDisposalTriggerNode.TypeFullName} {transientScopeDisposalTriggerNode.Reference} = {Constants.ThisKeyword} as {transientScopeDisposalTriggerNode.TypeFullName};");

    public string GenerateContainerFile() => _code.ToString();

    private void ObjectDisposedCheck(
        string disposedPropertyReference,
        string rangeFullName,
        string returnTypeFullName) => _code.AppendLine(
        $"if ({disposedPropertyReference}) throw new {_wellKnownTypes.ObjectDisposedException}(\"{rangeFullName}\", $\"[DIE] This scope \\\"{rangeFullName}\\\" is already disposed, so it can't create a \\\"{returnTypeFullName}\\\" instance anymore.\");");
}