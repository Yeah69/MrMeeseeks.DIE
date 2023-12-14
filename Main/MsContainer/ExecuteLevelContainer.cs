using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.Interceptors;
using MrMeeseeks.DIE.Logging;

// ReSharper disable InconsistentNaming

namespace MrMeeseeks.DIE.MsContainer;

[DecoratorSequenceChoice(typeof(ILogEnhancer), typeof(ExecuteLevelLogEnhancerDecorator))]
[CreateFunction(typeof(IExecute), "Create")]
internal sealed partial class ExecuteLevelContainer
{
    private readonly GeneratorExecutionContext DIE_Factory_GeneratorExecutionContext;
    private readonly Compilation DIE_Factory_Compilation;

    private ExecuteLevelContainer(
        GeneratorExecutionContext context)
    {
        DIE_Factory_Compilation = context.Compilation;
        DIE_Factory_GeneratorExecutionContext = context;
    }

    private ContainerLevelContainer DIE_Factory_ContainerLevelContainer(
        IContainerInfo containerInfo, 
        IInvocationTypeManager invocationTypeManager)
    {
        var container = ContainerLevelContainer.DIE_CreateContainer(
            DIE_Factory_GeneratorExecutionContext, 
            containerInfo,
            invocationTypeManager);
        DIE_AddForDisposalAsync(container);
        return container;
    }
    
    private partial void DIE_AddForDisposalAsync(IAsyncDisposable asyncDisposable);

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
}