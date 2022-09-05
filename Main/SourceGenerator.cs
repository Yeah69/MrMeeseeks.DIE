using MrMeeseeks.DIE.CodeBuilding;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.ResolutionBuilding;
using MrMeeseeks.DIE.ResolutionBuilding.Function;
using MrMeeseeks.DIE.Validation.Attributes;
using MrMeeseeks.DIE.Validation.Range;
using MrMeeseeks.DIE.Validation.Range.UserDefined;

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
        var _ = WellKnownTypes.TryCreate(context.Compilation, out var wellKnownTypes);
        var __ = WellKnownTypesAggregation.TryCreate(context.Compilation, out var wellKnownTypesAggregation);
        var ___ = WellKnownTypesChoice.TryCreate(context.Compilation, out var wellKnownTypesChoice);
        var ____ = WellKnownTypesMiscellaneous.TryCreate(context.Compilation, out var wellKnownTypesMiscellaneous);
        var errorDescriptionInsteadOfBuildFailure = context.Compilation.Assembly.GetAttributes()
            .Any(ad => wellKnownTypesMiscellaneous.ErrorDescriptionInsteadOfBuildFailureAttribute.Equals(ad.AttributeClass, SymbolEqualityComparer.Default));
        var diagLogger = new DiagLogger(errorDescriptionInsteadOfBuildFailure, context);
        var validateUserDefinedAddForDisposalSync = new ValidateUserDefinedAddForDisposalSync(wellKnownTypes);
        var validateUserDefinedAddForDisposalAsync = new ValidateUserDefinedAddForDisposalAsync(wellKnownTypes);
        var validateUserDefinedConstructorParametersInjectionMethod = new ValidateUserDefinedConstructorParametersInjectionMethod(wellKnownTypesMiscellaneous);
        var validateUserDefinedPropertiesInjectionMethod = new ValidateUserDefinedPropertiesInjectionMethod(wellKnownTypesMiscellaneous);
        var validateUserDefinedInitializerParametersInjectionMethod = new ValidateUserDefinedInitializerParametersInjectionMethod(wellKnownTypesMiscellaneous);
        var validateUserDefinedFactoryField = new ValidateUserDefinedFactoryField();
        var validateUserDefinedFactoryMethod = new ValidateUserDefinedFactoryMethod();
        var validateAttributes = new ValidateAttributes();
        var validateTransientScope = new ValidateTransientScope(
            validateUserDefinedAddForDisposalSync, 
            validateUserDefinedAddForDisposalAsync, 
            validateUserDefinedConstructorParametersInjectionMethod, 
            validateUserDefinedPropertiesInjectionMethod,
            validateUserDefinedInitializerParametersInjectionMethod,
            validateUserDefinedFactoryMethod,
            validateUserDefinedFactoryField,
            wellKnownTypes, 
            wellKnownTypesAggregation,
            wellKnownTypesMiscellaneous);
        var validateScope = new ValidateScope(
            validateUserDefinedAddForDisposalSync,
            validateUserDefinedAddForDisposalAsync, 
            validateUserDefinedConstructorParametersInjectionMethod, 
            validateUserDefinedPropertiesInjectionMethod,
            validateUserDefinedInitializerParametersInjectionMethod,
            validateUserDefinedFactoryMethod,
            validateUserDefinedFactoryField,
            wellKnownTypes,
            wellKnownTypesAggregation,
            wellKnownTypesMiscellaneous);
        var validateContainer = new ValidateContainer(
            validateTransientScope, 
            validateScope, 
            validateUserDefinedAddForDisposalSync,
            validateUserDefinedAddForDisposalAsync, 
            validateUserDefinedConstructorParametersInjectionMethod,
            validateUserDefinedPropertiesInjectionMethod,
            validateUserDefinedInitializerParametersInjectionMethod,
            validateUserDefinedFactoryMethod,
            validateUserDefinedFactoryField,
            wellKnownTypes,
            wellKnownTypesMiscellaneous);
        var assemblyTypesFromAttributes = new TypesFromAttributes(
            context.Compilation.Assembly.GetAttributes(), 
            null,
            null,
            validateAttributes,
            wellKnownTypesAggregation,
            wellKnownTypesChoice,
            wellKnownTypesMiscellaneous,
            wellKnownTypes);
        
        foreach (var diagnostic in assemblyTypesFromAttributes
                     .Warnings
                     .Concat(assemblyTypesFromAttributes.Errors))
            diagLogger.Log(diagnostic);

        if (assemblyTypesFromAttributes.Errors.Any())
            return;
        
        var containerGenerator = new ContainerGenerator(context, diagLogger, ContainerCodeBuilderFactory, TransientScopeCodeBuilderFactory, ScopeCodeBuilderFactory);
        var referenceGeneratorFactory = new ReferenceGeneratorFactory(ReferenceGeneratorFactory);
        var containerDieExceptionGenerator = new ContainerDieExceptionGenerator(context, wellKnownTypesMiscellaneous);
        var implementationTypeSetCache = new ImplementationTypeSetCache(context, wellKnownTypes);
        new ExecuteImpl(
            errorDescriptionInsteadOfBuildFailure,
            context,
            wellKnownTypesMiscellaneous,
            containerGenerator, 
            containerDieExceptionGenerator,
            validateContainer,
            ResolutionTreeFactory,
            ContainerInfoFactory, 
            diagLogger).Execute();
            
        IContainerResolutionBuilder ResolutionTreeFactory(IContainerInfo ci)
        {
            var containerTypesFromAttributes = new TypesFromAttributes(
                ci.ContainerType.GetAttributes(), 
                ci.ContainerType,
                ci.ContainerType,
                validateAttributes,
                wellKnownTypesAggregation,
                wellKnownTypesChoice,
                wellKnownTypesMiscellaneous,
                wellKnownTypes);
        
            foreach (var diagnostic in containerTypesFromAttributes.Warnings)
                diagLogger.Log(diagnostic);

            if (containerTypesFromAttributes.Errors.Any())
                throw new ValidationDieException(containerTypesFromAttributes.Errors.ToImmutableArray());
            
            var containerTypesFromAttributesList = ImmutableList.Create(
                (ITypesFromAttributes) assemblyTypesFromAttributes,
                containerTypesFromAttributes);

            var functionCycleTracker = new FunctionCycleTracker();

            return new ContainerResolutionBuilder(
                ci,
                
                new TransientScopeInterfaceResolutionBuilder(referenceGeneratorFactory, wellKnownTypes, functionCycleTracker, FunctionResolutionSynchronicityDecisionMakerFactory),
                referenceGeneratorFactory,
                new CheckTypeProperties(new CurrentlyConsideredTypes(containerTypesFromAttributesList, implementationTypeSetCache), wellKnownTypes),
                wellKnownTypes,
                ScopeManagerFactory,
                RangedFunctionGroupResolutionBuilderFactory,
                FunctionResolutionSynchronicityDecisionMakerFactory,
                CreateFunctionResolutionBuilderFactory,
                new UserDefinedElements(ci.ContainerType, ci.ContainerType, wellKnownTypes, wellKnownTypesMiscellaneous),
                functionCycleTracker);

            IScopeManager ScopeManagerFactory(
                IContainerResolutionBuilder containerResolutionBuilder,
                ITransientScopeInterfaceResolutionBuilder transientScopeInterfaceResolutionBuilder) => new ScopeManager(
                ci,
                containerResolutionBuilder,
                transientScopeInterfaceResolutionBuilder,
                containerTypesFromAttributesList,
                TransientScopeResolutionBuilderFactory,
                ScopeResolutionBuilderFactory,
                (rangeType, ad) =>
                {
                    var scopeTypesFromAttributes = new ScopeTypesFromAttributes(
                        ad,
                        rangeType,
                        ci.ContainerType,
                        validateAttributes,
                        wellKnownTypesAggregation,
                        wellKnownTypesChoice,
                        wellKnownTypesMiscellaneous,
                        wellKnownTypes);
                    
                    foreach (var diagnostic in scopeTypesFromAttributes.Warnings)
                        diagLogger.Log(diagnostic);

                    if (scopeTypesFromAttributes.Errors.Any())
                        throw new ValidationDieException(scopeTypesFromAttributes.Errors.ToImmutableArray());
                    
                    return scopeTypesFromAttributes;
                },
                tfa => new CheckTypeProperties(new CurrentlyConsideredTypes(tfa, implementationTypeSetCache), wellKnownTypes),
                st => new UserDefinedElements(st, ci.ContainerType, wellKnownTypes, wellKnownTypesMiscellaneous),
                new EmptyUserDefinedElements(),
                wellKnownTypesMiscellaneous);

            ITransientScopeResolutionBuilder TransientScopeResolutionBuilderFactory(
                string name,
                IContainerResolutionBuilder containerBuilder, 
                ITransientScopeInterfaceResolutionBuilder transientScopeInterfaceResolutionBuilder, 
                IScopeManager scopeManager,
                IUserDefinedElements userProvidedScopeElements,
                ICheckTypeProperties checkTypeProperties,
                IErrorContext errorContext) => new TransientScopeResolutionBuilder(
                name,
                containerBuilder,
                transientScopeInterfaceResolutionBuilder,
                scopeManager,
                userProvidedScopeElements,
                checkTypeProperties,
                errorContext,
            
                wellKnownTypes, 
                referenceGeneratorFactory,
                ScopeRootCreateFunctionResolutionBuilderFactory,
                RangedFunctionGroupResolutionBuilderFactory,
                FunctionResolutionSynchronicityDecisionMakerFactory,
                CreateFunctionResolutionBuilderFactory);
            IScopeResolutionBuilder ScopeResolutionBuilderFactory(
                string name,
                IContainerResolutionBuilder containerBuilder, 
                ITransientScopeInterfaceResolutionBuilder transientScopeInterfaceResolutionBuilder, 
                IScopeManager scopeManager,
                IUserDefinedElements userProvidedScopeElements,
                ICheckTypeProperties checkTypeProperties,
                IErrorContext errorContext) => new ScopeResolutionBuilder(
                name,
                containerBuilder,
                transientScopeInterfaceResolutionBuilder,
                scopeManager,
                userProvidedScopeElements,
                checkTypeProperties,
                errorContext,
            
                wellKnownTypes, 
                referenceGeneratorFactory,
                ScopeRootCreateFunctionResolutionBuilderFactory,
                RangedFunctionGroupResolutionBuilderFactory,
                FunctionResolutionSynchronicityDecisionMakerFactory,
                CreateFunctionResolutionBuilderFactory);

            ICreateFunctionResolutionBuilder CreateFunctionResolutionBuilderFactory(
                IRangeResolutionBaseBuilder rangeResolutionBaseBuilder,
                INamedTypeSymbol returnType,
                ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> parameters,
                string accessModifier) => new CreateFunctionResolutionBuilder(
                rangeResolutionBaseBuilder,
                returnType,
                parameters,
                FunctionResolutionSynchronicityDecisionMakerFactory(),
                accessModifier,

                wellKnownTypes,
                referenceGeneratorFactory,
                functionCycleTracker);

            IScopeRootCreateFunctionResolutionBuilder ScopeRootCreateFunctionResolutionBuilderFactory(
                IRangeResolutionBaseBuilder rangeResolutionBaseBuilder,
                SwitchImplementationParameter parameter) => new ScopeRootCreateFunctionResolutionBuilder(
                rangeResolutionBaseBuilder,
                parameter,
                FunctionResolutionSynchronicityDecisionMakerFactory(),

                wellKnownTypes,
                referenceGeneratorFactory,
                functionCycleTracker);

            IRangedFunctionResolutionBuilder RangedFunctionResolutionBuilderFactory(
                IRangeResolutionBaseBuilder rangeResolutionBaseBuilder,
                string reference,
                ForConstructorParameter forConstructorParameter,
                IFunctionResolutionSynchronicityDecisionMaker synchronicityDecisionMaker,
                object handleIdentity) => new RangedFunctionResolutionBuilder(
                rangeResolutionBaseBuilder,
                reference,
                forConstructorParameter,
                synchronicityDecisionMaker,
                handleIdentity,

                wellKnownTypes,
                referenceGeneratorFactory,
                functionCycleTracker);

            IRangedFunctionGroupResolutionBuilder RangedFunctionGroupResolutionBuilderFactory(
                string label,
                string? reference,
                INamedTypeSymbol implementationType,
                string decorationSuffix, 
                IRangeResolutionBaseBuilder rangeResolutionBaseBuilder,
                bool isTransientScopeInstance) => new RangedFunctionGroupResolutionBuilder(
                label,
                reference,
                implementationType,
                decorationSuffix,
                rangeResolutionBaseBuilder,
                isTransientScopeInstance,

                referenceGeneratorFactory,
                RangedFunctionResolutionBuilderFactory);

            IFunctionResolutionSynchronicityDecisionMaker FunctionResolutionSynchronicityDecisionMakerFactory() =>
                new FunctionResolutionSynchronicityDecisionMaker();
        }
        IContainerInfo ContainerInfoFactory(INamedTypeSymbol type) => new ContainerInfo(type, wellKnownTypesMiscellaneous);
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