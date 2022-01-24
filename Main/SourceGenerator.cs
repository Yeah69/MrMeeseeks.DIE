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

            var defaultTransientScopeType = ci.ContainerType.GetTypeMembers(Constants.DefaultTransientScopeName).FirstOrDefault();
            var defaultTransientScopeTypesFromAttributes = new ScopeTypesFromAttributes(defaultTransientScopeType?.GetAttributes() ?? ImmutableArray<AttributeData>.Empty, wellKnownTypes);

            var defaultScopeType = ci.ContainerType.GetTypeMembers(Constants.DefaultScopeName).FirstOrDefault();
            var defaultScopeTypesFromAttributes = new ScopeTypesFromAttributes(defaultScopeType?.GetAttributes() ?? ImmutableArray<AttributeData>.Empty, wellKnownTypes);

            return new ContainerResolutionBuilder(
                ci,
                
                new TransientScopeInterfaceResolutionBuilder(referenceGeneratorFactory),
                referenceGeneratorFactory,
                new CheckTypeProperties(new CurrentlyConsideredTypes(containerTypesFromAttributesList, context)),
                wellKnownTypes,
                TransientScopeResolutionBuilderFactory,
                ScopeResolutionBuilderFactory,
                new UserProvidedScopeElements(ci.ContainerType));

            ITransientScopeResolutionBuilder TransientScopeResolutionBuilderFactory(IContainerResolutionBuilder containerBuilder, ITransientScopeInterfaceResolutionBuilder transientScopeInterfaceResolutionBuilder) => new TransientScopeResolutionBuilder(
                containerBuilder,
                transientScopeInterfaceResolutionBuilder,
            
                wellKnownTypes, 
                referenceGeneratorFactory,
                new CheckTypeProperties(
                    new CurrentlyConsideredTypes(
                        containerTypesFromAttributesList.Add(defaultTransientScopeTypesFromAttributes), 
                        context)),
                defaultTransientScopeType is {} 
                    ? new UserProvidedScopeElements(defaultTransientScopeType) 
                    : new EmptyUserProvidedScopeElements());
            IScopeResolutionBuilder ScopeResolutionBuilderFactory(IContainerResolutionBuilder containerBuilder, ITransientScopeResolutionBuilder transientScopeResolutionBuilder, ITransientScopeInterfaceResolutionBuilder transientScopeInterfaceResolutionBuilder) => new ScopeResolutionBuilder(
                containerBuilder,
                transientScopeResolutionBuilder,
                transientScopeInterfaceResolutionBuilder,
            
                wellKnownTypes, 
                referenceGeneratorFactory,
                new CheckTypeProperties(
                    new CurrentlyConsideredTypes(
                        containerTypesFromAttributesList.Add(defaultScopeTypesFromAttributes), 
                        context)),
                defaultScopeType is {} 
                    ? new UserProvidedScopeElements(defaultScopeType) 
                    : new EmptyUserProvidedScopeElements());
            
            
        }
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