using System.IO;
using System.Threading;
using MrMeeseeks.DIE.Nodes;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.Delegates;
using MrMeeseeks.DIE.Nodes.Elements.Factories;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Elements.Tuples;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Utility;

namespace MrMeeseeks.DIE.Visitors;

internal interface IResolutionGraphAnalyticsNodeVisitor : INodeVisitor;

internal sealed class ResolutionGraphAnalyticsNodeVisitor : IResolutionGraphAnalyticsNodeVisitor
{
    private readonly IImmutableSet<INode>? _relevantNodes;
    private readonly IPaths _paths;
    private readonly IContainerInfo _containerInfo;
    private readonly string _dirPath;
    private readonly StringBuilder _code = new();
    private readonly StringBuilder _relations = new();
    private readonly Dictionary<INode, string> _nodeToReference = new();

    private int _referenceNumber = -1;
    private IFunctionNode? _currentFunctionNode;

    internal ResolutionGraphAnalyticsNodeVisitor(
        // parameters
        IImmutableSet<INode>? relevantNodes,

        // dependencies
        IPaths paths,
        IContainerInfo containerInfo)
    {
        _relevantNodes = relevantNodes;
        _paths = paths;
        _containerInfo = containerInfo;
        _dirPath = paths.Analytics;
    }
    
    private string GetOrAddReference(INode element) => _nodeToReference.TryGetValue(element, out var reference) 
        ? reference 
        : _nodeToReference[element] = $"ref{Interlocked.Increment(ref _referenceNumber)}";
    
    private string GetOrAddRangeCustomFactoryReference(
        string rangeReference, string customFactoryName) => _customFactoryToReference.TryGetValue((rangeReference, customFactoryName), out var reference) 
        ? reference 
        : _customFactoryToReference[(rangeReference, customFactoryName)] = $"ref{Interlocked.Increment(ref _referenceNumber)}";
    
    public void VisitIRangedInstanceFunctionNode(IRangedInstanceFunctionNode element) =>
        VisitISingleFunctionNode(element);

    public void VisitICreateTransientScopeFunctionNode(ICreateTransientScopeFunctionNode element) =>
        VisitISingleFunctionNode(element);

    public void VisitIMultiFunctionNode(IMultiFunctionNode element) => VisitIMultiFunctionNodeBase(element);

    public void VisitIMultiFunctionNodeBase(IMultiFunctionNodeBase element)
    {
        if (_relevantNodes is not null && !_relevantNodes.Contains(element))
            return;
        
        var previousFunctionNode = _currentFunctionNode;
        _currentFunctionNode = element;
        
        var previousReference = _currentReference;
        var reference = GetOrAddReference(element);
        _currentReference = reference;
        _code.AppendLine($$"""
                           package "{{element.ReturnedTypeFullName(ReturnTypeStatus.Ordinary)}} {{element.Name(ReturnTypeStatus.Ordinary)}}({{string.Join(", ", element.Parameters.Select(p => $"{p.Node.TypeFullName}"))}})" as {{reference}} {
                           """);

        foreach (var returnedElement in element.ReturnedElements)
            VisitIElementNode(returnedElement);
        
        foreach (var localFunction in element.LocalFunctions)
            VisitISingleFunctionNode(localFunction);
        
        _code.AppendLine($$"""
                           }
                           """);
        _currentReference = previousReference;
        
        _currentFunctionNode = previousFunctionNode;
    }

    private string? _currentRangeReference;
    private readonly Dictionary<(string RangeReference, string FactoryName), string> _customFactoryToReference = new();

    public void VisitIContainerNode(IContainerNode element)
    {
        if (_relevantNodes is not null && !_relevantNodes.Contains(element))
            return;

        var previousRangeReference = _currentRangeReference;
        var reference = GetOrAddReference(element);
        _currentRangeReference = reference;
        _code.AppendLine($$"""
@startuml
package "{{element.FullName}}" as {{reference}} {
""");
        
        foreach (var createContainerFunction in element.CreateContainerFunctions)
            VisitICreateContainerFunctionNode(createContainerFunction);
        
        foreach (var rootFunction in element.RootFunctions)
            VisitIEntryFunctionNode(rootFunction);
        
        VisitIRangeNode(element);
        
        foreach (var keyValuePair in _customFactoryToReference.Where(kvp => kvp.Key.RangeReference == reference))
            _code.AppendLine($$"""
object "{{keyValuePair.Key.FactoryName}}" as {{keyValuePair.Value}}
""");
        
        foreach (var scope in element.Scopes)
            VisitIScopeNode(scope);
        
        foreach (var transientScope in element.TransientScopes)
            VisitITransientScopeNode(transientScope);
        
        _code.AppendLine($$"""
{{_relations}}
}
@enduml
""");
        _currentRangeReference = previousRangeReference;
        
        if (!Directory.Exists(_dirPath))
            Directory.CreateDirectory(_dirPath);
        var fileNamePart = $"{_containerInfo.Namespace}{_containerInfo.Name}";
        File.WriteAllText(
            _relevantNodes is null 
                ? _paths.AnalyticsResolutionGraph(fileNamePart) 
                : _paths.AnalyticsErrorFilteredResolutionGraph(fileNamePart), 
            _code.ToString());
    }

    private void VisitIRangeNode(IRangeNode element)
    {
        foreach (var initializationFunction in element.InitializationFunctions)
            VisitIVoidFunctionNode(initializationFunction);

        foreach (var createFunctionBase in element.CreateFunctions)
            VisitICreateFunctionNodeBase(createFunctionBase);

        foreach (var rangedInstanceFunctionGroup in element.RangedInstanceFunctionGroups)
            VisitIRangedInstanceFunctionGroupNode(rangedInstanceFunctionGroup);
        
        foreach (var multiFunctionBase in element.MultiFunctions)
            switch (multiFunctionBase)
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
                    throw new ArgumentOutOfRangeException(nameof(element), $"Unknown type {element.GetType()}");
            }
    }

    private string? _currentReference;

    private void VisitISingleFunctionNode(ISingleFunctionNode element)
    {
        if (_relevantNodes is not null && !_relevantNodes.Contains(element))
            return;

        var previousFunctionNode = _currentFunctionNode;
        _currentFunctionNode = element;

        var previousReference = _currentReference;
        var reference = GetOrAddReference(element);
        _currentReference = reference;
        _code.AppendLine($$"""
package "{{element.ReturnedTypeFullName(ReturnTypeStatus.Ordinary)}} {{element.Name(ReturnTypeStatus.Ordinary)}}({{string.Join(", ", element.Parameters.Select(p => $"{p.Node.TypeFullName}"))}})" as {{reference}} {
""");

        VisitIElementNode(element.ReturnedElement);
        
        foreach (var localFunction in element.LocalFunctions)
            VisitISingleFunctionNode(localFunction);
        
        _code.AppendLine("}");
        _currentReference = previousReference;
        
        _currentFunctionNode = previousFunctionNode;
    }

    private void VisitIElementNode(IElementNode element)
    {
        switch (element)
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
            case IInterceptionElementNode interceptionElementNode:
                VisitIInterceptionElementNode(interceptionElementNode);
                break;
        }
    }

    public void VisitITransientScopeCallNode(ITransientScopeCallNode element) => 
        VisitIFunctionCallNode(element);

    public void VisitIScopeNode(IScopeNode element)
    {
        if (_relevantNodes is not null && !_relevantNodes.Contains(element))
            return;

        var reference = GetOrAddReference(element);
        _code.AppendLine($$"""
package "{{element.Name}}" as {{reference}} {
""");
        
        VisitIRangeNode(element);
        
        _code.AppendLine("""
}
@enduml
""");
    }

    public void VisitITransientScopeDisposalTriggerNode(ITransientScopeDisposalTriggerNode element)
    {
        if (_relevantNodes is not null && !_relevantNodes.Contains(element))
            return;

        var reference = GetOrAddReference(element);
        _code.AppendLine($$"""
object "Transient Scope Disposal Hook" as {{reference}}
""");
        _relations.AppendLine($"{_currentReference} --> {reference}");
    }

    public void VisitIEntryFunctionNode(IEntryFunctionNode element) =>
        VisitISingleFunctionNode(element);

    public void VisitIInitialOrdinarySubDisposalNode(IInitialOrdinarySubDisposalNode element) {}
    
    public void VisitIInitialTransientScopeSubDisposalNode(IInitialTransientScopeSubDisposalNode element) {}

    public void VisitITransientScopeInterfaceNode(ITransientScopeInterfaceNode element)
    {
    }

    public void VisitIWrappedAsyncFunctionCallNode(IWrappedAsyncFunctionCallNode element) => 
        VisitIFunctionCallNode(element);

    public void VisitINullNode(INullNode element)
    {
        if (_relevantNodes is not null && !_relevantNodes.Contains(element))
            return;

        var reference = GetOrAddReference(element);
        _code.AppendLine($$"""
object "null" as {{reference}}
""");
        _relations.AppendLine($"{_currentReference} --> {reference}");
    }

    public void VisitIInterceptionElementNode(IInterceptionElementNode element)
    {
    }

    public void VisitIFactoryPropertyNode(IFactoryPropertyNode element) => 
        VisitIFactoryNodeBase(element);

    public void VisitIReusedNode(IReusedNode element) => 
        VisitIElementNode(element.Inner);

    public void VisitIParameterNode(IParameterNode element)
    {
    }

    public void VisitIValueTupleNode(IValueTupleNode element)
    {
        if (_relevantNodes is not null && !_relevantNodes.Contains(element))
            return;

        var reference = GetOrAddReference(element);
        _code.AppendLine($$"""
map "ValueTuple<{{string.Join(", ", element.Parameters.Select(p => p.Node.TypeFullName))}}>" as {{reference}} {
""");
        foreach (var (parameterName, parameterElement) in element.Parameters)
            _code.AppendLine($"{parameterName} => {parameterElement.TypeFullName}");
        
        _code.AppendLine($$"""
}
""");
        _relations.AppendLine($"{_currentReference} --> {reference}");
        foreach (var (parameterName, elementNode) in element.Parameters)
        {
            var previousReference = _currentReference;
            _currentReference = $"{reference}::{parameterName}";
            
            VisitIElementNode(elementNode);
            
            _currentReference = previousReference;
        }
    }

    private void VisitIFactoryNodeBase(IFactoryNodeBase element) => 
        _relations.AppendLine($"{_currentReference} --> {GetOrAddRangeCustomFactoryReference(_currentRangeReference ?? "", element.Name)}");

    public void VisitIFactoryFieldNode(IFactoryFieldNode element) => 
        VisitIFactoryNodeBase(element);

    public void VisitIRangedInstanceFunctionGroupNode(IRangedInstanceFunctionGroupNode element)
    {
        if (_relevantNodes is not null && !_relevantNodes.Contains(element))
            return;

        foreach (var rangedInstanceFunctionNode in element.Overloads)
            VisitIRangedInstanceFunctionNode(rangedInstanceFunctionNode);
    }

    public void VisitITransientScopeNode(ITransientScopeNode element)
    {
        if (_relevantNodes is not null && !_relevantNodes.Contains(element))
            return;

        var reference = GetOrAddReference(element);
        _code.AppendLine($$"""
package "{{element.Name}}" as {{reference}} {
""");
        
        VisitIRangeNode(element);
        
        _code.AppendLine("""
}
@enduml
""");
    }

    public void VisitIFactoryFunctionNode(IFactoryFunctionNode element) => 
        VisitIFactoryNodeBase(element);

    public void VisitICreateScopeFunctionNode(ICreateScopeFunctionNode element) =>
        VisitISingleFunctionNode(element);

    public void VisitIValueTupleSyntaxNode(IValueTupleSyntaxNode element)
    {
        if (_relevantNodes is not null && !_relevantNodes.Contains(element))
            return;

        var reference = GetOrAddReference(element);
        _code.AppendLine($$"""
map "({{string.Join(", ", element.Items.Select(i => i.TypeFullName))}})" as {{reference}} {
""");
        int i = 1;
        foreach (var _ in element.Items)
        {
            _code.AppendLine($"Item{i} => {element.Items[i - 1].TypeFullName}");
            i++;
        }
        
        _code.AppendLine($$"""
}
""");
        _relations.AppendLine($"{_currentReference} --> {reference}");
        i = 1;
        foreach (var itemNode in element.Items)
        {
            var previousReference = _currentReference;
            _currentReference = $"{reference}::Item{i++}";
            
            VisitIElementNode(itemNode);
            
            _currentReference = previousReference;
        }
    }

    private void VisitIDelegateBaseNode(IDelegateBaseNode element)
    {
        if (_relevantNodes is not null && !_relevantNodes.Contains(element))
            return;

        if (_currentFunctionNode
                ?.LocalFunctions
                .FirstOrDefault(f => f.Name(ReturnTypeStatus.Ordinary) == element.MethodGroup) 
            is { } calledFunction)
        {
            _relations.AppendLine($"{_currentReference} --> {GetOrAddReference(calledFunction)}");
        }
    }

    public void VisitIFuncNode(IFuncNode element) => 
        VisitIDelegateBaseNode(element);

    public void VisitIImplicitScopeImplementationNode(IImplicitScopeImplementationNode element)
    {
    }

    public void VisitILazyNode(ILazyNode element) => 
        VisitIDelegateBaseNode(element);

    public void VisitIThreadLocalNode(IThreadLocalNode element) => 
        VisitIDelegateBaseNode(element);

    public void VisitIEnumerableBasedNode(IEnumerableBasedNode element) => 
        VisitIFunctionCallNode(element.EnumerableCall);

    public void VisitICreateContainerFunctionNode(ICreateContainerFunctionNode element)
    {
    }

    public void VisitIScopeCallNode(IScopeCallNode element) => 
        VisitIFunctionCallNode(element);

    public void VisitIInitializedInstanceNode(IInitializedInstanceNode element)
    {
    }

    public void VisitITupleNode(ITupleNode element)
    {
        if (_relevantNodes is not null && !_relevantNodes.Contains(element))
            return;

        var reference = GetOrAddReference(element);
        _code.AppendLine($$"""
map "Tuple<{{string.Join(", ", element.Parameters.Select(p => p.Node.TypeFullName))}}>" as {{reference}} {
""");
        foreach (var (parameterName, parameterElement) in element.Parameters)
            _code.AppendLine($"{parameterName} => {parameterElement.TypeFullName}");
        
        _code.AppendLine($$"""
}
""");
        _relations.AppendLine($"{_currentReference} --> {reference}");
        foreach (var (parameterName, elementNode) in element.Parameters)
        {
            var previousReference = _currentReference;
            _currentReference = $"{reference}::{parameterName}";
            
            VisitIElementNode(elementNode);
            
            _currentReference = previousReference;
        }
    }

    public void VisitIReferenceNode(IReferenceNode element)
    {
    }

    public void VisitIErrorNode(IErrorNode element)
    {
        if (_relevantNodes is not null && !_relevantNodes.Contains(element))
            return;

        var reference = GetOrAddReference(element);
        _code.AppendLine($"object \"{element.Message}\" as {reference} #line:red");
        _relations.AppendLine($"{_currentReference} --> {reference}");
    }

    public void VisitIImplementationNode(IImplementationNode element)
    {
        if (_relevantNodes is not null && !_relevantNodes.Contains(element))
            return;

        var reference = GetOrAddReference(element);
        _code.AppendLine($$"""
map "{{element.TypeFullName}}" as {{reference}} {
""");
        foreach (var (parameterName, parameterElement) in element.ConstructorParameters)
            _code.AppendLine($"ctor_{parameterName} => {parameterElement.TypeFullName}");
        foreach (var (propertyName, propertyElement) in element.Properties)
            _code.AppendLine($"prop_{propertyName} => {propertyElement.TypeFullName}");
        foreach (var (parameterName, parameterElement) in element.Initializer?.Parameters ?? Enumerable.Empty<(string Name, IElementNode Element)>())
            _code.AppendLine($"init_{parameterName} => {parameterElement.TypeFullName}");
        
        _code.AppendLine($$"""
}
""");
        _relations.AppendLine($"{_currentReference} --> {reference}");
        foreach (var (parameterName, elementNode) in element.ConstructorParameters)
        {
            var previousReference = _currentReference;
            _currentReference = $"{reference}::ctor_{parameterName}";
            
            VisitIElementNode(elementNode);
            
            _currentReference = previousReference;
        }
        foreach (var (propertyName, elementNode) in element.Properties)
        {
            var previousReference = _currentReference;
            _currentReference = $"{reference}::prop_{propertyName}";
            
            VisitIElementNode(elementNode);
            
            _currentReference = previousReference;
        }
        foreach (var (parameterName, elementNode) in element.Initializer?.Parameters ?? Enumerable.Empty<(string Name, IElementNode Element)>())
        {
            var previousReference = _currentReference;
            _currentReference = $"{reference}::init_{parameterName}";
            
            VisitIElementNode(elementNode);
            
            _currentReference = previousReference;
        }
    }

    public void VisitIRangedInstanceInterfaceFunctionNode(IRangedInstanceInterfaceFunctionNode element)
    {
    }

    private void VisitIFunctionCallNode(IFunctionCallNode element)
    {
        if (_relevantNodes is not null && !_relevantNodes.Contains(element))
            return;

        _relations.AppendLine($"{_currentReference} --> {GetOrAddReference(element.CalledFunction)}");
    }

    public void VisitIPlainFunctionCallNode(IPlainFunctionCallNode element) => 
        VisitIFunctionCallNode(element);

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

    public void VisitICreateFunctionNode(ICreateFunctionNode element) =>
        VisitISingleFunctionNode(element);

    public void VisitILocalFunctionNode(ILocalFunctionNode element) =>
        VisitISingleFunctionNode(element);

    public void VisitIVoidFunctionNode(IVoidFunctionNode element)
    {
        if (_relevantNodes is not null && !_relevantNodes.Contains(element))
            return;

        var previousFunctionNode = _currentFunctionNode;
        _currentFunctionNode = element;

        var previousReference = _currentReference;
        var reference = GetOrAddReference(element);
        _currentReference = reference;
        _code.AppendLine(
            $$"""
              package "void {{element.Name(ReturnTypeStatus.Ordinary)}}({{string.Join(", ", element.Parameters.Select(p => $"{p.Node.TypeFullName}"))}})" as {{reference}} {
              """);

        foreach (var initialization in element.Initializations)
            VisitIFunctionCallNode(initialization.Item1);
        
        foreach (var localFunction in element.LocalFunctions)
            VisitISingleFunctionNode(localFunction);
        
        _code.AppendLine("}");
        _currentReference = previousReference;
        
        _currentFunctionNode = previousFunctionNode;
    }

    public void VisitIOutParameterNode(IOutParameterNode element)
    {
    }

    public void VisitIMultiKeyValueFunctionNode(IMultiKeyValueFunctionNode multiKeyValueFunctionNode) => 
        VisitIMultiFunctionNodeBase(multiKeyValueFunctionNode);

    public void VisitIMultiKeyValueMultiFunctionNode(IMultiKeyValueMultiFunctionNode multiKeyValueMultiFunctionNode) => 
        VisitIMultiFunctionNodeBase(multiKeyValueMultiFunctionNode);

    public void VisitIKeyValueBasedNode(IKeyValueBasedNode keyValueBasedNode)
    {
    }

    public void VisitIKeyValuePairNode(IKeyValuePairNode keyValuePairNode)
    {
    }
}