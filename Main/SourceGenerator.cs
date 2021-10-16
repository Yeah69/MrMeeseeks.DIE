using Microsoft.CodeAnalysis;

namespace MrMeeseeks.DIE
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            new InitializeImpl(context, SyntaxReceiverFactory).Initialize();
            
            ISyntaxReceiver SyntaxReceiverFactory() => new SyntaxReceiver();
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var diagLogger = new DiagLogger(context);
            var getAllImplementations = new GetAllImplementations(context);
            var getAssemblyAttributes = new GetAssemblyAttributes(context);
            var _ = WellKnownTypes.TryCreate(context.Compilation, out var wellKnownTypes);
            var typeToImplementationMapper = new TypeToImplementationsMapper(wellKnownTypes, getAllImplementations, getAssemblyAttributes);
            var containerGenerator = new ContainerGenerator(context, diagLogger);
            var referenceGeneratorFactory = new ReferenceGeneratorFactory(ReferenceGeneratorFactory);
            var resolutionTreeFactory = new ResolutionTreeFactory(typeToImplementationMapper, referenceGeneratorFactory, wellKnownTypes);
            var containerErrorGenerator = new ContainerErrorGenerator(context);
            var resolutionTreeCreationErrorHarvester = new ResolutionTreeCreationErrorHarvester();
            new ExecuteImpl(
                context,
                wellKnownTypes,
                containerGenerator, 
                containerErrorGenerator,
                resolutionTreeFactory,
                resolutionTreeCreationErrorHarvester,
                ContainerInfoFactory, 
                diagLogger).Execute();
            
            IContainerInfo ContainerInfoFactory(INamedTypeSymbol type) => new ContainerInfo(type, wellKnownTypes);
            IReferenceGenerator ReferenceGeneratorFactory(int j) => new ReferenceGenerator(j);
        }
    }
}
