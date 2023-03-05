using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Functions;
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
        IUserDefinedElementsBase userDefinedElements,
        IScopeCheckTypeProperties checkTypeProperties,
        IReferenceGenerator referenceGenerator,
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous,
        Func<ITypeSymbol, IReadOnlyList<ITypeSymbol>, IRangeNode, IContainerNode, IUserDefinedElementsBase, ICheckTypeProperties, IReferenceGenerator, ICreateFunctionNodeRoot> createFunctionNodeFactory,
        Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IRangeNode, IContainerNode, IUserDefinedElementsBase, ICheckTypeProperties, IReferenceGenerator, IMultiFunctionNodeRoot> multiFunctionNodeFactory,
        Func<ScopeLevel, INamedTypeSymbol, IRangeNode, IContainerNode, IUserDefinedElementsBase, ICheckTypeProperties, IReferenceGenerator, IRangedInstanceFunctionGroupNode> rangedInstanceFunctionGroupNodeFactory,
        Func<IReadOnlyList<IInitializedInstanceNode>, IReadOnlyList<ITypeSymbol>, IRangeNode, IContainerNode, IReferenceGenerator, IVoidFunctionNodeRoot> voidFunctionNodeFactory, 
        Func<IReferenceGenerator, IDisposalHandlingNode> disposalHandlingNodeFactory,
        Func<INamedTypeSymbol, IReferenceGenerator, IInitializedInstanceNode> initializedInstanceNodeFactory)
        : base(
            scopeInfo.Name, 
            userDefinedElements, 
            checkTypeProperties, 
            referenceGenerator, 
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

        if (scopeInfo.ScopeType is { } scopeType
            && scopeType
                    .GetAttributes()
                    .FirstOrDefault(ad =>
                        CustomSymbolEqualityComparer.Default.Equals(ad.AttributeClass,
                            wellKnownTypesMiscellaneous.InitializedInstancesForScopesAttribute))
                is { ConstructorArguments.Length: 1 } initializedInstancesAttribute
            && initializedInstancesAttribute.ConstructorArguments[0].Kind == TypedConstantKind.Array)
        {
            var types = initializedInstancesAttribute
                 .ConstructorArguments[0]
                 .Values
                 .Select(tc => tc.Value)
                 .OfType<INamedTypeSymbol>();
            foreach (var type in types)
                InitializedInstanceNodesMap[type] = initializedInstanceNodeFactory(type, ReferenceGenerator);
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