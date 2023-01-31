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

namespace MrMeeseeks.DIE.Nodes.Mappers;

internal interface IElementNodeMapperBase
{
    IElementNode Map(ITypeSymbol type);
    IElementNode MapToImplementation(INamedTypeSymbol implementationType);
    IElementNode MapToOutParameter(ITypeSymbol type);
    ElementNodeMapperBase.PassedDependencies MapperDependencies { get; }
    void ResetFunction(ISingleFunctionNode parentFunction); // todo replace this workaround
}

internal abstract class ElementNodeMapperBase : IElementNodeMapperBase
{
    internal record PassedDependencies(
        ISingleFunctionNode ParentFunction,
        IRangeNode ParentRange,
        IContainerNode ParentContainer,
        IUserDefinedElements UserDefinedElements,
        ICheckTypeProperties CheckTypeProperties,
        IReferenceGenerator ReferenceGenerator);
    
    protected ISingleFunctionNode ParentFunction; // todo make readonly again
    protected readonly IRangeNode ParentRange;
    private readonly IContainerNode _parentContainer;
    private readonly IUserDefinedElements _userDefinedElements;
    private readonly ICheckTypeProperties _checkTypeProperties;
    private readonly IReferenceGenerator _referenceGenerator;
    private readonly IDiagLogger _diagLogger;
    protected readonly WellKnownTypes WellKnownTypes;
    private readonly Func<IFieldSymbol, IFunctionNode, IReferenceGenerator, IFactoryFieldNode> _factoryFieldNodeFactory;
    private readonly Func<IPropertySymbol, IFunctionNode, IReferenceGenerator, IFactoryPropertyNode> _factoryPropertyNodeFactory;
    private readonly Func<IMethodSymbol, IFunctionNode, IElementNodeMapperBase, IReferenceGenerator, IFactoryFunctionNode> _factoryFunctionNodeFactory;
    private readonly Func<INamedTypeSymbol, IFunctionNode, IElementNodeMapperBase, IReferenceGenerator, IValueTaskNode> _valueTaskNodeFactory;
    private readonly Func<INamedTypeSymbol, IFunctionNode, IElementNodeMapperBase, IReferenceGenerator, ITaskNode> _taskNodeFactory;
    private readonly Func<INamedTypeSymbol, IElementNodeMapperBase, IReferenceGenerator, IValueTupleNode> _valueTupleNodeFactory;
    private readonly Func<INamedTypeSymbol, IElementNodeMapperBase, IReferenceGenerator, IValueTupleSyntaxNode> _valueTupleSyntaxNodeFactory;
    private readonly Func<INamedTypeSymbol, IElementNodeMapperBase, IReferenceGenerator, ITupleNode> _tupleNodeFactory;
    private readonly Func<INamedTypeSymbol, ILocalFunctionNode, IReferenceGenerator, ILazyNode> _lazyNodeFactory;
    private readonly Func<INamedTypeSymbol, ILocalFunctionNode, IReferenceGenerator, IFuncNode> _funcNodeFactory;
    private readonly Func<ITypeSymbol, IReadOnlyList<IElementNode>, IReferenceGenerator, ICollectionNode> _collectionNodeFactory;
    private readonly Func<INamedTypeSymbol, IElementNode, IReferenceGenerator, IAbstractionNode> _abstractionNodeFactory;
    private readonly Func<INamedTypeSymbol, IMethodSymbol, IFunctionNode, IRangeNode, IElementNodeMapperBase, ICheckTypeProperties, IUserDefinedElements, IReferenceGenerator, IImplementationNode> _implementationNodeFactory;
    private readonly Func<ITypeSymbol, IReferenceGenerator, IOutParameterNode> _outParameterNodeFactory;
    private readonly Func<string, IErrorNode> _errorNodeFactory;
    private readonly Func<ITypeSymbol, IReferenceGenerator, INullNode> _nullNodeFactory;
    private readonly Func<ITypeSymbol, IReadOnlyList<ITypeSymbol>, ImmutableSortedDictionary<TypeKey, (ITypeSymbol, IParameterNode)>, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IElementNodeMapperBase, IReferenceGenerator, ILocalFunctionNode> _localFunctionNodeFactory;
    private readonly Func<IElementNodeMapperBase, PassedDependencies, (TypeKey, IElementNode), IOverridingElementNodeMapper> _overridingElementNodeMapperFactory;
    private readonly Func<IElementNodeMapperBase, PassedDependencies, (TypeKey, IReadOnlyList<IElementNode>), IOverridingElementNodeMapperComposite> _overridingElementNodeMapperCompositeFactory;
    private readonly Func<IElementNodeMapperBase, PassedDependencies, INonWrapToCreateElementNodeMapper> _nonWrapToCreateElementNodeMapperFactory;

    internal ElementNodeMapperBase(
        ISingleFunctionNode parentFunction,
        IRangeNode parentRange,
        IContainerNode parentContainer,
        IUserDefinedElements userDefinedElements,
        ICheckTypeProperties checkTypeProperties,
        IReferenceGenerator referenceGenerator,
        
        IDiagLogger diagLogger,
        WellKnownTypes wellKnownTypes,
        Func<IFieldSymbol, IFunctionNode, IReferenceGenerator, IFactoryFieldNode> factoryFieldNodeFactory,
        Func<IPropertySymbol, IFunctionNode, IReferenceGenerator, IFactoryPropertyNode> factoryPropertyNodeFactory,
        Func<IMethodSymbol, IFunctionNode, IElementNodeMapperBase, IReferenceGenerator, IFactoryFunctionNode> factoryFunctionNodeFactory,
        Func<INamedTypeSymbol, IFunctionNode, IElementNodeMapperBase, IReferenceGenerator, IValueTaskNode> valueTaskNodeFactory,
        Func<INamedTypeSymbol, IFunctionNode, IElementNodeMapperBase, IReferenceGenerator, ITaskNode> taskNodeFactory,
        Func<INamedTypeSymbol, IElementNodeMapperBase, IReferenceGenerator, IValueTupleNode> valueTupleNodeFactory,
        Func<INamedTypeSymbol, IElementNodeMapperBase, IReferenceGenerator, IValueTupleSyntaxNode> valueTupleSyntaxNodeFactory,
        Func<INamedTypeSymbol, IElementNodeMapperBase, IReferenceGenerator, ITupleNode> tupleNodeFactory,
        Func<INamedTypeSymbol, ILocalFunctionNode, IReferenceGenerator, ILazyNode> lazyNodeFactory,
        Func<INamedTypeSymbol, ILocalFunctionNode, IReferenceGenerator, IFuncNode> funcNodeFactory,
        Func<ITypeSymbol, IReadOnlyList<IElementNode>, IReferenceGenerator, ICollectionNode> collectionNodeFactory,
        Func<INamedTypeSymbol, IElementNode, IReferenceGenerator, IAbstractionNode> abstractionNodeFactory,
        Func<INamedTypeSymbol, IMethodSymbol, IFunctionNode, IRangeNode, IElementNodeMapperBase, ICheckTypeProperties, IUserDefinedElements, IReferenceGenerator, IImplementationNode> implementationNodeFactory,
        Func<ITypeSymbol, IReferenceGenerator, IOutParameterNode> outParameterNodeFactory,
        Func<string, IErrorNode> errorNodeFactory,
        Func<ITypeSymbol, IReferenceGenerator, INullNode> nullNodeFactory,
        Func<ITypeSymbol, IReadOnlyList<ITypeSymbol>, ImmutableSortedDictionary<TypeKey, (ITypeSymbol, IParameterNode)>, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IElementNodeMapperBase, IReferenceGenerator, ILocalFunctionNode> localFunctionNodeFactory,
        Func<IElementNodeMapperBase, PassedDependencies, (TypeKey, IElementNode), IOverridingElementNodeMapper> overridingElementNodeMapperFactory,
        Func<IElementNodeMapperBase, PassedDependencies, (TypeKey, IReadOnlyList<IElementNode>), IOverridingElementNodeMapperComposite> overridingElementNodeMapperCompositeFactory,
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
        _collectionNodeFactory = collectionNodeFactory;
        _abstractionNodeFactory = abstractionNodeFactory;
        _implementationNodeFactory = implementationNodeFactory;
        _outParameterNodeFactory = outParameterNodeFactory;
        _errorNodeFactory = errorNodeFactory;
        _nullNodeFactory = nullNodeFactory;
        _localFunctionNodeFactory = localFunctionNodeFactory;
        _overridingElementNodeMapperFactory = overridingElementNodeMapperFactory;
        _overridingElementNodeMapperCompositeFactory = overridingElementNodeMapperCompositeFactory;
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
        SymbolEqualityComparer.Default.Equals(potentialCollectionType.OriginalDefinition, WellKnownTypes.Enumerable1)
        || SymbolEqualityComparer.Default.Equals(potentialCollectionType.OriginalDefinition, WellKnownTypes.ReadOnlyCollection1)
        || SymbolEqualityComparer.Default.Equals(potentialCollectionType.OriginalDefinition, WellKnownTypes.ReadOnlyList1)
        || potentialCollectionType is IArrayTypeSymbol;

    public virtual IElementNode Map(ITypeSymbol type)
    {
        if (ParentFunction.Overrides.TryGetValue(type.ToTypeKey(), out var tuple))
            return tuple.Item2;

        if (_userDefinedElements.GetFactoryFieldFor(type) is { } instance)
            return _factoryFieldNodeFactory(instance, ParentFunction, _referenceGenerator).EnqueueTo(_parentContainer.BuildQueue);

        if (_userDefinedElements.GetFactoryPropertyFor(type) is { } property)
            return _factoryPropertyNodeFactory(property, ParentFunction, _referenceGenerator).EnqueueTo(_parentContainer.BuildQueue);

        if (_userDefinedElements.GetFactoryMethodFor(type) is { } method)
            return _factoryFunctionNodeFactory(method, ParentFunction, Next, _referenceGenerator).EnqueueTo(_parentContainer.BuildQueue);

        if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, WellKnownTypes.ValueTask1)
            && type is INamedTypeSymbol valueTask)
            return _valueTaskNodeFactory(valueTask, ParentFunction, NextForWraps, _referenceGenerator).EnqueueTo(_parentContainer.BuildQueue);

        if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, WellKnownTypes.Task1)
            && type is INamedTypeSymbol task)
            return _taskNodeFactory(task, ParentFunction, NextForWraps, _referenceGenerator).EnqueueTo(_parentContainer.BuildQueue);

        if (type.FullName().StartsWith("global::System.ValueTuple<") && type is INamedTypeSymbol valueTupleType)
            return _valueTupleNodeFactory(valueTupleType, NextForWraps, _referenceGenerator).EnqueueTo(_parentContainer.BuildQueue);

        if (type.FullName().StartsWith("(") && type.FullName().EndsWith(")") && type is INamedTypeSymbol syntaxValueTupleType)
            return _valueTupleSyntaxNodeFactory(syntaxValueTupleType, NextForWraps, _referenceGenerator).EnqueueTo(_parentContainer.BuildQueue);

        if (type.FullName().StartsWith("global::System.Tuple<") && type is INamedTypeSymbol tupleType)
            return _tupleNodeFactory(tupleType, NextForWraps, _referenceGenerator).EnqueueTo(_parentContainer.BuildQueue);

        if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, WellKnownTypes.Lazy1)
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
                _referenceGenerator).EnqueueTo(_parentContainer.BuildQueue);
            ParentFunction.AddLocalFunction(function);
            
            mapper.ResetFunction(function);
            
            return _lazyNodeFactory(lazyType, function, _referenceGenerator).EnqueueTo(_parentContainer.BuildQueue);
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
                _referenceGenerator).EnqueueTo(_parentContainer.BuildQueue);
            ParentFunction.AddLocalFunction(function);
            
            mapper.ResetFunction(function);
            
            return _funcNodeFactory(funcType, function, _referenceGenerator).EnqueueTo(_parentContainer.BuildQueue);
        }

        if (IsCollectionType(type))
            return MapToCollection(type);

        if (type is ({ TypeKind: TypeKind.Interface } or { TypeKind: TypeKind.Class, IsAbstract: true })
            and INamedTypeSymbol interfaceOrAbstractType)
        {
            return SwitchInterface(interfaceOrAbstractType);
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
                    return _nullNodeFactory(classOrStructType, _referenceGenerator).EnqueueTo(_parentContainer.BuildQueue);
                }
                return _errorNodeFactory($"Interface: Multiple or no implementations where a single is required for \"{classOrStructType.FullName()}\",").EnqueueTo(_parentContainer.BuildQueue);
            }
            
            var ret = _checkTypeProperties.ShouldBeScopeRoot(chosenImplementationType) switch
            {
                ScopeLevel.TransientScope => ParentRange.BuildTransientScopeCall(chosenImplementationType, ParentFunction),
                ScopeLevel.Scope => ParentRange.BuildScopeCall(chosenImplementationType, ParentFunction),
                _ => SwitchImplementation(chosenImplementationType)
            };

            return ret;
        }

        return _errorNodeFactory("Couldn't process in resolution tree creation.").EnqueueTo(_parentContainer.BuildQueue);
    }
    
    protected static ITypeSymbol GetCollectionsItemType(ITypeSymbol type) => type is IArrayTypeSymbol arrayTypeSymbol
        ? arrayTypeSymbol.ElementType
        : type is INamedTypeSymbol { TypeArguments.Length: 1 } collectionType
            ? collectionType.TypeArguments.First()
            : throw new ArgumentException("Given type is not supported collection type");

    private IElementNode MapToCollection(ITypeSymbol collectionType)
    {
        var outerItemType = GetCollectionsItemType(collectionType);
        var unwrappedItemType = TypeSymbolUtility.GetUnwrappedType(outerItemType, WellKnownTypes);

        if (unwrappedItemType is not INamedTypeSymbol innerItemType)
            return _errorNodeFactory("Inner collection type has to be a named type.").EnqueueTo(_parentContainer.BuildQueue);

        var elementNodes = _checkTypeProperties.MapToImplementations(innerItemType)
            .Select(i => _overridingElementNodeMapperFactory(this, MapperDependencies, (innerItemType.ConstructTypeUniqueKey(), MapToImplementation(i))).Map(outerItemType))
            .ToList();
        return _collectionNodeFactory(collectionType, elementNodes, _referenceGenerator).EnqueueTo(_parentContainer.BuildQueue);
    }

    /// <summary>
    /// Meant as entry point for mappings where concrete implementation type is already chosen.
    /// </summary>
    /// <returns></returns>
    public IElementNode MapToImplementation(INamedTypeSymbol implementationType)
    {
        if (_checkTypeProperties.GetConstructorChoiceFor(implementationType) is { } constructor)
            return _implementationNodeFactory(
                implementationType, 
                constructor,
                ParentFunction,
                ParentRange,
                NextForWraps, // Use the wrap variant, because "MapToImplementation" is entry point
                _checkTypeProperties,
                _userDefinedElements,
                _referenceGenerator)
                .EnqueueTo(_parentContainer.BuildQueue);
            
        if (implementationType.NullableAnnotation != NullableAnnotation.Annotated)
            return _errorNodeFactory(implementationType.InstanceConstructors.Length switch
            {
                0 => $"Class.Constructor: No constructor found for implementation {implementationType.FullName()}",
                > 1 =>
                    $"Class.Constructor: More than one constructor found for implementation {implementationType.FullName()}",
                _ => $"Class.Constructor: {implementationType.InstanceConstructors[0].Name} is not a method symbol"
            }).EnqueueTo(_parentContainer.BuildQueue);
            
        _diagLogger.Log(Diagnostics.NullResolutionWarning(
            $"Interface: Multiple or no implementations where a single is required for \"{implementationType.FullName()}\", but injecting null instead.",
            ExecutionPhase.Resolution));
        return _nullNodeFactory(implementationType, _referenceGenerator).EnqueueTo(_parentContainer.BuildQueue);
    }

    public IElementNode MapToOutParameter(ITypeSymbol type) => _outParameterNodeFactory(type, _referenceGenerator).EnqueueTo(_parentContainer.BuildQueue);
    public PassedDependencies MapperDependencies { get; }
    public void ResetFunction(ISingleFunctionNode parentFunction)
    {
        ParentFunction = parentFunction;
    }

    protected IElementNode SwitchImplementation(INamedTypeSymbol implementationType)
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
            _ => SwitchClass(implementationType)
        };

        return ret;

        IElementNode SwitchClass(INamedTypeSymbol impType)
        {
            if (_checkTypeProperties.GetConstructorChoiceFor(impType) is { } constructor)
                return _implementationNodeFactory(
                    impType, 
                    constructor, 
                    ParentFunction, 
                    ParentRange,
                    Next, 
                    _checkTypeProperties, 
                    _userDefinedElements,
                    _referenceGenerator).EnqueueTo(_parentContainer.BuildQueue);
            
            if (impType.NullableAnnotation != NullableAnnotation.Annotated)
                return _errorNodeFactory(impType.InstanceConstructors.Length switch
                {
                    0 => $"Class.Constructor: No constructor found for implementation {impType.FullName()}",
                    > 1 =>
                        $"Class.Constructor: More than one constructor found for implementation {impType.FullName()}",
                    _ => $"Class.Constructor: {impType.InstanceConstructors[0].Name} is not a method symbol"
                }).EnqueueTo(_parentContainer.BuildQueue);
            
            _diagLogger.Log(Diagnostics.NullResolutionWarning(
                $"Interface: Multiple or no implementations where a single is required for \"{impType.FullName()}\", but injecting null instead.",
                ExecutionPhase.Resolution));
            return _nullNodeFactory(impType, _referenceGenerator).EnqueueTo(_parentContainer.BuildQueue);
        }
    }
    
    

    private IElementNode SwitchInterface(INamedTypeSymbol interfaceType)
    {
        if (_checkTypeProperties.ShouldBeComposite(interfaceType))
        {
            var implementations = _checkTypeProperties.MapToImplementations(interfaceType);
            var compositeImplementationType = _checkTypeProperties.GetCompositeFor(interfaceType)
                ?? throw new ImpossibleDieException(new Guid("73D630AF-CAE0-4869-A55B-8F54193E6274"));
            var abstractionNodes = implementations.Select(i => MapToInterfaceForImplementation(i, this)).ToList();
            var compositeMapper = _overridingElementNodeMapperCompositeFactory(this, MapperDependencies, (interfaceType.ConstructTypeUniqueKey(), abstractionNodes));
            return MapToInterfaceForImplementation(compositeImplementationType, compositeMapper);
        }
        if (_checkTypeProperties.MapToSingleFittingImplementation(interfaceType) is not { } impType)
        {
            if (interfaceType.NullableAnnotation == NullableAnnotation.Annotated)
            {
                _diagLogger.Log(Diagnostics.NullResolutionWarning(
                    $"Interface: Multiple or no implementations where a single is required for \"{interfaceType.FullName()}\", but injecting null instead.",
                    ExecutionPhase.Resolution));
                return _nullNodeFactory(interfaceType, _referenceGenerator).EnqueueTo(_parentContainer.BuildQueue);
            }
            return _errorNodeFactory($"Interface: Multiple or no implementations where a single is required for \"{interfaceType.FullName()}\".").EnqueueTo(_parentContainer.BuildQueue);
        }

        return MapToInterfaceForImplementation(impType, this);

        IElementNode MapToInterfaceForImplementation(INamedTypeSymbol implementationType, IElementNodeMapperBase mapper)
        {
            var shouldBeDecorated = _checkTypeProperties.ShouldBeDecorated(interfaceType);
        
            var currentAbstractionNode = _abstractionNodeFactory(interfaceType, mapper.MapToImplementation(implementationType), _referenceGenerator).EnqueueTo(_parentContainer.BuildQueue);

            if (!shouldBeDecorated) return currentAbstractionNode;
            
            var decoratorTypes = new Queue<INamedTypeSymbol>(_checkTypeProperties.GetSequenceFor(interfaceType, implementationType));
            while (decoratorTypes.Any())
            {
                var decoratorType = decoratorTypes.Dequeue();
                var overridingElementNodeMapperFactory = _overridingElementNodeMapperFactory(this, MapperDependencies, (interfaceType.ConstructTypeUniqueKey(), currentAbstractionNode));
                currentAbstractionNode = _abstractionNodeFactory(
                        interfaceType, 
                        overridingElementNodeMapperFactory.MapToImplementation(decoratorType),
                        _referenceGenerator)
                    .EnqueueTo(_parentContainer.BuildQueue);
            }

            return currentAbstractionNode;
        }
    }
}