using MrMeeseeks.DIE.CodeBuilding;
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
        var checkTypeProperties = new CheckTypeProperties(typesFromTypeAggregatingAttributes, getAssemblyAttributes, wellKnownTypes, getSetOfTypesWithProperties);
        var typeToImplementationMapper = new TypeToImplementationsMapper(getAllImplementations, checkDecorators, checkTypeProperties);
        var containerGenerator = new ContainerGenerator(context, diagLogger, ContainerCodeBuilderFactory, TransientScopeCodeBuilderFactory, ScopeCodeBuilderFactory);
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
            
            new TransientScopeInterfaceResolutionBuilder(referenceGeneratorFactory),
            typeToImplementationMapper,
            referenceGeneratorFactory,
            checkTypeProperties,
            checkDecorators,
            wellKnownTypes,
            TransientScopeResolutionBuilderFactory,
            ScopeResolutionBuilderFactory,
            new UserProvidedScopeElements(ci.ContainerType));
        ITransientScopeResolutionBuilder TransientScopeResolutionBuilderFactory(IContainerResolutionBuilder containerBuilder, ITransientScopeInterfaceResolutionBuilder transientScopeInterfaceResolutionBuilder) => new TransientScopeResolutionBuilder(
            containerBuilder,
            transientScopeInterfaceResolutionBuilder,
            
            wellKnownTypes, 
            typeToImplementationMapper, 
            referenceGeneratorFactory,
            checkTypeProperties,
            checkDecorators,
            new EmptyUserProvidedScopeElements()); // todo Replace EmptyUserProvidedScopeElements with one for the scope specifically
        IScopeResolutionBuilder ScopeResolutionBuilderFactory(IContainerResolutionBuilder containerBuilder, ITransientScopeResolutionBuilder transientScopeResolutionBuilder, ITransientScopeInterfaceResolutionBuilder transientScopeInterfaceResolutionBuilder) => new ScopeResolutionBuilder(
            containerBuilder,
            transientScopeResolutionBuilder,
            transientScopeInterfaceResolutionBuilder,
            
            wellKnownTypes, 
            typeToImplementationMapper, 
            referenceGeneratorFactory,
            checkTypeProperties,
            checkDecorators,
            new EmptyUserProvidedScopeElements()); // todo Replace EmptyUserProvidedScopeElements with one for the scope specifically
        IContainerInfo ContainerInfoFactory(INamedTypeSymbol type) => new ContainerInfo(type, wellKnownTypes);
        IReferenceGenerator ReferenceGeneratorFactory(int j) => new ReferenceGenerator(j);

        IContainerCodeBuilder ContainerCodeBuilderFactory(
            IContainerInfo containerInfo,
            ContainerResolution containerResolution,
            ITransientScopeCodeBuilder transientScopeCodeBuilder,
            IScopeCodeBuilder scopeCodeBuilder) => new ContainerCodeBuilder(
            containerInfo,
            containerResolution,
            transientScopeCodeBuilder,
            scopeCodeBuilder,
            wellKnownTypes);

        ITransientScopeCodeBuilder TransientScopeCodeBuilderFactory(
            IContainerInfo containerInfo,
            TransientScopeResolution transientScopeResolution) => new TransientScopeCodeBuilder(
            containerInfo,
            transientScopeResolution,
            wellKnownTypes);

        IScopeCodeBuilder ScopeCodeBuilderFactory(
            IContainerInfo containerInfo,
            ScopeResolution scopeResolution,
            TransientScopeInterfaceResolution transientScopeInterfaceResolution) => new ScopeCodeBuilder(
            containerInfo,
            scopeResolution,
            transientScopeInterfaceResolution,
            wellKnownTypes);
    }
}