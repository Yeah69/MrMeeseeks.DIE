using Microsoft.CodeAnalysis.CSharp;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.Delegates;
using MrMeeseeks.DIE.Nodes.Elements.Factories;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Elements.Tasks;
using MrMeeseeks.DIE.Nodes.Elements.Tuples;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Visitors;

internal interface ICodeGenerationVisitor : INodeVisitor
{
    string GenerateContainerFile();
}

internal class CodeGenerationVisitor : ICodeGenerationVisitor
{
    private readonly StringBuilder _code = new();
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly WellKnownTypesCollections _wellKnownTypesCollections;

    public CodeGenerationVisitor(
        IContainerWideContext containerWideContext)
    {
        _wellKnownTypes = containerWideContext.WellKnownTypes;
        _wellKnownTypesCollections = containerWideContext.WellKnownTypesCollections;
    }

    public void VisitContainerNode(IContainerNode container)
    {
        var disposableImplementation = container.DisposalType.HasFlag(DisposalType.Async)
            ? $" , {_wellKnownTypes.IAsyncDisposable.FullName()}"
            : $" , {_wellKnownTypes.IAsyncDisposable.FullName()}, {_wellKnownTypes.IDisposable.FullName()}";

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
        
        UngenericToGenericTransformationFunction(container.TaskTransformationFunctions.UngenericValueTaskToGenericValueTask);
        UngenericToGenericTransformationFunction(container.TaskTransformationFunctions.UngenericValueTaskToGenericTask);
        UngenericToGenericTransformationFunction(container.TaskTransformationFunctions.UngenericTaskToGenericTask);
        UngenericToGenericTransformationFunction(container.TaskTransformationFunctions.UngenericTaskToGenericValueTask);
        
        GenericToGenericTransformationFunction(container.TaskTransformationFunctions.GenericValueTaskToGenericTask);
        GenericToGenericTransformationFunction(container.TaskTransformationFunctions.GenericTaskToGenericValueTask);
        
        _code.AppendLine("""
}
}
#nullable disable
""");

        void UngenericToGenericTransformationFunction(UngenericToGenericData data)
        {
            _code.AppendLine($$"""
private static async {{data.ReturnTypeFullName}} {{data.FunctionName}}<T>({{data.UngenericParameterTypeFullName}} {{data.UngenericParameterName}}, T {{data.ResultParameterName}})
{
await {{data.UngenericParameterName}};
return {{data.ResultParameterName}};
}
""");
        }

        void GenericToGenericTransformationFunction(GenericToGenericData data) => _code.AppendLine(
            $"private static async {data.ReturnTypeFullName} {data.FunctionName}<T>({data.ParameterTypeFullName} {data.ParameterName}) => await {data.ParameterName};");
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
            ? $" : {_wellKnownTypes.IAsyncDisposable.FullName()}" 
            : $" : {_wellKnownTypes.IAsyncDisposable.FullName()}, {_wellKnownTypes.IDisposable.FullName()}";
        
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
            ? $", {_wellKnownTypes.IAsyncDisposable.FullName()}" 
            : $", {_wellKnownTypes.IAsyncDisposable.FullName()}, {_wellKnownTypes.IDisposable.FullName()}";
        
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
            _code.AppendLine($"{scopeCall.DisposableCollectionReference}.Add(({_wellKnownTypes.IAsyncDisposable.FullName()}) {scopeCall.ScopeReference});");
        else if (scopeCall.DisposalType.HasFlag(DisposalType.Sync))
            _code.AppendLine($"{scopeCall.DisposableCollectionReference}.Add(({_wellKnownTypes.IDisposable.FullName()}) {scopeCall.ScopeReference});");
        VisitFunctionCallNode(scopeCall);
    }

    public void VisitTransientScopeCallNode(ITransientScopeCallNode transientScopeCall)
    {
        _code.AppendLine(
            $"{transientScopeCall.TransientScopeFullName} {transientScopeCall.TransientScopeReference} = new {transientScopeCall.TransientScopeFullName}({transientScopeCall.ContainerParameter});");
        if (transientScopeCall.DisposalType is not DisposalType.None)
        {
            var disposalType = transientScopeCall.DisposalType.HasFlag(DisposalType.Async) 
                ? _wellKnownTypes.IAsyncDisposable.FullName()
                : _wellKnownTypes.IDisposable.FullName();
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

        foreach (var multiFunctionNode in rangeNode.MultiFunctions)
            VisitMultiFunctionNode(multiFunctionNode);
        
        if (rangeNode is { AddForDisposal: true, DisposalHandling.SyncCollectionReference: { } syncCollectionReference })
            _code.AppendLine($$"""
private partial void {{Constants.UserDefinedAddForDisposal}}({{_wellKnownTypes.IDisposable.FullName()}} disposable) =>
{{syncCollectionReference}}.Add(({{_wellKnownTypes.IDisposable.FullName()}}) disposable);
""");

        if (rangeNode is { AddForDisposalAsync: true, DisposalHandling.AsyncCollectionReference: { } asyncCollectionReference })
            _code.AppendLine($$"""
private partial void {{Constants.UserDefinedAddForDisposalAsync}}({{_wellKnownTypes.IAsyncDisposable.FullName()}} asyncDisposable) =>
{{asyncCollectionReference}}.Add(({{_wellKnownTypes.IAsyncDisposable.FullName()}}) asyncDisposable);
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
        
        GenerateDisposalFunctionInner(DisposalType.Async);
        
        // Sync part

        if (!range.DisposalType.HasFlag(DisposalType.Async))
        {
            GenerateDisposalFunctionInner(DisposalType.Sync);
        }

        void GenerateDisposalFunctionInner(DisposalType type)
        {
            var functionNameSuffix = type is DisposalType.Async
                ? "Async"
                : "";
            
            var returnType = type is DisposalType.Async
                ? $"async {_wellKnownTypes.ValueTask.FullName()}"
                : "void";
            
            var awaitPrefix = type is DisposalType.Async
                ? "await "
                : "";

            var asyncDisposalInstruction = type is DisposalType.Async
                ? $"await {disposalHandling.DisposableLocalReference}.DisposeAsync();"
                : $"({disposalHandling.DisposableLocalReference} as {_wellKnownTypes.IDisposable.FullName()})?.Dispose();";

            var listOfExceptionsFullName = _wellKnownTypesCollections.List1.Construct(_wellKnownTypes.Exception).FullName();

            _code.AppendLine($$"""
public {{returnType}} Dispose{{functionNameSuffix}}()
{
var {{disposalHandling.DisposedLocalReference}} = global::System.Threading.Interlocked.Exchange(ref {{disposalHandling.DisposedFieldReference}}, 1);
if ({{disposalHandling.DisposedLocalReference}} != 0) return;
{{listOfExceptionsFullName}} {{disposalHandling.AggregateExceptionReference}} = new {{listOfExceptionsFullName}}();
""");

            foreach (var rangedInstanceFunctionGroup in range.RangedInstanceFunctionGroups)
                _code.AppendLine($"{awaitPrefix}{rangedInstanceFunctionGroup.LockReference}.Wait{functionNameSuffix}();");

            _code.AppendLine("""
try
{
""");

            switch (range)
            {
                case IContainerNode container:
                    var elementName = container.TransientScopeDisposalElement;
                    var asyncTransientScopeDisposalInstruction = type is DisposalType.Async && container.DisposalType is DisposalType.Async
                        ? $"await {elementName}.DisposeAsync();"
                        : $"({elementName} as {_wellKnownTypes.IDisposable.FullName()})?.Dispose();";
                    _code.AppendLine($$"""
while ({{container.TransientScopeDisposalReference}}.Count > 0)
{
var {{elementName}} = {{_wellKnownTypesCollections.Enumerable}}.FirstOrDefault({{container.TransientScopeDisposalReference}}.Keys);
if ({{elementName}} is not null && {{container.TransientScopeDisposalReference}}.TryRemove({{elementName}}, out _))
{
try
{
{{asyncTransientScopeDisposalInstruction}}
}
catch({{_wellKnownTypes.Exception.FullName()}} {{disposalHandling.AggregateExceptionItemReference}})
{
// catch and aggregate so other disposals are triggered
{{disposalHandling.AggregateExceptionReference}}.Add({{disposalHandling.AggregateExceptionItemReference}});
}
}
}
{{container.TransientScopeDisposalReference}}.Clear();
""");
                    break;
                case ITransientScopeNode transientScope:
                    var disposalType = transientScope.DisposalType.HasFlag(DisposalType.Async) 
                        ? _wellKnownTypes.IAsyncDisposable.FullName() 
                        : _wellKnownTypes.IDisposable.FullName();

                    _code.AppendLine(
                        $"{transientScope.ContainerReference}.{transientScope.TransientScopeDisposalReference}.TryRemove(({disposalType}) this, out _);");
                    break;
            }

            _code.AppendLine($$"""
while({{disposalHandling.AsyncCollectionReference}}.Count > 0 && {{disposalHandling.AsyncCollectionReference}}.TryTake(out var {{disposalHandling.DisposableLocalReference}}))
{
try
{
{{asyncDisposalInstruction}}
}
catch({{_wellKnownTypes.Exception.FullName()}} {{disposalHandling.AggregateExceptionItemReference}})
{
// catch and aggregate so other disposals are triggered
{{disposalHandling.AggregateExceptionReference}}.Add({{disposalHandling.AggregateExceptionItemReference}});
}
}
while({{disposalHandling.SyncCollectionReference}}.Count > 0 && {{disposalHandling.SyncCollectionReference}}.TryTake(out var {{disposalHandling.DisposableLocalReference}}))
{
try
{
{{disposalHandling.DisposableLocalReference}}.Dispose();
}
catch({{_wellKnownTypes.Exception.FullName()}} {{disposalHandling.AggregateExceptionItemReference}})
{
// catch and aggregate so other disposals are triggered
{{disposalHandling.AggregateExceptionReference}}.Add({{disposalHandling.AggregateExceptionItemReference}});
}
}
}
finally
{
""");

            foreach (var rangedInstanceFunctionGroup in range.RangedInstanceFunctionGroups)
                _code.AppendLine($"{rangedInstanceFunctionGroup.LockReference}.Release();");

            _code.AppendLine($$"""
}
if ({{disposalHandling.AggregateExceptionReference}}.Count == 1) throw {{disposalHandling.AggregateExceptionReference}}[0];
else if ({{disposalHandling.AggregateExceptionReference}}.Count > 1) throw new {{_wellKnownTypes.AggregateException}}({{disposalHandling.AggregateExceptionReference}});
}
""");
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
        var parameter = string.Join(",", singleFunction.Parameters.Select(r => $"{r.Node.TypeFullName} {r.Node.Reference}"));
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

    public void VisitCreateFunctionNode(ICreateFunctionNodeBase createFunction) => VisitSingleFunctionNode(createFunction);
    public void VisitEntryFunctionNode(IEntryFunctionNode entryFunction) => VisitSingleFunctionNode(entryFunction);
    public void VisitLocalFunctionNode(ILocalFunctionNode localFunction) => VisitSingleFunctionNode(localFunction);
    public void VisitRangedInstanceFunctionNode(IRangedInstanceFunctionNode rangedInstanceFunctionNode)
    {
        // Nothing to do here. It's generated in "VisitRangedInstanceFunctionGroupNode"
    }

    public void VisitRangedInstanceInterfaceFunctionNode(IRangedInstanceInterfaceFunctionNode rangedInstanceInterfaceFunctionNode)
    {
        var parameter = string.Join(",", rangedInstanceInterfaceFunctionNode.Parameters.Select(r => $"{r.Node.TypeFullName} {r.Node.Reference}"));
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
                overload.Parameters.Select(p => $"{p.Node.TypeFullName} {p.Node.Reference}"));
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
""");
            foreach (var localFunction in overload.LocalFunctions)
                VisitSingleFunctionNode(localFunction);
            _code.AppendLine("}");
        }
    }

    private void VisitFunctionCallNode(IFunctionCallNode functionCallNode)
    {
        var owner = functionCallNode.OwnerReference is { } ownerReference ? $"{ownerReference}." : ""; 
        var typeFullName = functionCallNode.Awaited
            ? functionCallNode.AsyncTypeFullName
            : functionCallNode.TypeFullName;
        var call = $"{owner}{functionCallNode.FunctionName}({string.Join(", ", functionCallNode.Parameters.Select(p => $"{p.Item1.Reference.PrefixAtIfKeyword()}: {p.Item2.Reference}"))})";
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
        VisitFactoryNodeBase(factoryFunctionNode, $"({string.Join(", ", factoryFunctionNode.Parameters.Select(t => $"{t.Name.PrefixAtIfKeyword()}: {t.Element.Reference}"))})");
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
                _code.AppendLine($"{valueTaskNode.TypeFullName} {valueTaskNode.Reference} = {valueTaskNode.ContainerTypeFullName}.{valueTaskNode.TaskTransformationFunctions.UngenericValueTaskToGenericValueTask.FunctionName}({valueTaskNode.AsyncReference}, {valueTaskNode.WrappedElement.Reference});");
                break;
            case AsyncWrappingStrategy.ImplementationFromTask:
                _code.AppendLine($"{valueTaskNode.TypeFullName} {valueTaskNode.Reference} = {valueTaskNode.ContainerTypeFullName}.{valueTaskNode.TaskTransformationFunctions.UngenericTaskToGenericValueTask.FunctionName}({valueTaskNode.AsyncReference}, {valueTaskNode.WrappedElement.Reference});");
                break;
            case AsyncWrappingStrategy.CollectionFromValueTask:
            case AsyncWrappingStrategy.FactoryFromValueTask:
            case AsyncWrappingStrategy.CallFromValueTask:
                _code.AppendLine($"{valueTaskNode.TypeFullName} {valueTaskNode.Reference} = {valueTaskNode.WrappedElement.Reference};");
                break;
            case AsyncWrappingStrategy.CollectionFromTask:
            case AsyncWrappingStrategy.FactoryFromTask:
            case AsyncWrappingStrategy.CallFromTask:
                _code.AppendLine($"{valueTaskNode.TypeFullName} {valueTaskNode.Reference} = {valueTaskNode.ContainerTypeFullName}.{valueTaskNode.TaskTransformationFunctions.GenericTaskToGenericValueTask.FunctionName}({valueTaskNode.WrappedElement.Reference});");
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
                _code.AppendLine($"{taskNode.TypeFullName} {taskNode.Reference} = {taskNode.ContainerTypeFullName}.{taskNode.TaskTransformationFunctions.UngenericValueTaskToGenericTask.FunctionName}({taskNode.AsyncReference}, {taskNode.WrappedElement.Reference});");
                break;
            case AsyncWrappingStrategy.ImplementationFromTask:
                _code.AppendLine($"{taskNode.TypeFullName} {taskNode.Reference} = {taskNode.ContainerTypeFullName}.{taskNode.TaskTransformationFunctions.UngenericTaskToGenericTask.FunctionName}({taskNode.AsyncReference}, {taskNode.WrappedElement.Reference});");
                break;
            case AsyncWrappingStrategy.CollectionFromValueTask:
            case AsyncWrappingStrategy.FactoryFromValueTask:
            case AsyncWrappingStrategy.CallFromValueTask:
                _code.AppendLine($"{taskNode.TypeFullName} {taskNode.Reference} = {taskNode.ContainerTypeFullName}.{taskNode.TaskTransformationFunctions.GenericValueTaskToGenericTask.FunctionName}({taskNode.WrappedElement.Reference});");
                break;
            case AsyncWrappingStrategy.CollectionFromTask:
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
            $"{tupleNode.TypeFullName} {tupleNode.Reference} = new {tupleNode.TypeFullName}({string.Join(", ", tupleNode.Parameters.Select(p => $"{p.Name.PrefixAtIfKeyword()}: {p.Node.Reference}"))});");
    }

    public void VisitValueTupleNode(IValueTupleNode valueTupleNode)
    {
        foreach (var parameter in valueTupleNode.Parameters)
            VisitElementNode(parameter.Node);
        _code.AppendLine(
            $"{valueTupleNode.TypeFullName} {valueTupleNode.Reference} = new {valueTupleNode.TypeFullName}({string.Join(", ", valueTupleNode.Parameters.Select(p => $"{p.Name.PrefixAtIfKeyword()}: {p.Node.Reference}"))});");
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
        switch (elementNode)
        {
            case IPlainFunctionCallNode createCallNode:
                VisitPlainFunctionCallNode(createCallNode);
                break;
            case IScopeCallNode scopeCallNode:
                VisitScopeCallNode(scopeCallNode);
                break;
            case ITransientScopeCallNode transientScopeCallNode:
                VisitTransientScopeCallNode(transientScopeCallNode);
                break;
            case IParameterNode parameterNode:
                VisitParameterNode(parameterNode);
                break;
            case IOutParameterNode outParameterNode:
                VisitOutParameterNode(outParameterNode);
                break;
            case IFactoryFieldNode factoryFieldNode:
                VisitFactoryFieldNode(factoryFieldNode);
                break;
            case IFactoryFunctionNode factoryFunctionNode:
                VisitFactoryFunctionNode(factoryFunctionNode);
                break;
            case IFactoryPropertyNode factoryPropertyNode:
                VisitFactoryPropertyNode(factoryPropertyNode);
                break;
            case IFuncNode funcNode:
                VisitFuncNode(funcNode);
                break;
            case ILazyNode lazyNode:
                VisitLazyNode(lazyNode);
                break;
            case IValueTaskNode valueTaskNode:
                VisitValueTaskNode(valueTaskNode);
                break;
            case ITaskNode taskNode:
                VisitTaskNode(taskNode);
                break;
            case ITupleNode tupleNode:
                VisitTupleNode(tupleNode);
                break;
            case IValueTupleNode valueTupleNode:
                VisitValueTupleNode(valueTupleNode);
                break;
            case IValueTupleSyntaxNode valueTupleSyntaxNode:
                VisitValueTupleSyntaxNode(valueTupleSyntaxNode);
                break;
            case IAbstractionNode abstractionNode:
                VisitAbstractionNode(abstractionNode);
                break;
            case IImplementationNode implementationNode:
                VisitImplementationNode(implementationNode);
                break;
            case ITransientScopeDisposalTriggerNode transientScopeDisposalTriggerNode:
                VisitTransientScopeDisposalTriggerNode(transientScopeDisposalTriggerNode);
                break;
            case INullNode nullNode:
                VisitNullNode(nullNode);
                break;
            case IEnumerableBasedNode enumerableBasedNode:
                VisitEnumerableBasedNode(enumerableBasedNode);
                break;
        }
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
            ? $" {{ {string.Join(", ", implementationNode.Properties.Select(p => $"{p.Name.PrefixAtIfKeyword()} = {p.Element.Reference}"))} }}"
            : "";
        var constructorParameters =
            string.Join(", ", implementationNode.ConstructorParameters.Select(d => $"{d.Name.PrefixAtIfKeyword()}: {d.Element.Reference}"));
        _code.AppendLine(
            $"{implementationNode.TypeFullName} {implementationNode.Reference} = new {implementationNode.ConstructorCallName}({constructorParameters}){objectInitializerParameter};");
        
        if (implementationNode.SyncDisposalCollectionReference is {} syncDisposalCollectionReference)
            _code.AppendLine(
                $"{syncDisposalCollectionReference}.Add(({_wellKnownTypes.IDisposable.FullName()}) {implementationNode.Reference});");
        if (implementationNode.AsyncDisposalCollectionReference is {} asyncDisposalCollectionReference)
            _code.AppendLine(
                $"{asyncDisposalCollectionReference}.Add(({_wellKnownTypes.IAsyncDisposable.FullName()}) {implementationNode.Reference});");

        if (implementationNode.Initializer is {} init)
        {
            if (init.UserDefinedInjection is {})
                ProcessUserDefinedInjection(init.UserDefinedInjection);
            foreach (var (_, element) in init.Parameters)
                VisitElementNode(element);
            var initializerParameters =
                string.Join(", ", init.Parameters.Select(d => $"{d.Name.PrefixAtIfKeyword()}: {d.Element.Reference}"));

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
                $"{userDefinedInjection.Name}({string.Join(", ", userDefinedInjection.Parameters.Select(p => $"{p.Name.PrefixAtIfKeyword()}: {(p.IsOut ? "out var " : "")} {p.Element.Reference}"))});");
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

    public void VisitNullNode(INullNode nullNode) => _code.AppendLine(
        $"{nullNode.TypeFullName} {nullNode.Reference} = ({nullNode.TypeFullName}) null;");

    public void VisitMultiFunctionNode(IMultiFunctionNode multiFunctionNode)
    {
        var accessibility = multiFunctionNode is { Accessibility: { } acc, ExplicitInterfaceFullName: null }
            ? $"{SyntaxFacts.GetText(acc)} "  
            : "";
        var asyncModifier = multiFunctionNode.SynchronicityDecision is SynchronicityDecision.AsyncTask or SynchronicityDecision.AsyncValueTask
            || multiFunctionNode.IsAsyncEnumerable
            ? "async "
            : "";
        var explicitInterfaceFullName = multiFunctionNode.ExplicitInterfaceFullName is { } interfaceName
            ? $"{interfaceName}."
            : "";
        var parameter = string.Join(",", multiFunctionNode.Parameters.Select(r => $"{r.Node.TypeFullName} {r.Node.Reference}"));
        _code.AppendLine($$"""
{{accessibility}}{{asyncModifier}}{{explicitInterfaceFullName}}{{multiFunctionNode.ReturnedTypeFullName}} {{multiFunctionNode.Name}}({{parameter}})
{
""");
        ObjectDisposedCheck(
            multiFunctionNode.DisposedPropertyReference, 
            multiFunctionNode.RangeFullName, 
            multiFunctionNode.ReturnedTypeFullName);
        foreach (var returnedElement in multiFunctionNode.ReturnedElements)
        {
            VisitElementNode(returnedElement);
            ObjectDisposedCheck(
                multiFunctionNode.DisposedPropertyReference, 
                multiFunctionNode.RangeFullName, 
                multiFunctionNode.ReturnedTypeFullName);
            if (multiFunctionNode.SynchronicityDecision == SynchronicityDecision.Sync)
                _code.AppendLine($"yield return {returnedElement.Reference};");
        }
            
        foreach (var localFunction in multiFunctionNode.LocalFunctions)
            VisitSingleFunctionNode(localFunction);
        
        _code.AppendLine(multiFunctionNode.SynchronicityDecision == SynchronicityDecision.Sync
            ? "yield break;"
            : $"return new {multiFunctionNode.ItemTypeFullName}[] {{ {string.Join(", ", multiFunctionNode.ReturnedElements.Select(re => re.Reference))} }};");
        
        _code.AppendLine("}");
    }

    public void VisitEnumerableBasedNode(IEnumerableBasedNode enumerableBasedNode)
    {
        VisitElementNode(enumerableBasedNode.EnumerableCall);
        if (enumerableBasedNode is { Type: EnumerableBasedType.IEnumerable or EnumerableBasedType.IAsyncEnumerable, }
            || enumerableBasedNode.CollectionData is not
            {
                CollectionReference: { } collectionReference, CollectionTypeFullName: { } collectionTypeFullName
            }) 
            return;
        if (enumerableBasedNode.SynchronicityDecision == SynchronicityDecision.Sync || enumerableBasedNode.Awaited)
            CollectionHandling(collectionTypeFullName, collectionReference, enumerableBasedNode.EnumerableCall.Reference);
        else if (enumerableBasedNode.SynchronicityDecision == SynchronicityDecision.AsyncValueTask && !enumerableBasedNode.Awaited)
        {
            _code.AppendLine($$"""
{{enumerableBasedNode.AsyncTypeFullName}} {{enumerableBasedNode.Reference}} = new {{enumerableBasedNode.AsyncTypeFullName}}({{enumerableBasedNode.EnumerableCall.Reference}}.AsTask().ContinueWith(t =>
{
if (t.IsCompletedSuccessfully) 
{
""");
            CollectionHandling(collectionTypeFullName, enumerableBasedNode.AsyncReference ?? "result", "t.Result");
            _code.AppendLine($$"""
return {{enumerableBasedNode.AsyncReference ?? "result"}};
}
if (t.IsFaulted && t.Exception is { }) throw t.Exception;
if (t.IsCanceled) throw new {{_wellKnownTypes.TaskCanceledException.FullName()}}(t);
throw new {{_wellKnownTypes.Exception.FullName()}}("[DIE] Something unexpected.");
}));
"""); // todo mark last exception as from DIE and give it unique GUID
        }
        else if (enumerableBasedNode.SynchronicityDecision == SynchronicityDecision.AsyncTask && !enumerableBasedNode.Awaited)
        {
            _code.AppendLine($$"""
{{enumerableBasedNode.AsyncTypeFullName}} {{enumerableBasedNode.Reference}} = {{enumerableBasedNode.EnumerableCall.Reference}}.ContinueWith(t =>
{
if (t.IsCompletedSuccessfully) 
{
""");
            CollectionHandling(collectionTypeFullName, enumerableBasedNode.AsyncReference ?? "result", "t.Result");
            _code.AppendLine($$"""
return {{enumerableBasedNode.AsyncReference ?? "result"}};
}
if (t.IsFaulted && t.Exception is { }) throw t.Exception;
if (t.IsCanceled) throw new {{_wellKnownTypes.TaskCanceledException.FullName()}}(t);
throw new {{_wellKnownTypes.Exception.FullName()}}("[DIE] Something unexpected.");
});
"""); // todo mark last exception as from DIE and give it unique GUID
        }

        void CollectionHandling(string typeFullName, string reference, string enumerableReference)
        {
            switch (enumerableBasedNode.Type)
            {
                case EnumerableBasedType.Array:
                    _code.AppendLine(
                        $"{typeFullName} {reference} = {_wellKnownTypesCollections.Enumerable}.ToArray({enumerableReference});");
                    break;
                case EnumerableBasedType.IList
                    or EnumerableBasedType.ICollection:
                    _code.AppendLine(
                        $"{typeFullName} {reference} = {_wellKnownTypesCollections.Enumerable}.ToList({enumerableReference});");
                    break;
                case EnumerableBasedType.ArraySegment:
                    _code.AppendLine(
                        $"{typeFullName} {reference} = new {typeFullName}({_wellKnownTypesCollections.Enumerable}.ToArray({enumerableReference}));");
                    break;
                case EnumerableBasedType.ReadOnlyCollection
                    or EnumerableBasedType.IReadOnlyCollection
                    or EnumerableBasedType.IReadOnlyList
                    when enumerableBasedNode.CollectionData is ReadOnlyCollectionData
                    {
                        ConcreteReadOnlyCollectionTypeFullName: { } concreteReadOnlyCollectionTypeFullName
                    }:
                    _code.AppendLine(
                        $"{typeFullName} {reference} = new {concreteReadOnlyCollectionTypeFullName}({_wellKnownTypesCollections.Enumerable}.ToList({enumerableReference}));");
                    break;
                case EnumerableBasedType.ConcurrentBag
                    or EnumerableBasedType.ConcurrentQueue
                    or EnumerableBasedType.ConcurrentStack
                    or EnumerableBasedType.HashSet
                    or EnumerableBasedType.LinkedList
                    or EnumerableBasedType.List
                    or EnumerableBasedType.Queue
                    or EnumerableBasedType.SortedSet
                    or EnumerableBasedType.Stack:
                    _code.AppendLine(
                        $"{typeFullName} {reference} = new {typeFullName}({enumerableReference});");
                    break;
                case EnumerableBasedType.ImmutableArray
                    or EnumerableBasedType.ImmutableHashSet
                    or EnumerableBasedType.ImmutableList
                    or EnumerableBasedType.ImmutableQueue
                    or EnumerableBasedType.ImmutableSortedSet
                    or EnumerableBasedType.ImmutableStack
                    when enumerableBasedNode.CollectionData is ImmutableCollectionData
                    {
                        ImmutableUngenericTypeFullName: { } immutableUngenericTypeFullName
                    }:
                    _code.AppendLine(
                        $"{typeFullName} {reference} = {immutableUngenericTypeFullName}.CreateRange({enumerableReference});");
                    break;
            }
        }
    }

    public void VisitErrorNode(IErrorNode errorNode)
    {
        // Nothing to do here
    }

    public string GenerateContainerFile() => _code.ToString();

    private void ObjectDisposedCheck(
        string disposedPropertyReference,
        string rangeFullName,
        string returnTypeFullName) => _code.AppendLine(
        $"if ({disposedPropertyReference}) throw new {_wellKnownTypes.ObjectDisposedException}(\"{rangeFullName}\", $\"[DIE] This scope \\\"{rangeFullName}\\\" is already disposed, so it can't create a \\\"{returnTypeFullName}\\\" instance anymore.\");");
}