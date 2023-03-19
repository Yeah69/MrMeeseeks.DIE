using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Mappers;
using MrMeeseeks.DIE.Nodes.Roots;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Nodes.Ranges;


internal interface IScopeNodeBase : IRangeNode
{
    string ContainerFullName { get; }
    string ContainerParameterReference { get; }
}

internal abstract class ScopeNodeBase : RangeNode, IScopeNodeBase
{
    internal ScopeNodeBase(
        IScopeInfo scopeInfo,
        IContainerNode parentContainer,
        IScopeManager scopeManager,
        IUserDefinedElements userDefinedElements,
        IReferenceGenerator referenceGenerator,
        IContainerWideContext containerWideContext,
        IMapperDataToFunctionKeyTypeConverter mapperDataToFunctionKeyTypeConverter,
        Func<MapperData, ITypeSymbol, IReadOnlyList<ITypeSymbol>, ICreateFunctionNodeRoot> createFunctionNodeFactory,
        Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IMultiFunctionNodeRoot> multiFunctionNodeFactory,
        Func<ScopeLevel, INamedTypeSymbol, IRangedInstanceFunctionGroupNode> rangedInstanceFunctionGroupNodeFactory,
        Func<IReadOnlyList<IInitializedInstanceNode>, IReadOnlyList<ITypeSymbol>, IVoidFunctionNodeRoot> voidFunctionNodeFactory, 
        Func<IDisposalHandlingNode> disposalHandlingNodeFactory,
        Func<INamedTypeSymbol, IInitializedInstanceNode> initializedInstanceNodeFactory)
        : base(
            scopeInfo.Name, 
            userDefinedElements, 
            mapperDataToFunctionKeyTypeConverter,
            createFunctionNodeFactory,  
            multiFunctionNodeFactory,
            rangedInstanceFunctionGroupNodeFactory,
            voidFunctionNodeFactory,
            disposalHandlingNodeFactory)
    {
        ParentContainer = parentContainer;
        ScopeManager = scopeManager;
        FullName = $"{parentContainer.Namespace}.{parentContainer.Name}.{scopeInfo.Name}";
        ContainerFullName = parentContainer.FullName;
        ContainerReference = referenceGenerator.Generate("_container");
        ContainerParameterReference = referenceGenerator.Generate("container");

        // todo support multiple initialized instances attributes
        if (scopeInfo.ScopeType is { } scopeType
            && (scopeType
                    .GetAttributes()
                    .FirstOrDefault(ad =>
                        CustomSymbolEqualityComparer.Default.Equals(ad.AttributeClass,
                            containerWideContext.WellKnownTypesMiscellaneous.InitializedInstancesAttribute)))
                is { ConstructorArguments.Length: 1 } initializedInstancesAttribute
            && initializedInstancesAttribute.ConstructorArguments[0].Kind == TypedConstantKind.Array)
        {
            var types = initializedInstancesAttribute
                 .ConstructorArguments[0]
                 .Values
                 .Select(tc => tc.Value)
                 .OfType<INamedTypeSymbol>();
            foreach (var type in types)
                InitializedInstanceNodesMap[type] = initializedInstanceNodeFactory(type);
        }
    }

    protected override IScopeManager ScopeManager { get; }
    protected override IContainerNode ParentContainer { get; }
    protected override string ContainerParameterForScope => ContainerReference;

    public override IFunctionCallNode BuildContainerInstanceCall(INamedTypeSymbol type, IFunctionNode callingFunction) => 
        ParentContainer.BuildContainerInstanceCall(ContainerReference, type, callingFunction);

    public override string FullName { get; }
    public override DisposalType DisposalType => ParentContainer.DisposalType;
    public string ContainerFullName { get; }
    public override string ContainerReference { get; }
    public string ContainerParameterReference { get; }
}