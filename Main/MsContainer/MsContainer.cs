using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Nodes.Roots;
using MsMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.MsContainer;

[ImplementationChoice(typeof(IRangeNode), typeof(ContainerNode))]
[CreateFunction(typeof(IContainerNodeRoot), "Create")]
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
    private void DIE_ConstrParams_UserDefinedElements(out (INamedTypeSymbol Range, INamedTypeSymbol Container) types) => 
        types = (DIE_Factory_ContainerInfo.ContainerType, DIE_Factory_ContainerInfo.ContainerType);

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
    [CustomScopeForRootTypes(typeof(ScopeNodeRoot))]
    private sealed partial class DIE_TransientScope_ScopeNodeRoot
    {
        
    }

    [ImplementationChoice(typeof(IRangeNode), typeof(TransientScopeNode))]
    [CustomScopeForRootTypes(typeof(TransientScopeNodeRoot))]
    private sealed partial class DIE_TransientScope_TransientScopeNodeRoot
    {
        
    }

    [ImplementationChoice(typeof(IFunctionNode), typeof(CreateFunctionNode))]
    [CustomScopeForRootTypes(typeof(CreateFunctionNodeRoot))]
    private sealed partial class DIE_Scope_CreateFunctionNodeRoot
    {
        
    }

    [ImplementationChoice(typeof(IFunctionNode), typeof(CreateScopeFunctionNode))]
    [CustomScopeForRootTypes(typeof(CreateScopeFunctionNodeRoot))]
    private sealed partial class DIE_Scope_CreateScopeFunctionNodeRoot
    {
        
    }

    [ImplementationChoice(typeof(IFunctionNode), typeof(CreateTransientScopeFunctionNode))]
    [CustomScopeForRootTypes(typeof(CreateTransientScopeFunctionNodeRoot))]
    private sealed partial class DIE_Scope_CreateTransientScopeFunctionNodeRoot
    {
        
    }

    [ImplementationChoice(typeof(IFunctionNode), typeof(EntryFunctionNode))]
    [CustomScopeForRootTypes(typeof(EntryFunctionNodeRoot))]
    private sealed partial class DIE_Scope_EntryFunctionNodeRoot
    {
        
    }

    [ImplementationChoice(typeof(IFunctionNode), typeof(LocalFunctionNode))]
    [CustomScopeForRootTypes(typeof(LocalFunctionNodeRoot))]
    private sealed partial class DIE_Scope_LocalFunctionNodeRoot
    {
        
    }

    [ImplementationChoice(typeof(IFunctionNode), typeof(RangedInstanceFunctionNode))]
    [CustomScopeForRootTypes(typeof(RangedInstanceFunctionNodeRoot))]
    private sealed partial class DIE_Scope_RangedInstanceFunctionNodeRoot
    {
        
    }

    [ImplementationChoice(typeof(IFunctionNode), typeof(RangedInstanceInterfaceFunctionNode))]
    [CustomScopeForRootTypes(typeof(RangedInstanceInterfaceFunctionNodeRoot))]
    private sealed partial class DIE_Scope_RangedInstanceInterfaceFunctionNodeRoot
    {
        
    }

    [ImplementationChoice(typeof(IFunctionNode), typeof(MultiFunctionNode))]
    [CustomScopeForRootTypes(typeof(MultiFunctionNodeRoot))]
    private sealed partial class DIE_Scope_MultiFunctionNodeRoot
    {
        
    }
}