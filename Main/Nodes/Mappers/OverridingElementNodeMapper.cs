using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.Delegates;
using MrMeeseeks.DIE.Nodes.Elements.Factories;
using MrMeeseeks.DIE.Nodes.Elements.Tasks;
using MrMeeseeks.DIE.Nodes.Elements.Tuples;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Nodes.Roots;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Nodes.Mappers;

internal interface IOverridingElementNodeMapper : IElementNodeMapperBase
{
}

internal class OverridingElementNodeMapper : ElementNodeMapperBase, IOverridingElementNodeMapper
{
    private readonly ImmutableQueue<(INamedTypeSymbol InterfaceType, INamedTypeSymbol ImplementationType)> _overrideParam;
    private readonly Func<INamedTypeSymbol, INamedTypeSymbol, IElementNodeMapperBase, IReferenceGenerator, IAbstractionNode> _abstractionNodeFactory;
    private readonly Func<IElementNodeMapperBase, PassedDependencies, ImmutableQueue<(INamedTypeSymbol, INamedTypeSymbol)>, IOverridingElementNodeMapper> _overridingElementNodeMapperFactory;

    public OverridingElementNodeMapper(
        IElementNodeMapperBase parentElementNodeMapper,
        PassedDependencies passedDependencies,
        ImmutableQueue<(INamedTypeSymbol, INamedTypeSymbol)> overrideParam,
        
        IDiagLogger diagLogger, 
        IContainerWideContext containerWideContext,
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
        Func<INamedTypeSymbol, IMethodSymbol, IFunctionNode, IRangeNode, IElementNodeMapperBase, ICheckTypeProperties, IUserDefinedElementsBase, IReferenceGenerator, IImplementationNode> implementationNodeFactory, 
        Func<ITypeSymbol, IReferenceGenerator, IOutParameterNode> outParameterNodeFactory,
        Func<string, ITypeSymbol, IRangeNode, IErrorNode> errorNodeFactory, 
        Func<ITypeSymbol, IReferenceGenerator, INullNode> nullNodeFactory,
        Func<ITypeSymbol, IReadOnlyList<ITypeSymbol>, ImmutableDictionary<ITypeSymbol, IParameterNode>, IRangeNode, IContainerNode, IUserDefinedElementsBase, ICheckTypeProperties, IElementNodeMapperBase, IReferenceGenerator, ILocalFunctionNodeRoot> localFunctionNodeFactory,
        Func<IElementNodeMapperBase, PassedDependencies, ImmutableQueue<(INamedTypeSymbol, INamedTypeSymbol)>, IOverridingElementNodeMapper> overridingElementNodeMapperFactory,
        Func<IElementNodeMapperBase, PassedDependencies, INonWrapToCreateElementNodeMapper> nonWrapToCreateElementNodeMapperFactory) 
        : base(passedDependencies.ParentFunction, 
            passedDependencies.ParentRange, 
            passedDependencies.ParentContainer, 
            passedDependencies.UserDefinedElements, 
            passedDependencies.CheckTypeProperties,
            passedDependencies.ReferenceGenerator,
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
            overridingElementNodeMapperFactory,
            nonWrapToCreateElementNodeMapperFactory)
    {
        Next = parentElementNodeMapper;
        _overrideParam = overrideParam;
        _abstractionNodeFactory = abstractionNodeFactory;
        _overridingElementNodeMapperFactory = overridingElementNodeMapperFactory;
    }

    protected override IElementNodeMapperBase NextForWraps => this;

    protected override IElementNodeMapperBase Next { get; }

    public override IElementNode Map(ITypeSymbol type, ImmutableStack<INamedTypeSymbol> implementationStack)
    {
        if (_overrideParam.Any() 
            && type is INamedTypeSymbol abstraction 
            && CustomSymbolEqualityComparer.Default.Equals(_overrideParam.Peek().InterfaceType, type))
        {
            var nextOverride = _overrideParam.Dequeue(out var currentOverride);
            var mapper = _overridingElementNodeMapperFactory(this, MapperDependencies, nextOverride);
            return _abstractionNodeFactory(abstraction, currentOverride.ImplementationType, mapper, MapperDependencies.ReferenceGenerator)
                .EnqueueBuildJobTo(MapperDependencies.ParentContainer.BuildQueue, implementationStack);
        }
        return base.Map(type, implementationStack);
    }
}