using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.CodeGeneration.Nodes;

internal interface IFunctionNodeGenerator : INodeGenerator;

internal sealed class FunctionNodeGenerator : IFunctionNodeGenerator
{
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly IFunctionNode _function;
    private readonly IRangeNode _range;
    private readonly IContainerNode _container;
    private readonly IDisposeUtility _disposeUtility;
    private readonly IReferenceGenerator _referenceGenerator;

    internal FunctionNodeGenerator(
        WellKnownTypes wellKnownTypes,
        IFunctionNode function, 
        IRangeNode range,
        IContainerNode container,
        IDisposeUtility disposeUtility,
        IReferenceGenerator referenceGenerator)
    {
        _wellKnownTypes = wellKnownTypes;
        _function = function;
        _range = range;
        _container = container;
        _disposeUtility = disposeUtility;
        _referenceGenerator = referenceGenerator;
    }

    public void Generate(StringBuilder code, ICodeGenerationVisitor visitor)
    {
        var consideredStatuses = Enum.GetValues(typeof(ReturnTypeStatus))
            .OfType<ReturnTypeStatus>()
            .Where(r => _function.ReturnTypeStatus.HasFlag(r));
        foreach (var returnTypeStatus in consideredStatuses)
        {
            var functionVisitor = visitor.CreateNestedFunctionVisitor(returnTypeStatus, _function.AsyncAwaitStatus);
            var asyncAwaitStatus = returnTypeStatus is ReturnTypeStatus.Ordinary 
                ? AsyncAwaitStatus.No 
                : _function.AsyncAwaitStatus;
            GenerateOneFunction(
                code, 
                functionVisitor, 
                _function is IMultiFunctionNodeBase { IsAsyncEnumerable: true },
                returnTypeStatus, 
                asyncAwaitStatus);
        }
    }

    private void GenerateOneFunction(
        StringBuilder code, 
        ICodeGenerationVisitor visitor,
        bool isAsyncEnumerable,
        ReturnTypeStatus returnTypeStatus, 
        AsyncAwaitStatus asyncAwaitStatus)
    {
        var typeHandleField = new Lazy<string>(() => _referenceGenerator.Generate("typeHandle"));
        var instance0 = new Lazy<string>(() => _referenceGenerator.Generate("instance"));
        var instance1 = new Lazy<string>(() => _referenceGenerator.Generate("instance"));
        var instance2 = new Lazy<string>(() => _referenceGenerator.Generate("instance"));
        var instance3 = new Lazy<string>(() => _referenceGenerator.Generate("instance"));
        var isAsyncAwait = asyncAwaitStatus is AsyncAwaitStatus.Yes;
        var isSomeTask = returnTypeStatus.HasFlag(ReturnTypeStatus.Task) || returnTypeStatus.HasFlag(ReturnTypeStatus.ValueTask);
        var isTask = returnTypeStatus.HasFlag(ReturnTypeStatus.Task);
        var isValueTask = returnTypeStatus.HasFlag(ReturnTypeStatus.ValueTask);
        code.AppendLine(
            $$"""
              {{GenerateMethodDeclaration(_function, returnTypeStatus, isAsyncAwait)}}
              {
              """);
        
        if (isAsyncAwait)
            code.AppendLine($"await {_wellKnownTypes.Task.FullName()}.{nameof(Task.Yield)}();");
        
        visitor.VisitIElementNode(_function.TransientScopeDisposalNode);
        visitor.VisitIElementNode(_function.SubDisposalNode);

        switch (_function)
        {
            case IRangedInstanceFunctionNode { Group: { IsOpenGeneric: true} group } rangedInstanceFunctionNode:
                code.AppendLine(
                    $$"""
                      var {{typeHandleField.Value}} = typeof({{rangedInstanceFunctionNode.ReturnedElement.TypeFullName}}).TypeHandle;
                      if ({{group.RangedInstanceStorageFieldName}}.TryGetValue({{typeHandleField.Value}}, out {{_wellKnownTypes.Object.FullName()}}? {{instance0.Value}}) && {{instance0.Value}} is {{rangedInstanceFunctionNode.ReturnedElement.TypeFullName}} {{instance1.Value}}) return {{ReturnExpression(instance1.Value)}};
                      {{(isAsyncAwait ? "await " : "")}}{{Constants.ThisKeyword}}.{{group.LockReference}}.Wait{{(isAsyncAwait ? "Async" : "")}}();
                      try
                      {
                      if ({{group.RangedInstanceStorageFieldName}}.TryGetValue({{typeHandleField.Value}}, out {{_wellKnownTypes.Object.FullName()}}? {{instance2.Value}}) && {{instance2.Value}} is {{rangedInstanceFunctionNode.ReturnedElement.TypeFullName}} {{instance3.Value}}) return {{ReturnExpression(instance3.Value)}};
                      """);
                break;
            case IRangedInstanceFunctionNode { Group: { IsOpenGeneric: false } group }:
                var checkAndReturnAlreadyCreatedInstance = group.IsCreatedForStructs is { } createdReference
                    ? $"if ({createdReference}) return {ReturnExpression(group.FieldReference)};"
                    : $"if (!{_wellKnownTypes.Object.FullName()}.ReferenceEquals({group.FieldReference}, null)) return {ReturnExpression(group.FieldReference)};";

                var waitLine = isAsyncAwait
                    ? $"await ({Constants.ThisKeyword}.{group.LockReference}?.WaitAsync() ?? {_wellKnownTypes.Task.FullName()}.{nameof(Task.CompletedTask)});"
                    : $"{Constants.ThisKeyword}.{group.LockReference}?.Wait();";

                code.AppendLine(
                    $$"""
                      {{checkAndReturnAlreadyCreatedInstance}}
                      {{waitLine}}
                      try
                      {
                      {{checkAndReturnAlreadyCreatedInstance}}
                      """);
                break;
        }
        
        var handlesDisposal = 
            !_function.IsSubDisposalAsParameter || !_function.IsTransientScopeDisposalAsParameter;

        if (handlesDisposal)
        {
            ObjectDisposedCheck(
                _range.DisposalHandling.DisposedPropertyReference,
                _range.FullName, 
                _function.ReturnedTypeFullName(returnTypeStatus));
            code.AppendLine(
                $$"""
                  try
                  {
                  {{_wellKnownTypes.Interlocked.FullName()}}.{{nameof(Interlocked.Increment)}}(ref {{_range.ResolutionCounterReference}});
                  """);
        }
        
        switch (_function)
        {
            case ISingleFunctionNode singleFunctionNode:
                visitor.VisitIElementNode(singleFunctionNode.ReturnedElement);
                
                break;
            case IMultiFunctionNodeBase multiFunctionNode:
                foreach (var returnedElement in multiFunctionNode.ReturnedElements)
                {
                    visitor.VisitIElementNode(returnedElement);
                    if (!isSomeTask || isAsyncEnumerable) code.AppendLine($"yield return {returnedElement.Reference};");
                }

                break;
            case IVoidFunctionNode voidFunctionNode:
                foreach (var (functionCallNode, initializedInstanceNode) in voidFunctionNode.Initializations)
                {
                    visitor.VisitIElementNode(functionCallNode);
                    code.AppendLine($"{initializedInstanceNode.Reference} = {functionCallNode.Reference};");
                }
                
                break;
        }
        
        if (handlesDisposal)
            ObjectDisposedCheck(
                _range.DisposalHandling.DisposedPropertyReference, 
                _range.FullName, 
                _function.ReturnedTypeFullName(returnTypeStatus));

        if (!_function.IsSubDisposalAsParameter)
        {
            code.AppendLine(
                $"{_range.DisposalHandling.CollectionReference}.{nameof(ConcurrentStack<ConcurrentStack<object>>.Push)}({_function.SubDisposalNode.Reference});");
        }

        if (!_function.IsTransientScopeDisposalAsParameter)
        {
            var containerReference = _range.ContainerReference is { } reference
                ? $"{reference}."
                : "";
            code.AppendLine(
                $"{containerReference}{_container.TransientScopeDisposalReference}.{nameof(List<object>.AddRange)}({_function.TransientScopeDisposalNode.Reference});");
        }
        

        switch (_function)
        {
            case IRangedInstanceFunctionNode { Group: { IsOpenGeneric: true} group } rangedInstanceFunctionNode:
                code.AppendLine(
                    $$"""
                      {{group.RangedInstanceStorageFieldName}}[{{typeHandleField}}] = {{rangedInstanceFunctionNode.ReturnedElement.Reference}};
                      return {{ReturnExpression(rangedInstanceFunctionNode.ReturnedElement.Reference)}};
                      """);
                break;
            case IRangedInstanceFunctionNode { Group: { IsOpenGeneric: false } group } rangedInstanceFunctionNode:
                code.AppendLine($"{group.FieldReference} = {rangedInstanceFunctionNode.ReturnedElement.Reference};");
                break;
            case ISingleFunctionNode singleFunctionNode:
                var returnLine = _function.SelectAsyncSingleReturnStrategy(returnTypeStatus, isAsyncAwait) switch
                {
                    AsyncSingleReturnStrategy.Pass => $"return {singleFunctionNode.ReturnedElement.Reference};",
                    AsyncSingleReturnStrategy.Await =>  $"return await {singleFunctionNode.ReturnedElement.Reference};",
                    AsyncSingleReturnStrategy.ValueTaskFromResult => 
                        $"return new {_wellKnownTypes.ValueTask?.FullName()}<{singleFunctionNode.ReturnedTypeFullName(ReturnTypeStatus.Ordinary)}>({_wellKnownTypes.Task.FullName()}.FromResult({singleFunctionNode.ReturnedElement.Reference}));",
                    AsyncSingleReturnStrategy.TaskFromResult => 
                        $"return {_wellKnownTypes.Task.FullName()}.FromResult({singleFunctionNode.ReturnedElement.Reference});",
                    _ => throw new ArgumentOutOfRangeException()
                };
                code.AppendLine(returnLine);
                break;
            case IVoidFunctionNode voidFunctionNode:
                var voidReturnStrategy = voidFunctionNode.SelectAsyncSingleReturnStrategy(returnTypeStatus, isAsyncAwait);
                if (voidReturnStrategy == AsyncSingleReturnStrategy.ValueTaskCompletedTask)
                    code.AppendLine($"return new {_wellKnownTypes.ValueTask!.FullName()}({_wellKnownTypes.Task.FullName()}.CompletedTask);");
                else if (voidReturnStrategy == AsyncSingleReturnStrategy.TaskCompletedTask)
                    code.AppendLine($"return {_wellKnownTypes.Task.FullName()}.CompletedTask;");
                break;
            case IMultiFunctionNodeBase multiFunctionNode:
                if (!isSomeTask || isAsyncEnumerable)
                    code.AppendLine("yield break;");
                else if (isAsyncAwait)
                    code.AppendLine($"return new {multiFunctionNode.ItemTypeFullName}[] {{ {string.Join(", ", multiFunctionNode.ReturnedElements.Select(re => re.Reference))} }};");
                else if (isValueTask)
                    code.AppendLine($"return new {_wellKnownTypes.ValueTask?.FullName()}<{multiFunctionNode.ReturnedTypeFullName(ReturnTypeStatus.Ordinary)}>({_wellKnownTypes.Task.FullName()}.FromResult(({multiFunctionNode.ReturnedTypeFullName(ReturnTypeStatus.Ordinary)}) new {multiFunctionNode.ItemTypeFullName}[] {{ {string.Join(", ", multiFunctionNode.ReturnedElements.Select(re => re.Reference))} }}));");
                else if (isTask)
                    code.AppendLine($"return {_wellKnownTypes.Task.FullName()}.FromResult(({multiFunctionNode.ReturnedTypeFullName(ReturnTypeStatus.Ordinary)}) new {multiFunctionNode.ItemTypeFullName}[] {{ {string.Join(", ", multiFunctionNode.ReturnedElements.Select(re => re.Reference))} }});");
                break;
        }
        
        
        if (handlesDisposal)
        {
            var parameters = !_function.IsTransientScopeDisposalAsParameter
                ? $"exception, {_function.SubDisposalNode.Reference}, {_function.TransientScopeDisposalNode.Reference}"
                : $"exception, {_function.SubDisposalNode.Reference}";
            
            var throwLine = isAsyncAwait && _wellKnownTypes.IAsyncDisposable is not null && _wellKnownTypes.ValueTask is not null
                ? $"throw await {_disposeUtility.DisposeExceptionHandlingAsyncFullyQualified}(({_disposeUtility.DisposableRangeInterfaceData.InterfaceNameFullyQualified}) {Constants.ThisKeyword}, {parameters});"
                : $"throw {(!_container.AsyncDisposablesPossible && _disposeUtility.DisposeExceptionHandlingSyncOnlyFullyQualified is {} syncOnlyName ? syncOnlyName : _disposeUtility.DisposeExceptionHandlingFullyQualified)}(({_disposeUtility.DisposableRangeInterfaceData.InterfaceNameFullyQualified}) {Constants.ThisKeyword}, {parameters});";
    
            code.AppendLine(
                $$"""
                  }
                  catch({{_wellKnownTypes.Exception.FullName()}} exception)
                  {
                  {{throwLine}}
                  }
                  finally
                  {
                  {{_wellKnownTypes.Interlocked.FullName()}}.{{nameof(Interlocked.Decrement)}}(ref {{_range.ResolutionCounterReference}});
                  }
                  """);
        }

        switch (_function)
        {
            case IRangedInstanceFunctionNode { Group: { IsOpenGeneric: true} group }:
                code.AppendLine(
                    $$"""
                      }
                      finally
                      {
                      {{Constants.ThisKeyword}}.{{group.LockReference}}.Release();
                      }
                      """);
                break;
            case IRangedInstanceFunctionNode { Group: { IsOpenGeneric: false } group }:
                if (group.IsCreatedForStructs is { } createdReference)
                    code.AppendLine($"{createdReference} = true;");
                code.AppendLine(
                    $$"""
                      }
                      finally
                      {
                      {{Constants.ThisKeyword}}.{{group.LockReference}}?.Release();
                      }
                      {{Constants.ThisKeyword}}.{{group.LockReference}} = null;
                      return {{ReturnExpression($"{Constants.ThisKeyword}.{group.FieldReference}")}};
                      """);
                break;
        }
            
        foreach (var localFunction in _function.LocalFunctions)
            visitor.VisitILocalFunctionNode(localFunction);
        
        code.AppendLine("}");
        return;

        void ObjectDisposedCheck(
            string disposedPropertyReference,
            string rangeFullName,
            string returnTypeFullName) => code.AppendLine(
            $"if ({disposedPropertyReference}) throw new {_wellKnownTypes.ObjectDisposedException}(\"{rangeFullName}\", $\"[DIE] This scope \\\"{rangeFullName}\\\" is already disposed, so it can't create a \\\"{returnTypeFullName}\\\" instance anymore.\");");

        string ReturnExpression(string reference)
        {
            if (isAsyncAwait)
                return reference;
            if (isValueTask)
                return $"new {_function.ReturnedTypeFullName(returnTypeStatus)}({_wellKnownTypes.Task.FullName()}.FromResult({reference}))";
            if (isTask)
                return $"{_wellKnownTypes.Task.FullName()}.FromResult({reference})";
            return reference;
        }
    }

    private static string GenerateMethodDeclaration(IFunctionNode functionNode, ReturnTypeStatus returnTypeStatus, bool isAsyncAwait)
    {
        var accessibility = functionNode is { Accessibility: { } acc, ExplicitInterfaceFullName: null }
            ? $"{SyntaxFacts.GetText(acc)} "  
            : "";
        var asyncModifier = isAsyncAwait
            ? "async "
            : "";
        var explicitInterfaceFullName = functionNode.ExplicitInterfaceFullName is { } interfaceName
            ? $"{interfaceName}."
            : "";
        var name = functionNode.Name(returnTypeStatus);
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
        var returnedTypeFullName = functionNode.ReturnedTypeFullName(returnTypeStatus);
        return $"{accessibility}{asyncModifier}{returnedTypeFullName} {explicitInterfaceFullName}{name}{typeParameters}({parameters}){typeParametersConstraints}";
    }
}