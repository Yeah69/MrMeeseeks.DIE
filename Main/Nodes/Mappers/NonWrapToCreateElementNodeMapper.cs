using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.Delegates;
using MrMeeseeks.DIE.Nodes.Elements.Factories;
using MrMeeseeks.DIE.Nodes.Elements.Tasks;
using MrMeeseeks.DIE.Nodes.Elements.Tuples;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Nodes.Roots;
using MrMeeseeks.DIE.Utility;

namespace MrMeeseeks.DIE.Nodes.Mappers;

internal interface INonWrapToCreateElementNodeMapper : IElementNodeMapperBase
{
}

internal class NonWrapToCreateElementNodeMapper : ElementNodeMapperBase, INonWrapToCreateElementNodeMapper
{
    public NonWrapToCreateElementNodeMapper(
        IElementNodeMapperBase parentElementNodeMapper,
        
        IFunctionNode parentFunction,
        IRangeNode parentRange,
        IContainerNode parentContainer,
        ITransientScopeWideContext transientScopeWideContext,
        IDiagLogger diagLogger, 
        IContainerWideContext containerWideContext, 
        Func<IFieldSymbol, IFactoryFieldNode> factoryFieldNodeFactory, 
        Func<IPropertySymbol, IFactoryPropertyNode> factoryPropertyNodeFactory, 
        Func<IMethodSymbol, IElementNodeMapperBase, IFactoryFunctionNode> factoryFunctionNodeFactory, 
        Func<INamedTypeSymbol, IElementNodeMapperBase, IValueTaskNode> valueTaskNodeFactory, 
        Func<INamedTypeSymbol, IElementNodeMapperBase, ITaskNode> taskNodeFactory, 
        Func<INamedTypeSymbol, IElementNodeMapperBase, IValueTupleNode> valueTupleNodeFactory, 
        Func<INamedTypeSymbol, IElementNodeMapperBase, IValueTupleSyntaxNode> valueTupleSyntaxNodeFactory, 
        Func<INamedTypeSymbol, IElementNodeMapperBase, ITupleNode> tupleNodeFactory, 
        Func<INamedTypeSymbol, ILocalFunctionNode, ILazyNode> lazyNodeFactory, 
        Func<INamedTypeSymbol, ILocalFunctionNode, IFuncNode> funcNodeFactory, 
        Func<ITypeSymbol, IEnumerableBasedNode> enumerableBasedNodeFactory,
        Func<INamedTypeSymbol, INamedTypeSymbol, IElementNodeMapperBase, IAbstractionNode> abstractionNodeFactory, 
        Func<INamedTypeSymbol, IMethodSymbol, IElementNodeMapperBase, IImplementationNode> implementationNodeFactory, 
        Func<ITypeSymbol, IOutParameterNode> outParameterNodeFactory,
        Func<string, ITypeSymbol, IErrorNode> errorNodeFactory, 
        Func<ITypeSymbol, INullNode> nullNodeFactory,
        Func<ITypeSymbol, IReadOnlyList<ITypeSymbol>, ImmutableDictionary<ITypeSymbol, IParameterNode>, ILocalFunctionNodeRoot> localFunctionNodeFactory,
        Func<IElementNodeMapperBase, ImmutableQueue<(INamedTypeSymbol, INamedTypeSymbol)>, IOverridingElementNodeMapper> overridingElementNodeMapperFactory) 
        : base(parentFunction, 
            parentRange, 
            parentContainer, 
            transientScopeWideContext, 
            diagLogger, 
            containerWideContext, 
            factoryFieldNodeFactory, 
            factoryPropertyNodeFactory, 
            factoryFunctionNodeFactory, 
            valueTaskNodeFactory, 
            taskNodeFactory, 
            valueTupleNodeFactory, 
            valueTupleSyntaxNodeFactory, 
            tupleNodeFactory, 
            lazyNodeFactory, 
            funcNodeFactory, 
            enumerableBasedNodeFactory,
            abstractionNodeFactory,
            implementationNodeFactory, 
            outParameterNodeFactory,
            errorNodeFactory, 
            nullNodeFactory,
            localFunctionNodeFactory,
            overridingElementNodeMapperFactory)
    {
        Next = parentElementNodeMapper;
    }

    protected override IElementNodeMapperBase NextForWraps => this;

    protected override IElementNodeMapperBase Next { get; }

    public override IElementNode Map(ITypeSymbol type, ImmutableStack<INamedTypeSymbol> implementationStack) => 
        TypeSymbolUtility.IsWrapType(type, WellKnownTypes) 
            ? base.Map(type, implementationStack) 
            : ParentRange.BuildCreateCall(type, ParentFunction);
}