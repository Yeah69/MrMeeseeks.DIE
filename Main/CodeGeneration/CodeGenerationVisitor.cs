using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.Delegates;
using MrMeeseeks.DIE.Nodes.Elements.Factories;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Elements.Tuples;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.CodeGeneration;

internal interface ICodeGenerationVisitor : INodeVisitor
{
    string GenerateContainerFile();
    void VisitICreateFunctionNodeBase(ICreateFunctionNodeBase element);
    void VisitIElementNode(IElementNode elementNode);
    CodeGenerationFunctionVisitor CreateNestedFunctionVisitor(
        ReturnTypeStatus returnTypeStatus,
        AsyncAwaitStatus asyncAwaitStatus);
}

internal sealed class CodeGenerationVisitor : CodeGenerationVisitorBase
{
    internal CodeGenerationVisitor(
        WellKnownTypes wellKnownTypes,
        WellKnownTypesCollections wellKnownTypesCollections,
        Func<StringBuilder, ReturnTypeStatus, AsyncAwaitStatus, CodeGenerationFunctionVisitor> codeGenerationFunctionVisitorFactory)
        : base(new(), ReturnTypeStatus.Ordinary, AsyncAwaitStatus.No, wellKnownTypes, wellKnownTypesCollections, codeGenerationFunctionVisitorFactory)
    {
    }
}

internal sealed class CodeGenerationFunctionVisitor : CodeGenerationVisitorBase
{
    internal CodeGenerationFunctionVisitor(
        // parameters
        StringBuilder code,
        ReturnTypeStatus returnTypeStatus,
        AsyncAwaitStatus asyncAwaitStatus,
        
        // dependencies
        WellKnownTypes wellKnownTypes,
        WellKnownTypesCollections wellKnownTypesCollections,
        Func<StringBuilder, ReturnTypeStatus, AsyncAwaitStatus, CodeGenerationFunctionVisitor> codeGenerationFunctionVisitorFactory)
        : base(code, returnTypeStatus, asyncAwaitStatus, wellKnownTypes, wellKnownTypesCollections, codeGenerationFunctionVisitorFactory)
    {
    }
}

internal class CodeGenerationVisitorBase : ICodeGenerationVisitor
{
    private readonly StringBuilder _code;
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly WellKnownTypesCollections _wellKnownTypesCollections;
    private readonly Func<StringBuilder, ReturnTypeStatus, AsyncAwaitStatus, CodeGenerationFunctionVisitor> _codeGenerationFunctionVisitorFactory;
    private readonly ReturnTypeStatus _returnTypeStatus;
    private readonly AsyncAwaitStatus _asyncAwaitStatus;

    internal CodeGenerationVisitorBase(
        // parameters
        StringBuilder code,
        ReturnTypeStatus returnTypeStatus,
        AsyncAwaitStatus asyncAwaitStatus,
        
        // dependencies
        WellKnownTypes wellKnownTypes,
        WellKnownTypesCollections wellKnownTypesCollections,
        Func<StringBuilder, ReturnTypeStatus, AsyncAwaitStatus, CodeGenerationFunctionVisitor> codeGenerationFunctionVisitorFactory)
    {
        _code = code;
        _wellKnownTypes = wellKnownTypes;
        _wellKnownTypesCollections = wellKnownTypesCollections;
        _codeGenerationFunctionVisitorFactory = codeGenerationFunctionVisitorFactory;
        _returnTypeStatus = returnTypeStatus;
        _asyncAwaitStatus = asyncAwaitStatus;
    }

    private bool CurrentFunctionAsyncAwait =>
        _returnTypeStatus.HasFlag(ReturnTypeStatus.ValueTask) || _returnTypeStatus.HasFlag(ReturnTypeStatus.Task) 
        && _asyncAwaitStatus is AsyncAwaitStatus.Yes;

    public void VisitIContainerNode(IContainerNode container) => 
        container.GetGenerator().Generate(_code, this);

    public void VisitICreateContainerFunctionNode(ICreateContainerFunctionNode createContainerFunction)
    {
        var isAsyncAwait = createContainerFunction.ReturnTypeStatus.HasFlag(ReturnTypeStatus.Task) || 
                           createContainerFunction.ReturnTypeStatus.HasFlag(ReturnTypeStatus.ValueTask);
        var asyncPrefix = isAsyncAwait
            ? "async "
            : "";
        var awaitPrefix = isAsyncAwait
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
            private interface {{transientScopeInterface.Name}} : {{GenerateDisposalInterfaceAssignments()}}
            {
            """);
        foreach (var rangedInstanceInterfaceFunctionNode in transientScopeInterface.Functions)
            VisitIRangedInstanceInterfaceFunctionNode(rangedInstanceInterfaceFunctionNode);
        
        _code.AppendLine("}");
    }

    public void VisitIScopeNode(IScopeNode scope) => 
        scope.GetGenerator().Generate(_code, this);

    public void VisitITransientScopeNode(ITransientScopeNode transientScope) => 
        transientScope.GetGenerator().Generate(_code, this);

    private string GenerateDisposalInterfaceAssignments() =>
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

    public void VisitIInitialTransientScopeSubDisposalNode(IInitialTransientScopeSubDisposalNode element)
    {
        VisitIInitialSubDisposalNode(element);
    }

    public void VisitIScopeCallNode(IScopeCallNode scopeCall)
    {
        VisitIElementNode(scopeCall.ScopeConstruction);
        _code.AppendLine($"{scopeCall.SubDisposalReference}.{nameof(List<object>.Add)}({scopeCall.ScopeConstruction.Reference});");
        GenerateInitialization(scopeCall.Initialization, scopeCall.ScopeConstruction.Reference);
        VisitIFunctionCallNode(scopeCall);
    }

    public void VisitITransientScopeCallNode(ITransientScopeCallNode transientScopeCall)
    {
        VisitIElementNode(transientScopeCall.ScopeConstruction);
        var owner = transientScopeCall.ContainerReference is { } containerReference
            ? $"{containerReference}."
            : "";
        _code.AppendLine($"{owner}{transientScopeCall.TransientScopeDisposalReference}.{nameof(List<object>.Add)}({transientScopeCall.ScopeConstruction.Reference});");
        GenerateInitialization(transientScopeCall.Initialization, transientScopeCall.ScopeConstruction.Reference);
        VisitIFunctionCallNode(transientScopeCall);
    }

    public void VisitIReferenceNode(IReferenceNode element)
    {
        // Nothing to do here. This element just gets referenced elsewhere.
    }

    private void GenerateInitialization(IFunctionCallNode? maybeInitialization, string ownerReference)
    {
        if (maybeInitialization is { } initialization)
        {
            var asyncPrefix = CurrentFunctionAsyncAwait
                ? "await "
                : "";

            _code.AppendLine(
                $"{asyncPrefix}{ownerReference}.{initialization.FunctionName(_returnTypeStatus)}({string.Join(", ", initialization.Parameters.Select(p =>$"{p.Item1.Reference.PrefixAtIfKeyword()}: {p.Item2.Reference}"))});");
        }
    }

    public void VisitICreateFunctionNodeBase(ICreateFunctionNodeBase element)
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

    public void VisitICreateFunctionNode(ICreateFunctionNode createFunction) => 
        createFunction.GetGenerator().Generate(_code, this);
    public void VisitIEntryFunctionNode(IEntryFunctionNode entryFunction) => 
        entryFunction.GetGenerator().Generate(_code, this);
    public void VisitILocalFunctionNode(ILocalFunctionNode localFunction) => 
        localFunction.GetGenerator().Generate(_code, this);
    public void VisitIRangedInstanceFunctionNode(IRangedInstanceFunctionNode rangedInstanceFunctionNode) => 
        rangedInstanceFunctionNode.GetGenerator().Generate(_code, this);

    public void VisitIRangedInstanceInterfaceFunctionNode(IRangedInstanceInterfaceFunctionNode rangedInstanceInterfaceFunctionNode)
    {
        var parameter = string.Join(
            ",",
            rangedInstanceInterfaceFunctionNode
                .Parameters
                .Select(r => $"{r.Node.TypeFullName} {r.Node.Reference}")
                .AppendIf(
                    $"{rangedInstanceInterfaceFunctionNode.SubDisposalNode.TypeFullName} {rangedInstanceInterfaceFunctionNode.SubDisposalNode.Reference}", 
                    rangedInstanceInterfaceFunctionNode.IsSubDisposalAsParameter)
                .AppendIf(
                    $"{rangedInstanceInterfaceFunctionNode.TransientScopeDisposalNode.TypeFullName} {rangedInstanceInterfaceFunctionNode.TransientScopeDisposalNode.Reference}",
                    rangedInstanceInterfaceFunctionNode.IsTransientScopeDisposalAsParameter));
        var consideredStatuses = Enum.GetValues(typeof(ReturnTypeStatus))
            .OfType<ReturnTypeStatus>()
            .Where(r => rangedInstanceInterfaceFunctionNode.ReturnTypeStatus.HasFlag(r));
        foreach (var status in consideredStatuses)
            _code.AppendLine($"{rangedInstanceInterfaceFunctionNode.ReturnedTypeFullName(status)} {rangedInstanceInterfaceFunctionNode.Name(status)}({parameter});");
    }

    public void VisitIRangedInstanceFunctionGroupNode(IRangedInstanceFunctionGroupNode rangedInstanceFunctionGroupNode)
    {
        if (rangedInstanceFunctionGroupNode.IsOpenGeneric)
            _code.AppendLine(
                $"private {_wellKnownTypes.SemaphoreSlim.FullName()} {rangedInstanceFunctionGroupNode.LockReference} = new {_wellKnownTypes.SemaphoreSlim.FullName()}(1);");
        else if (rangedInstanceFunctionGroupNode.IsCreatedForStructs is null)
            _code.AppendLine(
                $$"""
                  private {{rangedInstanceFunctionGroupNode.TypeFullName}}? {{rangedInstanceFunctionGroupNode.FieldReference}};
                  private {{_wellKnownTypes.SemaphoreSlim.FullName()}}? {{rangedInstanceFunctionGroupNode.LockReference}} = new {{_wellKnownTypes.SemaphoreSlim.FullName()}}(1);
                  """);
        else
            _code.AppendLine(
                $$"""
                  private {{rangedInstanceFunctionGroupNode.TypeFullName}} {{rangedInstanceFunctionGroupNode.FieldReference}};
                  private {{_wellKnownTypes.SemaphoreSlim.FullName()}}? {{rangedInstanceFunctionGroupNode.LockReference}} = new {{_wellKnownTypes.SemaphoreSlim.FullName()}}(1);
                  private bool {{rangedInstanceFunctionGroupNode.IsCreatedForStructs}};
                  """);
        
        foreach (var overload in rangedInstanceFunctionGroupNode.Overloads)
        {
            overload.Group = rangedInstanceFunctionGroupNode;
            VisitIRangedInstanceFunctionNode(overload);
            overload.Group = null;
        }
    }

    public void VisitIInitialSubDisposalNode(IInitialSubDisposalNode element) => 
        _code.AppendLine($"{element.TypeFullName} {element.Reference} = new {element.TypeFullName}({element.SubDisposalCount});");

    public void VisitIWrappedAsyncFunctionCallNode(IWrappedAsyncFunctionCallNode functionCallNode)
    {
        var owner = functionCallNode.OwnerReference is { } ownerReference ? $"{ownerReference}." : ""; 
        var typeFullName = functionCallNode.TypeFullName;
        var parameters = string.Join(", ", functionCallNode
            .Parameters
            .Select(p => $"{p.Item1.Reference.PrefixAtIfKeyword()}: {p.Item2.Reference}")
            .AppendIf(
                $"{functionCallNode.SubDisposalParameter?.Called.Reference}: {functionCallNode.SubDisposalParameter?.Calling.Reference}",
                functionCallNode.SubDisposalParameter is not null)
            .AppendIf(
                $"{functionCallNode.TransientScopeDisposalParameter?.Called.Reference}: {functionCallNode.TransientScopeDisposalParameter?.Calling.Reference}",
                functionCallNode.TransientScopeDisposalParameter is not null));
        var typeParameters = functionCallNode.TypeParameters.Any()
            ? $"<{string.Join(", ", functionCallNode.TypeParameters.Select(p => p.Name))}>"
            : "";
        var functionName = functionCallNode.Transformation switch
        {
            AsyncFunctionCallTransformation.ValueTaskFromValueTask or AsyncFunctionCallTransformation.ValueTaskFromForcedValueTask
                or AsyncFunctionCallTransformation.TaskFromValueTask or AsyncFunctionCallTransformation.TaskFromForcedValueTask 
                => functionCallNode.CalledFunction.Name(ReturnTypeStatus.ValueTask),
            AsyncFunctionCallTransformation.ValueTaskFromTask or AsyncFunctionCallTransformation.ValueTaskFromForcedTask
                or AsyncFunctionCallTransformation.TaskFromTask or AsyncFunctionCallTransformation.TaskFromForcedTask 
                => functionCallNode.CalledFunction.Name(ReturnTypeStatus.Task),
            AsyncFunctionCallTransformation.ValueTaskFromSync or AsyncFunctionCallTransformation.TaskFromSync
                => functionCallNode.CalledFunction.Name(ReturnTypeStatus.Ordinary),
            _ => throw new ArgumentOutOfRangeException(nameof(functionCallNode), $"Switch in DIE type {nameof(CodeGenerationVisitor)} is not exhaustive.")
        };
        var call = $"{owner}{functionName}{typeParameters}({parameters})";
        call = functionCallNode.Transformation switch
        {
            AsyncFunctionCallTransformation.ValueTaskFromValueTask => $"new {typeFullName}({call})",
            AsyncFunctionCallTransformation.ValueTaskFromForcedValueTask => call,
            AsyncFunctionCallTransformation.ValueTaskFromTask => $"new {typeFullName}({call})",
            AsyncFunctionCallTransformation.ValueTaskFromForcedTask => $"new {typeFullName}({call})",
            AsyncFunctionCallTransformation.ValueTaskFromSync => $"new {typeFullName}({call})",
            AsyncFunctionCallTransformation.TaskFromValueTask => $"{_wellKnownTypes.Task.FullName()}.FromResult({call})",
            AsyncFunctionCallTransformation.TaskFromForcedValueTask => $"{call}.AsTask()",
            AsyncFunctionCallTransformation.TaskFromTask => $"{_wellKnownTypes.Task.FullName()}.FromResult({call})",
            AsyncFunctionCallTransformation.TaskFromForcedTask => call,
            AsyncFunctionCallTransformation.TaskFromSync => $"{_wellKnownTypes.Task.FullName()}.FromResult({call})",
            _ => throw new ArgumentOutOfRangeException(nameof(functionCallNode), $"Switch in DIE type {nameof(CodeGenerationVisitor)} is not exhaustive.")
        };
        _code.AppendLine($"{typeFullName} {functionCallNode.Reference} = ({typeFullName}){call};");
    }

    private void VisitIFunctionCallNode(IFunctionCallNode functionCallNode)
    {
        var owner = functionCallNode.OwnerReference is { } ownerReference ? $"{ownerReference}." : ""; 
        var typeFullName = functionCallNode.TypeFullName;
        var parameters = string.Join(", ", functionCallNode
            .Parameters
            .Select(p => $"{p.Item1.Reference.PrefixAtIfKeyword()}: {p.Item2.Reference}")
            .AppendIf(
                $"{functionCallNode.SubDisposalParameter?.Called.Reference}: {functionCallNode.SubDisposalParameter?.Calling.Reference}",
                functionCallNode.SubDisposalParameter is not null)
            .AppendIf(
                $"{functionCallNode.TransientScopeDisposalParameter?.Called.Reference}: {functionCallNode.TransientScopeDisposalParameter?.Calling.Reference}",
                functionCallNode.TransientScopeDisposalParameter is not null));
        var typeParameters = functionCallNode.TypeParameters.Any()
            ? $"<{string.Join(", ", functionCallNode.TypeParameters.Select(p => p.Name))}>"
            : "";
        var functionName = _returnTypeStatus.HasFlag(ReturnTypeStatus.Ordinary)
            ? functionCallNode.FunctionName(ReturnTypeStatus.Ordinary)
            : functionCallNode.CalledFunction.ReturnTypeStatus.HasFlag(ReturnTypeStatus.ValueTask) 
                ? functionCallNode.FunctionName(ReturnTypeStatus.ValueTask)
                : functionCallNode.FunctionName(ReturnTypeStatus.Task);
        var call = $"{owner}{functionName}{typeParameters}({parameters})";
        call = CurrentFunctionAsyncAwait && functionCallNode.CalledFunction is not IMultiFunctionNodeBase { IsAsyncEnumerable: true } 
            ? $"(await {call})" 
            : call;
        _code.AppendLine($"{typeFullName} {functionCallNode.Reference} = ({typeFullName}){call};");
    }

    public void VisitICreateScopeFunctionNode(ICreateScopeFunctionNode element) => 
        element.GetGenerator().Generate(_code, this);

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
        element.GetGenerator().Generate(_code, this);

    public void VisitIFuncNode(IFuncNode funcNode) =>
        _code.AppendLine($"{funcNode.TypeFullName} {funcNode.Reference} = {funcNode.MethodGroup};");

    public void VisitIImplicitScopeImplementationNode(IImplicitScopeImplementationNode element)
    {
        foreach (var (_, subElement) in element.Properties)
            VisitIElementNode(subElement);
        _code.AppendLine($"{element.TypeFullName} {element.Reference} = new {element.TypeFullName}() {{ {string.Join(", ", element.Properties.Select(t => $"{t.Name} = {t.Element.Reference}"))} }};");
    }

    public void VisitILazyNode(ILazyNode lazyNode) => 
        _code.AppendLine($"{lazyNode.TypeFullName} {lazyNode.Reference} = new {lazyNode.TypeFullName}({lazyNode.MethodGroup});");
    
    public void VisitIThreadLocalNode(IThreadLocalNode threadLocalNode)
    {
        _code.AppendLine(
            $"{threadLocalNode.TypeFullName} {threadLocalNode.Reference} = new {threadLocalNode.TypeFullName}({threadLocalNode.MethodGroup}, false);");
        if (threadLocalNode.SubDisposalReference is {} subDisposalReference)
            _code.AppendLine($"{subDisposalReference}.Add({threadLocalNode.Reference});");
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

    public void VisitIElementNode(IElementNode elementNode)
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
            case IReferenceNode referenceNode:
                VisitIReferenceNode(referenceNode);
                break;
            case IImplicitScopeImplementationNode implicitScopeImplementationNode:
                VisitIImplicitScopeImplementationNode(implicitScopeImplementationNode);
                break;
            case IInitialOrdinarySubDisposalNode initialOrdinarySubDisposalNode:
                VisitIInitialOrdinarySubDisposalNode(initialOrdinarySubDisposalNode);
                break;
            case IInitialTransientScopeSubDisposalNode initialTransientScopeSubDisposalNode:
                VisitIInitialTransientScopeSubDisposalNode(initialTransientScopeSubDisposalNode);
                break;
        }
    }

    public CodeGenerationFunctionVisitor CreateNestedFunctionVisitor(ReturnTypeStatus returnTypeStatus,
        AsyncAwaitStatus asyncAwaitStatus) =>
        _codeGenerationFunctionVisitorFactory(_code, returnTypeStatus, asyncAwaitStatus);

    public void VisitIImplementationNode(IImplementationNode implementationNode)
    {
        if (implementationNode.UserDefinedInjectionConstructor is not null)
            ProcessUserDefinedInjection(implementationNode.UserDefinedInjectionConstructor);
        if (implementationNode.UserDefinedInjectionProperties is not null)
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

        if (implementationNode.AggregateForDisposal) 
            _code.AppendLine($"{implementationNode.SubDisposalReference}.{nameof(List<object>.Add)}({implementationNode.Reference});");

        if (implementationNode.Initializer is {} init)
        {
            if (init.UserDefinedInjection is not null)
                ProcessUserDefinedInjection(init.UserDefinedInjection);
            foreach (var (_, element) in init.Parameters)
                VisitIElementNode(element);
            var initializerParameters =
                string.Join(", ", init.Parameters.Select(d => $"{d.Name.PrefixAtIfKeyword()}: {d.Element.Reference}"));

            var prefix = implementationNode.InitializerReturnsSomeTask
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
        _code.AppendLine(
            $"{transientScopeDisposalTriggerNode.TypeFullName} {transientScopeDisposalTriggerNode.Reference} = {Constants.ThisKeyword} as {transientScopeDisposalTriggerNode.TypeFullName};");
    }

    public void VisitINullNode(INullNode nullNode) => _code.AppendLine(
        $"{nullNode.TypeFullName} {nullNode.Reference} = ({nullNode.TypeFullName}) null;");

    public void VisitIInitialOrdinarySubDisposalNode(IInitialOrdinarySubDisposalNode element) => 
        VisitIInitialSubDisposalNode(element);

    public void VisitIMultiFunctionNode(IMultiFunctionNode multiFunctionNode) =>
        multiFunctionNode.GetGenerator().Generate(_code, this);

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

    public void VisitIVoidFunctionNode(IVoidFunctionNode voidFunctionNode) =>
        voidFunctionNode.GetGenerator().Generate(_code, this);

    public void VisitIMultiKeyValueFunctionNode(IMultiKeyValueFunctionNode multiKeyValueFunctionNode) =>
        multiKeyValueFunctionNode.GetGenerator().Generate(_code, this);

    public void VisitIMultiKeyValueMultiFunctionNode(IMultiKeyValueMultiFunctionNode multiKeyValueMultiFunctionNode) =>
        multiKeyValueMultiFunctionNode.GetGenerator().Generate(_code, this);

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

    private readonly HashSet<IReusedNode> _doneReusedNodes = [];
    public void VisitIReusedNode(IReusedNode reusedNode)
    {
        if (!_doneReusedNodes.Add(reusedNode)) return;
        VisitIElementNode(reusedNode.Inner);
    }

    public string GenerateContainerFile() => _code.ToString();
}