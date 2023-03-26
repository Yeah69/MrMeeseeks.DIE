using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Logging;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Nodes.Roots;
// ReSharper disable InconsistentNaming

namespace MrMeeseeks.DIE.MsContainer;

[ImplementationChoice(typeof(IRangeNode), typeof(ContainerNode))]
[ImplementationChoice(typeof(ICheckTypeProperties), typeof(ContainerCheckTypeProperties))]
[ImplementationChoice(typeof(IFunctionLevelLogMessageEnhancer), typeof(FunctionLevelLogMessageEnhancerForRanges))]
[CreateFunction(typeof(IExecuteContainer), "Create")]
internal sealed partial class MsContainer
{
    private readonly GeneratorExecutionContext DIE_Factory_GeneratorExecutionContext;
    private readonly Compilation DIE_Factory_Compilation;
    private readonly IContainerInfo DIE_Factory_ContainerInfo;

    public MsContainer(
        GeneratorExecutionContext context, 
        IContainerInfo dieFactoryContainerInfo)
    {
        DIE_Factory_ContainerInfo = dieFactoryContainerInfo;
        DIE_Factory_Compilation = context.Compilation;
        DIE_Factory_GeneratorExecutionContext = context;
    }

    [UserDefinedConstructorParametersInjection(typeof(UserDefinedElements))]
    private void DIE_ConstrParams_UserDefinedElements(out (INamedTypeSymbol? Range, INamedTypeSymbol Container) types) => 
        types = (DIE_Factory_ContainerInfo.ContainerType, DIE_Factory_ContainerInfo.ContainerType);

    [UserDefinedConstructorParametersInjection(typeof(ScopeInfo))]
    private void DIE_ConstrParams_ScopeInfo(
        out string name,
        out INamedTypeSymbol? scopeType)
    {
        name = "";
        scopeType = null;
    }

    private WellKnownTypes DIE_Factory_WellKnownTypes() => 
        WellKnownTypes.Create(DIE_Factory_Compilation);

    private WellKnownTypesAggregation DIE_Factory_WellKnownTypesAggregation() => 
        WellKnownTypesAggregation.Create(DIE_Factory_Compilation);

    private WellKnownTypesCollections DIE_Factory_WellKnownTypesCollections() => 
        WellKnownTypesCollections.Create(DIE_Factory_Compilation);

    private WellKnownTypesChoice DIE_Factory_WellKnownTypesChoice() => 
        WellKnownTypesChoice.Create(DIE_Factory_Compilation);

    private WellKnownTypesMiscellaneous DIE_Factory_WellKnownTypesMiscellaneous() => 
        WellKnownTypesMiscellaneous.Create(DIE_Factory_Compilation);

    [ImplementationChoice(typeof(IRangeNode), typeof(ScopeNode))]
    [ImplementationChoice(typeof(ICheckTypeProperties), typeof(ScopeCheckTypeProperties))]
    [CustomScopeForRootTypes(typeof(ScopeNodeRoot))]
    [InitializedInstances(typeof(ScopeInfo), typeof(ReferenceGenerator))]
    private sealed partial class DIE_TransientScope_ScopeNodeRoot
    {
        [UserDefinedConstructorParametersInjection(typeof(UserDefinedElements))]
        private void DIE_ConstrParams_UserDefinedElements(
            IContainerInfoContext containerInfoContext,
            IScopeInfo scopeInfo,
            out (INamedTypeSymbol? Range, INamedTypeSymbol Container) types) => 
            types = (scopeInfo.ScopeType, containerInfoContext.ContainerInfo.ContainerType);
    }

    [ImplementationChoice(typeof(IRangeNode), typeof(TransientScopeNode))]
    [ImplementationChoice(typeof(ICheckTypeProperties), typeof(ScopeCheckTypeProperties))]
    [CustomScopeForRootTypes(typeof(TransientScopeNodeRoot))]
    [InitializedInstances(typeof(ScopeInfo), typeof(ReferenceGenerator))]
    private sealed partial class DIE_TransientScope_TransientScopeNodeRoot
    {
        [UserDefinedConstructorParametersInjection(typeof(UserDefinedElements))]
        private void DIE_ConstrParams_UserDefinedElements(
            IContainerInfoContext containerInfoContext,
            IScopeInfo scopeInfo,
            out (INamedTypeSymbol? Range, INamedTypeSymbol Container) types) => 
            types = (scopeInfo.ScopeType, containerInfoContext.ContainerInfo.ContainerType);
    }

    [ImplementationChoice(typeof(IFunctionNode), typeof(CreateFunctionNode))]
    [ImplementationChoice(typeof(IFunctionLevelLogMessageEnhancer), typeof(FunctionLevelLogMessageEnhancer))]
    [CustomScopeForRootTypes(typeof(CreateFunctionNodeRoot))]
    [InitializedInstances(typeof(ReferenceGenerator))]
    private sealed partial class DIE_Scope_CreateFunctionNodeRoot
    {
    }

    [ImplementationChoice(typeof(IFunctionNode), typeof(CreateScopeFunctionNode))]
    [ImplementationChoice(typeof(IFunctionLevelLogMessageEnhancer), typeof(FunctionLevelLogMessageEnhancer))]
    [CustomScopeForRootTypes(typeof(CreateScopeFunctionNodeRoot))]
    [InitializedInstances(typeof(ReferenceGenerator))]
    private sealed partial class DIE_Scope_CreateScopeFunctionNodeRoot
    {
    }

    [ImplementationChoice(typeof(IFunctionNode), typeof(CreateTransientScopeFunctionNode))]
    [ImplementationChoice(typeof(IFunctionLevelLogMessageEnhancer), typeof(FunctionLevelLogMessageEnhancer))]
    [CustomScopeForRootTypes(typeof(CreateTransientScopeFunctionNodeRoot))]
    [InitializedInstances(typeof(ReferenceGenerator))]
    private sealed partial class DIE_Scope_CreateTransientScopeFunctionNodeRoot
    {
    }

    [ImplementationChoice(typeof(IFunctionNode), typeof(EntryFunctionNode))]
    [ImplementationChoice(typeof(IFunctionLevelLogMessageEnhancer), typeof(FunctionLevelLogMessageEnhancer))]
    [CustomScopeForRootTypes(typeof(EntryFunctionNodeRoot))]
    [InitializedInstances(typeof(ReferenceGenerator))]
    private sealed partial class DIE_Scope_EntryFunctionNodeRoot
    {
    }

    [ImplementationChoice(typeof(IFunctionNode), typeof(LocalFunctionNode))]
    [ImplementationChoice(typeof(IFunctionLevelLogMessageEnhancer), typeof(FunctionLevelLogMessageEnhancer))]
    [CustomScopeForRootTypes(typeof(LocalFunctionNodeRoot))]
    [InitializedInstances(typeof(ReferenceGenerator))]
    private sealed partial class DIE_Scope_LocalFunctionNodeRoot
    {
    }

    [ImplementationChoice(typeof(IFunctionNode), typeof(RangedInstanceFunctionNode))]
    [ImplementationChoice(typeof(IFunctionLevelLogMessageEnhancer), typeof(FunctionLevelLogMessageEnhancer))]
    [CustomScopeForRootTypes(typeof(RangedInstanceFunctionNodeRoot))]
    [InitializedInstances(typeof(ReferenceGenerator))]
    private sealed partial class DIE_Scope_RangedInstanceFunctionNodeRoot
    {
    }

    [ImplementationChoice(typeof(IFunctionNode), typeof(RangedInstanceInterfaceFunctionNode))]
    [ImplementationChoice(typeof(IFunctionLevelLogMessageEnhancer), typeof(FunctionLevelLogMessageEnhancer))]
    [CustomScopeForRootTypes(typeof(RangedInstanceInterfaceFunctionNodeRoot))]
    [InitializedInstances(typeof(ReferenceGenerator))]
    private sealed partial class DIE_Scope_RangedInstanceInterfaceFunctionNodeRoot
    {
    }

    [ImplementationChoice(typeof(IFunctionNode), typeof(MultiFunctionNode))]
    [ImplementationChoice(typeof(IFunctionLevelLogMessageEnhancer), typeof(FunctionLevelLogMessageEnhancer))]
    [CustomScopeForRootTypes(typeof(MultiFunctionNodeRoot))]
    [InitializedInstances(typeof(ReferenceGenerator))]
    private sealed partial class DIE_Scope_MultiFunctionNodeRoot
    {
    }

    [ImplementationChoice(typeof(IFunctionNode), typeof(VoidFunctionNode))]
    [ImplementationChoice(typeof(IFunctionLevelLogMessageEnhancer), typeof(FunctionLevelLogMessageEnhancer))]
    [CustomScopeForRootTypes(typeof(VoidFunctionNodeRoot))]
    [InitializedInstances(typeof(ReferenceGenerator))]
    private sealed partial class DIE_Scope_VoidFunctionNodeRoot
    {
    }
}