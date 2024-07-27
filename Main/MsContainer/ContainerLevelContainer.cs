using MrMeeseeks.DIE.CodeGeneration;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.Logging;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Nodes.Roots;
using MrMeeseeks.SourceGeneratorUtility;

// ReSharper disable InconsistentNaming

namespace MrMeeseeks.DIE.MsContainer;

internal interface IContainerLevelOnlyContainerInstance;

[ContainerInstanceImplementationAggregation(
    typeof(GeneratorExecutionContext),
    typeof(NamedTypeCache) // Cache should be initialized on first run and read after it
    )]
[ContainerInstanceAbstractionAggregation(typeof(IContainerLevelOnlyContainerInstance))]
[ImplementationChoice(typeof(IRangeNode), typeof(ContainerNode))]
[ImplementationChoice(typeof(ICheckTypeProperties), typeof(ContainerCheckTypeProperties))]
[DecoratorSequenceChoice(typeof(ILogEnhancer), typeof(ILogEnhancer), typeof(ContainerLevelLogEnhancerDecorator), typeof(ExecuteLevelLogEnhancerDecorator))]
[CreateFunction(typeof(IExecuteContainer), "Create")]
internal sealed partial class ContainerLevelContainer
{
    private readonly GeneratorExecutionContext DIE_Factory_GeneratorExecutionContext;
    private readonly Compilation DIE_Factory_Compilation;
    private readonly ContainerInfo DIE_Factory_ContainerInfo;
    private readonly RequiredKeywordUtility DIE_Factory_RequiredKeywordUtility;
    private readonly DisposeUtility DIE_Factory_DisposeUtility;
    private readonly ReferenceGeneratorCounter DIE_Factory_referenceGeneratorCounter;

    private ContainerLevelContainer(
        GeneratorExecutionContext context, 
        ContainerInfo dieFactoryContainerInfo,
        RequiredKeywordUtility dieFactoryRequiredKeywordUtility,
        DisposeUtility dieFactoryDisposeUtility, 
        ReferenceGeneratorCounter dieFactoryReferenceGeneratorCounter)
    {
        DIE_Factory_ContainerInfo = dieFactoryContainerInfo;
        DIE_Factory_RequiredKeywordUtility = dieFactoryRequiredKeywordUtility;
        DIE_Factory_DisposeUtility = dieFactoryDisposeUtility;
        DIE_Factory_referenceGeneratorCounter = dieFactoryReferenceGeneratorCounter;
        DIE_Factory_Compilation = context.Compilation;
        DIE_Factory_GeneratorExecutionContext = context;
    }

    [UserDefinedConstructorParametersInjection(typeof(UserDefinedElements))]
    private void DIE_ConstrParams_UserDefinedElements(out (INamedTypeSymbol? Range, INamedTypeSymbol Container) types) => 
        types = (DIE_Factory_ContainerInfo.ContainerType, DIE_Factory_ContainerInfo.ContainerType);

    [UserDefinedConstructorParametersInjection(typeof(ScopeInfo))]
    private static void DIE_ConstrParams_ScopeInfo(
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

    private WellKnownTypesMapping DIE_Factory_WellKnownTypesMapping() => 
        WellKnownTypesMapping.Create(DIE_Factory_Compilation);

    [ImplementationChoice(typeof(IRangeNode), typeof(ScopeNode))]
    [InitializedInstances(typeof(ReferenceGenerator))]
    private abstract class ScopeObject;
    
    [ImplementationChoice(typeof(ICheckTypeProperties), typeof(ScopeCheckTypeProperties))]
    [InitializedInstances(typeof(ScopeInfo))]
    private abstract class TransientScopeBase : ScopeObject
    {
        [UserDefinedConstructorParametersInjection(typeof(UserDefinedElements))]
        protected static void DIE_ConstrParams_UserDefinedElements(
            IContainerInfo containerInfo,
            IScopeInfo scopeInfo,
            out (INamedTypeSymbol? Range, INamedTypeSymbol Container) types) => 
            types = (scopeInfo.ScopeType, containerInfo.ContainerType);
    }

    [ImplementationChoice(typeof(IRangeNode), typeof(ScopeNode))]
    [CustomScopeForRootTypes(typeof(ScopeNodeRoot))]
    private sealed partial class DIE_TransientScope_ScopeNodeRoot : TransientScopeBase;

    [ImplementationChoice(typeof(IRangeNode), typeof(TransientScopeNode))]
    [CustomScopeForRootTypes(typeof(TransientScopeNodeRoot))]
    private sealed partial class DIE_TransientScope_TransientScopeNodeRoot : TransientScopeBase;

    [DecoratorSequenceChoice(typeof(ILogEnhancer), typeof(ILogEnhancer), typeof(FunctionLevelLogEnhancerDecorator), typeof(ContainerLevelLogEnhancerDecorator), typeof(ExecuteLevelLogEnhancerDecorator))]
    private abstract class ScopeBase : ScopeObject
    {
        protected readonly IRangeNode DIE_Factory_parentRange;
        protected readonly ICheckTypeProperties DIE_Factory_checkTypeProperties;

        protected ScopeBase(
            IRangeNode parentRange,
            ICheckTypeProperties checkTypeProperties)
        {
            DIE_Factory_parentRange = parentRange;
            DIE_Factory_checkTypeProperties = checkTypeProperties;
        }
    }

    [ImplementationChoice(typeof(IFunctionNode), typeof(CreateFunctionNode))]
    [CustomScopeForRootTypes(typeof(CreateFunctionNodeRoot))]
    [InitializedInstances(typeof(CreateFunctionNode))]
    private sealed partial class DIE_Scope_CreateFunctionNodeRoot : ScopeBase
    {
        internal DIE_Scope_CreateFunctionNodeRoot(IRangeNode parentRange, ICheckTypeProperties checkTypeProperties) 
            : base(parentRange, checkTypeProperties) { }
    }

    [ImplementationChoice(typeof(IFunctionNode), typeof(CreateScopeFunctionNode))]
    [CustomScopeForRootTypes(typeof(CreateScopeFunctionNodeRoot))]
    [InitializedInstances(typeof(CreateScopeFunctionNode))]
    private sealed partial class DIE_Scope_CreateScopeFunctionNodeRoot : ScopeBase
    {
        internal DIE_Scope_CreateScopeFunctionNodeRoot(IRangeNode parentRange, ICheckTypeProperties checkTypeProperties) 
            : base(parentRange, checkTypeProperties) { }
    }

    [ImplementationChoice(typeof(IFunctionNode), typeof(CreateTransientScopeFunctionNode))]
    [CustomScopeForRootTypes(typeof(CreateTransientScopeFunctionNodeRoot))]
    [InitializedInstances(typeof(CreateTransientScopeFunctionNode))]
    private sealed partial class DIE_Scope_CreateTransientScopeFunctionNodeRoot : ScopeBase
    {
        internal DIE_Scope_CreateTransientScopeFunctionNodeRoot(IRangeNode parentRange, ICheckTypeProperties checkTypeProperties) 
            : base(parentRange, checkTypeProperties) { }
    }

    [ImplementationChoice(typeof(IFunctionNode), typeof(EntryFunctionNode))]
    [CustomScopeForRootTypes(typeof(EntryFunctionNodeRoot))]
    [InitializedInstances(typeof(EntryFunctionNode))]
    private sealed partial class DIE_Scope_EntryFunctionNodeRoot : ScopeBase
    {
        internal DIE_Scope_EntryFunctionNodeRoot(IRangeNode parentRange, ICheckTypeProperties checkTypeProperties) 
            : base(parentRange, checkTypeProperties) { }
    }

    [ImplementationChoice(typeof(IFunctionNode), typeof(LocalFunctionNode))]
    [CustomScopeForRootTypes(typeof(LocalFunctionNodeRoot))]
    [InitializedInstances(typeof(LocalFunctionNode))]
    private sealed partial class DIE_Scope_LocalFunctionNodeRoot : ScopeBase
    {
        internal DIE_Scope_LocalFunctionNodeRoot(IRangeNode parentRange, ICheckTypeProperties checkTypeProperties) 
            : base(parentRange, checkTypeProperties) { }
    }

    [ImplementationChoice(typeof(IFunctionNode), typeof(RangedInstanceFunctionNode))]
    [CustomScopeForRootTypes(typeof(RangedInstanceFunctionNodeRoot))]
    [InitializedInstances(typeof(RangedInstanceFunctionNode))]
    private sealed partial class DIE_Scope_RangedInstanceFunctionNodeRoot : ScopeBase
    {
        internal DIE_Scope_RangedInstanceFunctionNodeRoot(IRangeNode parentRange, ICheckTypeProperties checkTypeProperties) 
            : base(parentRange, checkTypeProperties) { }
    }

    [ImplementationChoice(typeof(IFunctionNode), typeof(RangedInstanceInterfaceFunctionNode))]
    [CustomScopeForRootTypes(typeof(RangedInstanceInterfaceFunctionNodeRoot))]
    [InitializedInstances(typeof(RangedInstanceInterfaceFunctionNode))]
    private sealed partial class DIE_Scope_RangedInstanceInterfaceFunctionNodeRoot : ScopeBase
    {
        internal DIE_Scope_RangedInstanceInterfaceFunctionNodeRoot(IRangeNode parentRange, ICheckTypeProperties checkTypeProperties) 
            : base(parentRange, checkTypeProperties) { }
    }

    [ImplementationChoice(typeof(IFunctionNode), typeof(MultiFunctionNode))]
    [CustomScopeForRootTypes(typeof(MultiFunctionNodeRoot))]
    [InitializedInstances(typeof(MultiFunctionNode))]
    private sealed partial class DIE_Scope_MultiFunctionNodeRoot : ScopeBase
    {
        internal DIE_Scope_MultiFunctionNodeRoot(IRangeNode parentRange, ICheckTypeProperties checkTypeProperties) 
            : base(parentRange, checkTypeProperties) { }
    }

    [ImplementationChoice(typeof(IFunctionNode), typeof(VoidFunctionNode))]
    [CustomScopeForRootTypes(typeof(VoidFunctionNodeRoot))]
    [InitializedInstances(typeof(VoidFunctionNode))]
    private sealed partial class DIE_Scope_VoidFunctionNodeRoot : ScopeBase
    {
        internal DIE_Scope_VoidFunctionNodeRoot(IRangeNode parentRange, ICheckTypeProperties checkTypeProperties) 
            : base(parentRange, checkTypeProperties) { }
    }

    [ImplementationChoice(typeof(IFunctionNode), typeof(MultiKeyValueFunctionNode))]
    [CustomScopeForRootTypes(typeof(MultiKeyValueFunctionNodeRoot))]
    [InitializedInstances(typeof(MultiKeyValueFunctionNode))]
    private sealed partial class DIE_Scope_MultiKeyValueFunctionNodeRoot : ScopeBase
    {
        internal DIE_Scope_MultiKeyValueFunctionNodeRoot(IRangeNode parentRange, ICheckTypeProperties checkTypeProperties) 
            : base(parentRange, checkTypeProperties) { }
    }

    [ImplementationChoice(typeof(IFunctionNode), typeof(MultiKeyValueMultiFunctionNode))]
    [CustomScopeForRootTypes(typeof(MultiKeyValueMultiFunctionNodeRoot))]
    [InitializedInstances(typeof(MultiKeyValueMultiFunctionNode))]
    private sealed partial class DIE_Scope_MultiKeyValueMultiFunctionNodeRoot : ScopeBase
    {
        internal DIE_Scope_MultiKeyValueMultiFunctionNodeRoot(IRangeNode parentRange, ICheckTypeProperties checkTypeProperties) 
            : base(parentRange, checkTypeProperties) { }
    }
}