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

internal interface IOverridingElementNodeMapperComposite : IElementNodeMapperBase
{
}

internal class OverridingElementNodeMapperComposite : ElementNodeMapperBase, IOverridingElementNodeMapperComposite
{
    private readonly (TypeKey Key, IReadOnlyList<IElementNode> Nodes) _override;
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly IContainerNode _parentContainer;
    private readonly IReferenceGenerator _referenceGenerator;
    
    private readonly Func<ITypeSymbol, IReadOnlyList<IElementNode>, IReferenceGenerator, ICollectionNode> _collectionNodeFactory;
    private readonly Func<IElementNodeMapperBase, PassedDependencies, (TypeKey, IElementNode), IOverridingElementNodeMapper> _overridingElementNodeMapperFactory;

    public OverridingElementNodeMapperComposite(
        IElementNodeMapperBase parentElementNodeMapper,
        PassedDependencies passedDependencies,
        (TypeKey, IReadOnlyList<IElementNode>) @override,
        
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
        Func<INamedTypeSymbol, IMethodSymbol, IFunctionNode, IElementNodeMapperBase, ICheckTypeProperties, IUserDefinedElements, IReferenceGenerator, IImplementationNode> implementationNodeFactory, 
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
        _overridingElementNodeMapperFactory = overridingElementNodeMapperFactory;
        _wellKnownTypes = wellKnownTypes;
        _collectionNodeFactory = collectionNodeFactory;
        _parentContainer = passedDependencies.ParentContainer;
        _referenceGenerator = passedDependencies.ReferenceGenerator;
    }

    protected override IElementNodeMapperBase NextForWraps => this;

    protected override IElementNodeMapperBase Next { get; }

    private bool IsLegitCompositeCollectionInnerType(ITypeSymbol innerCollectionType, TypeKey seekedType)
    {
        var currentInnerType = innerCollectionType;
        do
        {
            if (currentInnerType.ToTypeKey().Equals(seekedType))
                return true;
            if (currentInnerType is not INamedTypeSymbol temp
                || SymbolEqualityComparer.Default.Equals(_wellKnownTypes.Task1, temp.OriginalDefinition)
                || SymbolEqualityComparer.Default.Equals(_wellKnownTypes.ValueTask1, temp.OriginalDefinition))
                return false;
            currentInnerType = temp;
        } while (true);
    }

    public override IElementNode Map(ITypeSymbol type)
    {
        if (IsCollectionType(type))
        {
            var itemType = GetCollectionsItemType(type);
            if (IsLegitCompositeCollectionInnerType(itemType, _override.Item1))
            {
                var itemNodes = _override
                    .Nodes
                    .Select(n => _overridingElementNodeMapperFactory(this, MapperDependencies, (_override.Key, n)).Map(itemType))
                    .ToList();
                return _collectionNodeFactory(type, itemNodes, _referenceGenerator).EnqueueTo(_parentContainer.BuildQueue);
            }
        }
        return base.Map(type);
    }
}