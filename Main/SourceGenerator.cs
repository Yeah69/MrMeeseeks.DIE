using MrMeeseeks.DIE.CodeBuilding;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.Delegates;
using MrMeeseeks.DIE.Nodes.Elements.Factories;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Elements.Tasks;
using MrMeeseeks.DIE.Nodes.Elements.Tuples;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Mappers;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.ResolutionBuilding;
using MrMeeseeks.DIE.ResolutionBuilding.Function;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.DIE.Validation.Attributes;
using MrMeeseeks.DIE.Validation.Range;
using MrMeeseeks.DIE.Validation.Range.UserDefined;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE;

[Generator]
public class SourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        new InitializeImpl(context, SyntaxReceiverFactory).Initialize();
            
        ISyntaxReceiver SyntaxReceiverFactory() => new SyntaxReceiver();
        //if (!Debugger.IsAttached)
        {
            //Debugger.Launch();
        }
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var wellKnownTypesMiscellaneous = WellKnownTypesMiscellaneous.Create(context.Compilation);
        var errorDescriptionInsteadOfBuildFailure = context.Compilation.Assembly.GetAttributes()
            .Any(ad => SymbolEqualityComparer.Default.Equals(wellKnownTypesMiscellaneous.ErrorDescriptionInsteadOfBuildFailureAttribute, ad.AttributeClass));

        WellKnownTypes wellKnownTypes = WellKnownTypes.Create(context.Compilation);
        var wellKnownTypesAggregation = WellKnownTypesAggregation.Create(context.Compilation);
        var wellKnownTypesChoice = WellKnownTypesChoice.Create(context.Compilation);
        DiagLogger diagLogger = new DiagLogger(errorDescriptionInsteadOfBuildFailure, context);
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
        
        var containerGenerator = new ContainerGenerator(context, ContainerCodeBuilderFactory, TransientScopeCodeBuilderFactory, ScopeCodeBuilderFactory);
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
            ContainerNodeFactory,
            CreateCodeGenerationVisitor,
            referenceGeneratorFactory,
            diagLogger).Execute();

        ICodeGenerationVisitor CreateCodeGenerationVisitor() => new CodeGenerationVisitor(wellKnownTypes);

        IContainerNode ContainerNodeFactory(
            IContainerInfo ci)
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
                (ITypesFromAttributes)assemblyTypesFromAttributes,
                containerTypesFromAttributes);

            return new ContainerNode(
                ci,
                containerTypesFromAttributesList,
                CreateUserDefinedElements,
                CreateCheckTypeProperties(containerTypesFromAttributesList),
                referenceGeneratorFactory.Create(),
                CreateFunctionNode,
                CreateRangedInstanceFunctionGroupNode,
                CreateEntryFunctionNode,
                CreateTransientScopeInterfaceNode,
                CreateScopeManager);
        }

        ITransientScopeInterfaceNode CreateTransientScopeInterfaceNode(
            IContainerNode container,
            IReferenceGenerator referenceGenerator) =>
            new TransientScopeInterfaceNode(container, referenceGenerator, CreateRangedInstanceInterfaceFunctionNode);

        IRangedInstanceInterfaceFunctionNode CreateRangedInstanceInterfaceFunctionNode(
            INamedTypeSymbol type, 
            IReadOnlyList<ITypeSymbol> parameters,
            IContainerNode parentContainer, 
            IReferenceGenerator referenceGenerator) =>
            new RangedInstanceInterfaceFunctionNode(
                type,
                parameters,
                parentContainer,
                referenceGenerator,
                CreatePlainFunctionCallNode,
                CreateScopeCallNode,
                CreateTransientScopeCallNode,
                CreateParameterNode,
                wellKnownTypes);

        Nodes.IScopeManager CreateScopeManager(
            IContainerInfo containerInfo,
            IContainerNode container,
            ITransientScopeInterfaceNode transientScopeInterface,
            ImmutableList<ITypesFromAttributes> typesFromAttributesList,
            IReferenceGenerator referenceGenerator) =>
            new Nodes.ScopeManager(
                containerInfo,
                container,
                transientScopeInterface,
                typesFromAttributesList,
                referenceGenerator,
                CreateScopeNode,
                CreateTransientScopeNode,
                (rangeType, ad) =>
                {
                    var scopeTypesFromAttributes = new ScopeTypesFromAttributes(
                        ad,
                        rangeType,
                        containerInfo.ContainerType,
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
                CreateCheckTypeProperties,
                CreateUserDefinedElements,
                new EmptyUserDefinedElements(),
                wellKnownTypesMiscellaneous);

        IScopeNode CreateScopeNode(
            string name,
            IContainerNode container,
            ITransientScopeInterfaceNode transientScopeInterface,
            Nodes.IScopeManager scopeManager,
            IUserDefinedElements userDefinedElements,
            ICheckTypeProperties checkTypeProperties,
            IReferenceGenerator referenceGenerator) =>
            new ScopeNode(
                name,
                container,
                transientScopeInterface,
                scopeManager,
                userDefinedElements,
                checkTypeProperties,
                referenceGenerator,
                CreateFunctionNode,
                CreateScopeFunctionNode,
                CreateRangedInstanceFunctionGroupNode);

        ITransientScopeNode CreateTransientScopeNode(
            string name,
            IContainerNode container,
            Nodes.IScopeManager scopeManager,
            IUserDefinedElements userDefinedElements,
            ICheckTypeProperties checkTypeProperties,
            IReferenceGenerator referenceGenerator) =>
            new TransientScopeNode(
                name,
                container,
                scopeManager,
                userDefinedElements,
                checkTypeProperties,
                referenceGenerator,
                CreateFunctionNode,
                CreateScopeFunctionNode,
                CreateRangedInstanceFunctionGroupNode);

        IPlainFunctionCallNode CreatePlainFunctionCallNode(
            string? ownerReference,
            IFunctionNode calledFunction,
            IReadOnlyList<(IParameterNode, IParameterNode)> parameters,
            IReferenceGenerator referenceGenerator) =>
            new PlainFunctionCallNode(
                ownerReference,
                calledFunction,
                parameters,
                referenceGenerator);

        ICreateFunctionNode CreateFunctionNode(
            ITypeSymbol typeSymbol,
            IReadOnlyList<ITypeSymbol> parameters,
            IRangeNode parentRange,
            IContainerNode parentContainer,
            IUserDefinedElements userDefinedElements,
            ICheckTypeProperties checkTypeProperties,
            IReferenceGenerator referenceGenerator) =>
            new CreateFunctionNode(
                typeSymbol, 
                parameters,
                parentRange,
                parentContainer,
                userDefinedElements,
                checkTypeProperties,
                referenceGenerator,
                CreateElementNodeMapper,
                CreatePlainFunctionCallNode, 
                CreateScopeCallNode,
                CreateTransientScopeCallNode,
                CreateParameterNode,
                wellKnownTypes);

        ICreateScopeFunctionNode CreateScopeFunctionNode(
            INamedTypeSymbol typeSymbol,
            IReadOnlyList<ITypeSymbol> parameters,
            IRangeNode parentRange,
            IContainerNode parentContainer,
            IUserDefinedElements userDefinedElements,
            ICheckTypeProperties checkTypeProperties,
            IReferenceGenerator referenceGenerator) =>
            new CreateScopeFunctionNode(
                typeSymbol, 
                parameters,
                parentRange,
                parentContainer,
                userDefinedElements,
                checkTypeProperties,
                referenceGenerator,
                CreateElementNodeMapper,
                CreatePlainFunctionCallNode, 
                CreateScopeCallNode,
                CreateTransientScopeCallNode,
                CreateParameterNode,
                wellKnownTypes);

        ILocalFunctionNode CreateLocalFunctionNode(
            ITypeSymbol typeSymbol,
            IReadOnlyList<ITypeSymbol> parameters,
            ImmutableSortedDictionary<TypeKey, (ITypeSymbol, IParameterNode)> closureParameters, 
            IRangeNode parentRange,
            IContainerNode parentContainer,
            IUserDefinedElements userDefinedElements,
            ICheckTypeProperties checkTypeProperties,
            IElementNodeMapperBase mapper,
            IReferenceGenerator referenceGenerator) =>
            new LocalFunctionNode(
                typeSymbol, 
                parameters,
                closureParameters,
                parentRange,
                parentContainer,
                userDefinedElements,
                checkTypeProperties,
                mapper,
                referenceGenerator,
                CreateParameterNode,
                CreatePlainFunctionCallNode,
                CreateScopeCallNode,
                CreateTransientScopeCallNode,
                wellKnownTypes);
        
        IRangedInstanceFunctionNode CreateRangedInstanceFunctionNode(
            ScopeLevel level,
            INamedTypeSymbol type,
            IReadOnlyList<ITypeSymbol> parameters,
            IRangeNode parentRange,
            IContainerNode parentContainer,
            IUserDefinedElements userDefinedElements,
            ICheckTypeProperties checkTypeProperties,
            IReferenceGenerator referenceGenerator) =>
            new RangedInstanceFunctionNode(
                level,
                type,
                parameters,
                parentRange,
                parentContainer,
                userDefinedElements,
                checkTypeProperties,
                referenceGenerator,
                CreateElementNodeMapper, 
                CreatePlainFunctionCallNode,
                CreateScopeCallNode,
                CreateTransientScopeCallNode,
                CreateParameterNode,
                wellKnownTypes);

        
        IRangedInstanceFunctionGroupNode CreateRangedInstanceFunctionGroupNode(
            ScopeLevel level,
            INamedTypeSymbol type,
            IRangeNode parentRange,
            IContainerNode parentContainer,
            IUserDefinedElements userDefinedElements,
            ICheckTypeProperties checkTypeProperties,
            IReferenceGenerator referenceGenerator) =>
            new RangedInstanceFunctionGroupNode(
                level,
                type,
                parentContainer,
                parentRange,
                checkTypeProperties,
                userDefinedElements,
                referenceGenerator,
                CreateRangedInstanceFunctionNode);
        
        IEntryFunctionNode CreateEntryFunctionNode(
            ITypeSymbol typeSymbol,
            string prefix,
            IRangeNode parentRange,
            IContainerNode parentContainer,
            IUserDefinedElements userDefinedElements,
            ICheckTypeProperties checkTypeProperties,
            IReferenceGenerator referenceGenerator) =>
            new EntryFunctionNode(
                typeSymbol,
                prefix,
                parentRange,
                parentContainer,
                userDefinedElements,
                checkTypeProperties,
                referenceGenerator,
                wellKnownTypes, 
                CreateElementNodeMapper, 
                CreateNonWrapToCreateElementNodeMapper,
                CreatePlainFunctionCallNode,
                CreateScopeCallNode,
                CreateTransientScopeCallNode,
                CreateParameterNode);

        IScopeCallNode CreateScopeCallNode(
            string containerParameter, 
            string transientScopeInterfaceParameter,
            IScopeNode scope,
            IFunctionNode calledFunction,
            IReadOnlyList<(IParameterNode, IParameterNode)> parameters,
            IReferenceGenerator referenceGenerator) =>
            new ScopeCallNode(
                containerParameter,
                transientScopeInterfaceParameter,
                scope,
                calledFunction,
                parameters,
                referenceGenerator);

        ITransientScopeCallNode CreateTransientScopeCallNode(
            string containerParameter, 
            ITransientScopeNode transientScope,
            IFunctionNode calledFunction,
            IReadOnlyList<(IParameterNode, IParameterNode)> parameters,
            IReferenceGenerator referenceGenerator) =>
            new TransientScopeCallNode(
                containerParameter,
                transientScope,
                calledFunction,
                parameters,
                referenceGenerator);

        IOverridingElementNodeMapper CreateOverridingElementNodeMapper(
            IElementNodeMapperBase parentElementNodeMapper,
            ElementNodeMapperBase.PassedDependencies passedDependencies,
            (TypeKey, IElementNode) @override) =>
            new OverridingElementNodeMapper(
                parentElementNodeMapper,
                passedDependencies,
                @override,
                diagLogger,
                wellKnownTypes,
                CreateFactoryFieldNode,
                CreateFactoryPropertyNode,
                CreateFactoryFunctionNode,
                CreateValueTaskNode,
                CreateTaskNode,
                CreateValueTupleNode,
                CreateValueTupleSyntaxNode,
                CreateTupleNode,
                CreateLazyNode,
                CreateFuncNode,
                CreateCollectionNode,
                CreateAbstractionNode,
                CreateImplementationNode,
                CreateOutParameterNode,
                CreateErrorNode,
                CreateNullNode,
                CreateLocalFunctionNode,
                CreateOverridingElementNodeMapper,
                CreateOverridingElementNodeMapperComposite,
                CreateNonWrapToCreateElementNodeMapper);

        INonWrapToCreateElementNodeMapper CreateNonWrapToCreateElementNodeMapper(
            IElementNodeMapperBase parentElementNodeMapper,
            ElementNodeMapperBase.PassedDependencies passedDependencies) =>
            new NonWrapToCreateElementNodeMapper(
                parentElementNodeMapper,
                passedDependencies,
                diagLogger,
                wellKnownTypes,
                CreateFactoryFieldNode,
                CreateFactoryPropertyNode,
                CreateFactoryFunctionNode,
                CreateValueTaskNode,
                CreateTaskNode,
                CreateValueTupleNode,
                CreateValueTupleSyntaxNode,
                CreateTupleNode,
                CreateLazyNode,
                CreateFuncNode,
                CreateCollectionNode,
                CreateAbstractionNode,
                CreateImplementationNode,
                CreateOutParameterNode,
                CreateErrorNode,
                CreateNullNode,
                CreateLocalFunctionNode,
                CreateOverridingElementNodeMapper,
                CreateOverridingElementNodeMapperComposite,
                CreateNonWrapToCreateElementNodeMapper);

        IOverridingElementNodeMapperComposite CreateOverridingElementNodeMapperComposite(
            IElementNodeMapperBase parentElementNodeMapper,
            ElementNodeMapperBase.PassedDependencies passedDependencies,
            (TypeKey, IReadOnlyList<IElementNode>) @override) =>
            new OverridingElementNodeMapperComposite(
                parentElementNodeMapper,
                passedDependencies,
                @override,
                diagLogger,
                wellKnownTypes,
                CreateFactoryFieldNode,
                CreateFactoryPropertyNode,
                CreateFactoryFunctionNode,
                CreateValueTaskNode,
                CreateTaskNode,
                CreateValueTupleNode,
                CreateValueTupleSyntaxNode,
                CreateTupleNode,
                CreateLazyNode,
                CreateFuncNode,
                CreateCollectionNode,
                CreateAbstractionNode,
                CreateImplementationNode,
                CreateOutParameterNode,
                CreateErrorNode,
                CreateNullNode,
                CreateLocalFunctionNode,
                CreateOverridingElementNodeMapper,
                CreateOverridingElementNodeMapperComposite,
                CreateNonWrapToCreateElementNodeMapper);

        IElementNodeMapperBase CreateElementNodeMapper(
            ISingleFunctionNode parentFunction,
            IRangeNode parentRange,
            IContainerNode parentContainer,
            IUserDefinedElements userDefinedElements,
            ICheckTypeProperties checkTypeProperties,
            IReferenceGenerator referenceGenerator) =>
            new ElementNodeMapper(
                parentFunction,
                parentRange,
                parentContainer,
                userDefinedElements,
                checkTypeProperties,
                referenceGenerator,
                diagLogger,
                wellKnownTypes,
                CreateFactoryFieldNode,
                CreateFactoryPropertyNode,
                CreateFactoryFunctionNode,
                CreateValueTaskNode,
                CreateTaskNode,
                CreateValueTupleNode,
                CreateValueTupleSyntaxNode,
                CreateTupleNode,
                CreateLazyNode,
                CreateFuncNode,
                CreateCollectionNode,
                CreateAbstractionNode,
                CreateImplementationNode,
                CreateOutParameterNode,
                CreateErrorNode,
                CreateNullNode,
                CreateLocalFunctionNode,
                CreateOverridingElementNodeMapper,
                CreateOverridingElementNodeMapperComposite,
                CreateNonWrapToCreateElementNodeMapper);

        INullNode CreateNullNode(ITypeSymbol nullableType, IReferenceGenerator referenceGenerator) => new NullNode(nullableType, referenceGenerator);

        IErrorNode CreateErrorNode(string message) => new ErrorNode(message);

        IImplementationNode CreateImplementationNode(
            INamedTypeSymbol implementationType, 
            IMethodSymbol constructor,
            IFunctionNode parentFunction,
            IElementNodeMapperBase typeToElementNodeMapper,
            ICheckTypeProperties checkTypeProperties,
            IUserDefinedElements userDefinedElements, 
            IReferenceGenerator referenceGenerator) =>
            new ImplementationNode(
                implementationType, 
                constructor, 
                parentFunction,
                typeToElementNodeMapper,
                checkTypeProperties,
                userDefinedElements, 
                referenceGenerator,
                wellKnownTypes);

        IOutParameterNode CreateOutParameterNode(ITypeSymbol type, IReferenceGenerator referenceGenerator) =>
            new OutParameterNode(type, referenceGenerator);

        IAbstractionNode CreateAbstractionNode(INamedTypeSymbol abstractionType, IElementNode elementNode, IReferenceGenerator referenceGenerator) =>
            new AbstractionNode(abstractionType, elementNode, referenceGenerator);

        ICollectionNode CreateCollectionNode(ITypeSymbol collectionType, IReadOnlyList<IElementNode> itemNodes, IReferenceGenerator referenceGenerator) =>
            new CollectionNode(collectionType, itemNodes, referenceGenerator);

        IFuncNode CreateFuncNode(INamedTypeSymbol funcType, ILocalFunctionNode function, IReferenceGenerator referenceGenerator) =>
            new FuncNode(funcType, function, referenceGenerator);

        ILazyNode CreateLazyNode(INamedTypeSymbol lazyType, ILocalFunctionNode function, IReferenceGenerator referenceGenerator) =>
            new LazyNode(lazyType, function, referenceGenerator);

        ITupleNode CreateTupleNode(INamedTypeSymbol tupleType, IElementNodeMapperBase typeToElementNodeMapper, IReferenceGenerator referenceGenerator) =>
            new TupleNode(tupleType, typeToElementNodeMapper, referenceGenerator);

        IValueTupleSyntaxNode CreateValueTupleSyntaxNode(INamedTypeSymbol valueTupleSyntaxType, IElementNodeMapperBase typeToElementNodeMapper, IReferenceGenerator referenceGenerator) =>
            new ValueTupleSyntaxNode(valueTupleSyntaxType, typeToElementNodeMapper, referenceGenerator);

        IValueTupleNode CreateValueTupleNode(INamedTypeSymbol valueTupleType,
            IElementNodeMapperBase typeToElementNodeMapper, IReferenceGenerator referenceGenerator) =>
            new ValueTupleNode(valueTupleType, typeToElementNodeMapper, referenceGenerator);

        ITaskNode CreateTaskNode(INamedTypeSymbol taskType, IFunctionNode parentFunction, IElementNodeMapperBase typeToElementNodeMapper, IReferenceGenerator referenceGenerator) =>
            new TaskNode(taskType, parentFunction, typeToElementNodeMapper, referenceGenerator);

        IValueTaskNode CreateValueTaskNode(INamedTypeSymbol valueTaskType, IFunctionNode parentFunction, IElementNodeMapperBase typeToElementNodeMapper, IReferenceGenerator referenceGenerator) =>
            new ValueTaskNode(valueTaskType, parentFunction, typeToElementNodeMapper, referenceGenerator);

        IFactoryFunctionNode CreateFactoryFunctionNode(
            IMethodSymbol methodSymbol, 
            IFunctionNode parentFunction,
            IElementNodeMapperBase typeToElementNodeMapper, 
            IReferenceGenerator referenceGenerator) =>
            new FactoryFunctionNode(methodSymbol, parentFunction, typeToElementNodeMapper, referenceGenerator, wellKnownTypes);

        IFactoryPropertyNode CreateFactoryPropertyNode(
            IPropertySymbol property,
            IFunctionNode parentFunction, 
            IReferenceGenerator referenceGenerator) =>
            new FactoryPropertyNode(property, parentFunction, referenceGenerator, wellKnownTypes);

        IFactoryFieldNode CreateFactoryFieldNode(IFieldSymbol field, IFunctionNode parentFunction, IReferenceGenerator referenceGenerator) =>
            new FactoryFieldNode(field, parentFunction, referenceGenerator, wellKnownTypes);

        IParameterNode CreateParameterNode(ITypeSymbol type, IReferenceGenerator referenceGenerator) => new ParameterNode(type, referenceGenerator);
            
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
                ITypeSymbol returnType,
                ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> parameters,
                string accessModifier) => new CreateFunctionResolutionBuilder(
                rangeResolutionBaseBuilder,
                returnType,
                parameters,
                FunctionResolutionSynchronicityDecisionMakerFactory(),
                accessModifier,

                wellKnownTypes,
                referenceGeneratorFactory,
                functionCycleTracker,
                diagLogger);

            IScopeRootCreateFunctionResolutionBuilder ScopeRootCreateFunctionResolutionBuilderFactory(
                IRangeResolutionBaseBuilder rangeResolutionBaseBuilder,
                SwitchImplementationParameter parameter) => new ScopeRootCreateFunctionResolutionBuilder(
                rangeResolutionBaseBuilder,
                parameter,
                FunctionResolutionSynchronicityDecisionMakerFactory(),

                wellKnownTypes,
                referenceGeneratorFactory,
                functionCycleTracker,
                diagLogger);

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
                functionCycleTracker,
                diagLogger);

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
        IReferenceGenerator ReferenceGeneratorFactory(int j) => new ReferenceGenerator(j, diagLogger);

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
        
        ICheckTypeProperties CreateCheckTypeProperties(IReadOnlyList<ITypesFromAttributes> typesFromAttributes) =>
            new CheckTypeProperties(
                new CurrentlyConsideredTypes(typesFromAttributes, implementationTypeSetCache), 
                wellKnownTypes);

        IUserDefinedElements CreateUserDefinedElements(
            INamedTypeSymbol rangeType,
            INamedTypeSymbol containerType) =>
            new UserDefinedElements(rangeType, containerType, wellKnownTypes, wellKnownTypesMiscellaneous);

    }
}