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

internal interface ITransientScopeNode : IScopeNodeBase
{
    string TransientScopeInterfaceName { get; }
    string TransientScopeDisposalReference { get; }
    ITransientScopeCallNode BuildTransientScopeCallFunction(
        string containerParameter,
        INamedTypeSymbol type,
        IRangeNode callingRange,
        IFunctionNode callingFunction,
        IElementNodeMapperBase transientScopeImplementationMapper);
}

internal sealed partial class TransientScopeNode : ScopeNodeBase, ITransientScopeNode, ITransientScopeInstance
{
    private readonly Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, ICreateTransientScopeFunctionNodeRoot> _createTransientScopeFunctionNodeFactory;

    internal TransientScopeNode(
        IScopeInfo scopeInfo,
        IContainerNode parentContainer,
        IScopeManager scopeManager,
        IUserDefinedElements userDefinedElements,
        IReferenceGenerator referenceGenerator,
        ITypeParameterUtility typeParameterUtility,
        IRangeUtility rangeUtility,
        IRequiredKeywordUtility requiredKeywordUtility,
        WellKnownTypes wellKnownTypes,
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous,
        IMapperDataToFunctionKeyTypeConverter mapperDataToFunctionKeyTypeConverter,
        Func<MapperData, ITypeSymbol, IReadOnlyList<ITypeSymbol>, ICreateFunctionNodeRoot> createFunctionNodeFactory,
        Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IMultiFunctionNodeRoot> multiFunctionNodeFactory,
        Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IMultiKeyValueFunctionNodeRoot> multiKeyValueFunctionNodeFactory,
        Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IMultiKeyValueMultiFunctionNodeRoot> multiKeyValueMultiFunctionNodeFactory,
        Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, ICreateTransientScopeFunctionNodeRoot> createTransientScopeFunctionNodeFactory,
        Func<ScopeLevel, INamedTypeSymbol, IRangedInstanceFunctionGroupNode> rangedInstanceFunctionGroupNodeFactory,
        Func<IReadOnlyList<IInitializedInstanceNode>, IReadOnlyList<ITypeSymbol>, IVoidFunctionNodeRoot> voidFunctionNodeFactory, 
        Func<IDisposalHandlingNode> disposalHandlingNodeFactory,
        Func<INamedTypeSymbol, IInitializedInstanceNode> initializedInstanceNodeFactory)
        : base (
            scopeInfo, 
            parentContainer,
            scopeManager,
            userDefinedElements, 
            referenceGenerator,
            typeParameterUtility,
            rangeUtility,
            requiredKeywordUtility,
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
        _createTransientScopeFunctionNodeFactory = createTransientScopeFunctionNodeFactory;
        TransientScopeInterfaceName = parentContainer.TransientScopeInterface.Name;
        TransientScopeDisposalReference = parentContainer.TransientScopeDisposalReference;
    }
    protected override string ContainerParameterForScope => ContainerReference;

    public override IFunctionCallNode BuildContainerInstanceCall(INamedTypeSymbol type, IFunctionNode callingFunction)
    {
        return ParentContainer.BuildContainerInstanceCall(ContainerReference, type, callingFunction);
    }

    public override IFunctionCallNode BuildTransientScopeInstanceCall(INamedTypeSymbol type, IFunctionNode callingFunction) => 
        ParentContainer.TransientScopeInterface.BuildTransientScopeInstanceCall($"({Constants.ThisKeyword} as {ParentContainer.TransientScopeInterface.FullName})", type, callingFunction);

    public string TransientScopeInterfaceName { get; }
    public string TransientScopeDisposalReference { get; }

    public ITransientScopeCallNode BuildTransientScopeCallFunction(
        string containerParameter,
        INamedTypeSymbol type,
        IRangeNode callingRange,
        IFunctionNode callingFunction,
        IElementNodeMapperBase transientScopeImplementationMapper)
    {
        return FunctionResolutionUtility.GetOrCreateFunctionCall(
            type,
            callingFunction,
            _createFunctions,
            () => _createTransientScopeFunctionNodeFactory(
                    (INamedTypeSymbol) TypeParameterUtility.ReplaceTypeParametersByCustom(type),
                    callingFunction.Overrides.Select(kvp => kvp.Key).ToList())
                .Function
                .EnqueueBuildJobTo(ParentContainer.BuildQueue, new(ImmutableStack<INamedTypeSymbol>.Empty, null)),
            f => f.CreateTransientScopeCall(
                type, 
                containerParameter, 
                callingRange, 
                callingFunction, 
                this,
                TypeParameterUtility.ExtractTypeParameters(type),
                transientScopeImplementationMapper));
    }
}