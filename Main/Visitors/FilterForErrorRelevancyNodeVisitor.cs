using MrMeeseeks.DIE.Nodes;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.Delegates;
using MrMeeseeks.DIE.Nodes.Elements.Factories;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Elements.Tuples;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Visitors;

internal interface IFilterForErrorRelevancyNodeVisitor : INodeVisitor
{
    IImmutableSet<INode> ErrorRelevantNodes { get; }
}

internal sealed class FilterForErrorRelevancyNodeVisitor : IFilterForErrorRelevancyNodeVisitor
{
    private readonly HashSet<INode> _errorRelevantNodes = new();
    
    public IImmutableSet<INode> ErrorRelevantNodes => _errorRelevantNodes.ToImmutableHashSet();
    
    private readonly Stack<INode> _currentNodeStack = new();

    public void VisitIRangedInstanceFunctionNode(IRangedInstanceFunctionNode element) =>
        VisitISingleFunctionNode(element);

    public void VisitICreateTransientScopeFunctionNode(ICreateTransientScopeFunctionNode element) =>
        VisitISingleFunctionNode(element);

    public void VisitIMultiFunctionNode(IMultiFunctionNode element) => VisitIMultiFunctionNodeBase(element);

    private void VisitIMultiFunctionNodeBase(IMultiFunctionNodeBase element)
    {
        _currentNodeStack.Push(element);
        
        foreach (var returnedElement in element.ReturnedElements)
            VisitIElementNode(returnedElement);
        
        foreach (var localFunction in element.LocalFunctions)
            VisitISingleFunctionNode(localFunction);
        
        _currentNodeStack.Pop();
    }

    public void VisitIContainerNode(IContainerNode element)
    {
        _currentNodeStack.Push(element);
        
        foreach (var createContainerFunction in element.CreateContainerFunctions)
            VisitICreateContainerFunctionNode(createContainerFunction);
        
        foreach (var rootFunction in element.RootFunctions)
            VisitIEntryFunctionNode(rootFunction);
        
        VisitIRangeNode(element);
        
        foreach (var scope in element.Scopes)
            VisitIScopeNode(scope);
        
        foreach (var transientScope in element.TransientScopes)
            VisitITransientScopeNode(transientScope);
        
        _currentNodeStack.Pop();
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

    private void VisitISingleFunctionNode(ISingleFunctionNode element)
    {
        _currentNodeStack.Push(element);

        VisitIElementNode(element.ReturnedElement);
        
        foreach (var localFunction in element.LocalFunctions)
            VisitISingleFunctionNode(localFunction);
        
        _currentNodeStack.Pop();
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
        }
    }

    public void VisitITransientScopeCallNode(ITransientScopeCallNode element) {}

    public void VisitIScopeNode(IScopeNode element)
    {
        _currentNodeStack.Push(element);

        VisitIRangeNode(element);
        
        _currentNodeStack.Pop();
    }

    public void VisitITransientScopeDisposalTriggerNode(ITransientScopeDisposalTriggerNode element)
    {
    }

    public void VisitIEntryFunctionNode(IEntryFunctionNode element) =>
        VisitISingleFunctionNode(element);

    public void VisitITransientScopeInterfaceNode(ITransientScopeInterfaceNode element)
    {
    }

    public void VisitIWrappedAsyncFunctionCallNode(IWrappedAsyncFunctionCallNode element) {}

    public void VisitIImplicitScopeImplementationNode(IImplicitScopeImplementationNode element)
    {
        // Nothing to do here
    }

    public void VisitINullNode(INullNode element)
    {
    }

    public void VisitIFactoryPropertyNode(IFactoryPropertyNode element) {}

    public void VisitIReusedNode(IReusedNode element) => 
        VisitIElementNode(element.Inner);

    public void VisitIParameterNode(IParameterNode element)
    {
    }

    public void VisitIValueTupleNode(IValueTupleNode element)
    {
        _currentNodeStack.Push(element);

        foreach (var (_, elementNode) in element.Parameters)
            VisitIElementNode(elementNode);
        
        _currentNodeStack.Pop();
    }

    public void VisitIFactoryFieldNode(IFactoryFieldNode element) {}

    public void VisitIRangedInstanceFunctionGroupNode(IRangedInstanceFunctionGroupNode element)
    {
        foreach (var rangedInstanceFunctionNode in element.Overloads)
            VisitIRangedInstanceFunctionNode(rangedInstanceFunctionNode);
    }

    public void VisitITransientScopeNode(ITransientScopeNode element)
    {
        _currentNodeStack.Push(element);

        VisitIRangeNode(element);
        
        _currentNodeStack.Pop();
    }

    public void VisitIFactoryFunctionNode(IFactoryFunctionNode element) {}

    public void VisitICreateScopeFunctionNode(ICreateScopeFunctionNode element) =>
        VisitISingleFunctionNode(element);

    public void VisitIValueTupleSyntaxNode(IValueTupleSyntaxNode element)
    {
        _currentNodeStack.Push(element);

        foreach (var itemNode in element.Items)
            VisitIElementNode(itemNode);
        
        _currentNodeStack.Pop();
    }

    public void VisitIFuncNode(IFuncNode element) {}

    public void VisitILazyNode(ILazyNode element) {}

    public void VisitIThreadLocalNode(IThreadLocalNode element) {}

    public void VisitIEnumerableBasedNode(IEnumerableBasedNode element) {}

    public void VisitICreateContainerFunctionNode(ICreateContainerFunctionNode element)
    {
    }

    public void VisitIScopeCallNode(IScopeCallNode element) {}

    public void VisitIInitializedInstanceNode(IInitializedInstanceNode element)
    {
    }

    public void VisitITupleNode(ITupleNode element)
    {
        _currentNodeStack.Push(element);

        foreach (var (_, elementNode) in element.Parameters)
            VisitIElementNode(elementNode);
        
        _currentNodeStack.Pop();
    }

    public void VisitIErrorNode(IErrorNode element)
    {
        _currentNodeStack.Push(element);
        
        foreach (var node in _currentNodeStack)
            _errorRelevantNodes.Add(node);

        _currentNodeStack.Pop();
    }

    public void VisitIImplementationNode(IImplementationNode element)
    {
        _currentNodeStack.Push(element);

        foreach (var (_, elementNode) in element.ConstructorParameters)
            VisitIElementNode(elementNode);
        foreach (var (_, elementNode) in element.Properties)
            VisitIElementNode(elementNode);
        foreach (var (_, elementNode) in element.Initializer?.Parameters ?? Enumerable.Empty<(string Name, IElementNode Element)>())
            VisitIElementNode(elementNode);
        
        _currentNodeStack.Pop();
    }

    public void VisitIRangedInstanceInterfaceFunctionNode(IRangedInstanceInterfaceFunctionNode element)
    {
    }

    public void VisitIPlainFunctionCallNode(IPlainFunctionCallNode element) {}

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

    public void VisitIReferenceNode(IReferenceNode element)
    {
        // Nothing to do here
    }

    public void VisitIVoidFunctionNode(IVoidFunctionNode element)
    {
        _currentNodeStack.Push(element);

        foreach (var localFunction in element.LocalFunctions)
            VisitISingleFunctionNode(localFunction);
        
        _currentNodeStack.Pop();
    }

    public void VisitIOutParameterNode(IOutParameterNode element)
    {
    }

    public void VisitIMultiKeyValueFunctionNode(IMultiKeyValueFunctionNode element) =>
        VisitIMultiFunctionNodeBase(element);

    public void VisitIMultiKeyValueMultiFunctionNode(IMultiKeyValueMultiFunctionNode element) =>
        VisitIMultiFunctionNodeBase(element);

    public void VisitIKeyValueBasedNode(IKeyValueBasedNode element) { }

    public void VisitIKeyValuePairNode(IKeyValuePairNode element)
    {
        _currentNodeStack.Push(element);

        VisitIElementNode(element.Value);
        
        _currentNodeStack.Pop();
    }
}