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
        var assemblyTypesFromAttributes = new TypesFromAttributesBase(
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
        
        var referenceGeneratorFactory = new ReferenceGeneratorFactory(ReferenceGeneratorFactory);
        var containerDieExceptionGenerator = new ContainerDieExceptionGenerator(context, wellKnownTypesMiscellaneous);
        var implementationTypeSetCache = new ImplementationTypeSetCache(context, wellKnownTypes);
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

        ICodeGenerationVisitor CreateCodeGenerationVisitor() => new CodeGenerationVisitor(wellKnownTypes, wellKnownTypesCollections);

        IContainerNode ContainerNodeFactory(
            IContainerInfo ci)
        {
            var containerTypesFromAttributes = new TypesFromAttributesBase(
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
            new TaskTransformationFunctions(referenceGenerator, wellKnownTypes);

        IDisposalHandlingNode CreateDisposalHandlingNode(IReferenceGenerator referenceGenerator) =>
            new DisposalHandlingNode(referenceGenerator, wellKnownTypes);

        ITransientScopeInterfaceNode CreateTransientScopeInterfaceNode(
            IContainerNode container,
            IReferenceGenerator referenceGenerator) =>
            new TransientScopeInterfaceNode(container, referenceGenerator, CreateRangedInstanceInterfaceFunctionNode);

        IRangedInstanceInterfaceFunctionNode CreateRangedInstanceInterfaceFunctionNode(
            INamedTypeSymbol type, 
            IReadOnlyList<ITypeSymbol> parameters,
            IContainerNode parentContainer, 
            IRangeNode parentRange,
            IReferenceGenerator referenceGenerator) =>
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
                wellKnownTypes);

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
                    var scopeTypesFromAttributes = new ScopeTypesFromAttributesBase(
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
                CreateMultiFunctionNode,
                CreateScopeFunctionNode,
                CreateRangedInstanceFunctionGroupNode,
                CreateDisposalHandlingNode);

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
                CreateMultiFunctionNode,
                CreateTransientScopeFunctionNode,
                CreateRangedInstanceFunctionGroupNode,
                CreateDisposalHandlingNode);

        IMultiFunctionNode CreateMultiFunctionNode(
            INamedTypeSymbol enumerableType,
            IReadOnlyList<ITypeSymbol> parameters,
            IRangeNode parentNode,
            IContainerNode parentContainer,
            IUserDefinedElements userDefinedElements,
            ICheckTypeProperties checkTypeProperties,
            IReferenceGenerator referenceGenerator) =>
            new MultiFunctionNodeBase(
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
                wellKnownTypes,
                wellKnownTypesCollections);

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

        ICreateTransientScopeFunctionNode CreateTransientScopeFunctionNode(
            INamedTypeSymbol typeSymbol,
            IReadOnlyList<ITypeSymbol> parameters,
            IRangeNode parentRange,
            IContainerNode parentContainer,
            IUserDefinedElements userDefinedElements,
            ICheckTypeProperties checkTypeProperties,
            IReferenceGenerator referenceGenerator) =>
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
                wellKnownTypes);

        ITransientScopeDisposalElementNodeMapper CreateTransientScopeDisposalElementNodeMapper(
            IElementNodeMapperBase parentMapper,
            ElementNodeMapperBase.PassedDependencies passedDependencies) =>
            new TransientScopeDisposalElementNodeMapper(
                parentMapper,
                passedDependencies,
                diagLogger,
                wellKnownTypes,
                wellKnownTypesCollections,
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

        ILocalFunctionNode CreateLocalFunctionNode(
            ITypeSymbol typeSymbol,
            IReadOnlyList<ITypeSymbol> parameters,
            ImmutableDictionary<ITypeSymbol, IParameterNode> closureParameters, 
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
            IReadOnlyList<ITypeSymbol> parameters,
            IRangeNode parentRange,
            IContainerNode parentContainer,
            IUserDefinedElements userDefinedElements,
            ICheckTypeProperties checkTypeProperties,
            IReferenceGenerator referenceGenerator) =>
            new EntryFunctionNode(
                typeSymbol,
                prefix,
                parameters,
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
            ImmutableQueue<(INamedTypeSymbol, INamedTypeSymbol)> @override) =>
            new OverridingElementNodeMapper(
                parentElementNodeMapper,
                passedDependencies,
                @override,
                diagLogger,
                wellKnownTypes,
                wellKnownTypesCollections,
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
            (INamedTypeSymbol, INamedTypeSymbol) @override) =>
            new OverridingElementNodeWithDecorationMapper(
                parentElementNodeMapper,
                passedDependencies,
                @override,
                diagLogger,
                wellKnownTypes,
                wellKnownTypesCollections,
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
                wellKnownTypes,
                wellKnownTypesCollections,
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
                wellKnownTypesCollections,
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
                wellKnownTypes,
                wellKnownTypesCollections);

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
            IUserDefinedElements userDefinedElements, 
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
                wellKnownTypes);

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
            new FactoryFunctionNode(methodSymbol, parentFunction, typeToElementNodeMapper, referenceGenerator, wellKnownTypes);

        IFactoryPropertyNode CreateFactoryPropertyNode(
            IPropertySymbol property,
            IFunctionNode parentFunction, 
            IReferenceGenerator referenceGenerator) =>
            new FactoryPropertyNode(property, parentFunction, referenceGenerator, wellKnownTypes);

        IFactoryFieldNode CreateFactoryFieldNode(IFieldSymbol field, IFunctionNode parentFunction, IReferenceGenerator referenceGenerator) =>
            new FactoryFieldNode(field, parentFunction, referenceGenerator, wellKnownTypes);

        IParameterNode CreateParameterNode(ITypeSymbol type, IReferenceGenerator referenceGenerator) => new ParameterNode(type, referenceGenerator);
            
        IContainerInfo ContainerInfoFactory(INamedTypeSymbol type) => new ContainerInfo(type, wellKnownTypesMiscellaneous);
        IReferenceGenerator ReferenceGeneratorFactory(int j) => new ReferenceGenerator(j, diagLogger);
        
        ICheckTypeProperties CreateCheckTypeProperties(IReadOnlyList<ITypesFromAttributesBase> typesFromAttributes) =>
            new CheckTypeProperties(
                new CurrentlyConsideredTypes(typesFromAttributes, implementationTypeSetCache), 
                wellKnownTypes);

        IUserDefinedElements CreateUserDefinedElements(
            INamedTypeSymbol rangeType,
            INamedTypeSymbol containerType) =>
            new UserDefinedElements(rangeType, containerType, wellKnownTypes, wellKnownTypesMiscellaneous);

    }
}