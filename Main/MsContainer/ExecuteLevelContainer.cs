using MrMeeseeks.DIE.CodeGeneration;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.Logging;

// ReSharper disable InconsistentNaming

namespace MrMeeseeks.DIE.MsContainer;

[DecoratorSequenceChoice(typeof(ILogEnhancer), typeof(ILogEnhancer), typeof(ExecuteLevelLogEnhancerDecorator))]
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

    private sealed partial class DIE_DefaultTransientScope
    {
        private readonly GeneratorExecutionContext DIE_Factory_GeneratorExecutionContext;
        internal DIE_DefaultTransientScope(GeneratorExecutionContext context)
        {
            DIE_Factory_GeneratorExecutionContext = context;
        }
        
        private IExecuteContainer DIE_Factory_IExecuteContainer(
            ContainerInfo containerInfo, 
            RequiredKeywordUtility requiredKeywordUtility,
            DisposeUtility disposeUtility,
            ReferenceGeneratorCounter referenceGeneratorCounter)
        {
#pragma warning disable CA2000 *** Manually added for disposal
            var container = ContainerLevelContainer.DIE_CreateContainer(
                DIE_Factory_GeneratorExecutionContext, 
                containerInfo,
                requiredKeywordUtility,
                disposeUtility,
                referenceGeneratorCounter);
#pragma warning restore CA2000
            DIE_AddForDisposal(container);
            return container.Create();
        }
        
        private partial void DIE_AddForDisposal(IDisposable disposable);
    }
}