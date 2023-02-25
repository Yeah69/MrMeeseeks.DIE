using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.Delegates;
using MrMeeseeks.DIE.Nodes.Elements.Factories;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Elements.Tasks;
using MrMeeseeks.DIE.Nodes.Elements.Tuples;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Visitors;

internal interface INodeVisitor
{
    void VisitContainerNode(IContainerNode container);
    void VisitTransientScopeInterfaceNode(ITransientScopeInterfaceNode transientScopeInterface);
    void VisitScopeNode(IScopeNode scope);
    void VisitTransientScopeNode(ITransientScopeNode transientScope);
    void VisitScopeCallNode(IScopeCallNode scopeCall);
    void VisitTransientScopeCallNode(ITransientScopeCallNode transientScopeCall);
    void VisitCreateFunctionNode(ICreateFunctionNode createFunction);
    void VisitEntryFunctionNode(IEntryFunctionNode entryFunction);
    void VisitLocalFunctionNode(ILocalFunctionNode localFunction);
    void VisitRangedInstanceFunctionNode(IRangedInstanceFunctionNode rangedInstanceFunctionNode);
    void VisitRangedInstanceInterfaceFunctionNode(IRangedInstanceInterfaceFunctionNode rangedInstanceInterfaceFunctionNode);
    void VisitRangedInstanceFunctionGroupNode(IRangedInstanceFunctionGroupNode rangedInstanceFunctionGroupNode);
    void VisitPlainFunctionCallNode(IPlainFunctionCallNode functionCallNode);
    void VisitFactoryFieldNode(IFactoryFieldNode factoryFieldNode);
    void VisitFactoryPropertyNode(IFactoryPropertyNode factoryPropertyNode);
    void VisitFactoryFunctionNode(IFactoryFunctionNode factoryFunctionNode);
    void VisitFuncNode(IFuncNode lazyNode);
    void VisitLazyNode(ILazyNode lazyNode);
    void VisitValueTaskNode(IValueTaskNode valueTaskNode);
    void VisitTaskNode(ITaskNode taskNode);
    void VisitTupleNode(ITupleNode tupleNode);
    void VisitValueTupleNode(IValueTupleNode valueTupleNode);
    void VisitValueTupleSyntaxNode(IValueTupleSyntaxNode valueTupleSyntaxNode);
    void VisitImplementationNode(IImplementationNode implementationNode);
    void VisitParameterNode(IParameterNode parameterNode);
    void VisitOutParameterNode(IOutParameterNode outParameterNode);
    void VisitAbstractionNode(IAbstractionNode abstractionNode);
    void VisitTransientScopeDisposalTriggerNode(ITransientScopeDisposalTriggerNode transientScopeDisposalTriggerNode);
    void VisitNullNode(INullNode nullNode);
    void VisitMultiFunctionNode(IMultiFunctionNode multiFunctionNode);
    void VisitEnumerableBasedNode(IEnumerableBasedNode enumerableBasedNode);
    void VisitErrorNode(IErrorNode errorNode);
}