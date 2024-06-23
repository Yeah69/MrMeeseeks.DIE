using MrMeeseeks.DIE.CodeGeneration;
using MrMeeseeks.DIE.CodeGeneration.Nodes;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Mappers;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Roots;
using MrMeeseeks.DIE.Utility;

namespace MrMeeseeks.DIE.Nodes.Ranges;

internal interface IScopeNode : IScopeNodeBase
{
    string TransientScopeInterfaceName { get; }
    string TransientScopeInterfaceReference { get; }
    IScopeCallNode BuildScopeCallFunction(
        string containerParameter,
        string transientScopeInterfaceParameter,
        INamedTypeSymbol type,
        IRangeNode callingRange,
        IFunctionNode callingFunction,
        IElementNodeMapperBase scopeImplementationMapper);
}

internal sealed partial class ScopeNode : ScopeNodeBase, IScopeNode, IScopeInstance
{
    private readonly Lazy<IScopeNodeGenerator> _scopeNodeGenerator;
    private readonly Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, ICreateScopeFunctionNodeRoot> _createScopeFunctionNodeFactory;

    internal ScopeNode(
        IScopeInfo scopeInfo,
        IContainerNode parentContainer,
        ITransientScopeInterfaceNode transientScopeInterfaceNode,
        IScopeManager scopeManager,
        IUserDefinedElements userDefinedElements,
        IReferenceGenerator referenceGenerator,
        ITypeParameterUtility typeParameterUtility,
        IRangeUtility rangeUtility,
        IRequiredKeywordUtility requiredKeywordUtility,
        ICheckTypeProperties checkTypeProperties,
        WellKnownTypes wellKnownTypes,
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous,
        IMapperDataToFunctionKeyTypeConverter mapperDataToFunctionKeyTypeConverter,
        Lazy<IScopeNodeGenerator> scopeNodeGenerator,
        Func<MapperData, ITypeSymbol, IReadOnlyList<ITypeSymbol>, ImplementationMappingConfiguration?, ICreateFunctionNodeRoot> createFunctionNodeFactory,
        Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IMultiFunctionNodeRoot> multiFunctionNodeFactory,
        Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IMultiKeyValueFunctionNodeRoot> multiKeyValueFunctionNodeFactory,
        Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IMultiKeyValueMultiFunctionNodeRoot> multiKeyValueMultiFunctionNodeFactory,
        Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, ICreateScopeFunctionNodeRoot> createScopeFunctionNodeFactory,
        Func<ScopeLevel, INamedTypeSymbol, IRangedInstanceFunctionGroupNode> rangedInstanceFunctionGroupNodeFactory,
        Func<IReadOnlyList<IInitializedInstanceNode>, IReadOnlyList<ITypeSymbol>, IVoidFunctionNodeRoot> voidFunctionNodeFactory, 
        Func<IDisposalHandlingNode> disposalHandlingNodeFactory,
        Func<INamedTypeSymbol, IInitializedInstanceNode> initializedInstanceNodeFactory)
        : base(
            scopeInfo, 
            parentContainer,
            scopeManager,
            userDefinedElements, 
            referenceGenerator,
            typeParameterUtility,
            rangeUtility,
            requiredKeywordUtility,
            checkTypeProperties,
            wellKnownTypes,
            wellKnownTypesMiscellaneous,
            mapperDataToFunctionKeyTypeConverter,
            createFunctionNodeFactory,  
            multiFunctionNodeFactory,
            multiKeyValueFunctionNodeFactory,
            multiKeyValueMultiFunctionNodeFactory,
            rangedInstanceFunctionGroupNodeFactory,
            voidFunctionNodeFactory,
            disposalHandlingNodeFactory,
            initializedInstanceNodeFactory)
    {
        _scopeNodeGenerator = scopeNodeGenerator;
        _createScopeFunctionNodeFactory = createScopeFunctionNodeFactory;
        TransientScopeInterfaceName = transientScopeInterfaceNode.Name; // Using name instead of full name to prevent compile error as the transient scope interface is not defined before code generation
        TransientScopeInterfaceReference = referenceGenerator.Generate("TransientScope");
    }
    protected override string ContainerParameterForScope => ContainerReference;
    protected override string TransientScopeInterfaceParameterForScope => TransientScopeInterfaceReference;

    public override INodeGenerator GetGenerator() => _scopeNodeGenerator.Value;

    public override IFunctionCallNode BuildContainerInstanceCall(INamedTypeSymbol type, IFunctionNode callingFunction) => 
        ParentContainer.BuildContainerInstanceCall(ContainerReference, type, callingFunction);

    public override IFunctionCallNode BuildTransientScopeInstanceCall(INamedTypeSymbol type, IFunctionNode callingFunction) =>
        ParentContainer.TransientScopeInterface.BuildTransientScopeInstanceCall(
            TransientScopeInterfaceReference, 
            type,
            callingFunction);

    public IScopeCallNode BuildScopeCallFunction(
        string containerParameter,
        string transientScopeInterfaceParameter,
        INamedTypeSymbol type,
        IRangeNode callingRange,
        IFunctionNode callingFunction,
        IElementNodeMapperBase scopeImplementationMapper)
    {
        return FunctionResolutionUtility.GetOrCreateFunctionCall(
            type,
            callingFunction,
            _createFunctions,
            () => _createScopeFunctionNodeFactory(
                    (INamedTypeSymbol) TypeParameterUtility.ReplaceTypeParametersByCustom(type),
                    callingFunction.Overrides.Select(kvp => kvp.Key).ToList())
                .Function
                .EnqueueBuildJobTo(ParentContainer.BuildQueue, new(ImmutableStack<INamedTypeSymbol>.Empty, null)),
            f => f.CreateScopeCall(
                type, 
                containerParameter, 
                transientScopeInterfaceParameter, 
                callingRange, 
                callingFunction,
                this, TypeParameterUtility.ExtractTypeParameters(type),
                scopeImplementationMapper));
    }

    public string TransientScopeInterfaceName { get; }
    public string TransientScopeInterfaceReference { get; }
}