using MrMeeseeks.DIE.CodeBuilding;
using MrMeeseeks.DIE.Configuration;
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
        var attributeTypesFromAttributes = new TypesFromAttributes(context.Compilation.Assembly.GetAttributes(), wellKnownTypes);
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
            
        IContainerResolutionBuilder ResolutionTreeFactory(IContainerInfo ci)
        {
            var containerTypesFromAttributesList = ImmutableList.Create(
                (ITypesFromAttributes) attributeTypesFromAttributes,
                new TypesFromAttributes(ci.ContainerType.GetAttributes(), wellKnownTypes));

            return new ContainerResolutionBuilder(
                ci,
                
                new TransientScopeInterfaceResolutionBuilder(referenceGeneratorFactory),
                referenceGeneratorFactory,
                new CheckTypeProperties(new CurrentlyConsideredTypes(containerTypesFromAttributesList, context)),
                wellKnownTypes,
                ScopeManagerFactory,
                FunctionResolutionBuilderFactory,
                new UserProvidedScopeElements(ci.ContainerType));

            IScopeManager ScopeManagerFactory(
                IContainerResolutionBuilder containerResolutionBuilder,
                ITransientScopeInterfaceResolutionBuilder transientScopeInterfaceResolutionBuilder) => new ScopeManager(
                ci,
                containerResolutionBuilder,
                transientScopeInterfaceResolutionBuilder,
                containerTypesFromAttributesList,
                TransientScopeResolutionBuilderFactory,
                ScopeResolutionBuilderFactory,
                ad => new ScopeTypesFromAttributes(ad, wellKnownTypes),
                tfa => new CheckTypeProperties(new CurrentlyConsideredTypes(tfa, context)),
                st => new UserProvidedScopeElements(st),
                new EmptyUserProvidedScopeElements(),
                wellKnownTypes);

            ITransientScopeResolutionBuilder TransientScopeResolutionBuilderFactory(
                string name,
                IContainerResolutionBuilder containerBuilder, 
                ITransientScopeInterfaceResolutionBuilder transientScopeInterfaceResolutionBuilder, 
                IScopeManager scopeManager,
                IUserProvidedScopeElements userProvidedScopeElements,
                ICheckTypeProperties checkTypeProperties) => new TransientScopeResolutionBuilder(
                name,
                containerBuilder,
                transientScopeInterfaceResolutionBuilder,
                scopeManager,
                userProvidedScopeElements,
                checkTypeProperties,
            
                wellKnownTypes, 
                referenceGeneratorFactory,
                FunctionResolutionBuilderFactory);
            IScopeResolutionBuilder ScopeResolutionBuilderFactory(
                string name,
                IContainerResolutionBuilder containerBuilder, 
                ITransientScopeInterfaceResolutionBuilder transientScopeInterfaceResolutionBuilder, 
                IScopeManager scopeManager,
                IUserProvidedScopeElements userProvidedScopeElements,
                ICheckTypeProperties checkTypeProperties) => new ScopeResolutionBuilder(
                name,
                containerBuilder,
                transientScopeInterfaceResolutionBuilder,
                scopeManager,
                userProvidedScopeElements,
                checkTypeProperties,
            
                wellKnownTypes, 
                referenceGeneratorFactory,
                FunctionResolutionBuilderFactory);

            IFunctionResolutionBuilder FunctionResolutionBuilderFactory(
                IRangeResolutionBaseBuilder rangeResolutionBaseBuilder) => new FunctionResolutionBuilder(
                rangeResolutionBaseBuilder,

                wellKnownTypes,
                referenceGeneratorFactory);
        }
        IContainerInfo ContainerInfoFactory(INamedTypeSymbol type) => new ContainerInfo(type, wellKnownTypes);
        IReferenceGenerator ReferenceGeneratorFactory(int j) => new ReferenceGenerator(j);

        IContainerCodeBuilder ContainerCodeBuilderFactory(
            IContainerInfo containerInfo,
            ContainerResolution containerResolution,
            IReadOnlyList<ITransientScopeCodeBuilder> transientScopeCodeBuilders,
            IReadOnlyList<IScopeCodeBuilder> scopeCodeBuilders) => new ContainerCodeBuilder(
            containerInfo,
            containerResolution,
            transientScopeCodeBuilders,
            scopeCodeBuilders,
            wellKnownTypes);

        ITransientScopeCodeBuilder TransientScopeCodeBuilderFactory(
            IContainerInfo containerInfo,
            TransientScopeResolution transientScopeResolution,
            ContainerResolution containerResolution) => new TransientScopeCodeBuilder(
            containerInfo,
            transientScopeResolution,
            containerResolution,
            wellKnownTypes);

        IScopeCodeBuilder ScopeCodeBuilderFactory(
            IContainerInfo containerInfo,
            ScopeResolution scopeResolution,
            TransientScopeInterfaceResolution transientScopeInterfaceResolution,
            ContainerResolution containerResolution) => new ScopeCodeBuilder(
            containerInfo,
            scopeResolution,
            transientScopeInterfaceResolution,
            containerResolution,
            wellKnownTypes);
    }
}