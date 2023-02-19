using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.Delegates;
using MrMeeseeks.DIE.Nodes.Elements.Factories;
using MrMeeseeks.DIE.Nodes.Elements.Tasks;
using MrMeeseeks.DIE.Nodes.Elements.Tuples;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Mappers;

internal interface IElementNodeMapperBase
{
    IElementNode Map(ITypeSymbol type, ImmutableStack<INamedTypeSymbol> implementationStack);
    IElementNode MapToImplementation(
        ImplementationMappingConfiguration config,
        INamedTypeSymbol implementationType,
        ImmutableStack<INamedTypeSymbol> implementationStack);
    IElementNode MapToOutParameter(ITypeSymbol type, ImmutableStack<INamedTypeSymbol> implementationStack);
    ElementNodeMapperBase.PassedDependencies MapperDependencies { get; }
    void ResetFunction(ISingleFunctionNode parentFunction); // todo replace this workaround
}

internal record ImplementationMappingConfiguration(
    bool CheckForScopeRoot,
    bool CheckForRangedInstance);

internal abstract class ElementNodeMapperBase : IElementNodeMapperBase
{
    internal record PassedDependencies(
        IFunctionNode ParentFunction,
        IRangeNode ParentRange,
        IContainerNode ParentContainer,
        IUserDefinedElements UserDefinedElements,
        ICheckTypeProperties CheckTypeProperties,
        IReferenceGenerator ReferenceGenerator);
    
    protected IFunctionNode ParentFunction; // todo make readonly again
    protected readonly IRangeNode ParentRange;
    private readonly IContainerNode _parentContainer;
    private readonly IUserDefinedElements _userDefinedElements;
    private readonly ICheckTypeProperties _checkTypeProperties;
    private readonly IReferenceGenerator _referenceGenerator;
    private readonly IDiagLogger _diagLogger;
    protected readonly WellKnownTypes WellKnownTypes;
    private readonly WellKnownTypesCollections _wellKnownTypesCollections;
    private readonly Func<IFieldSymbol, IFunctionNode, IReferenceGenerator, IFactoryFieldNode> _factoryFieldNodeFactory;
    private readonly Func<IPropertySymbol, IFunctionNode, IReferenceGenerator, IFactoryPropertyNode> _factoryPropertyNodeFactory;
    private readonly Func<IMethodSymbol, IFunctionNode, IElementNodeMapperBase, IReferenceGenerator, IFactoryFunctionNode> _factoryFunctionNodeFactory;
    private readonly Func<INamedTypeSymbol, IContainerNode, IFunctionNode, IElementNodeMapperBase, IReferenceGenerator, IValueTaskNode> _valueTaskNodeFactory;
    private readonly Func<INamedTypeSymbol, IContainerNode, IFunctionNode, IElementNodeMapperBase, IReferenceGenerator, ITaskNode> _taskNodeFactory;
    private readonly Func<INamedTypeSymbol, IElementNodeMapperBase, IReferenceGenerator, IValueTupleNode> _valueTupleNodeFactory;
    private readonly Func<INamedTypeSymbol, IElementNodeMapperBase, IReferenceGenerator, IValueTupleSyntaxNode> _valueTupleSyntaxNodeFactory;
    private readonly Func<INamedTypeSymbol, IElementNodeMapperBase, IReferenceGenerator, ITupleNode> _tupleNodeFactory;
    private readonly Func<INamedTypeSymbol, ILocalFunctionNode, IReferenceGenerator, ILazyNode> _lazyNodeFactory;
    private readonly Func<INamedTypeSymbol, ILocalFunctionNode, IReferenceGenerator, IFuncNode> _funcNodeFactory;
    private readonly Func<ITypeSymbol, IRangeNode, IFunctionNode, IReferenceGenerator, IEnumerableBasedNode> _enumerableBasedNodeFactory;
    private readonly Func<INamedTypeSymbol, INamedTypeSymbol, IElementNodeMapperBase, IReferenceGenerator, IAbstractionNode> _abstractionNodeFactory;
    private readonly Func<INamedTypeSymbol, IMethodSymbol, IFunctionNode, IRangeNode, IElementNodeMapperBase, ICheckTypeProperties, IUserDefinedElements, IReferenceGenerator, IImplementationNode> _implementationNodeFactory;
    private readonly Func<ITypeSymbol, IReferenceGenerator, IOutParameterNode> _outParameterNodeFactory;
    private readonly Func<string, IErrorNode> _errorNodeFactory;
    private readonly Func<ITypeSymbol, IReferenceGenerator, INullNode> _nullNodeFactory;
    private readonly Func<ITypeSymbol, IReadOnlyList<ITypeSymbol>, ImmutableSortedDictionary<TypeKey, (ITypeSymbol, IParameterNode)>, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IElementNodeMapperBase, IReferenceGenerator, ILocalFunctionNode> _localFunctionNodeFactory;
    private readonly Func<IElementNodeMapperBase, PassedDependencies, ImmutableQueue<(TypeKey, INamedTypeSymbol)>, IOverridingElementNodeMapper> _overridingElementNodeMapperFactory;
    private readonly Func<IElementNodeMapperBase, PassedDependencies, INonWrapToCreateElementNodeMapper> _nonWrapToCreateElementNodeMapperFactory;

    internal ElementNodeMapperBase(
        IFunctionNode parentFunction,
        IRangeNode parentRange,
        IContainerNode parentContainer,
        IUserDefinedElements userDefinedElements,
        ICheckTypeProperties checkTypeProperties,
        IReferenceGenerator referenceGenerator,
        
        IDiagLogger diagLogger,
        WellKnownTypes wellKnownTypes,
        WellKnownTypesCollections wellKnownTypesCollections,
        Func<IFieldSymbol, IFunctionNode, IReferenceGenerator, IFactoryFieldNode> factoryFieldNodeFactory,
        Func<IPropertySymbol, IFunctionNode, IReferenceGenerator, IFactoryPropertyNode> factoryPropertyNodeFactory,
        Func<IMethodSymbol, IFunctionNode, IElementNodeMapperBase, IReferenceGenerator, IFactoryFunctionNode> factoryFunctionNodeFactory,
        Func<INamedTypeSymbol, IContainerNode, IFunctionNode, IElementNodeMapperBase, IReferenceGenerator, IValueTaskNode> valueTaskNodeFactory,
        Func<INamedTypeSymbol, IContainerNode, IFunctionNode, IElementNodeMapperBase, IReferenceGenerator, ITaskNode> taskNodeFactory,
        Func<INamedTypeSymbol, IElementNodeMapperBase, IReferenceGenerator, IValueTupleNode> valueTupleNodeFactory,
        Func<INamedTypeSymbol, IElementNodeMapperBase, IReferenceGenerator, IValueTupleSyntaxNode> valueTupleSyntaxNodeFactory,
        Func<INamedTypeSymbol, IElementNodeMapperBase, IReferenceGenerator, ITupleNode> tupleNodeFactory,
        Func<INamedTypeSymbol, ILocalFunctionNode, IReferenceGenerator, ILazyNode> lazyNodeFactory,
        Func<INamedTypeSymbol, ILocalFunctionNode, IReferenceGenerator, IFuncNode> funcNodeFactory,
        Func<ITypeSymbol, IRangeNode, IFunctionNode, IReferenceGenerator, IEnumerableBasedNode> enumerableBasedNodeFactory,
        Func<INamedTypeSymbol, INamedTypeSymbol, IElementNodeMapperBase, IReferenceGenerator, IAbstractionNode> abstractionNodeFactory,
        Func<INamedTypeSymbol, IMethodSymbol, IFunctionNode, IRangeNode, IElementNodeMapperBase, ICheckTypeProperties, IUserDefinedElements, IReferenceGenerator, IImplementationNode> implementationNodeFactory,
        Func<ITypeSymbol, IReferenceGenerator, IOutParameterNode> outParameterNodeFactory,
        Func<string, IErrorNode> errorNodeFactory,
        Func<ITypeSymbol, IReferenceGenerator, INullNode> nullNodeFactory,
        Func<ITypeSymbol, IReadOnlyList<ITypeSymbol>, ImmutableSortedDictionary<TypeKey, (ITypeSymbol, IParameterNode)>, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IElementNodeMapperBase, IReferenceGenerator, ILocalFunctionNode> localFunctionNodeFactory,
        Func<IElementNodeMapperBase, PassedDependencies, ImmutableQueue<(TypeKey, INamedTypeSymbol)>, IOverridingElementNodeMapper> overridingElementNodeMapperFactory,
        Func<IElementNodeMapperBase, PassedDependencies, INonWrapToCreateElementNodeMapper> nonWrapToCreateElementNodeMapperFactory)
    {
        ParentFunction = parentFunction;
        ParentRange = parentRange;
        _parentContainer = parentContainer;
        _userDefinedElements = userDefinedElements;
        _checkTypeProperties = checkTypeProperties;
        _referenceGenerator = referenceGenerator;
        _diagLogger = diagLogger;
        WellKnownTypes = wellKnownTypes;
        _wellKnownTypesCollections = wellKnownTypesCollections;
        _factoryFieldNodeFactory = factoryFieldNodeFactory;
        _factoryPropertyNodeFactory = factoryPropertyNodeFactory;
        _factoryFunctionNodeFactory = factoryFunctionNodeFactory;
        _valueTaskNodeFactory = valueTaskNodeFactory;
        _taskNodeFactory = taskNodeFactory;
        _valueTupleNodeFactory = valueTupleNodeFactory;
        _valueTupleSyntaxNodeFactory = valueTupleSyntaxNodeFactory;
        _tupleNodeFactory = tupleNodeFactory;
        _lazyNodeFactory = lazyNodeFactory;
        _funcNodeFactory = funcNodeFactory;
        _enumerableBasedNodeFactory = enumerableBasedNodeFactory;
        _abstractionNodeFactory = abstractionNodeFactory;
        _implementationNodeFactory = implementationNodeFactory;
        _outParameterNodeFactory = outParameterNodeFactory;
        _errorNodeFactory = errorNodeFactory;
        _nullNodeFactory = nullNodeFactory;
        _localFunctionNodeFactory = localFunctionNodeFactory;
        _overridingElementNodeMapperFactory = overridingElementNodeMapperFactory;
        _nonWrapToCreateElementNodeMapperFactory = nonWrapToCreateElementNodeMapperFactory;

        MapperDependencies = new PassedDependencies(
            parentFunction, 
            parentRange, 
            parentContainer, 
            userDefinedElements,
            checkTypeProperties,
            referenceGenerator);
    }
    
    protected abstract IElementNodeMapperBase NextForWraps { get; }

    protected abstract IElementNodeMapperBase Next { get; }
    
    protected bool IsCollectionType(ITypeSymbol potentialCollectionType) =>
        CustomSymbolEqualityComparer.Default.Equals(potentialCollectionType.OriginalDefinition, _wellKnownTypesCollections.IEnumerable1)
        || CustomSymbolEqualityComparer.Default.Equals(potentialCollectionType.OriginalDefinition, _wellKnownTypesCollections.IReadOnlyCollection1)
        || CustomSymbolEqualityComparer.Default.Equals(potentialCollectionType.OriginalDefinition, _wellKnownTypesCollections.IReadOnlyList1)
        || potentialCollectionType is IArrayTypeSymbol;

    public virtual IElementNode Map(ITypeSymbol type, ImmutableStack<INamedTypeSymbol> implementationStack)
    {
        if (ParentFunction.Overrides.TryGetValue(type.ToTypeKey(), out var tuple))
            return tuple.Item2;

        if (_userDefinedElements.GetFactoryFieldFor(type) is { } instance)
            return _factoryFieldNodeFactory(instance, ParentFunction, _referenceGenerator)
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, implementationStack);

        if (_userDefinedElements.GetFactoryPropertyFor(type) is { } property)
            return _factoryPropertyNodeFactory(property, ParentFunction, _referenceGenerator)
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, implementationStack);

        if (_userDefinedElements.GetFactoryMethodFor(type) is { } method)
            return _factoryFunctionNodeFactory(method, ParentFunction, Next, _referenceGenerator)
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, implementationStack);

        if (CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, WellKnownTypes.ValueTask1)
            && type is INamedTypeSymbol valueTask)
            return _valueTaskNodeFactory(valueTask, _parentContainer, ParentFunction, NextForWraps, _referenceGenerator)
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, implementationStack);

        if (CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, WellKnownTypes.Task1)
            && type is INamedTypeSymbol task)
            return _taskNodeFactory(task, _parentContainer, ParentFunction, NextForWraps, _referenceGenerator)
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, implementationStack);

        if (type.FullName().StartsWith("global::System.ValueTuple<") && type is INamedTypeSymbol valueTupleType)
            return _valueTupleNodeFactory(valueTupleType, NextForWraps, _referenceGenerator)
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, implementationStack);

        if (type.FullName().StartsWith("(") && type.FullName().EndsWith(")") && type is INamedTypeSymbol syntaxValueTupleType)
            return _valueTupleSyntaxNodeFactory(syntaxValueTupleType, NextForWraps, _referenceGenerator)
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, implementationStack);

        if (type.FullName().StartsWith("global::System.Tuple<") && type is INamedTypeSymbol tupleType)
            return _tupleNodeFactory(tupleType, NextForWraps, _referenceGenerator)
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, implementationStack);

        if (CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, WellKnownTypes.Lazy1)
            && type is INamedTypeSymbol lazyType)
        {
            if (lazyType.TypeArguments.SingleOrDefault() is not { } valueType)
            {
                return _errorNodeFactory(lazyType.TypeArguments.Length switch 
                {
                    0 => "Lazy: No type argument",
                    > 1 => "Lazy: more than one type argument",
                    _ => $"Lazy: {lazyType.TypeArguments.First().FullName()} is not a type symbol",
                });
            }

            var mapper = _nonWrapToCreateElementNodeMapperFactory(this, MapperDependencies);

            var function = _localFunctionNodeFactory(
                valueType,
                Array.Empty<ITypeSymbol>(),
                ParentFunction.Overrides,
                ParentRange,
                _parentContainer,
                _userDefinedElements,
                _checkTypeProperties,
                mapper, 
                _referenceGenerator)
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, implementationStack);
            ParentFunction.AddLocalFunction(function);
            
            mapper.ResetFunction(function);
            
            return _lazyNodeFactory(lazyType, function, _referenceGenerator)
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, implementationStack);
        }

        if (type.TypeKind == TypeKind.Delegate 
            && type.FullName().StartsWith("global::System.Func<")
            && type is INamedTypeSymbol funcType)
        {
            if (funcType.TypeArguments.LastOrDefault() is not { } returnType)
            {
                return _errorNodeFactory(funcType.TypeArguments.Length switch 
                {
                    0 => "Func: No type argument",
                    _ => $"Func: {funcType.TypeArguments.Last().FullName()} is not a type symbol",
                });
            }
            
            var lambdaParameters = funcType
                .TypeArguments
                .Take(funcType.TypeArguments.Length - 1)
                .ToArray();

            var mapper = _nonWrapToCreateElementNodeMapperFactory(this, MapperDependencies);

            var function = _localFunctionNodeFactory(
                returnType,
                lambdaParameters,
                ParentFunction.Overrides,
                ParentRange,
                _parentContainer,
                _userDefinedElements,
                _checkTypeProperties,
                mapper,
                _referenceGenerator)
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, implementationStack);
            ParentFunction.AddLocalFunction(function);
            
            mapper.ResetFunction(function);
            
            return _funcNodeFactory(funcType, function, _referenceGenerator)
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, implementationStack);
        }
        
        if (CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.IEnumerable1)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.IAsyncEnumerable1)
            || type is IArrayTypeSymbol
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.IList1)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.ICollection1)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.ReadOnlyCollection1)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.IReadOnlyCollection1)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.IReadOnlyList1)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.ArraySegment1)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.ConcurrentBag1)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.ConcurrentQueue1)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.ConcurrentStack1)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.HashSet1)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.LinkedList1)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.List1)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.Queue1)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.SortedSet1)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.Stack1)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.ImmutableArray1)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.ImmutableHashSet1)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.ImmutableList1)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.ImmutableQueue1)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.ImmutableSortedSet1)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.ImmutableStack1))
            return _enumerableBasedNodeFactory(type, ParentRange, ParentFunction, _referenceGenerator)
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, implementationStack);

        if (type is ({ TypeKind: TypeKind.Interface } or { TypeKind: TypeKind.Class, IsAbstract: true })
            and INamedTypeSymbol interfaceOrAbstractType)
        {
            return SwitchInterface(interfaceOrAbstractType, implementationStack);
        }

        if (type is INamedTypeSymbol { TypeKind: TypeKind.Class or TypeKind.Struct } classOrStructType)
        {
            if (_checkTypeProperties.MapToSingleFittingImplementation(classOrStructType) is not { } chosenImplementationType)
            {
                if (classOrStructType.NullableAnnotation == NullableAnnotation.Annotated)
                {
                    _diagLogger.Log(Diagnostics.NullResolutionWarning(
                        $"Interface: Multiple or no implementations where a single is required for \"{classOrStructType.FullName()}\", but injecting null instead.",
                        ExecutionPhase.Resolution));
                    return _nullNodeFactory(classOrStructType, _referenceGenerator)
                        .EnqueueBuildJobTo(_parentContainer.BuildQueue, implementationStack);
                }
                return _errorNodeFactory($"Interface: Multiple or no implementations where a single is required for \"{classOrStructType.FullName()}\",")
                    .EnqueueBuildJobTo(_parentContainer.BuildQueue, implementationStack);
            }

            return SwitchImplementation(
                new(true, true),
                chosenImplementationType,
                implementationStack,
                Next);
        }

        return _errorNodeFactory("Couldn't process in resolution tree creation.")
            .EnqueueBuildJobTo(_parentContainer.BuildQueue, implementationStack);
    }
    
    protected static ITypeSymbol GetCollectionsItemType(ITypeSymbol type) => type is IArrayTypeSymbol arrayTypeSymbol
        ? arrayTypeSymbol.ElementType
        : type is INamedTypeSymbol { TypeArguments.Length: 1 } collectionType
            ? collectionType.TypeArguments.First()
            : throw new ArgumentException("Given type is not supported collection type");

    /// <summary>
    /// Meant as entry point for mappings where concrete implementation type is already chosen.
    /// </summary>
    public IElementNode MapToImplementation(
        ImplementationMappingConfiguration config,
        INamedTypeSymbol implementationType,
        ImmutableStack<INamedTypeSymbol> implementationStack) =>
        SwitchImplementation(
            config,
            implementationType, 
            implementationStack, 
            NextForWraps); // Use NextForWraps, cause MapToImplementation is entry point

    public IElementNode MapToOutParameter(ITypeSymbol type, ImmutableStack<INamedTypeSymbol> implementationStack) => 
        _outParameterNodeFactory(type, _referenceGenerator)
            .EnqueueBuildJobTo(_parentContainer.BuildQueue, implementationStack);
    public PassedDependencies MapperDependencies { get; }
    public void ResetFunction(ISingleFunctionNode parentFunction)
    {
        ParentFunction = parentFunction;
    }

    private IElementNode SwitchImplementation(
        ImplementationMappingConfiguration config,
        INamedTypeSymbol implementationType, 
        ImmutableStack<INamedTypeSymbol> implementationSet,
        IElementNodeMapperBase nextMapper)
    {
        if (config.CheckForScopeRoot)
        {
            var ret = _checkTypeProperties.ShouldBeScopeRoot(implementationType) switch
            {
                ScopeLevel.TransientScope => ParentRange.BuildTransientScopeCall(implementationType, ParentFunction),
                ScopeLevel.Scope => ParentRange.BuildScopeCall(implementationType, ParentFunction),
                _ => (IElementNode?) null
            };
            if (ret is not null)
                return ret;
        }
        
        if (config.CheckForRangedInstance)
        {
            var scopeLevel = _checkTypeProperties.GetScopeLevelFor(implementationType);

            /* todo replace? if (scopeLevel != ScopeLevel.None 
                && _scopedInstancesReferenceCache.TryGetValue(implementationType, out var scopedReference))
                return (scopedReference, null);*/

            var ret = scopeLevel switch
            {
                ScopeLevel.Container => ParentRange.BuildContainerInstanceCall(implementationType, ParentFunction),
                ScopeLevel.TransientScope => ParentRange.BuildTransientScopeInstanceCall(implementationType, ParentFunction),
                ScopeLevel.Scope => ParentRange.BuildScopeInstanceCall(implementationType, ParentFunction),
                _ => null
            };
            if (ret is not null)
                return ret;
        }

        if (_checkTypeProperties.GetConstructorChoiceFor(implementationType) is { } constructor)
            return _implementationNodeFactory(
                    implementationType, 
                    constructor, 
                    ParentFunction, 
                    ParentRange,
                    nextMapper, 
                    _checkTypeProperties, 
                    _userDefinedElements,
                    _referenceGenerator)
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, implementationSet);
            
        if (implementationType.NullableAnnotation != NullableAnnotation.Annotated)
            return _errorNodeFactory(implementationType.InstanceConstructors.Length switch
            {
                0 => $"Class.Constructor: No constructor found for implementation {implementationType.FullName()}",
                > 1 =>
                    $"Class.Constructor: More than one constructor found for implementation {implementationType.FullName()}",
                _ => $"Class.Constructor: {implementationType.InstanceConstructors[0].Name} is not a method symbol"
            }).EnqueueBuildJobTo(_parentContainer.BuildQueue, implementationSet);
            
        _diagLogger.Log(Diagnostics.NullResolutionWarning(
            $"Interface: Multiple or no implementations where a single is required for \"{implementationType.FullName()}\", but injecting null instead.",
            ExecutionPhase.Resolution));
        return _nullNodeFactory(implementationType, _referenceGenerator)
            .EnqueueBuildJobTo(_parentContainer.BuildQueue, implementationSet);
    }
    
    private IElementNode SwitchInterface(INamedTypeSymbol interfaceType, ImmutableStack<INamedTypeSymbol> implementationSet)
    {
        if (_checkTypeProperties.ShouldBeComposite(interfaceType)
            && _checkTypeProperties.GetCompositeFor(interfaceType) is {} compositeImplementationType)
            return SwitchInterfaceWithPotentialDecoration(interfaceType, compositeImplementationType, implementationSet, Next);
        if (_checkTypeProperties.MapToSingleFittingImplementation(interfaceType) is not { } impType)
        {
            if (interfaceType.NullableAnnotation == NullableAnnotation.Annotated)
            {
                _diagLogger.Log(Diagnostics.NullResolutionWarning(
                    $"Interface: Multiple or no implementations where a single is required for \"{interfaceType.FullName()}\", but injecting null instead.",
                    ExecutionPhase.Resolution));
                return _nullNodeFactory(interfaceType, _referenceGenerator)
                    .EnqueueBuildJobTo(_parentContainer.BuildQueue, implementationSet);
            }
            return _errorNodeFactory($"Interface: Multiple or no implementations where a single is required for \"{interfaceType.FullName()}\".")
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, implementationSet);
        }

        return SwitchInterfaceWithPotentialDecoration(interfaceType, impType, implementationSet, this);
    }

    protected IElementNode SwitchInterfaceWithPotentialDecoration(
        INamedTypeSymbol interfaceType,
        INamedTypeSymbol implementationType, 
        ImmutableStack<INamedTypeSymbol> implementationSet,
        IElementNodeMapperBase mapper) // todo mapper parameter needed?
    {
        var shouldBeDecorated = _checkTypeProperties.ShouldBeDecorated(interfaceType);
        if (!shouldBeDecorated)
            return _abstractionNodeFactory(interfaceType, implementationType, mapper, _referenceGenerator)
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, implementationSet);

        var interfaceTypeKey = interfaceType.ConstructTypeUniqueKey();
        var decoratorSequence = _checkTypeProperties.GetSequenceFor(interfaceType, implementationType)
            .Reverse()
            .Append(implementationType)
            .ToList();
        var outerDecorator = decoratorSequence[0];
        var decoratorTypes = ImmutableQueue.CreateRange(
            (decoratorSequence.Count > 1 
                ? decoratorSequence.Skip(1) // skip the outer decorator
                : decoratorSequence) 
            .Select(t => (interfaceTypeKey, t))
            .Append((interfaceTypeKey, implementationType)));
            
        var overridingMapper = _overridingElementNodeMapperFactory(this, MapperDependencies, decoratorTypes);
        return _abstractionNodeFactory(interfaceType, outerDecorator, overridingMapper, _referenceGenerator)
            .EnqueueBuildJobTo(_parentContainer.BuildQueue, implementationSet);
    }
}