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
        var typeHandleField = new Lazy<string>(() => _referenceGenerator.Generate("typeHandle"));
        var instance0 = new Lazy<string>(() => _referenceGenerator.Generate("instance"));
        var instance1 = new Lazy<string>(() => _referenceGenerator.Generate("instance"));
        var instance2 = new Lazy<string>(() => _referenceGenerator.Generate("instance"));
        var instance3 = new Lazy<string>(() => _referenceGenerator.Generate("instance"));
        var isAsync =
            _function.SynchronicityDecision is SynchronicityDecision.AsyncTask or SynchronicityDecision.AsyncValueTask;
        code.AppendLine(
            $$"""
              {{GenerateMethodDeclaration(_function)}}
              {
              """);
        
        if (IsAsyncFunction(_function))
            code.AppendLine($"await {_wellKnownTypes.Task.FullName()}.{nameof(Task.Yield)}();");
        
        visitor.VisitIElementNode(_function.TransientScopeDisposalNode);
        visitor.VisitIElementNode(_function.SubDisposalNode);

        switch (_function)
        {
            case IRangedInstanceFunctionNode { Group: { IsOpenGeneric: true} group } rangedInstanceFunctionNode:
                code.AppendLine(
                    $$"""
                      var {{typeHandleField.Value}} = typeof({{rangedInstanceFunctionNode.ReturnedElement.TypeFullName}}).TypeHandle;
                      if ({{group.RangedInstanceStorageFieldName}}.TryGetValue({{typeHandleField.Value}}, out {{_wellKnownTypes.Object.FullName()}}? {{instance0.Value}}) && {{instance0.Value}} is {{rangedInstanceFunctionNode.ReturnedElement.TypeFullName}} {{instance1.Value}}) return {{instance1.Value}};
                      {{(isAsync ? "await " : "")}}{{Constants.ThisKeyword}}.{{group.LockReference}}.Wait{{(isAsync ? "Async" : "")}}();
                      try
                      {
                      if ({{group.RangedInstanceStorageFieldName}}.TryGetValue({{typeHandleField.Value}}, out {{_wellKnownTypes.Object.FullName()}}? {{instance2.Value}}) && {{instance2.Value}} is {{rangedInstanceFunctionNode.ReturnedElement.TypeFullName}} {{instance3.Value}}) return {{instance3.Value}};
                      """);
                break;
            case IRangedInstanceFunctionNode { Group: { IsOpenGeneric: false } group }:
                var checkAndReturnAlreadyCreatedInstance = group.IsCreatedForStructs is { } createdReference
                    ? $"if ({createdReference}) return {group.FieldReference};"
                    : $"if (!{_wellKnownTypes.Object.FullName()}.ReferenceEquals({group.FieldReference}, null)) return {group.FieldReference};";

                var waitLine = isAsync
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
                _function.ReturnedTypeFullName);
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
                    if (multiFunctionNode.SynchronicityDecision == SynchronicityDecision.Sync)
                        code.AppendLine($"yield return {returnedElement.Reference};");
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
                _function.ReturnedTypeFullName);

        if (!_function.IsSubDisposalAsParameter)
        {
            code.AppendLine(
                $"{_range.DisposalHandling.CollectionReference}.{nameof(List<List<object>>.Add)}({_function.SubDisposalNode.Reference});");
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
                      return {{rangedInstanceFunctionNode.ReturnedElement.Reference}};
                      """);
                break;
            case IRangedInstanceFunctionNode { Group: { IsOpenGeneric: false } group } rangedInstanceFunctionNode:
                code.AppendLine($"{group.FieldReference} = {rangedInstanceFunctionNode.ReturnedElement.Reference};");
                break;
            case ISingleFunctionNode {SynchronicityDecisionKind: var synchronicityDecisionKind  } singleFunctionNode:
                code.AppendLine($"return {(synchronicityDecisionKind is SynchronicityDecisionKind.AsyncNatural ? "await " : "")}{singleFunctionNode.ReturnedElement.Reference};");
                break;
            case IMultiFunctionNodeBase multiFunctionNode:
                code.AppendLine(multiFunctionNode.SynchronicityDecision == SynchronicityDecision.Sync
                    ? "yield break;"
                    : $"return new {multiFunctionNode.ItemTypeFullName}[] {{ {string.Join(", ", multiFunctionNode.ReturnedElements.Select(re => re.Reference))} }};");
                
                break;
        }
        
        
        if (handlesDisposal)
        {
            var parameters = !_function.IsTransientScopeDisposalAsParameter
                ? $"exception, {_function.SubDisposalNode.Reference}, {_function.TransientScopeDisposalNode.Reference}"
                : $"exception, {_function.SubDisposalNode.Reference}";
            
            var throwLine = isAsync && _wellKnownTypes.IAsyncDisposable is not null && _wellKnownTypes.ValueTask is not null
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
                  """);
            if (_wellKnownTypes.ValueTask is not null && _wellKnownTypes.IAsyncDisposable is not null && _disposeUtility.ReleaseDisposeAsyncFullyQualified is {} releaseDisposeAsyncFullyQualified)
            {
                code.AppendLine(
                    $"{releaseDisposeAsyncFullyQualified}(ref {_range.DisposalHandling.DisposedFieldReference}, ref {_range.ResolutionCounterReference}, {_range.ReleaseDisposeAsyncReference});");
            }
            code.AppendLine("}");
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
                      return {{Constants.ThisKeyword}}.{{group.FieldReference}};
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
    
    }

    private static string GenerateMethodDeclaration(IFunctionNode functionNode)
    {
        var accessibility = functionNode is { Accessibility: { } acc, ExplicitInterfaceFullName: null }
            ? $"{SyntaxFacts.GetText(acc)} "  
            : "";
        var asyncModifier = 
            IsAsyncFunction(functionNode)
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
    
    private static bool IsAsyncFunction(IFunctionNode functionNode) =>
        functionNode.SynchronicityDecision is SynchronicityDecision.AsyncTask or SynchronicityDecision.AsyncValueTask 
        || functionNode is IMultiFunctionNodeBase { IsAsyncEnumerable: true };
}