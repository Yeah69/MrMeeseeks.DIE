using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.Delegates;
using MrMeeseeks.DIE.Nodes.Elements.Factories;
using MrMeeseeks.DIE.Nodes.Elements.Tasks;
using MrMeeseeks.DIE.Nodes.Elements.Tuples;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Utility;

namespace MrMeeseeks.DIE.Nodes.Mappers;

internal interface IElementNodeMapper : IElementNodeMapperBase
{
}

internal class ElementNodeMapper : ElementNodeMapperBase, IElementNodeMapper
{
    public ElementNodeMapper(
        IFunctionNode parentFunction, 
        IRangeNode parentRange, 
        IContainerNode parentContainer, 
        IUserDefinedElements userDefinedElements, 
        ICheckTypeProperties checkTypeProperties, 
        IReferenceGenerator referenceGenerator, 
        IDiagLogger diagLogger, 
        WellKnownTypes wellKnownTypes, 
        WellKnownTypesCollections wellKnownTypesCollections, 
        Func<IFieldSymbol, IFunctionNode, IReferenceGenerator, IFactoryFieldNode> factoryFieldNodeFactory, 
        Func<IPropertySymbol, IFunctionNode, IReferenceGenerator, IFactoryPropertyNode> factoryPropertyNodeFactory, 
        Func<IMethodSymbol, IFunctionNode, IElementNodeMapperBase, IReferenceGenerator, IFactoryFunctionNode> factoryFunctionNodeFactory, 
        Func<INamedTypeSymbol, IContainerNode, IFunctionNode, IElementNodeMapperBase, IReferenceGenerator, IValueTaskNode> valueTaskNodeFactory, 
        Func<INamedTypeSymbol, IContainerNode, IFunctionNode, IElementNodeMapperBase, IReferenceGenerator, ITaskNode> taskNodeFactory, 
        Func<INamedTypeSymbol, IElementNodeMapperBase, IReferenceGenerator, IValueTupleNode> valueTupleNodeFactory, 
        Func<INamedTypeSymbol, IElementNodeMapperBase, IReferenceGenerator, IValueTupleSyntaxNode> valueTupleSyntaxNodeFactory, 
        Func<INamedTypeSymbol, IElementNodeMapperBase, IReferenceGenerator, ITupleNode> tupleNodeFactory, 
        Func<INamedTypeSymbol, ILocalFunctionNode, IReferenceGenerator, ILazyNode> lazyNodeFactory, 
        Func<INamedTypeSymbol, ILocalFunctionNode, IReferenceGenerator, IFuncNode> funcNodeFactory, 
        Func<ITypeSymbol, IRangeNode, IFunctionNode, IReferenceGenerator, IEnumerableBasedNode> enumerableBasedNodeFactory,
        Func<INamedTypeSymbol, INamedTypeSymbol, IElementNodeMapperBase, IReferenceGenerator, IAbstractionNode> abstractionNodeFactory, 
        Func<INamedTypeSymbol, IMethodSymbol, IFunctionNode, IRangeNode, IElementNodeMapperBase, ICheckTypeProperties, IUserDefinedElements, IReferenceGenerator, IImplementationNode> implementationNodeFactory, 
        Func<ITypeSymbol, IReferenceGenerator, IOutParameterNode> outParameterNodeFactory,
        Func<string, IErrorNode> errorNodeFactory, 
        Func<ITypeSymbol, IReferenceGenerator, INullNode> nullNodeFactory,
        Func<ITypeSymbol, IReadOnlyList<ITypeSymbol>, ImmutableSortedDictionary<TypeKey, (ITypeSymbol, IParameterNode)>, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IElementNodeMapperBase, IReferenceGenerator, ILocalFunctionNode> localFunctionNodeFactory,
        Func<IElementNodeMapperBase, PassedDependencies, ImmutableQueue<(INamedTypeSymbol, INamedTypeSymbol)>, IOverridingElementNodeMapper> overridingElementNodeMapperFactory,
        Func<IElementNodeMapperBase, PassedDependencies, INonWrapToCreateElementNodeMapper> nonWrapToCreateElementNodeMapperFactory) 
        : base(parentFunction, 
            parentRange, 
            parentContainer, 
            userDefinedElements, 
            checkTypeProperties,
            referenceGenerator,
            diagLogger, 
            wellKnownTypes, 
            wellKnownTypesCollections, 
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
            overridingElementNodeMapperFactory,
            nonWrapToCreateElementNodeMapperFactory)
    {
    }

    protected override IElementNodeMapperBase NextForWraps => this;
    protected override IElementNodeMapperBase Next => this;
}