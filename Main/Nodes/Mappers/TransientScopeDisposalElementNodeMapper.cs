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

internal interface ITransientScopeDisposalElementNodeMapper : IElementNodeMapperBase
{
}

internal class TransientScopeDisposalElementNodeMapper : ElementNodeMapperBase, ITransientScopeDisposalElementNodeMapper
{
    private readonly PassedDependencies _passedDependencies;
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly Func<INamedTypeSymbol, IReferenceGenerator, ITransientScopeDisposalTriggerNode> _transientScopeDisposalTriggerNodeFactory;

    public TransientScopeDisposalElementNodeMapper(
        IElementNodeMapperBase parentElementNodeMapper,
        PassedDependencies passedDependencies,
        
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
        Func<IElementNodeMapperBase, PassedDependencies, INonWrapToCreateElementNodeMapper> nonWrapToCreateElementNodeMapperFactory,
        Func<INamedTypeSymbol, IReferenceGenerator, ITransientScopeDisposalTriggerNode> transientScopeDisposalTriggerNodeFactory) 
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
        _passedDependencies = passedDependencies;
        _wellKnownTypes = wellKnownTypes;
        _transientScopeDisposalTriggerNodeFactory = transientScopeDisposalTriggerNodeFactory;
        Next = parentElementNodeMapper;
    }

    protected override IElementNodeMapperBase NextForWraps => this;

    protected override IElementNodeMapperBase Next { get; }

    public override IElementNode Map(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedType
            && (SymbolEqualityComparer.Default.Equals(namedType, _wellKnownTypes.Disposable)
                || SymbolEqualityComparer.Default.Equals(namedType, _wellKnownTypes.AsyncDisposable)))
            return _transientScopeDisposalTriggerNodeFactory(namedType, _passedDependencies.ReferenceGenerator);

        return base.Map(type);
    }
}