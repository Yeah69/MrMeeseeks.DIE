using MrMeeseeks.DIE.ResolutionBuilding;

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
        var typesFromTypeAggregatingAttributes = new TypesFromTypeAggregatingAttributes(wellKnownTypes, getAssemblyAttributes);
        var getAllImplementations = new GetAllImplementations(context, typesFromTypeAggregatingAttributes);
        var getSetOfTypesWithProperties = new GetSetOfTypesWithProperties(getAllImplementations);
        var checkDecorators = new CheckDecorators(wellKnownTypes, getAssemblyAttributes, typesFromTypeAggregatingAttributes, getSetOfTypesWithProperties);
        var checkTypeProperties = new CheckTypeProperties(typesFromTypeAggregatingAttributes, getSetOfTypesWithProperties);
        var typeToImplementationMapper = new TypeToImplementationsMapper(getAllImplementations, checkDecorators);
        var containerGenerator = new ContainerGenerator(context, wellKnownTypes, diagLogger);
        var referenceGeneratorFactory = new ReferenceGeneratorFactory(ReferenceGeneratorFactory);
        var containerErrorGenerator = new ContainerErrorGenerator(context);
        var resolutionTreeCreationErrorHarvester = new ResolutionTreeCreationErrorHarvester();
        new ExecuteImpl(
            context,
            wellKnownTypes,
            containerGenerator, 
            containerErrorGenerator,
            ResolutionTreeFactory,
            resolutionTreeCreationErrorHarvester,
            ContainerInfoFactory, 
            diagLogger).Execute();
            
        IContainerResolutionBuilder ResolutionTreeFactory(IContainerInfo ci) => new ContainerResolutionBuilder(
            ci,
            typeToImplementationMapper,
            referenceGeneratorFactory,
            checkTypeProperties,
            checkDecorators,
            wellKnownTypes,
            ScopeResolutionBuilderFactory);
        IScopeResolutionBuilder ScopeResolutionBuilderFactory(IContainerResolutionBuilder containerBuilder) => new ScopeResolutionBuilder(
            containerBuilder,
            wellKnownTypes, 
            typeToImplementationMapper, 
            referenceGeneratorFactory,
            checkTypeProperties,
            checkDecorators);
        IContainerInfo ContainerInfoFactory(INamedTypeSymbol type) => new ContainerInfo(type, wellKnownTypes);
        IReferenceGenerator ReferenceGeneratorFactory(int j) => new ReferenceGenerator(j);
    }
}