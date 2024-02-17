using Microsoft.CodeAnalysis.CSharp;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.Delegates;
using MrMeeseeks.DIE.Nodes.Elements.Factories;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Elements.Tuples;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Visitors;

internal interface ICodeGenerationVisitor : INodeVisitor
{
    string GenerateContainerFile();
}

internal sealed class CodeGenerationVisitor : ICodeGenerationVisitor
{
    private readonly IReferenceGenerator _referenceGenerator;
    private readonly StringBuilder _code = new();
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly WellKnownTypesCollections _wellKnownTypesCollections;

    public CodeGenerationVisitor(
        IContainerWideContext containerWideContext,
        IReferenceGenerator referenceGenerator)
    {
        _referenceGenerator = referenceGenerator;
        _wellKnownTypes = containerWideContext.WellKnownTypes;
        _wellKnownTypesCollections = containerWideContext.WellKnownTypesCollections;
    }

    public void VisitIContainerNode(IContainerNode container)
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
        if (container.GenerateEmptyConstructor) 
            _code.AppendLine($"private {container.Name}() {{ }}");
        
        foreach (var containerCreateContainerFunction in container.CreateContainerFunctions)
            VisitICreateContainerFunctionNode(containerCreateContainerFunction);

        foreach (var entryFunctionNode in container.RootFunctions)
            VisitIEntryFunctionNode(entryFunctionNode);

        GenerateRangeNodeContent(container);
        
        var dictionaryTypeName = container.DisposalType.HasFlag(DisposalType.Async)
            ? _wellKnownTypes.ConcurrentDictionaryOfAsyncDisposable.FullName()
            : _wellKnownTypes.ConcurrentDictionaryOfSyncDisposable.FullName();

        _code.AppendLine(
            $"private {dictionaryTypeName} {container.TransientScopeDisposalReference} = new {dictionaryTypeName}();");
        
        VisitITransientScopeInterfaceNode(container.TransientScopeInterface);
        
        foreach (var scope in container.Scopes)
            VisitIScopeNode(scope);
        
        foreach (var transientScope in container.TransientScopes)
            VisitITransientScopeNode(transientScope);
        
        _code.AppendLine("""
}
}
#nullable disable
""");
    }

    public void VisitICreateContainerFunctionNode(ICreateContainerFunctionNode createContainerFunction)
    {
        var asyncPrefix = createContainerFunction.InitializationAwaited
            ? "async "
            : "";
        var awaitPrefix = createContainerFunction.InitializationAwaited
            ? "await "
            : "";
        
        _code.AppendLine($$"""
public static {{asyncPrefix}}{{createContainerFunction.ReturnTypeFullName}} {{Constants.CreateContainerFunctionName}}({{string.Join(", ", createContainerFunction.Parameters.Select(p => $"{p.TypeFullName} {p.Reference.PrefixAtIfKeyword()}"))}})
{
{{createContainerFunction.ContainerTypeFullName}} {{createContainerFunction.ContainerReference}} = new {{createContainerFunction.ContainerTypeFullName}}({{string.Join(", ", createContainerFunction.Parameters.Select(p => $"{p.Reference.PrefixAtIfKeyword()}: {p.Reference.PrefixAtIfKeyword()}"))}});
""");
        if (createContainerFunction.InitializationFunctionName is { } initializationFunctionName)
            _code.AppendLine($"{awaitPrefix}{createContainerFunction.ContainerReference}.{initializationFunctionName}();");
        _code.AppendLine($$"""
return {{createContainerFunction.ContainerReference}};
}
""");
    }

    public void VisitITransientScopeInterfaceNode(ITransientScopeInterfaceNode transientScopeInterface)
    {
        _code.AppendLine($$"""
private interface {{transientScopeInterface.Name}}
{
""");
        foreach (var rangedInstanceInterfaceFunctionNode in transientScopeInterface.Functions)
            VisitIRangedInstanceInterfaceFunctionNode(rangedInstanceInterfaceFunctionNode);
        
        _code.AppendLine("}");
    }

    public void VisitIScopeNode(IScopeNode scope)
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

    public void VisitITransientScopeNode(ITransientScopeNode transientScope)
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

    public void VisitIScopeCallNode(IScopeCallNode scopeCall)
    {
        _code.AppendLine(
            $"{scopeCall.ScopeFullName} {scopeCall.ScopeReference} = new {scopeCall.ScopeFullName}({scopeCall.ContainerParameter}, {scopeCall.TransientScopeInterfaceParameter});");
        if (scopeCall.DisposalType.HasFlag(DisposalType.Async))
            _code.AppendLine($"{scopeCall.DisposableCollectionReference}.Add(({_wellKnownTypes.IAsyncDisposable.FullName()}) {scopeCall.ScopeReference});");
        else if (scopeCall.DisposalType.HasFlag(DisposalType.Sync))
            _code.AppendLine($"{scopeCall.DisposableCollectionReference}.Add(({_wellKnownTypes.IDisposable.FullName()}) {scopeCall.ScopeReference});");
        GenerateInitialization(scopeCall.Initialization, scopeCall.ScopeReference);
        VisitIFunctionCallNode(scopeCall);
    }

    public void VisitITransientScopeCallNode(ITransientScopeCallNode transientScopeCall)
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
        GenerateInitialization(transientScopeCall.Initialization, transientScopeCall.TransientScopeReference);
        VisitIFunctionCallNode(transientScopeCall);
    }

    private void GenerateInitialization(IFunctionCallNode? maybeInitialization, string ownerReference)
    {
        if (maybeInitialization is { } initialization)
        {
            var asyncPrefix = initialization.Awaited
                ? "await "
                : "";

            _code.AppendLine(
                $"{asyncPrefix}{ownerReference}.{initialization.FunctionName}({string.Join(", ", initialization.Parameters.Select(p =>$"{p.Item1.Reference.PrefixAtIfKeyword()}: {p.Item2.Reference}"))});");
        }
    }

    private void GenerateRangeNodeContent(IRangeNode rangeNode)
    {
        foreach (var initializedInstance in rangeNode.InitializedInstances)
            VisitIInitializedInstanceNode(initializedInstance);
        
        foreach (var initializationFunction in rangeNode.InitializationFunctions)
            VisitIVoidFunctionNode(initializationFunction);
        
        foreach (var createFunctionNode in rangeNode.CreateFunctions)
            VisitICreateFunctionNodeBase(createFunctionNode);
        
        if (rangeNode.HasGenericRangeInstanceFunctionGroups)
            _code.AppendLine($"private readonly {_wellKnownTypes.ConcurrentDictionaryOfRuntimeTypeHandleToObject.FullName()} {rangeNode.RangedInstanceStorageFieldName} = new {_wellKnownTypes.ConcurrentDictionaryOfRuntimeTypeHandleToObject.FullName()}();");

        foreach (var rangedInstanceFunctionGroup in rangeNode.RangedInstanceFunctionGroups)
            VisitIRangedInstanceFunctionGroupNode(rangedInstanceFunctionGroup);

        foreach (var multiFunctionNodeBase in rangeNode.MultiFunctions)
            switch (multiFunctionNodeBase)
            {
                case IMultiFunctionNode multiFunctionNode:
                    VisitIMultiFunctionNode(multiFunctionNode);
                    break;
                case IMultiKeyValueFunctionNode multiKeyValueFunctionNode:
                    VisitIMultiKeyValueFunctionNode(multiKeyValueFunctionNode);
                    break;
                case IMultiKeyValueMultiFunctionNode multiKeyValueMultiFunctionNode:
                    VisitIMultiKeyValueMultiFunctionNode(multiKeyValueMultiFunctionNode);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(multiFunctionNodeBase));
            }
        
        if (rangeNode is { AddForDisposal: { } addForDisposal, DisposalHandling.SyncCollectionReference: { } syncCollectionReference })
            GenerateAddForDisposal(
                addForDisposal,
                Constants.UserDefinedAddForDisposal,
                _wellKnownTypes.IDisposable.FullName(),
                "disposable",
                syncCollectionReference);

        if (rangeNode is { AddForDisposalAsync: { } addForDisposalAsync, DisposalHandling.AsyncCollectionReference: { } asyncCollectionReference })
            GenerateAddForDisposal(
                addForDisposalAsync,
                Constants.UserDefinedAddForDisposalAsync,
                _wellKnownTypes.IAsyncDisposable.FullName(),
                "asyncDisposable",
                asyncCollectionReference);
        
        GenerateDisposalFunction(rangeNode);
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
            _code.AppendLine(
                $$"""
                  {{declaredAccessibility}} {{sealedModifier}}{{virtualModifier}}{{overrideModifier}}partial void {{methodName}}({{disposableFullName}} {{disposableParameterName}}) =>
                  {{collectionReference}}.Add(({{disposableFullName}}) {{disposableParameterName}});
                  """);
        }
    }

    private void VisitICreateFunctionNodeBase(ICreateFunctionNodeBase element)
    {
        switch (element)
        {
            case ICreateFunctionNode createFunctionNode:
                VisitICreateFunctionNode(createFunctionNode);
                break;
            case ICreateScopeFunctionNode createScopeFunctionNode:
                VisitICreateScopeFunctionNode(createScopeFunctionNode);
                break;
            case ICreateTransientScopeFunctionNode createTransientScopeFunctionNode:
                VisitICreateTransientScopeFunctionNode(createTransientScopeFunctionNode);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(element));
        }
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

    private void VisitISingleFunctionNode(ISingleFunctionNode singleFunction, bool doDisposedChecks)
    {
        _code.AppendLine($$"""
{{GenerateMethodDeclaration(singleFunction)}}
{
""");
        
        if (doDisposedChecks)
            ObjectDisposedCheck(
                singleFunction.DisposedPropertyReference, 
                singleFunction.RangeFullName, 
                singleFunction.ReturnedTypeFullName);
        
        VisitIElementNode(singleFunction.ReturnedElement);
        
        if (doDisposedChecks)
            ObjectDisposedCheck(
                singleFunction.DisposedPropertyReference, 
                singleFunction.RangeFullName, 
                singleFunction.ReturnedTypeFullName);
        _code.AppendLine($"return {singleFunction.ReturnedElement.Reference};");
            
        foreach (var localFunction in singleFunction.LocalFunctions)
            VisitISingleFunctionNode(localFunction, true);
        
        _code.AppendLine("}");
    }

    public void VisitICreateFunctionNode(ICreateFunctionNode createFunction) => VisitISingleFunctionNode(createFunction, false);
    public void VisitIEntryFunctionNode(IEntryFunctionNode entryFunction) => VisitISingleFunctionNode(entryFunction, true);
    public void VisitILocalFunctionNode(ILocalFunctionNode localFunction) => VisitISingleFunctionNode(localFunction, true);
    public void VisitIRangedInstanceFunctionNode(IRangedInstanceFunctionNode rangedInstanceFunctionNode)
    {
        // Nothing to do here. It's generated in "VisitRangedInstanceFunctionGroupNode"
    }

    public void VisitIRangedInstanceInterfaceFunctionNode(IRangedInstanceInterfaceFunctionNode rangedInstanceInterfaceFunctionNode)
    {
        var parameter = string.Join(",", rangedInstanceInterfaceFunctionNode.Parameters.Select(r => $"{r.Node.TypeFullName} {r.Node.Reference}"));
        _code.AppendLine($"{rangedInstanceInterfaceFunctionNode.ReturnedTypeFullName} {rangedInstanceInterfaceFunctionNode.Name}({parameter});");
    }

    public void VisitIRangedInstanceFunctionGroupNode(IRangedInstanceFunctionGroupNode rangedInstanceFunctionGroupNode)
    {
        if (rangedInstanceFunctionGroupNode.IsOpenGeneric)
            Generic();
        else if (rangedInstanceFunctionGroupNode.IsCreatedForStructs is null)
            RefLike();
        else
            Struct();

        void Generic()
        {
            _code.AppendLine(
                $$"""
                  private {{_wellKnownTypes.SemaphoreSlim.FullName()}} {{rangedInstanceFunctionGroupNode.LockReference}} = new {{_wellKnownTypes.SemaphoreSlim.FullName()}}(1);
                  """);
            var typeHandleField = _referenceGenerator.Generate("typeHandle");

            foreach (var overload in rangedInstanceFunctionGroupNode.Overloads)
            {
                _code.AppendLine(GenerateMethodDeclaration(overload));
                
                var instance0 = _referenceGenerator.Generate("instance");
                var instance1 = _referenceGenerator.Generate("instance");
                
                var isAsync =
                    overload.SynchronicityDecision is SynchronicityDecision.AsyncTask or SynchronicityDecision.AsyncValueTask;
                _code.AppendLine($$"""
{
var {{typeHandleField}} = typeof({{overload.ReturnedElement.TypeFullName}}).TypeHandle;
if ({{rangedInstanceFunctionGroupNode.RangedInstanceStorageFieldName}}.TryGetValue({{typeHandleField}}, out {{_wellKnownTypes.Object.FullName()}}? {{instance0}}) && {{instance0}} is {{overload.ReturnedElement.TypeFullName}} {{instance1}}) return {{instance1}};
{{(isAsync ? "await " : "")}}{{Constants.ThisKeyword}}.{{rangedInstanceFunctionGroupNode.LockReference}}.Wait{{(isAsync ? "Async" : "")}}();
try
{
""");
                
                ObjectDisposedCheck(
                    overload.DisposedPropertyReference, 
                    overload.RangeFullName, 
                    overload.ReturnedTypeFullName);
                
                var instance2 = _referenceGenerator.Generate("instance");
                var instance3 = _referenceGenerator.Generate("instance");
                _code.AppendLine(
                    $"if ({rangedInstanceFunctionGroupNode.RangedInstanceStorageFieldName}.TryGetValue({typeHandleField}, out {_wellKnownTypes.Object.FullName()}? {instance2}) && {instance2} is {overload.ReturnedElement.TypeFullName} {instance3}) return {instance3};");
                
                VisitIElementNode(overload.ReturnedElement);

                _code.AppendLine($"{rangedInstanceFunctionGroupNode.RangedInstanceStorageFieldName}[{typeHandleField}] = {overload.ReturnedElement.Reference};");
                
                ObjectDisposedCheck(
                    overload.DisposedPropertyReference, 
                    overload.RangeFullName, 
                    overload.ReturnedTypeFullName);
                
                _code.AppendLine($$"""
return {{overload.ReturnedElement.Reference}};
}
finally
{
{{Constants.ThisKeyword}}.{{rangedInstanceFunctionGroupNode.LockReference}}.Release();
}
""");
                foreach (var localFunction in overload.LocalFunctions)
                    VisitISingleFunctionNode(localFunction, true);
                _code.AppendLine("}");
            }
        }

        void RefLike()
        {
            _code.AppendLine($$"""
private {{rangedInstanceFunctionGroupNode.TypeFullName}}? {{rangedInstanceFunctionGroupNode.FieldReference}};
private {{_wellKnownTypes.SemaphoreSlim.FullName()}} {{rangedInstanceFunctionGroupNode.LockReference}} = new {{_wellKnownTypes.SemaphoreSlim.FullName()}}(1);
""");

            foreach (var overload in rangedInstanceFunctionGroupNode.Overloads)
            {
                _code.AppendLine(GenerateMethodDeclaration(overload));

                var checkAndReturnAlreadyCreatedInstance = $"if (!{_wellKnownTypes.Object.FullName()}.ReferenceEquals({rangedInstanceFunctionGroupNode.FieldReference}, null)) return {rangedInstanceFunctionGroupNode.FieldReference};";
                
                var isAsync =
                    overload.SynchronicityDecision is SynchronicityDecision.AsyncTask or SynchronicityDecision.AsyncValueTask;
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
                
                VisitIElementNode(overload.ReturnedElement);

                _code.AppendLine($$"""
{{rangedInstanceFunctionGroupNode.FieldReference}} = {{overload.ReturnedElement.Reference}};
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
                    VisitISingleFunctionNode(localFunction, true);
                _code.AppendLine("}");
            }
        }

        void Struct()
        {
            _code.AppendLine($$"""
private {{rangedInstanceFunctionGroupNode.TypeFullName}} {{rangedInstanceFunctionGroupNode.FieldReference}};
private {{_wellKnownTypes.SemaphoreSlim.FullName()}} {{rangedInstanceFunctionGroupNode.LockReference}} = new {{_wellKnownTypes.SemaphoreSlim.FullName()}}(1);
""");

            _code.AppendLine($"private bool {rangedInstanceFunctionGroupNode.IsCreatedForStructs};");

            foreach (var overload in rangedInstanceFunctionGroupNode.Overloads)
            {
                _code.AppendLine(GenerateMethodDeclaration(overload));

                var checkAndReturnAlreadyCreatedInstance = $"if ({rangedInstanceFunctionGroupNode.IsCreatedForStructs}) return {rangedInstanceFunctionGroupNode.FieldReference};";
                
                var isAsync =
                    overload.SynchronicityDecision is SynchronicityDecision.AsyncTask or SynchronicityDecision.AsyncValueTask;
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
                
                VisitIElementNode(overload.ReturnedElement);

                _code.AppendLine($$"""
{{rangedInstanceFunctionGroupNode.FieldReference}} = {{overload.ReturnedElement.Reference}};
{{rangedInstanceFunctionGroupNode.IsCreatedForStructs}} = true;
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
                _code.AppendLine($"return {Constants.ThisKeyword}.{rangedInstanceFunctionGroupNode.FieldReference};");
                foreach (var localFunction in overload.LocalFunctions)
                    VisitISingleFunctionNode(localFunction, true);
                _code.AppendLine("}");
            }
        }
    }

    public void VisitIWrappedAsyncFunctionCallNode(IWrappedAsyncFunctionCallNode functionCallNode)
    {
        var owner = functionCallNode.OwnerReference is { } ownerReference ? $"{ownerReference}." : ""; 
        var typeFullName = functionCallNode.TypeFullName;
        var typeParameters = functionCallNode.TypeParameters.Any()
            ? $"<{string.Join(", ", functionCallNode.TypeParameters.Select(p => p.Name))}>"
            : "";
        var call = $"{owner}{functionCallNode.FunctionName}{typeParameters}({string.Join(", ", functionCallNode.Parameters.Select(p => $"{p.Item1.Reference.PrefixAtIfKeyword()}: {p.Item2.Reference}"))})";
        call = functionCallNode.Transformation switch
        {
            AsyncFunctionCallTransformation.ValueTaskFromValueTask => call,
            AsyncFunctionCallTransformation.ValueTaskFromTask => $"new {typeFullName}({call})",
            AsyncFunctionCallTransformation.ValueTaskFromSync => $"new {typeFullName}({call})",
            AsyncFunctionCallTransformation.TaskFromValueTask => $"{call}.AsTask()",
            AsyncFunctionCallTransformation.TaskFromTask => call,
            AsyncFunctionCallTransformation.TaskFromSync => $"{_wellKnownTypes.Task}.FromResult({call})",
            _ => throw new ArgumentOutOfRangeException()
        };
        _code.AppendLine($"{typeFullName} {functionCallNode.Reference} = ({typeFullName}){call};");
    }

    private void VisitIFunctionCallNode(IFunctionCallNode functionCallNode)
    {
        var owner = functionCallNode.OwnerReference is { } ownerReference ? $"{ownerReference}." : ""; 
        var typeFullName = functionCallNode.TypeFullName;
        var typeParameters = functionCallNode.TypeParameters.Any()
            ? $"<{string.Join(", ", functionCallNode.TypeParameters.Select(p => p.Name))}>"
            : "";
        var call = $"{owner}{functionCallNode.FunctionName}{typeParameters}({string.Join(", ", functionCallNode.Parameters.Select(p => $"{p.Item1.Reference.PrefixAtIfKeyword()}: {p.Item2.Reference}"))})";
        call = functionCallNode.Awaited ? $"(await {call})" : call;
        _code.AppendLine($"{typeFullName} {functionCallNode.Reference} = ({typeFullName}){call};");
    }

    public void VisitICreateScopeFunctionNode(ICreateScopeFunctionNode element) => 
        VisitISingleFunctionNode(element, false);

    public void VisitIPlainFunctionCallNode(IPlainFunctionCallNode plainFunctionCallNode) => VisitIFunctionCallNode(plainFunctionCallNode);

    private void VisitIFactoryNodeBase(IFactoryNodeBase factoryNode, string optionalParameters)
    {
        var typeFullName = factoryNode.Awaited
            ? factoryNode.AsyncTypeFullName
            : factoryNode.TypeFullName;
        var awaitPrefix = factoryNode.Awaited ? "await " : "";
        _code.AppendLine($"{typeFullName} {factoryNode.Reference} = ({typeFullName}){awaitPrefix}{factoryNode.Name}{optionalParameters};");
    }

    public void VisitIFactoryFieldNode(IFactoryFieldNode factoryFieldNode)
    {
        VisitIFactoryNodeBase(factoryFieldNode, "");
    }

    public void VisitIFactoryPropertyNode(IFactoryPropertyNode factoryPropertyNode)
    {
        VisitIFactoryNodeBase(factoryPropertyNode, "");
    }

    public void VisitIFactoryFunctionNode(IFactoryFunctionNode factoryFunctionNode)
    {
        foreach (var (_, element) in factoryFunctionNode.Parameters)
            VisitIElementNode(element);
        VisitIFactoryNodeBase(factoryFunctionNode, $"({string.Join(", ", factoryFunctionNode.Parameters.Select(t => $"{t.Name.PrefixAtIfKeyword()}: {t.Element.Reference}"))})");
    }

    public void VisitICreateTransientScopeFunctionNode(ICreateTransientScopeFunctionNode element) =>
        VisitISingleFunctionNode(element, false);

    public void VisitIFuncNode(IFuncNode funcNode) =>
        _code.AppendLine($"{funcNode.TypeFullName} {funcNode.Reference} = {funcNode.MethodGroup};");

    public void VisitILazyNode(ILazyNode lazyNode) => 
        _code.AppendLine($"{lazyNode.TypeFullName} {lazyNode.Reference} = new {lazyNode.TypeFullName}({lazyNode.MethodGroup});");
    
    public void VisitIThreadLocalNode(IThreadLocalNode threadLocalNode)
    {
        _code.AppendLine(
            $"{threadLocalNode.TypeFullName} {threadLocalNode.Reference} = new {threadLocalNode.TypeFullName}({threadLocalNode.MethodGroup}, false);");
        if (threadLocalNode.SyncDisposalCollectionReference is {} syncDisposalCollectionReference)
            _code.AppendLine(
                $"{syncDisposalCollectionReference}.Add(({_wellKnownTypes.IDisposable.FullName()}) {threadLocalNode.Reference});");
    }

    public void VisitITupleNode(ITupleNode tupleNode)
    {
        foreach (var parameter in tupleNode.Parameters)
            VisitIElementNode(parameter.Node);
        _code.AppendLine(
            $"{tupleNode.TypeFullName} {tupleNode.Reference} = new {tupleNode.TypeFullName}({string.Join(", ", tupleNode.Parameters.Select(p => $"{p.Name.PrefixAtIfKeyword()}: {p.Node.Reference}"))});");
    }

    public void VisitIValueTupleNode(IValueTupleNode valueTupleNode)
    {
        foreach (var parameter in valueTupleNode.Parameters)
            VisitIElementNode(parameter.Node);
        _code.AppendLine(
            $"{valueTupleNode.TypeFullName} {valueTupleNode.Reference} = new {valueTupleNode.TypeFullName}({string.Join(", ", valueTupleNode.Parameters.Select(p => $"{p.Name.PrefixAtIfKeyword()}: {p.Node.Reference}"))});");
    }

    public void VisitIValueTupleSyntaxNode(IValueTupleSyntaxNode valueTupleSyntaxNode)
    {
        foreach (var item in valueTupleSyntaxNode.Items)
        {
            VisitIElementNode(item);
        }
        _code.AppendLine($"{valueTupleSyntaxNode.TypeFullName} {valueTupleSyntaxNode.Reference} = ({string.Join(", ", valueTupleSyntaxNode.Items.Select(d => d.Reference))});");
    }

    private void VisitIElementNode(IElementNode elementNode)
    {
        switch (elementNode)
        {
            case IPlainFunctionCallNode createCallNode:
                VisitIPlainFunctionCallNode(createCallNode);
                break;
            case IWrappedAsyncFunctionCallNode asyncFunctionCallNode:
                VisitIWrappedAsyncFunctionCallNode(asyncFunctionCallNode);
                break;
            case IScopeCallNode scopeCallNode:
                VisitIScopeCallNode(scopeCallNode);
                break;
            case ITransientScopeCallNode transientScopeCallNode:
                VisitITransientScopeCallNode(transientScopeCallNode);
                break;
            case IParameterNode parameterNode:
                VisitIParameterNode(parameterNode);
                break;
            case IOutParameterNode outParameterNode:
                VisitIOutParameterNode(outParameterNode);
                break;
            case IFactoryFieldNode factoryFieldNode:
                VisitIFactoryFieldNode(factoryFieldNode);
                break;
            case IFactoryFunctionNode factoryFunctionNode:
                VisitIFactoryFunctionNode(factoryFunctionNode);
                break;
            case IFactoryPropertyNode factoryPropertyNode:
                VisitIFactoryPropertyNode(factoryPropertyNode);
                break;
            case IFuncNode funcNode:
                VisitIFuncNode(funcNode);
                break;
            case ILazyNode lazyNode:
                VisitILazyNode(lazyNode);
                break;
            case IThreadLocalNode threadLocalNode:
                VisitIThreadLocalNode(threadLocalNode);
                break;
            case ITupleNode tupleNode:
                VisitITupleNode(tupleNode);
                break;
            case IValueTupleNode valueTupleNode:
                VisitIValueTupleNode(valueTupleNode);
                break;
            case IValueTupleSyntaxNode valueTupleSyntaxNode:
                VisitIValueTupleSyntaxNode(valueTupleSyntaxNode);
                break;
            case IImplementationNode implementationNode:
                VisitIImplementationNode(implementationNode);
                break;
            case ITransientScopeDisposalTriggerNode transientScopeDisposalTriggerNode:
                VisitITransientScopeDisposalTriggerNode(transientScopeDisposalTriggerNode);
                break;
            case INullNode nullNode:
                VisitINullNode(nullNode);
                break;
            case IEnumerableBasedNode enumerableBasedNode:
                VisitIEnumerableBasedNode(enumerableBasedNode);
                break;
            case IReusedNode reusedNode:
                VisitIReusedNode(reusedNode);
                break;
            case IKeyValueBasedNode keyValueBasedNode:
                VisitIKeyValueBasedNode(keyValueBasedNode);
                break;
            case IKeyValuePairNode keyValuePairNode:
                VisitIKeyValuePairNode(keyValuePairNode);
                break;
        }
    }

    public void VisitIImplementationNode(IImplementationNode implementationNode)
    {
        if (implementationNode.UserDefinedInjectionConstructor is {})
            ProcessUserDefinedInjection(implementationNode.UserDefinedInjectionConstructor);
        if (implementationNode.UserDefinedInjectionProperties is {})
            ProcessUserDefinedInjection(implementationNode.UserDefinedInjectionProperties);
        foreach (var (_, element) in implementationNode.ConstructorParameters)
            VisitIElementNode(element);
        foreach (var (_, element)  in implementationNode.Properties)
            VisitIElementNode(element);
        var objectInitializerParameter = implementationNode.Properties.Any()
            ? $" {{ {string.Join(", ", implementationNode.Properties.Select(p => $"{p.Name.PrefixAtIfKeyword()} = {p.Element.Reference}"))} }}"
            : "";
        var constructorParameters =
            string.Join(", ", implementationNode.ConstructorParameters.Select(d => $"{d.Name.PrefixAtIfKeyword()}: {d.Element.Reference}"));
        var cast = implementationNode.TypeFullName == implementationNode.ImplementationTypeFullName
            ? ""
            : $"({implementationNode.TypeFullName}) ";
        _code.AppendLine(
            $"{implementationNode.TypeFullName} {implementationNode.Reference} = {cast}new {implementationNode.ConstructorCallName}({constructorParameters}){objectInitializerParameter};");
        
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
                VisitIElementNode(element);
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
                VisitIElementNode(element);
            var generics = userDefinedInjection.TypeParameters.Count > 0
                ? $"<{string.Join(", ", userDefinedInjection.TypeParameters)}>"
                : "";
            _code.AppendLine(
                $"{userDefinedInjection.Name}{generics}({string.Join(", ", userDefinedInjection.Parameters.Select(p => $"{p.Name.PrefixAtIfKeyword()}: {(p.IsOut ? "out var " : "")} {p.Element.Reference}"))});");
        }
    }

    public void VisitIParameterNode(IParameterNode parameterNode)
    {
        // Processing is done in associated function node
    }

    public void VisitIOutParameterNode(IOutParameterNode outParameterNode)
    {
        // Processing is done in associated implementation node
    }

    public void VisitITransientScopeDisposalTriggerNode(ITransientScopeDisposalTriggerNode transientScopeDisposalTriggerNode)
    {
        transientScopeDisposalTriggerNode.CheckSynchronicity();
        _code.AppendLine(
            $"{transientScopeDisposalTriggerNode.TypeFullName} {transientScopeDisposalTriggerNode.Reference} = {Constants.ThisKeyword} as {transientScopeDisposalTriggerNode.TypeFullName};");
    }

    public void VisitINullNode(INullNode nullNode) => _code.AppendLine(
        $"{nullNode.TypeFullName} {nullNode.Reference} = ({nullNode.TypeFullName}) null;");

    public void VisitIMultiFunctionNode(IMultiFunctionNode multiFunctionNode) =>
        VisitIMultiFunctionNodeBase(multiFunctionNode);

    private void VisitIMultiFunctionNodeBase(IMultiFunctionNodeBase multiFunctionNodeBase)
    {
        _code.AppendLine($$"""
{{GenerateMethodDeclaration(multiFunctionNodeBase)}}
{
""");
        
        ObjectDisposedCheck(
            multiFunctionNodeBase.DisposedPropertyReference, 
            multiFunctionNodeBase.RangeFullName, 
            multiFunctionNodeBase.ReturnedTypeFullName);
        foreach (var returnedElement in multiFunctionNodeBase.ReturnedElements)
        {
            VisitIElementNode(returnedElement);
            ObjectDisposedCheck(
                multiFunctionNodeBase.DisposedPropertyReference, 
                multiFunctionNodeBase.RangeFullName, 
                multiFunctionNodeBase.ReturnedTypeFullName);
            if (multiFunctionNodeBase.SynchronicityDecision == SynchronicityDecision.Sync)
                _code.AppendLine($"yield return {returnedElement.Reference};");
        }
            
        foreach (var localFunction in multiFunctionNodeBase.LocalFunctions)
            VisitISingleFunctionNode(localFunction, true);
        
        _code.AppendLine(multiFunctionNodeBase.SynchronicityDecision == SynchronicityDecision.Sync
            ? "yield break;"
            : $"return new {multiFunctionNodeBase.ItemTypeFullName}[] {{ {string.Join(", ", multiFunctionNodeBase.ReturnedElements.Select(re => re.Reference))} }};");
        
        _code.AppendLine("}");
    }

    public void VisitIEnumerableBasedNode(IEnumerableBasedNode enumerableBasedNode)
    {
        VisitIElementNode(enumerableBasedNode.EnumerableCall);
        if (enumerableBasedNode is { Type: EnumerableBasedType.IEnumerable or EnumerableBasedType.IAsyncEnumerable }
            || enumerableBasedNode.CollectionData is not
            {
                CollectionReference: { } collectionReference, CollectionTypeFullName: { } collectionTypeFullName
            }) 
            return;
        switch (enumerableBasedNode.Type)
        {
            case EnumerableBasedType.Array:
                _code.AppendLine(
                    $"{collectionTypeFullName} {collectionReference} = {_wellKnownTypesCollections.Enumerable}.ToArray({enumerableBasedNode.EnumerableCall.Reference});");
                break;
            case EnumerableBasedType.IList
                or EnumerableBasedType.ICollection:
                _code.AppendLine(
                    $"{collectionTypeFullName} {collectionReference} = {_wellKnownTypesCollections.Enumerable}.ToList({enumerableBasedNode.EnumerableCall.Reference});");
                break;
            case EnumerableBasedType.ArraySegment:
                _code.AppendLine(
                    $"{collectionTypeFullName} {collectionReference} = new {collectionTypeFullName}({_wellKnownTypesCollections.Enumerable}.ToArray({enumerableBasedNode.EnumerableCall.Reference}));");
                break;
            case EnumerableBasedType.ReadOnlyCollection:
                _code.AppendLine(
                    $"{collectionTypeFullName} {collectionReference} = new {collectionTypeFullName}({_wellKnownTypesCollections.Enumerable}.ToList({enumerableBasedNode.EnumerableCall.Reference}));");
                break;
            case EnumerableBasedType.IReadOnlyCollection
                or EnumerableBasedType.IReadOnlyList
                when enumerableBasedNode.CollectionData is ReadOnlyInterfaceCollectionData
                {
                    ConcreteCollectionTypeFullName: { } concreteCollectionTypeFullName
                }:
                _code.AppendLine(
                    $"{collectionTypeFullName} {collectionReference} = new {concreteCollectionTypeFullName}({_wellKnownTypesCollections.Enumerable}.ToList({enumerableBasedNode.EnumerableCall.Reference}));");
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
                    $"{collectionTypeFullName} {collectionReference} = new {collectionTypeFullName}({enumerableBasedNode.EnumerableCall.Reference});");
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
                    $"{collectionTypeFullName} {collectionReference} = {immutableUngenericTypeFullName}.CreateRange({enumerableBasedNode.EnumerableCall.Reference});");
                break;
        }
    }

    public void VisitIErrorNode(IErrorNode errorNode)
    {
        // Nothing to do here
    }

    public void VisitIInitializedInstanceNode(IInitializedInstanceNode initializedInstanceNode)
    {
        var initialValue = initializedInstanceNode.IsReferenceType
            ? "null!"
            : $"new {initializedInstanceNode.TypeFullName}()";
        _code.AppendLine($"private {initializedInstanceNode.TypeFullName} {initializedInstanceNode.Reference} = {initialValue};");
    }

    public void VisitIVoidFunctionNode(IVoidFunctionNode voidFunctionNode)
    {
        _code.AppendLine($$"""
{{GenerateMethodDeclaration(voidFunctionNode)}}
{
""");
        foreach (var (functionCallNode, initializedInstanceNode) in voidFunctionNode.Initializations)
        {
            VisitIElementNode(functionCallNode);
            _code.AppendLine($"{initializedInstanceNode.Reference} = {functionCallNode.Reference};");
        }
            
        foreach (var localFunction in voidFunctionNode.LocalFunctions)
            VisitISingleFunctionNode(localFunction, true);
        
        _code.AppendLine("}");
    }

    public void VisitIMultiKeyValueFunctionNode(IMultiKeyValueFunctionNode multiKeyValueFunctionNode) =>
        VisitIMultiFunctionNodeBase(multiKeyValueFunctionNode);

    public void VisitIMultiKeyValueMultiFunctionNode(IMultiKeyValueMultiFunctionNode multiKeyValueMultiFunctionNode) =>
        VisitIMultiFunctionNodeBase(multiKeyValueMultiFunctionNode);

    public void VisitIKeyValueBasedNode(IKeyValueBasedNode keyValueBasedNode)
    {
        VisitIElementNode(keyValueBasedNode.EnumerableCall);
        if (keyValueBasedNode is { Type: KeyValueBasedType.SingleIEnumerable or KeyValueBasedType.SingleIAsyncEnumerable }
            || keyValueBasedNode.MapData is not
            {
                MapReference: { } mapReference, MapTypeFullName: { } mapTypeFullName
            }) 
            return;
        switch (keyValueBasedNode.Type)
        {
            case KeyValueBasedType.SingleIDictionary 
                or KeyValueBasedType.SingleIReadOnlyDictionary
                or KeyValueBasedType.SingleDictionary:
                _code.AppendLine(
                    $"{mapTypeFullName} {mapReference} = {_wellKnownTypesCollections.Enumerable}.ToDictionary({keyValueBasedNode.EnumerableCall.Reference}, kvp => kvp.Key, kvp => kvp.Value);");
                break;
            case KeyValueBasedType.SingleReadOnlyDictionary 
                or KeyValueBasedType.SingleSortedDictionary
                or KeyValueBasedType.SingleSortedList:
                _code.AppendLine(
                    $"{mapTypeFullName} {mapReference} = new {mapTypeFullName}({_wellKnownTypesCollections.Enumerable}.ToDictionary({keyValueBasedNode.EnumerableCall.Reference}, kvp => kvp.Key, kvp => kvp.Value));");
                break;
            case KeyValueBasedType.SingleImmutableDictionary
                or KeyValueBasedType.SingleImmutableSortedDictionary
                when keyValueBasedNode.MapData is ImmutableMapData
                {
                    ImmutableUngenericTypeFullName: { } immutableUngenericTypeFullName
                }:
                _code.AppendLine(
                    $"{mapTypeFullName} {mapReference} = {immutableUngenericTypeFullName}.CreateRange({keyValueBasedNode.EnumerableCall.Reference});");
                break;
        }
    }

    public void VisitIKeyValuePairNode(IKeyValuePairNode keyValuePairNode)
    {
        VisitIElementNode(keyValuePairNode.Value);
        var keyLiteral = keyValuePairNode.KeyType.TypeKind == TypeKind.Enum 
            ? $"({keyValuePairNode.KeyType.FullName()}) {SymbolDisplay.FormatPrimitive(keyValuePairNode.Key, true, false)}" 
            : CustomSymbolEqualityComparer.Default.Equals(keyValuePairNode.KeyType, _wellKnownTypes.Type) 
                ? $"typeof({(keyValuePairNode.Key as ITypeSymbol)?.FullName() ?? ""})" 
                : SymbolDisplay.FormatPrimitive(keyValuePairNode.Key, true, false);
        _code.AppendLine(
            $"{keyValuePairNode.TypeFullName} {keyValuePairNode.Reference} = new {keyValuePairNode.TypeFullName}({keyLiteral}, {keyValuePairNode.Value.Reference});");
    }

    private readonly HashSet<IReusedNode> _doneReusedNodes = new();
    public void VisitIReusedNode(IReusedNode reusedNode)
    {
        if (_doneReusedNodes.Contains(reusedNode)) return;
        _doneReusedNodes.Add(reusedNode);
        VisitIElementNode(reusedNode.Inner);
    }

    public string GenerateContainerFile() => _code.ToString();

    private void ObjectDisposedCheck(
        string disposedPropertyReference,
        string rangeFullName,
        string returnTypeFullName) => _code.AppendLine(
        $"if ({disposedPropertyReference}) throw new {_wellKnownTypes.ObjectDisposedException}(\"{rangeFullName}\", $\"[DIE] This scope \\\"{rangeFullName}\\\" is already disposed, so it can't create a \\\"{returnTypeFullName}\\\" instance anymore.\");");

    private static string GenerateMethodDeclaration(IFunctionNode functionNode)
    {
        var accessibility = functionNode is { Accessibility: { } acc, ExplicitInterfaceFullName: null }
            ? $"{SyntaxFacts.GetText(acc)} "  
            : "";
        var asyncModifier = 
            functionNode.SynchronicityDecision is SynchronicityDecision.AsyncTask or SynchronicityDecision.AsyncValueTask 
            || functionNode is IMultiFunctionNodeBase { IsAsyncEnumerable: true } 
            ? "async "
            : "";
        var explicitInterfaceFullName = functionNode.ExplicitInterfaceFullName is { } interfaceName
            ? $"{interfaceName}."
            : "";
        var typeParameters = "";
        var typeParametersConstraints = "";
        if (functionNode is IReturningFunctionNode returningFunctionNode && returningFunctionNode.TypeParameters.Any())
        {
            typeParameters = $"<{string.Join(", ", returningFunctionNode.TypeParameters.Select(p => p.Name))}>";
            typeParametersConstraints = string.Join("", returningFunctionNode
                .TypeParameters
                .Where(p => p.HasValueTypeConstraint 
                            || p.HasReferenceTypeConstraint
                            || p.HasNotNullConstraint 
                            || p.HasUnmanagedTypeConstraint
                            || p.HasConstructorConstraint
                            || p.ConstraintTypes.Length > 0)
                .Select(p =>
                {
                    var constraints = new List<string>();
                    if (p.HasUnmanagedTypeConstraint)
                        constraints.Add("unmanaged");
                    else if (p.HasValueTypeConstraint)
                        constraints.Add("struct");
                    else if (p.HasReferenceTypeConstraint)
                        constraints.Add($"class{(p.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated ? "?" : "")}");
                    if (p.HasNotNullConstraint)
                        constraints.Add("notnull");
                    constraints.AddRange(p.ConstraintTypes.Select((t, i) => t.WithNullableAnnotation(p.ConstraintNullableAnnotations[i]).FullName()));
                    if (p.HasConstructorConstraint)
                        constraints.Add("new()");
                    return $"{Environment.NewLine}where {p.Name} : {string.Join(", ", constraints)}";
                }));
        }
        
        var parameters = string.Join(",", functionNode.Parameters.Select(r => $"{r.Node.TypeFullName} {r.Node.Reference}"));
        return $"{accessibility}{asyncModifier}{functionNode.ReturnedTypeFullName} {explicitInterfaceFullName}{functionNode.Name}{typeParameters}({parameters}){typeParametersConstraints}";
    }
}