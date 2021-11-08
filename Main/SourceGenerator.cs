namespace MrMeeseeks.DIE;

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
        var _ = WellKnownTypes.TryCreate(context.Compilation, out var wellKnownTypes);
        var getAssemblyAttributes = new GetAssemblyAttributes(context);
        var typesFromAttributes = new TypesFromAttributes(wellKnownTypes, getAssemblyAttributes);
        var getAllImplementations = new GetAllImplementations(context, typesFromAttributes);
        var typeToImplementationMapper = new TypeToImplementationsMapper(getAllImplementations);
        var containerGenerator = new ContainerGenerator(context, wellKnownTypes, diagLogger);
        var referenceGeneratorFactory = new ReferenceGeneratorFactory(ReferenceGeneratorFactory);
        var checkDisposalManagement = new CheckTypeProperties(getAllImplementations, typesFromAttributes);
        var resolutionTreeFactory = new ResolutionTreeFactory(typeToImplementationMapper, referenceGeneratorFactory, checkDisposalManagement, wellKnownTypes);
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