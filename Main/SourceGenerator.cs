﻿using MrMeeseeks.DIE.CodeBuilding;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.ResolutionBuilding;
using MrMeeseeks.DIE.ResolutionBuilding.Function;

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
        var containerDieExceptionGenerator = new ContainerDieExceptionGenerator(context, wellKnownTypes);
        var resolutionTreeCreationErrorHarvester = new ResolutionTreeCreationErrorHarvester();
        var implementationTypeSetCache = new ImplementationTypeSetCache(context, wellKnownTypes);
        new ExecuteImpl(
            context,
            wellKnownTypes,
            containerGenerator, 
            containerErrorGenerator,
            containerDieExceptionGenerator,
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
                
                new TransientScopeInterfaceResolutionBuilder(referenceGeneratorFactory, wellKnownTypes, RangedFunctionGroupResolutionBuilderFactory, FunctionResolutionSynchronicityDecisionMakerFactory),
                referenceGeneratorFactory,
                new CheckTypeProperties(new CurrentlyConsideredTypes(containerTypesFromAttributesList, implementationTypeSetCache), wellKnownTypes),
                wellKnownTypes,
                ScopeManagerFactory,
                ContainerCreateFunctionResolutionBuilderFactory,
                RangedFunctionGroupResolutionBuilderFactory,
                FunctionResolutionSynchronicityDecisionMakerFactory,
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
                tfa => new CheckTypeProperties(new CurrentlyConsideredTypes(tfa, implementationTypeSetCache), wellKnownTypes),
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
                ScopeRootCreateFunctionResolutionBuilderFactory,
                RangedFunctionGroupResolutionBuilderFactory,
                FunctionResolutionSynchronicityDecisionMakerFactory);
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
                ScopeRootCreateFunctionResolutionBuilderFactory,
                RangedFunctionGroupResolutionBuilderFactory,
                FunctionResolutionSynchronicityDecisionMakerFactory);

            ILocalFunctionResolutionBuilder LocalFunctionResolutionBuilderFactory(
                IRangeResolutionBaseBuilder rangeResolutionBaseBuilder,
                INamedTypeSymbol returnType,
                IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> parameters) => new LocalFunctionResolutionBuilder(
                rangeResolutionBaseBuilder,
                returnType,
                parameters,
                FunctionResolutionSynchronicityDecisionMakerFactory(),

                wellKnownTypes,
                referenceGeneratorFactory,
                LocalFunctionResolutionBuilderFactory);

            IContainerCreateFunctionResolutionBuilder ContainerCreateFunctionResolutionBuilderFactory(
                IRangeResolutionBaseBuilder rangeResolutionBaseBuilder,
                INamedTypeSymbol returnType) => new ContainerCreateFunctionResolutionBuilder(
                rangeResolutionBaseBuilder,
                returnType,
                FunctionResolutionSynchronicityDecisionMakerFactory(),

                wellKnownTypes,
                referenceGeneratorFactory,
                LocalFunctionResolutionBuilderFactory);

            IScopeRootCreateFunctionResolutionBuilder ScopeRootCreateFunctionResolutionBuilderFactory(
                IRangeResolutionBaseBuilder rangeResolutionBaseBuilder,
                SwitchImplementationParameter parameter) => new ScopeRootCreateFunctionResolutionBuilder(
                rangeResolutionBaseBuilder,
                parameter,
                FunctionResolutionSynchronicityDecisionMakerFactory(),

                wellKnownTypes,
                referenceGeneratorFactory,
                LocalFunctionResolutionBuilderFactory);

            IRangedFunctionResolutionBuilder RangedFunctionResolutionBuilderFactory(
                IRangeResolutionBaseBuilder rangeResolutionBaseBuilder,
                string reference,
                ForConstructorParameter forConstructorParameter,
                IFunctionResolutionSynchronicityDecisionMaker synchronicityDecisionMaker) => new RangedFunctionResolutionBuilder(
                rangeResolutionBaseBuilder,
                reference,
                forConstructorParameter,
                synchronicityDecisionMaker,

                wellKnownTypes,
                referenceGeneratorFactory,
                LocalFunctionResolutionBuilderFactory);

            IRangedFunctionGroupResolutionBuilder RangedFunctionGroupResolutionBuilderFactory(
                string label,
                string? reference,
                INamedTypeSymbol implementationType,
                string decorationSuffix, 
                IRangeResolutionBaseBuilder rangeResolutionBaseBuilder) => new RangedFunctionGroupResolutionBuilder(
                label,
                reference,
                implementationType,
                decorationSuffix,
                rangeResolutionBaseBuilder,

                referenceGeneratorFactory,
                RangedFunctionResolutionBuilderFactory);

            IFunctionResolutionSynchronicityDecisionMaker FunctionResolutionSynchronicityDecisionMakerFactory() =>
                new FunctionResolutionSynchronicityDecisionMaker();
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