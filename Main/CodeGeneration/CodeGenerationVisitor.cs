using System.Threading;
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
}

internal sealed class CodeGenerationVisitor : ICodeGenerationVisitor
{
    private readonly IReferenceGenerator _referenceGenerator;
    private readonly StringBuilder _code = new();
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly WellKnownTypesCollections _wellKnownTypesCollections;

    internal CodeGenerationVisitor(
        WellKnownTypes wellKnownTypes,
        WellKnownTypesCollections wellKnownTypesCollections,
        IReferenceGenerator referenceGenerator)
    {
        _referenceGenerator = referenceGenerator;
        _wellKnownTypes = wellKnownTypes;
        _wellKnownTypesCollections = wellKnownTypesCollections;
    }

    public void VisitIContainerNode(IContainerNode container) => 
        container.GetGenerator().Generate(_code, this);

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
            var asyncPrefix = initialization.Awaited
                ? "await "
                : "";

            _code.AppendLine(
                $"{asyncPrefix}{ownerReference}.{initialization.FunctionName}({string.Join(", ", initialization.Parameters.Select(p =>$"{p.Item1.Reference.PrefixAtIfKeyword()}: {p.Item2.Reference}"))});");
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

    private void VisitISingleFunctionNode(ISingleFunctionNode singleFunction)
    {
        _code.AppendLine(
            $$"""
              {{GenerateMethodDeclaration(singleFunction)}}
              {
              """);
        
        VisitIElementNode(singleFunction.TransientScopeDisposalNode);
        VisitIElementNode(singleFunction.SubDisposalNode);
        
        var handlesDisposal = 
            !singleFunction.IsSubDisposalAsParameter || !singleFunction.IsTransientScopeDisposalAsParameter;

        if (handlesDisposal)
        {
            ObjectDisposedCheck(
                singleFunction.DisposedPropertyReference, 
                singleFunction.RangeFullName, 
                singleFunction.ReturnedTypeFullName);
            _code.AppendLine(
                $$"""
                  try
                  {
                  {{_wellKnownTypes.Interlocked.FullName()}}.{{nameof(Interlocked.Increment)}}(ref {{singleFunction.ResolutionCounterReference}});
                  """);
        }
        
        VisitIElementNode(singleFunction.ReturnedElement);
        
        if (handlesDisposal)
            ObjectDisposedCheck(
                singleFunction.DisposedPropertyReference, 
                singleFunction.RangeFullName, 
                singleFunction.ReturnedTypeFullName);

        if (!singleFunction.IsSubDisposalAsParameter)
        {
            _code.AppendLine(
                $"{singleFunction.DisposalCollectionReference}.{nameof(List<List<object>>.Add)}({singleFunction.SubDisposalNode.Reference});");
        }

        if (!singleFunction.IsTransientScopeDisposalAsParameter)
        {
            var containerReference = singleFunction.ContainerReference is { } reference
                ? $"{reference}."
                : "";
            _code.AppendLine(
                $"{containerReference}{singleFunction.TransientScopeDisposalReference}.{nameof(List<object>.AddRange)}({singleFunction.TransientScopeDisposalNode.Reference});");
        }
        
        _code.AppendLine($"return {singleFunction.ReturnedElement.Reference};");
        
        if (handlesDisposal)
        {
            var isAsync = singleFunction.SynchronicityDecision
                    is SynchronicityDecision.AsyncTask or SynchronicityDecision.AsyncValueTask;
            
            var parameters = !singleFunction.IsTransientScopeDisposalAsParameter
                ? $"exception, {singleFunction.SubDisposalNode.Reference}, {singleFunction.TransientScopeDisposalNode.Reference}"
                : $"exception, {singleFunction.SubDisposalNode.Reference}";
            
            var throwLine = isAsync && _wellKnownTypes.IAsyncDisposable is not null && _wellKnownTypes.ValueTask is not null
                ? $"throw await {singleFunction.DisposeExceptionHandlingAsyncMethodName}({parameters});"
                : $"throw {singleFunction.DisposeExceptionHandlingMethodName}({parameters});";
    
            _code.AppendLine(
                $$"""
                  }
                  catch({{_wellKnownTypes.Exception.FullName()}} exception)
                  {
                  {{throwLine}}
                  }
                  finally
                  {
                  {{_wellKnownTypes.Interlocked.FullName()}}.{{nameof(Interlocked.Decrement)}}(ref {{singleFunction.ResolutionCounterReference}});
                  }
                  """);
        }
            
        foreach (var localFunction in singleFunction.LocalFunctions)
            VisitILocalFunctionNode(localFunction);
        
        _code.AppendLine("}");
        return;

        void ObjectDisposedCheck(
            string disposedPropertyReference,
            string rangeFullName,
            string returnTypeFullName) => _code.AppendLine(
            $"if ({disposedPropertyReference}) throw new {_wellKnownTypes.ObjectDisposedException}(\"{rangeFullName}\", $\"[DIE] This scope \\\"{rangeFullName}\\\" is already disposed, so it can't create a \\\"{returnTypeFullName}\\\" instance anymore.\");");
    }

    public void VisitICreateFunctionNode(ICreateFunctionNode createFunction) => VisitISingleFunctionNode(createFunction);
    public void VisitIEntryFunctionNode(IEntryFunctionNode entryFunction) => VisitISingleFunctionNode(entryFunction);
    public void VisitILocalFunctionNode(ILocalFunctionNode localFunction) => VisitISingleFunctionNode(localFunction);
    public void VisitIRangedInstanceFunctionNode(IRangedInstanceFunctionNode rangedInstanceFunctionNode)
    {
        // Nothing to do here. It's generated in "VisitRangedInstanceFunctionGroupNode"
    }

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
                $"private {_wellKnownTypes.SemaphoreSlim.FullName()} {rangedInstanceFunctionGroupNode.LockReference} = new {_wellKnownTypes.SemaphoreSlim.FullName()}(1);");
            var typeHandleField = _referenceGenerator.Generate("typeHandle");

            foreach (var overload in rangedInstanceFunctionGroupNode.Overloads)
            {
                _code.AppendLine(GenerateMethodDeclaration(overload));
                
                var instance0 = _referenceGenerator.Generate("instance");
                var instance1 = _referenceGenerator.Generate("instance");
                var instance2 = _referenceGenerator.Generate("instance");
                var instance3 = _referenceGenerator.Generate("instance");
                
                var isAsync =
                    overload.SynchronicityDecision is SynchronicityDecision.AsyncTask or SynchronicityDecision.AsyncValueTask;
                _code.AppendLine(
                    $$"""
                      {
                      var {{typeHandleField}} = typeof({{overload.ReturnedElement.TypeFullName}}).TypeHandle;
                      if ({{rangedInstanceFunctionGroupNode.RangedInstanceStorageFieldName}}.TryGetValue({{typeHandleField}}, out {{_wellKnownTypes.Object.FullName()}}? {{instance0}}) && {{instance0}} is {{overload.ReturnedElement.TypeFullName}} {{instance1}}) return {{instance1}};
                      {{(isAsync ? "await " : "")}}{{Constants.ThisKeyword}}.{{rangedInstanceFunctionGroupNode.LockReference}}.Wait{{(isAsync ? "Async" : "")}}();
                      try
                      {
                      if ({{rangedInstanceFunctionGroupNode.RangedInstanceStorageFieldName}}.TryGetValue({{typeHandleField}}, out {{_wellKnownTypes.Object.FullName()}}? {{instance2}}) && {{instance2}} is {{overload.ReturnedElement.TypeFullName}} {{instance3}}) return {{instance3}};
                      """);
                
                VisitIElementNode(overload.ReturnedElement);

                _code.AppendLine(
                    $$"""
                      {{rangedInstanceFunctionGroupNode.RangedInstanceStorageFieldName}}[{{typeHandleField}}] = {{overload.ReturnedElement.Reference}};
                      return {{overload.ReturnedElement.Reference}};
                      }
                      finally
                      {
                      {{Constants.ThisKeyword}}.{{rangedInstanceFunctionGroupNode.LockReference}}.Release();
                      }
                      """);
                foreach (var localFunction in overload.LocalFunctions)
                    VisitILocalFunctionNode(localFunction);
                _code.AppendLine("}");
            }
        }

        void RefLike()
        {
            _code.AppendLine(
                $$"""
                  private {{rangedInstanceFunctionGroupNode.TypeFullName}}? {{rangedInstanceFunctionGroupNode.FieldReference}};
                  private {{_wellKnownTypes.SemaphoreSlim.FullName()}}? {{rangedInstanceFunctionGroupNode.LockReference}} = new {{_wellKnownTypes.SemaphoreSlim.FullName()}}(1);
                  """);

            foreach (var overload in rangedInstanceFunctionGroupNode.Overloads)
            {
                _code.AppendLine(GenerateMethodDeclaration(overload));

                var checkAndReturnAlreadyCreatedInstance = $"if (!{_wellKnownTypes.Object.FullName()}.ReferenceEquals({rangedInstanceFunctionGroupNode.FieldReference}, null)) return {rangedInstanceFunctionGroupNode.FieldReference};";
                
                var waitLine = overload.SynchronicityDecision is SynchronicityDecision.AsyncTask or SynchronicityDecision.AsyncValueTask
                    ? $"await ({Constants.ThisKeyword}.{rangedInstanceFunctionGroupNode.LockReference}?.WaitAsync() ?? {_wellKnownTypes.Task.FullName()}.{nameof(Task.CompletedTask)});"
                    : $"{Constants.ThisKeyword}.{rangedInstanceFunctionGroupNode.LockReference}?.Wait();";
                
                _code.AppendLine(
                    $$"""
                      {
                      {{checkAndReturnAlreadyCreatedInstance}}
                      {{waitLine}}
                      try
                      {
                      {{checkAndReturnAlreadyCreatedInstance}}
                      """);
                
                VisitIElementNode(overload.ReturnedElement);

                _code.AppendLine(
                    $$"""
                      {{rangedInstanceFunctionGroupNode.FieldReference}} = {{overload.ReturnedElement.Reference}};
                      }
                      finally
                      {
                      {{Constants.ThisKeyword}}.{{rangedInstanceFunctionGroupNode.LockReference}}?.Release();
                      }
                      {{Constants.ThisKeyword}}.{{rangedInstanceFunctionGroupNode.LockReference}} = null;
                      return {{Constants.ThisKeyword}}.{{rangedInstanceFunctionGroupNode.FieldReference}};
                      """);
                
                foreach (var localFunction in overload.LocalFunctions)
                    VisitILocalFunctionNode(localFunction);
                _code.AppendLine("}");
            }
        }

        void Struct()
        {
            _code.AppendLine($$"""
                private {{rangedInstanceFunctionGroupNode.TypeFullName}} {{rangedInstanceFunctionGroupNode.FieldReference}};
                private {{_wellKnownTypes.SemaphoreSlim.FullName()}}? {{rangedInstanceFunctionGroupNode.LockReference}} = new {{_wellKnownTypes.SemaphoreSlim.FullName()}}(1);
                """);

            _code.AppendLine($"private bool {rangedInstanceFunctionGroupNode.IsCreatedForStructs};");

            foreach (var overload in rangedInstanceFunctionGroupNode.Overloads)
            {
                _code.AppendLine(GenerateMethodDeclaration(overload));

                var checkAndReturnAlreadyCreatedInstance = $"if ({rangedInstanceFunctionGroupNode.IsCreatedForStructs}) return {rangedInstanceFunctionGroupNode.FieldReference};";
                
                var waitLine = overload.SynchronicityDecision is SynchronicityDecision.AsyncTask or SynchronicityDecision.AsyncValueTask
                    ? $"await ({Constants.ThisKeyword}.{rangedInstanceFunctionGroupNode.LockReference}?.WaitAsync() ?? {_wellKnownTypes.Task.FullName()}.{nameof(Task.CompletedTask)});"
                    : $"{Constants.ThisKeyword}.{rangedInstanceFunctionGroupNode.LockReference}?.Wait();";

                _code.AppendLine(
                    $$"""
                      {
                      {{checkAndReturnAlreadyCreatedInstance}}
                      {{waitLine}}
                      try
                      {
                      {{checkAndReturnAlreadyCreatedInstance}}
                      """);
                
                VisitIElementNode(overload.ReturnedElement);

                _code.AppendLine(
                    $$"""
                      {{rangedInstanceFunctionGroupNode.FieldReference}} = {{overload.ReturnedElement.Reference}};
                      {{rangedInstanceFunctionGroupNode.IsCreatedForStructs}} = true;
                      }
                      finally
                      {
                      {{Constants.ThisKeyword}}.{{rangedInstanceFunctionGroupNode.LockReference}}?.Release();
                      }
                      {{Constants.ThisKeyword}}.{{rangedInstanceFunctionGroupNode.LockReference}} = null;
                      return {{Constants.ThisKeyword}}.{{rangedInstanceFunctionGroupNode.FieldReference}};
                      """);
            
                foreach (var localFunction in overload.LocalFunctions)
                    VisitILocalFunctionNode(localFunction);
                _code.AppendLine("}");
            }
        }
    }

    public void VisitIInitialSubDisposalNode(IInitialSubDisposalNode element) => 
        _code.AppendLine($"{element.TypeFullName} {element.Reference} = new {element.TypeFullName}();");

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
            .Append($"{functionCallNode.TransientScopeDisposalParameter.Called.Reference}: {functionCallNode.TransientScopeDisposalParameter.Calling.Reference}"));
        var typeParameters = functionCallNode.TypeParameters.Any()
            ? $"<{string.Join(", ", functionCallNode.TypeParameters.Select(p => p.Name))}>"
            : "";
        var call = $"{owner}{functionCallNode.FunctionName}{typeParameters}({parameters})";
        call = functionCallNode.Transformation switch
        {
            AsyncFunctionCallTransformation.ValueTaskFromValueTask => call,
            AsyncFunctionCallTransformation.ValueTaskFromTask => $"new {typeFullName}({call})",
            AsyncFunctionCallTransformation.ValueTaskFromSync => $"new {typeFullName}({call})",
            AsyncFunctionCallTransformation.TaskFromValueTask => $"{call}.AsTask()",
            AsyncFunctionCallTransformation.TaskFromTask => call,
            AsyncFunctionCallTransformation.TaskFromSync => $"{_wellKnownTypes.Task}.FromResult({call})",
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
            .Append($"{functionCallNode.TransientScopeDisposalParameter.Called.Reference}: {functionCallNode.TransientScopeDisposalParameter.Calling.Reference}"));
        var typeParameters = functionCallNode.TypeParameters.Any()
            ? $"<{string.Join(", ", functionCallNode.TypeParameters.Select(p => p.Name))}>"
            : "";
        var call = $"{owner}{functionCallNode.FunctionName}{typeParameters}({parameters})";
        call = functionCallNode.Awaited ? $"(await {call})" : call;
        _code.AppendLine($"{typeFullName} {functionCallNode.Reference} = ({typeFullName}){call};");
    }

    public void VisitICreateScopeFunctionNode(ICreateScopeFunctionNode element) => 
        VisitISingleFunctionNode(element);

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
        VisitISingleFunctionNode(element);

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
            case IInitialSubDisposalNode initialSubDisposalNode:
                VisitIInitialSubDisposalNode(initialSubDisposalNode);
                break;
        }
    }

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
        
        foreach (var returnedElement in multiFunctionNodeBase.ReturnedElements)
        {
            VisitIElementNode(returnedElement);
            if (multiFunctionNodeBase.SynchronicityDecision == SynchronicityDecision.Sync)
                _code.AppendLine($"yield return {returnedElement.Reference};");
        }
            
        foreach (var localFunction in multiFunctionNodeBase.LocalFunctions)
            VisitILocalFunctionNode(localFunction);
        
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
        
        VisitIElementNode(voidFunctionNode.SubDisposalNode);
        VisitIElementNode(voidFunctionNode.TransientScopeDisposalNode);
        
        _code.AppendLine(
            $$"""
              try
              {
              {{_wellKnownTypes.Interlocked.FullName()}}.{{nameof(Interlocked.Increment)}}(ref {{voidFunctionNode.ResolutionCounterReference}});
              """);
        
        foreach (var (functionCallNode, initializedInstanceNode) in voidFunctionNode.Initializations)
        {
            VisitIElementNode(functionCallNode);
            _code.AppendLine($"{initializedInstanceNode.Reference} = {functionCallNode.Reference};");
        }

        if (!voidFunctionNode.IsSubDisposalAsParameter)
        {
            _code.AppendLine(
                $"{voidFunctionNode.DisposalCollectionReference}.{nameof(List<List<object>>.Add)}({voidFunctionNode.SubDisposalNode.Reference});");
        }

        if (!voidFunctionNode.IsTransientScopeDisposalAsParameter)
        {
            var containerReference = voidFunctionNode.ContainerReference is { } reference
                ? $"{reference}."
                : "";
            _code.AppendLine(
                $"{containerReference}{voidFunctionNode.TransientScopeDisposalReference}.{nameof(List<object>.AddRange)}({voidFunctionNode.TransientScopeDisposalNode.Reference});");
        }
        
        var isAsync = voidFunctionNode.SynchronicityDecision
            is SynchronicityDecision.AsyncTask or SynchronicityDecision.AsyncValueTask;
        
        var parameters = !voidFunctionNode.IsTransientScopeDisposalAsParameter
            ? $"exception, {voidFunctionNode.SubDisposalNode.Reference}, {voidFunctionNode.TransientScopeDisposalNode.Reference}"
            : $"exception, {voidFunctionNode.SubDisposalNode.Reference}";
        
        var throwLine = isAsync && _wellKnownTypes.IAsyncDisposable is not null && _wellKnownTypes.ValueTask is not null
            ? $"throw await {voidFunctionNode.DisposeExceptionHandlingAsyncMethodName}({parameters});"
            : $"throw {voidFunctionNode.DisposeExceptionHandlingMethodName}({parameters});";

        _code.AppendLine(
            $$"""
              }
              catch({{_wellKnownTypes.Exception.FullName()}} exception)
              {
              {{throwLine}}
              }
              finally
              {
              {{_wellKnownTypes.Interlocked.FullName()}}.{{nameof(Interlocked.Decrement)}}(ref {{voidFunctionNode.ResolutionCounterReference}});
              }
              """);
            
        foreach (var localFunction in voidFunctionNode.LocalFunctions)
            VisitILocalFunctionNode(localFunction);
        
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
        
        var parameters = string.Join(",", functionNode
            .Parameters
            .Select(r => $"{r.Node.TypeFullName} {r.Node.Reference}")
            .AppendIf($"{functionNode.SubDisposalNode.TypeFullName} {functionNode.SubDisposalNode.Reference}", functionNode.IsSubDisposalAsParameter)
            .AppendIf($"{functionNode.TransientScopeDisposalNode.TypeFullName} {functionNode.TransientScopeDisposalNode.Reference}", functionNode.IsTransientScopeDisposalAsParameter));
        return $"{accessibility}{asyncModifier}{functionNode.ReturnedTypeFullName} {explicitInterfaceFullName}{functionNode.Name}{typeParameters}({parameters}){typeParametersConstraints}";
    }
}