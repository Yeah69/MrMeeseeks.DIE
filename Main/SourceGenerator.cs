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
using MrMeeseeks.DIE.RangeRoots;
using MrMeeseeks.DIE.Validation.Attributes;
using MrMeeseeks.DIE.Validation.Range;
using MrMeeseeks.DIE.Validation.Range.UserDefined;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility;

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
        WellKnownTypesCollections wellKnownTypesCollections = WellKnownTypesCollections.Create(context.Compilation);
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous = WellKnownTypesMiscellaneous.Create(context.Compilation);
        var errorDescriptionInsteadOfBuildFailure = context.Compilation.Assembly.GetAttributes()
            .Any(ad => CustomSymbolEqualityComparer.Default.Equals(wellKnownTypesMiscellaneous.ErrorDescriptionInsteadOfBuildFailureAttribute, ad.AttributeClass));

        WellKnownTypes wellKnownTypes = WellKnownTypes.Create(context.Compilation);
        WellKnownTypesAggregation wellKnownTypesAggregation = WellKnownTypesAggregation.Create(context.Compilation);
        WellKnownTypesChoice wellKnownTypesChoice = WellKnownTypesChoice.Create(context.Compilation);
        IContainerWideContext containerWideContext = new ContainerWideContext(
            wellKnownTypes,
            wellKnownTypesAggregation,
            wellKnownTypesChoice,
            wellKnownTypesCollections,
            wellKnownTypesMiscellaneous);
        DiagLogger diagLogger = new DiagLogger(errorDescriptionInsteadOfBuildFailure, context);
        var validateUserDefinedAddForDisposalSync = new ValidateUserDefinedAddForDisposalSync(containerWideContext);
        var validateUserDefinedAddForDisposalAsync = new ValidateUserDefinedAddForDisposalAsync(containerWideContext);
        var validateUserDefinedConstructorParametersInjectionMethod = new ValidateUserDefinedConstructorParametersInjectionMethod(containerWideContext);
        var validateUserDefinedPropertiesInjectionMethod = new ValidateUserDefinedPropertiesInjectionMethod(containerWideContext);
        var validateUserDefinedInitializerParametersInjectionMethod = new ValidateUserDefinedInitializerParametersInjectionMethod(containerWideContext);
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
            containerWideContext);
        var validateScope = new ValidateScope(
            validateUserDefinedAddForDisposalSync,
            validateUserDefinedAddForDisposalAsync, 
            validateUserDefinedConstructorParametersInjectionMethod, 
            validateUserDefinedPropertiesInjectionMethod,
            validateUserDefinedInitializerParametersInjectionMethod,
            validateUserDefinedFactoryMethod,
            validateUserDefinedFactoryField,
            containerWideContext);
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
            containerWideContext);
        var assemblyTypesFromAttributes = new TypesFromAttributes(
            context.Compilation.Assembly.GetAttributes(), 
            null,
            null,
            validateAttributes,
            containerWideContext);
        
        foreach (var diagnostic in assemblyTypesFromAttributes
                     .Warnings
                     .Concat(assemblyTypesFromAttributes.Errors))
            diagLogger.Log(diagnostic);

        if (assemblyTypesFromAttributes.Errors.Any())
            return;
        
        var referenceGeneratorFactory = new ReferenceGeneratorFactory(ReferenceGeneratorFactory);
        var containerDieExceptionGenerator = new ContainerDieExceptionGenerator(context, containerWideContext);
        var implementationTypeSetCache = new ImplementationTypeSetCache(context, containerWideContext);
        new ExecuteImpl(
            errorDescriptionInsteadOfBuildFailure,
            context,
            wellKnownTypesMiscellaneous,
            containerDieExceptionGenerator,
            validateContainer,
            ContainerInfoFactory, 
            ContainerNodeFactory,
            CreateCodeGenerationVisitor,
            diagLogger).Execute();

        ICodeGenerationVisitor CreateCodeGenerationVisitor() => new CodeGenerationVisitor(containerWideContext);

        IContainerNode ContainerNodeFactory(
            IContainerInfo ci)
        {
            var containerTypesFromAttributes = new TypesFromAttributes(
                ci.ContainerType.GetAttributes(),
                ci.ContainerType,
                ci.ContainerType,
                validateAttributes,
                containerWideContext);

            foreach (var diagnostic in containerTypesFromAttributes.Warnings)
                diagLogger.Log(diagnostic);

            if (containerTypesFromAttributes.Errors.Any())
                throw new ValidationDieException(containerTypesFromAttributes.Errors.ToImmutableArray());

            var containerTypesFromAttributesList = ImmutableList.Create(
                (ITypesFromAttributesBase)assemblyTypesFromAttributes,
                containerTypesFromAttributes);

            return new ContainerNode(
                ci,
                containerTypesFromAttributesList,
                CreateUserDefinedElements,
                CreateCheckTypeProperties(containerTypesFromAttributesList),
                referenceGeneratorFactory.Create(),
                new Nodes.FunctionCycleTracker(),
                CreateFunctionNode,
                CreateMultiFunctionNode,
                CreateRangedInstanceFunctionGroupNode,
                CreateEntryFunctionNode,
                CreateTransientScopeInterfaceNode,
                CreateTaskTransformationFunctions,
                CreateScopeManager,
                CreateDisposalHandlingNode);
        }

        ITaskTransformationFunctions CreateTaskTransformationFunctions(IReferenceGenerator referenceGenerator) =>
            new TaskTransformationFunctions(referenceGenerator, containerWideContext);

        IDisposalHandlingNode CreateDisposalHandlingNode(IReferenceGenerator referenceGenerator) =>
            new DisposalHandlingNode(referenceGenerator, containerWideContext);

        ITransientScopeInterfaceNode CreateTransientScopeInterfaceNode(
            IContainerNode container,
            IReferenceGenerator referenceGenerator) =>
            new TransientScopeInterfaceNode(container, referenceGenerator, CreateRangedInstanceInterfaceFunctionNode);

        IRangedInstanceInterfaceFunctionNodeRoot CreateRangedInstanceInterfaceFunctionNode(
            INamedTypeSymbol type, 
            IReadOnlyList<ITypeSymbol> parameters,
            IContainerNode parentContainer, 
            IRangeNode parentRange,
            IReferenceGenerator referenceGenerator) =>
            new RangedInstanceInterfaceFunctionNodeRoot(
            new RangedInstanceInterfaceFunctionNode(
                type,
                parameters,
                parentContainer,
                parentRange,
                referenceGenerator,
                CreatePlainFunctionCallNode,
                CreateScopeCallNode,
                CreateTransientScopeCallNode,
                CreateParameterNode,
                containerWideContext));

        Nodes.IScopeManager CreateScopeManager(
            IContainerInfo containerInfo,
            IContainerNode container,
            ITransientScopeInterfaceNode transientScopeInterface,
            ImmutableList<ITypesFromAttributesBase> typesFromAttributesList,
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
                        containerWideContext);
                    
                    foreach (var diagnostic in scopeTypesFromAttributes.Warnings)
                        diagLogger.Log(diagnostic);

                    if (scopeTypesFromAttributes.Errors.Any())
                        throw new ValidationDieException(scopeTypesFromAttributes.Errors.ToImmutableArray());
                    
                    return scopeTypesFromAttributes;
                },
                CreateCheckTypeProperties,
                CreateUserDefinedElements,
                new EmptyUserDefinedElements(),
                containerWideContext);

        IScopeNode CreateScopeNode(
            string name,
            IContainerNode container,
            ITransientScopeInterfaceNode transientScopeInterface,
            Nodes.IScopeManager scopeManager,
            IUserDefinedElementsBase userDefinedElements,
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
                CreateMultiFunctionNode,
                CreateScopeFunctionNode,
                CreateRangedInstanceFunctionGroupNode,
                CreateDisposalHandlingNode);

        ITransientScopeNode CreateTransientScopeNode(
            string name,
            IContainerNode container,
            Nodes.IScopeManager scopeManager,
            IUserDefinedElementsBase userDefinedElements,
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
                CreateMultiFunctionNode,
                CreateTransientScopeFunctionNode,
                CreateRangedInstanceFunctionGroupNode,
                CreateDisposalHandlingNode);

        IMultiFunctionNodeRoot CreateMultiFunctionNode(
            INamedTypeSymbol enumerableType,
            IReadOnlyList<ITypeSymbol> parameters,
            IRangeNode parentNode,
            IContainerNode parentContainer,
            IUserDefinedElementsBase userDefinedElements,
            ICheckTypeProperties checkTypeProperties,
            IReferenceGenerator referenceGenerator) =>
            new MultiFunctionNodeRoot(
                new MultiFunctionNode(
                    enumerableType,
                    parameters,
                    parentNode,
                    parentContainer,
                    userDefinedElements,
                    checkTypeProperties,
                    referenceGenerator,
                    CreateParameterNode,
                    CreatePlainFunctionCallNode,
                    CreateScopeCallNode,
                    CreateTransientScopeCallNode,
                    CreateElementNodeMapper,
                    CreateOverridingElementNodeWithDecorationMapper,
                    containerWideContext));

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

        ICreateFunctionNodeRoot CreateFunctionNode(
            ITypeSymbol typeSymbol,
            IReadOnlyList<ITypeSymbol> parameters,
            IRangeNode parentRange,
            IContainerNode parentContainer,
            IUserDefinedElementsBase userDefinedElements,
            ICheckTypeProperties checkTypeProperties,
            IReferenceGenerator referenceGenerator) =>
            new CreateFunctionNodeRoot(
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
                    containerWideContext));

        ICreateScopeFunctionNodeRoot CreateScopeFunctionNode(
            INamedTypeSymbol typeSymbol,
            IReadOnlyList<ITypeSymbol> parameters,
            IRangeNode parentRange,
            IContainerNode parentContainer,
            IUserDefinedElementsBase userDefinedElements,
            ICheckTypeProperties checkTypeProperties,
            IReferenceGenerator referenceGenerator) =>
            new CreateScopeFunctionNodeRoot(
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
                    containerWideContext));

        ICreateTransientScopeFunctionNodeRoot CreateTransientScopeFunctionNode(
            INamedTypeSymbol typeSymbol,
            IReadOnlyList<ITypeSymbol> parameters,
            IRangeNode parentRange,
            IContainerNode parentContainer,
            IUserDefinedElementsBase userDefinedElements,
            ICheckTypeProperties checkTypeProperties,
            IReferenceGenerator referenceGenerator) =>
            new CreateTransientScopeFunctionNodeRoot(
                new CreateTransientScopeFunctionNode(
                    typeSymbol,
                    parameters,
                    parentRange,
                    parentContainer,
                    userDefinedElements,
                    checkTypeProperties,
                    referenceGenerator,
                    CreateElementNodeMapper,
                    CreateTransientScopeDisposalElementNodeMapper,
                    CreatePlainFunctionCallNode,
                    CreateScopeCallNode,
                    CreateTransientScopeCallNode,
                    CreateParameterNode,
                    containerWideContext));

        ITransientScopeDisposalElementNodeMapper CreateTransientScopeDisposalElementNodeMapper(
            IElementNodeMapperBase parentMapper,
            ElementNodeMapperBase.PassedDependencies passedDependencies) =>
            new TransientScopeDisposalElementNodeMapper(
                parentMapper,
                passedDependencies,
                diagLogger,
                containerWideContext,
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
                CreateEnumerableBasedNode,
                CreateAbstractionNode,
                CreateImplementationNode,
                CreateOutParameterNode,
                CreateErrorNode,
                CreateNullNode,
                CreateLocalFunctionNode,
                CreateOverridingElementNodeMapper,
                CreateNonWrapToCreateElementNodeMapper,
                CreateTransientScopeDisposalTriggerNode);

        ITransientScopeDisposalTriggerNode CreateTransientScopeDisposalTriggerNode(
            INamedTypeSymbol disposalType,
            IReferenceGenerator referenceGenerator) =>
            new TransientScopeDisposalTriggerNode(disposalType, referenceGenerator);

        ILocalFunctionNodeRoot CreateLocalFunctionNode(
            ITypeSymbol typeSymbol,
            IReadOnlyList<ITypeSymbol> parameters,
            ImmutableDictionary<ITypeSymbol, IParameterNode> closureParameters,
            IRangeNode parentRange,
            IContainerNode parentContainer,
            IUserDefinedElementsBase userDefinedElements,
            ICheckTypeProperties checkTypeProperties,
            IElementNodeMapperBase mapper,
            IReferenceGenerator referenceGenerator) =>
            new LocalFunctionNodeRoot(
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
                    containerWideContext));

        IRangedInstanceFunctionNodeRoot CreateRangedInstanceFunctionNode(
            ScopeLevel level,
            INamedTypeSymbol type,
            IReadOnlyList<ITypeSymbol> parameters,
            IRangeNode parentRange,
            IContainerNode parentContainer,
            IUserDefinedElementsBase userDefinedElements,
            ICheckTypeProperties checkTypeProperties,
            IReferenceGenerator referenceGenerator) =>
            new RangedInstanceFunctionNodeRoot(
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
                    containerWideContext));

        
        IRangedInstanceFunctionGroupNode CreateRangedInstanceFunctionGroupNode(
            ScopeLevel level,
            INamedTypeSymbol type,
            IRangeNode parentRange,
            IContainerNode parentContainer,
            IUserDefinedElementsBase userDefinedElements,
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

        IEntryFunctionNodeRoot CreateEntryFunctionNode(
            ITypeSymbol typeSymbol,
            string prefix,
            IReadOnlyList<ITypeSymbol> parameters,
            IRangeNode parentRange,
            IContainerNode parentContainer,
            IUserDefinedElementsBase userDefinedElements,
            ICheckTypeProperties checkTypeProperties,
            IReferenceGenerator referenceGenerator) =>
            new EntryFunctionNodeRoot(
                new EntryFunctionNode(
                    typeSymbol,
                    prefix,
                    parameters,
                    parentRange,
                    parentContainer,
                    userDefinedElements,
                    checkTypeProperties,
                    referenceGenerator,
                    containerWideContext,
                    CreateElementNodeMapper,
                    CreateNonWrapToCreateElementNodeMapper,
                    CreatePlainFunctionCallNode,
                    CreateScopeCallNode,
                    CreateTransientScopeCallNode,
                    CreateParameterNode));

        IScopeCallNode CreateScopeCallNode(
            string containerParameter, 
            string transientScopeInterfaceParameter,
            IScopeNode scope,
            IRangeNode callingRange,
            IFunctionNode calledFunction,
            IReadOnlyList<(IParameterNode, IParameterNode)> parameters,
            IReferenceGenerator referenceGenerator) =>
            new ScopeCallNode(
                containerParameter,
                transientScopeInterfaceParameter,
                scope,
                callingRange,
                calledFunction,
                parameters,
                referenceGenerator);

        ITransientScopeCallNode CreateTransientScopeCallNode(
            string containerParameter, 
            ITransientScopeNode transientScope,
            IContainerNode parentContainer,
            IRangeNode callingRange,
            IFunctionNode calledFunction,
            IReadOnlyList<(IParameterNode, IParameterNode)> parameters,
            IReferenceGenerator referenceGenerator) =>
            new TransientScopeCallNode(
                containerParameter,
                transientScope,
                parentContainer,
                callingRange,
                calledFunction,
                parameters,
                referenceGenerator);

        IOverridingElementNodeMapper CreateOverridingElementNodeMapper(
            IElementNodeMapperBase parentElementNodeMapper,
            ElementNodeMapperBase.PassedDependencies passedDependencies,
            ImmutableQueue<(INamedTypeSymbol, INamedTypeSymbol)> overrideParam) =>
            new OverridingElementNodeMapper(
                parentElementNodeMapper,
                passedDependencies,
                overrideParam,
                diagLogger,
                containerWideContext,
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
                CreateEnumerableBasedNode,
                CreateAbstractionNode,
                CreateImplementationNode,
                CreateOutParameterNode,
                CreateErrorNode,
                CreateNullNode,
                CreateLocalFunctionNode,
                CreateOverridingElementNodeMapper,
                CreateNonWrapToCreateElementNodeMapper);

        IOverridingElementNodeWithDecorationMapper CreateOverridingElementNodeWithDecorationMapper(
            IElementNodeMapperBase parentElementNodeMapper,
            ElementNodeMapperBase.PassedDependencies passedDependencies,
            (INamedTypeSymbol, INamedTypeSymbol) overrideParam) =>
            new OverridingElementNodeWithDecorationMapper(
                parentElementNodeMapper,
                passedDependencies,
                overrideParam,
                diagLogger,
                containerWideContext,
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
                CreateEnumerableBasedNode,
                CreateAbstractionNode,
                CreateImplementationNode,
                CreateOutParameterNode,
                CreateErrorNode,
                CreateNullNode,
                CreateLocalFunctionNode,
                CreateOverridingElementNodeMapper,
                CreateNonWrapToCreateElementNodeMapper);

        INonWrapToCreateElementNodeMapper CreateNonWrapToCreateElementNodeMapper(
            IElementNodeMapperBase parentElementNodeMapper,
            ElementNodeMapperBase.PassedDependencies passedDependencies) =>
            new NonWrapToCreateElementNodeMapper(
                parentElementNodeMapper,
                passedDependencies,
                diagLogger,
                containerWideContext,
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
                CreateEnumerableBasedNode,
                CreateAbstractionNode,
                CreateImplementationNode,
                CreateOutParameterNode,
                CreateErrorNode,
                CreateNullNode,
                CreateLocalFunctionNode,
                CreateOverridingElementNodeMapper,
                CreateNonWrapToCreateElementNodeMapper);

        IElementNodeMapper CreateElementNodeMapper(
            IFunctionNode parentFunction,
            IRangeNode parentRange,
            IContainerNode parentContainer,
            IUserDefinedElementsBase userDefinedElements,
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
                containerWideContext,
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
                CreateEnumerableBasedNode,
                CreateAbstractionNode,
                CreateImplementationNode,
                CreateOutParameterNode,
                CreateErrorNode,
                CreateNullNode,
                CreateLocalFunctionNode,
                CreateOverridingElementNodeMapper,
                CreateNonWrapToCreateElementNodeMapper);

        IEnumerableBasedNode CreateEnumerableBasedNode(
            ITypeSymbol collectionType,
            IRangeNode parentRange,
            IFunctionNode parentFunction,
            IReferenceGenerator referenceGenerator) =>
            new EnumerableBasedNode(
                collectionType,
                parentRange,
                parentFunction,
                referenceGenerator,
                containerWideContext);

        INullNode CreateNullNode(ITypeSymbol nullableType, IReferenceGenerator referenceGenerator) => new NullNode(nullableType, referenceGenerator);

        IErrorNode CreateErrorNode(
            string message,
            ITypeSymbol currentType,
            IRangeNode parentRange) => 
            new ErrorNode(message, currentType, parentRange, diagLogger);

        IImplementationNode CreateImplementationNode(
            INamedTypeSymbol implementationType, 
            IMethodSymbol constructor,
            IFunctionNode parentFunction,
            IRangeNode parentRange,
            IElementNodeMapperBase typeToElementNodeMapper,
            ICheckTypeProperties checkTypeProperties,
            IUserDefinedElementsBase userDefinedElements, 
            IReferenceGenerator referenceGenerator) =>
            new ImplementationNode(
                implementationType, 
                constructor, 
                parentFunction,
                parentRange,
                typeToElementNodeMapper,
                checkTypeProperties,
                userDefinedElements, 
                referenceGenerator,
                containerWideContext);

        IOutParameterNode CreateOutParameterNode(ITypeSymbol type, IReferenceGenerator referenceGenerator) =>
            new OutParameterNode(type, referenceGenerator);

        IAbstractionNode CreateAbstractionNode(
            INamedTypeSymbol abstractionType, 
            INamedTypeSymbol implementationType,
            IElementNodeMapperBase mapper,
            IReferenceGenerator referenceGenerator) =>
            new AbstractionNode(abstractionType, implementationType, mapper, referenceGenerator);

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

        ITaskNode CreateTaskNode(
            INamedTypeSymbol taskType,
            IContainerNode parentContainer, 
            IFunctionNode parentFunction, 
            IElementNodeMapperBase typeToElementNodeMapper, 
            IReferenceGenerator referenceGenerator) =>
            new TaskNode(taskType, parentContainer, parentFunction, typeToElementNodeMapper, referenceGenerator);

        IValueTaskNode CreateValueTaskNode(
            INamedTypeSymbol valueTaskType, 
            IContainerNode parentContainer, 
            IFunctionNode parentFunction,
            IElementNodeMapperBase typeToElementNodeMapper, 
            IReferenceGenerator referenceGenerator) =>
            new ValueTaskNode(valueTaskType, parentContainer, parentFunction, typeToElementNodeMapper, referenceGenerator);

        IFactoryFunctionNode CreateFactoryFunctionNode(
            IMethodSymbol methodSymbol, 
            IFunctionNode parentFunction,
            IElementNodeMapperBase typeToElementNodeMapper, 
            IReferenceGenerator referenceGenerator) =>
            new FactoryFunctionNode(methodSymbol, parentFunction, typeToElementNodeMapper, referenceGenerator, containerWideContext);

        IFactoryPropertyNode CreateFactoryPropertyNode(
            IPropertySymbol property,
            IFunctionNode parentFunction, 
            IReferenceGenerator referenceGenerator) =>
            new FactoryPropertyNode(property, parentFunction, referenceGenerator, containerWideContext);

        IFactoryFieldNode CreateFactoryFieldNode(IFieldSymbol field, IFunctionNode parentFunction, IReferenceGenerator referenceGenerator) =>
            new FactoryFieldNode(field, parentFunction, referenceGenerator, containerWideContext);

        IParameterNode CreateParameterNode(ITypeSymbol type, IReferenceGenerator referenceGenerator) => new ParameterNode(type, referenceGenerator);
            
        IContainerInfo ContainerInfoFactory(INamedTypeSymbol type) => new ContainerInfo(type, wellKnownTypesMiscellaneous);
        IReferenceGenerator ReferenceGeneratorFactory(int j) => new ReferenceGenerator(j, diagLogger);
        
        ICheckTypeProperties CreateCheckTypeProperties(IReadOnlyList<ITypesFromAttributesBase> typesFromAttributes) =>
            new CheckTypeProperties(
                new CurrentlyConsideredTypes(typesFromAttributes, implementationTypeSetCache), 
                containerWideContext);

        IUserDefinedElements CreateUserDefinedElements(
            (INamedTypeSymbol Range, INamedTypeSymbol Container) types) =>
            new UserDefinedElements(types, containerWideContext);

    }
}