using Microsoft.CodeAnalysis;
using StrongInject;

namespace MrMeeseeks.DIE
{
    public interface IExecuteContainer
    {
        
    }

    [Register(typeof(ExecuteImpl), typeof(IExecute))]
    [Register(typeof(ContainerGenerator), typeof(IContainerGenerator))]
    [Register(typeof(ContainerErrorGenerator), typeof(IContainerErrorGenerator))]
    [Register(typeof(ResolutionTreeFactory), typeof(IResolutionTreeFactory))]
    [Register(typeof(ResolutionTreeCreationErrorHarvester), typeof(IResolutionTreeCreationErrorHarvester))]
    [Register(typeof(ContainerInfo), typeof(IContainerInfo))]
    [Register(typeof(DiagLogger), typeof(IDiagLogger))]
    [Register(typeof(TypeToImplementationsMapper), typeof(ITypeToImplementationsMapper))]
    [Register(typeof(ReferenceGeneratorFactory), typeof(IReferenceGeneratorFactory))]
    [Register(typeof(GetAllImplementations), typeof(IGetAllImplementations))]
    [Register(typeof(GetAssemblyAttributes), typeof(IGetAssemblyAttributes))]
    [Register(typeof(ReferenceGenerator), typeof(IReferenceGenerator))]
    internal partial class ExecuteContainer : IExecuteContainer, StrongInject.IContainer<IExecute>
    {
        [Instance] public GeneratorExecutionContext Context { get; }
        [Instance] public WellKnownTypes WellKnownTypes { get; }

        public ExecuteContainer(GeneratorExecutionContext context)
        {
            Context = context;
            WellKnownTypes.TryCreate(context.Compilation, out var wellKnownTypes);
            WellKnownTypes = wellKnownTypes;
        }
    }
}