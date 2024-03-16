using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Mappers;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Roots;
using MrMeeseeks.DIE.Utility;

namespace MrMeeseeks.DIE.Nodes.Ranges;


internal interface IScopeNodeBase : IRangeNode
{
    INamedTypeSymbol? ImplementationType { get; }
    string ContainerFullName { get; }
    bool GenerateEmptyConstructor { get; }
}

internal abstract class ScopeNodeBase : RangeNode, IScopeNodeBase
{
    internal ScopeNodeBase(
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
        Func<ScopeLevel, INamedTypeSymbol, IRangedInstanceFunctionGroupNode> rangedInstanceFunctionGroupNodeFactory,
        Func<IReadOnlyList<IInitializedInstanceNode>, IReadOnlyList<ITypeSymbol>, IVoidFunctionNodeRoot> voidFunctionNodeFactory, 
        Func<IDisposalHandlingNode> disposalHandlingNodeFactory,
        Func<INamedTypeSymbol, IInitializedInstanceNode> initializedInstanceNodeFactory)
        : base(
            scopeInfo.Name, 
            scopeInfo.ScopeType,
            userDefinedElements, 
            mapperDataToFunctionKeyTypeConverter,
            typeParameterUtility,
            rangeUtility,
            wellKnownTypes,
            wellKnownTypesMiscellaneous,
            referenceGenerator,
            createFunctionNodeFactory,  
            multiFunctionNodeFactory,
            multiKeyValueFunctionNodeFactory,
            multiKeyValueMultiFunctionNodeFactory,
            rangedInstanceFunctionGroupNodeFactory,
            voidFunctionNodeFactory,
            disposalHandlingNodeFactory,
            initializedInstanceNodeFactory)
    {
        ParentContainer = parentContainer;
        ScopeManager = scopeManager;
        FullName = $"{parentContainer.Namespace}.{parentContainer.Name}.{scopeInfo.Name}";
        ContainerFullName = parentContainer.FullName;
        ContainerReference = referenceGenerator.Generate("Container");
        ImplementationType = scopeInfo.ScopeType;
        
        GenerateEmptyConstructor =
            scopeInfo.ScopeType is null || scopeInfo.ScopeType.InstanceConstructors.Any(ic => ic.IsImplicitlyDeclared);
        requiredKeywordUtility.SetRequiredKeywordAsRequired();
    }

    protected override IScopeManager ScopeManager { get; }
    protected override IContainerNode ParentContainer { get; }
    protected override string ContainerParameterForScope => ContainerReference;

    public override IFunctionCallNode BuildContainerInstanceCall(INamedTypeSymbol type, IFunctionNode callingFunction) => 
        ParentContainer.BuildContainerInstanceCall(ContainerReference, type, callingFunction);

    public override string FullName { get; }
    public override DisposalType DisposalType => ParentContainer.DisposalType;
    public INamedTypeSymbol? ImplementationType { get; }
    public string ContainerFullName { get; }
    public bool GenerateEmptyConstructor { get; }
    public override string ContainerReference { get; }
}