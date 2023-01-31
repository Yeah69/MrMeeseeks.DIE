using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.Delegates;
using MrMeeseeks.DIE.Nodes.Elements.Factories;
using MrMeeseeks.DIE.Nodes.Elements.Tasks;
using MrMeeseeks.DIE.Nodes.Elements.Tuples;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Utility;

namespace MrMeeseeks.DIE.Nodes.Mappers;

internal interface IOverridingElementNodeMapper : IElementNodeMapperBase
{
}

internal class OverridingElementNodeMapper : ElementNodeMapperBase, IOverridingElementNodeMapper
{
    private readonly (TypeKey Key, IElementNode Node) _override;

    public OverridingElementNodeMapper(
        IElementNodeMapperBase parentElementNodeMapper,
        PassedDependencies passedDependencies,
        (TypeKey, IElementNode) @override,
        
        IDiagLogger diagLogger, 
        WellKnownTypes wellKnownTypes, 
        Func<IFieldSymbol, IFunctionNode, IReferenceGenerator, IFactoryFieldNode> factoryFieldNodeFactory, 
        Func<IPropertySymbol, IFunctionNode, IReferenceGenerator, IFactoryPropertyNode> factoryPropertyNodeFactory, 
        Func<IMethodSymbol, IFunctionNode, IElementNodeMapperBase, IReferenceGenerator, IFactoryFunctionNode> factoryFunctionNodeFactory, 
        Func<INamedTypeSymbol, IFunctionNode, IElementNodeMapperBase, IReferenceGenerator, IValueTaskNode> valueTaskNodeFactory, 
        Func<INamedTypeSymbol, IFunctionNode, IElementNodeMapperBase, IReferenceGenerator, ITaskNode> taskNodeFactory, 
        Func<INamedTypeSymbol, IElementNodeMapperBase, IReferenceGenerator, IValueTupleNode> valueTupleNodeFactory, 
        Func<INamedTypeSymbol, IElementNodeMapperBase, IReferenceGenerator, IValueTupleSyntaxNode> valueTupleSyntaxNodeFactory, 
        Func<INamedTypeSymbol, IElementNodeMapperBase, IReferenceGenerator, ITupleNode> tupleNodeFactory, 
        Func<INamedTypeSymbol, ILocalFunctionNode, IReferenceGenerator, ILazyNode> lazyNodeFactory, 
        Func<INamedTypeSymbol, ILocalFunctionNode, IReferenceGenerator, IFuncNode> funcNodeFactory, 
        Func<ITypeSymbol, IReadOnlyList<IElementNode>, IReferenceGenerator, ICollectionNode> collectionNodeFactory, 
        Func<INamedTypeSymbol, IElementNode, IReferenceGenerator, IAbstractionNode> abstractionNodeFactory, 
        Func<INamedTypeSymbol, IMethodSymbol, IFunctionNode, IRangeNode, IElementNodeMapperBase, ICheckTypeProperties, IUserDefinedElements, IReferenceGenerator, IImplementationNode> implementationNodeFactory, 
        Func<ITypeSymbol, IReferenceGenerator, IOutParameterNode> outParameterNodeFactory,
        Func<string, IErrorNode> errorNodeFactory, 
        Func<ITypeSymbol, IReferenceGenerator, INullNode> nullNodeFactory,
        Func<ITypeSymbol, IReadOnlyList<ITypeSymbol>, ImmutableSortedDictionary<TypeKey, (ITypeSymbol, IParameterNode)>, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IElementNodeMapperBase, IReferenceGenerator, ILocalFunctionNode> localFunctionNodeFactory,
        Func<IElementNodeMapperBase, PassedDependencies, (TypeKey, IElementNode), IOverridingElementNodeMapper> overridingElementNodeMapperFactory,
        Func<IElementNodeMapperBase, PassedDependencies, (TypeKey, IReadOnlyList<IElementNode>), IOverridingElementNodeMapperComposite> overridingElementNodeMapperCompositeFactory,
        Func<IElementNodeMapperBase, PassedDependencies, INonWrapToCreateElementNodeMapper> nonWrapToCreateElementNodeMapperFactory) 
        : base(passedDependencies.ParentFunction, 
            passedDependencies.ParentRange, 
            passedDependencies.ParentContainer, 
            passedDependencies.UserDefinedElements, 
            passedDependencies.CheckTypeProperties,
            passedDependencies.ReferenceGenerator,
            diagLogger, 
            wellKnownTypes, 
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
            collectionNodeFactory, 
            abstractionNodeFactory,
            implementationNodeFactory, 
            outParameterNodeFactory,
            errorNodeFactory, 
            nullNodeFactory,
            localFunctionNodeFactory,
            overridingElementNodeMapperFactory,
            overridingElementNodeMapperCompositeFactory,
            nonWrapToCreateElementNodeMapperFactory)
    {
        Next = parentElementNodeMapper;
        _override = @override;
    }

    protected override IElementNodeMapperBase NextForWraps => this;

    protected override IElementNodeMapperBase Next { get; }

    public override IElementNode Map(ITypeSymbol type)
    {
        var typeKey = type.ToTypeKey();
        if (Equals(_override.Key, typeKey))
            return _override.Node;
        else
            return base.Map(type);
    }
}