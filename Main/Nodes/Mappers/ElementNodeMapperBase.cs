using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Logging;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.Delegates;
using MrMeeseeks.DIE.Nodes.Elements.Factories;
using MrMeeseeks.DIE.Nodes.Elements.Tuples;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Nodes.Roots;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Mappers;

internal interface IElementNodeMapperBase
{
    IElementNode Map(ITypeSymbol type, PassedContext passedContext);
    IElementNode MapToImplementation(
        ImplementationMappingConfiguration config,
        INamedTypeSymbol? abstractionType,
        INamedTypeSymbol implementationType,
        PassedContext passedContext);
    IElementNode MapToOutParameter(ITypeSymbol type, PassedContext passedContext);
}

internal sealed record ImplementationMappingConfiguration(
    bool CheckForScopeRoot,
    bool CheckForRangedInstance,
    bool CheckForInitializedInstance);

internal abstract class ElementNodeMapperBase : IElementNodeMapperBase
{
    protected readonly IFunctionNode ParentFunction;
    protected readonly IRangeNode ParentRange;
    private readonly IContainerNode _parentContainer;
    private readonly ILocalDiagLogger _localDiagLogger;
    private readonly ITypeParameterUtility _typeParameterUtility;
    private readonly ICheckIterableTypes _checkIterableTypes;
    private readonly IUserDefinedElements _userDefinedElements;
    private readonly ICheckTypeProperties _checkTypeProperties;
    protected readonly WellKnownTypes WellKnownTypes;
    private readonly Func<IFieldSymbol, IFactoryFieldNode> _factoryFieldNodeFactory;
    private readonly Func<IPropertySymbol, IFactoryPropertyNode> _factoryPropertyNodeFactory;
    private readonly Func<IMethodSymbol, IElementNodeMapperBase, IFactoryFunctionNode> _factoryFunctionNodeFactory;
    private readonly Func<INamedTypeSymbol, IElementNodeMapperBase, IValueTupleNode> _valueTupleNodeFactory;
    private readonly Func<INamedTypeSymbol, IElementNodeMapperBase, IValueTupleSyntaxNode> _valueTupleSyntaxNodeFactory;
    private readonly Func<INamedTypeSymbol, IElementNodeMapperBase, ITupleNode> _tupleNodeFactory;
    private readonly Func<(INamedTypeSymbol Outer, INamedTypeSymbol Inner), ILocalFunctionNode, IReadOnlyList<ITypeSymbol>, ILazyNode> _lazyNodeFactory;
    private readonly Func<(INamedTypeSymbol Outer, INamedTypeSymbol Inner), ILocalFunctionNode, IReadOnlyList<ITypeSymbol>, IThreadLocalNode> _threadLocalNodeFactory;
    private readonly Func<(INamedTypeSymbol Outer, INamedTypeSymbol Inner), ILocalFunctionNode, IReadOnlyList<ITypeSymbol>, IFuncNode> _funcNodeFactory;
    private readonly Func<ITypeSymbol, IEnumerableBasedNode> _enumerableBasedNodeFactory;
    private readonly Func<INamedTypeSymbol, IKeyValueBasedNode> _keyValueBasedNodeFactory;
    private readonly Func<INamedTypeSymbol?, INamedTypeSymbol, IMethodSymbol, IElementNodeMapperBase, IImplementationNode> _implementationNodeFactory;
    private readonly Func<ITypeSymbol, IOutParameterNode> _outParameterNodeFactory;
    private readonly Func<string, ITypeSymbol, IErrorNode> _errorNodeFactory;
    private readonly Func<ITypeSymbol, INullNode> _nullNodeFactory;
    private readonly Func<IElementNode, IReusedNode> _reusedNodeFactory;
    private readonly Func<ITypeSymbol, IReadOnlyList<ITypeSymbol>, ImmutableDictionary<ITypeSymbol, IParameterNode>, ILocalFunctionNodeRoot> _localFunctionNodeFactory;
    private readonly Func<IElementNodeMapperBase, ImmutableQueue<(INamedTypeSymbol, INamedTypeSymbol)>, IOverridingElementNodeMapper> _overridingElementNodeMapperFactory;
    
    internal ElementNodeMapperBase(
        IFunctionNode parentFunction,
        IRangeNode parentRange,
        IContainerNode parentContainer,
        ITransientScopeWideContext transientScopeWideContext,
        ILocalDiagLogger localDiagLogger,
        ITypeParameterUtility typeParameterUtility,
        IContainerWideContext containerWideContext,
        ICheckIterableTypes checkIterableTypes,
        Func<IFieldSymbol, IFactoryFieldNode> factoryFieldNodeFactory,
        Func<IPropertySymbol, IFactoryPropertyNode> factoryPropertyNodeFactory,
        Func<IMethodSymbol, IElementNodeMapperBase, IFactoryFunctionNode> factoryFunctionNodeFactory,
        Func<INamedTypeSymbol, IElementNodeMapperBase, IValueTupleNode> valueTupleNodeFactory,
        Func<INamedTypeSymbol, IElementNodeMapperBase, IValueTupleSyntaxNode> valueTupleSyntaxNodeFactory,
        Func<INamedTypeSymbol, IElementNodeMapperBase, ITupleNode> tupleNodeFactory,
        Func<(INamedTypeSymbol Outer, INamedTypeSymbol Inner), ILocalFunctionNode, IReadOnlyList<ITypeSymbol>, ILazyNode> lazyNodeFactory,
        Func<(INamedTypeSymbol Outer, INamedTypeSymbol Inner), ILocalFunctionNode, IReadOnlyList<ITypeSymbol>, IThreadLocalNode> threadLocalNodeFactory,
        Func<(INamedTypeSymbol Outer, INamedTypeSymbol Inner), ILocalFunctionNode, IReadOnlyList<ITypeSymbol>, IFuncNode> funcNodeFactory,
        Func<ITypeSymbol, IEnumerableBasedNode> enumerableBasedNodeFactory,
        Func<INamedTypeSymbol, IKeyValueBasedNode> keyValueBasedNodeFactory,
        Func<INamedTypeSymbol?, INamedTypeSymbol, IMethodSymbol, IElementNodeMapperBase, IImplementationNode> implementationNodeFactory,
        Func<ITypeSymbol, IOutParameterNode> outParameterNodeFactory,
        Func<string, ITypeSymbol, IErrorNode> errorNodeFactory,
        Func<ITypeSymbol, INullNode> nullNodeFactory,
        Func<IElementNode, IReusedNode> reusedNodeFactory,
        Func<ITypeSymbol, IReadOnlyList<ITypeSymbol>, ImmutableDictionary<ITypeSymbol, IParameterNode>, ILocalFunctionNodeRoot> localFunctionNodeFactory,
        Func<IElementNodeMapperBase, ImmutableQueue<(INamedTypeSymbol, INamedTypeSymbol)>, IOverridingElementNodeMapper> overridingElementNodeMapperFactory)
    {
        ParentFunction = parentFunction;
        ParentRange = parentRange;
        _parentContainer = parentContainer;
        _localDiagLogger = localDiagLogger;
        _typeParameterUtility = typeParameterUtility;
        _checkIterableTypes = checkIterableTypes;
        _userDefinedElements = transientScopeWideContext.UserDefinedElements;
        _checkTypeProperties = transientScopeWideContext.CheckTypeProperties;
        WellKnownTypes = containerWideContext.WellKnownTypes;
        _factoryFieldNodeFactory = factoryFieldNodeFactory;
        _factoryPropertyNodeFactory = factoryPropertyNodeFactory;
        _factoryFunctionNodeFactory = factoryFunctionNodeFactory;
        _valueTupleNodeFactory = valueTupleNodeFactory;
        _valueTupleSyntaxNodeFactory = valueTupleSyntaxNodeFactory;
        _tupleNodeFactory = tupleNodeFactory;
        _lazyNodeFactory = lazyNodeFactory;
        _threadLocalNodeFactory = threadLocalNodeFactory;
        _funcNodeFactory = funcNodeFactory;
        _enumerableBasedNodeFactory = enumerableBasedNodeFactory;
        _keyValueBasedNodeFactory = keyValueBasedNodeFactory;
        _implementationNodeFactory = implementationNodeFactory;
        _outParameterNodeFactory = outParameterNodeFactory;
        _errorNodeFactory = errorNodeFactory;
        _nullNodeFactory = nullNodeFactory;
        _reusedNodeFactory = reusedNodeFactory;
        _localFunctionNodeFactory = localFunctionNodeFactory;
        _overridingElementNodeMapperFactory = overridingElementNodeMapperFactory;
    }
    
    protected abstract IElementNodeMapperBase NextForWraps { get; }

    protected abstract IElementNodeMapperBase Next { get; }

    protected virtual MapperData GetMapperDataForAsyncWrapping() => 
        new VanillaMapperData();

    public virtual IElementNode Map(ITypeSymbol type, PassedContext passedContext)
    {
        if (ParentFunction.Overrides.TryGetValue(type, out var tuple))
            return tuple;

        if (_userDefinedElements.GetFactoryFieldFor(type) is { } instance)
            return _factoryFieldNodeFactory(instance)
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, passedContext);

        if (_userDefinedElements.GetFactoryPropertyFor(type) is { } property)
            return _factoryPropertyNodeFactory(property)
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, passedContext);

        if (_userDefinedElements.GetFactoryMethodFor(type) is { } method)
            return _factoryFunctionNodeFactory(method, Next)
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, passedContext);

        if (CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, WellKnownTypes.ValueTask1)
            && type is INamedTypeSymbol valueTask)
            return ParentRange.BuildAsyncCreateCall(GetMapperDataForAsyncWrapping(), valueTask.TypeArguments[0], SynchronicityDecision.AsyncValueTask, ParentFunction);

        if (CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, WellKnownTypes.Task1)
            && type is INamedTypeSymbol task)
            return ParentRange.BuildAsyncCreateCall(GetMapperDataForAsyncWrapping(), task.TypeArguments[0], SynchronicityDecision.AsyncTask, ParentFunction);

        if (type.FullName().StartsWith("global::System.ValueTuple<", StringComparison.Ordinal) && type is INamedTypeSymbol valueTupleType)
            return _valueTupleNodeFactory(valueTupleType, NextForWraps)
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, passedContext);
        
        if (type.FullName().StartsWith("(", StringComparison.Ordinal) && type.FullName().EndsWith(")", StringComparison.Ordinal) && type is INamedTypeSymbol syntaxValueTupleType)
            return _valueTupleSyntaxNodeFactory(syntaxValueTupleType, NextForWraps)
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, passedContext);

        if (type.FullName().StartsWith("global::System.Tuple<", StringComparison.Ordinal) && type is INamedTypeSymbol tupleType)
            return _tupleNodeFactory(tupleType, NextForWraps)
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, passedContext);

        if (CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, WellKnownTypes.Lazy1)
            && type is INamedTypeSymbol lazyType)
        {
            return CreateDelegateNode(
                lazyType, 
                lazyType.TypeArguments.SingleOrDefault(), 
                Array.Empty<ITypeSymbol>(), 
                _lazyNodeFactory, 
                "Lazy");
        }

        if (CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, WellKnownTypes.ThreadLocal1)
            && type is INamedTypeSymbol threadLocalType)
        {
            return CreateDelegateNode(
                threadLocalType, 
                threadLocalType.TypeArguments.SingleOrDefault(), 
                Array.Empty<ITypeSymbol>(), 
                _threadLocalNodeFactory, 
                "ThreadLocal");
        }

        if (type.TypeKind == TypeKind.Delegate 
            && type.FullName().StartsWith("global::System.Func<", StringComparison.Ordinal)
            && type is INamedTypeSymbol funcType)
        {
            return CreateDelegateNode(
                funcType, 
                funcType.TypeArguments.LastOrDefault(), 
                funcType.TypeArguments.Take(funcType.TypeArguments.Length - 1).ToArray(), 
                _funcNodeFactory, 
                "Func");
        }

        if (_checkIterableTypes.IsMapType(type) && type is INamedTypeSymbol mapType)
        {
            return _keyValueBasedNodeFactory(mapType)
                    .EnqueueBuildJobTo(_parentContainer.BuildQueue, passedContext);
        }

        if (_checkIterableTypes.IsCollectionType(type))
            return _enumerableBasedNodeFactory(type)
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, passedContext);

        if (type is ({ TypeKind: TypeKind.Interface } or { TypeKind: TypeKind.Class, IsAbstract: true })
            and INamedTypeSymbol interfaceOrAbstractType)
        {
            return SwitchInterface(interfaceOrAbstractType, passedContext);
        }

        if (type is INamedTypeSymbol { TypeKind: TypeKind.Class or TypeKind.Struct } classOrStructType)
        {
            var implementationType = classOrStructType;
            var isNullableStruct = classOrStructType.TypeArguments.Length == 1
                                   && classOrStructType.TypeArguments.Single() is INamedTypeSymbol maybeInnerStruct
                                   && CustomSymbolEqualityComparer.Default.Equals(classOrStructType,
                                       WellKnownTypes.Nullable1.Construct(maybeInnerStruct));
            if (isNullableStruct && classOrStructType.TypeArguments.Single() is INamedTypeSymbol innerType)
            {
                // Take inner type of Nullable<T>
                implementationType = innerType;
            }
            
            if (_checkTypeProperties.MapToSingleFittingImplementation(implementationType, passedContext.InjectionKeyModification) is not { } chosenImplementationType)
            {
                if (classOrStructType.NullableAnnotation == NullableAnnotation.Annotated || isNullableStruct)
                {
                    _localDiagLogger.Warning(WarningLogData.NullResolutionWarning(
                        $"Class: Multiple or no implementations where a single is required for \"{classOrStructType.FullName()}\", but injecting null instead."),
                        Location.None);
                    return _nullNodeFactory(classOrStructType)
                        .EnqueueBuildJobTo(_parentContainer.BuildQueue, passedContext);
                }
                return _errorNodeFactory(
                        $"Class: Multiple or no implementations where a single is required for \"{classOrStructType.FullName()}\",",
                        classOrStructType)
                    .EnqueueBuildJobTo(_parentContainer.BuildQueue, passedContext);
            }

            return SwitchImplementation(
                new(true, true, true),
                null,
                chosenImplementationType,
                passedContext,
                Next);
        }

        return _errorNodeFactory(
                "Couldn't process in resolution tree creation.",
                type)
            .EnqueueBuildJobTo(_parentContainer.BuildQueue, passedContext);

        IElementNode CreateDelegateNode<TElementNode>(
            INamedTypeSymbol delegateType, 
            ITypeSymbol? returnType, 
            IReadOnlyList<ITypeSymbol> lambdaParameters,
            Func<(INamedTypeSymbol Outer, INamedTypeSymbol Inner), ILocalFunctionNode, IReadOnlyList<ITypeSymbol>, TElementNode> factory,
            string logLabel)
            where TElementNode : IElementNode
        {
            if (returnType is null)
            {
                return _errorNodeFactory(
                        $"{logLabel}: {delegateType.TypeArguments.Last().FullName()} is not a type symbol",
                        type)
                    .EnqueueBuildJobTo(_parentContainer.BuildQueue, passedContext);
            }

            var returnTypeForFunction = _typeParameterUtility.ReplaceTypeParametersByCustom(returnType);
            var function = _localFunctionNodeFactory(
                    returnTypeForFunction,
                    lambdaParameters,
                    ParentFunction.Overrides)
                .Function
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, passedContext);
            ParentFunction.AddLocalFunction(function);

            var delegateTypeTypeArguments = delegateType.TypeArguments.ToArray();
            delegateTypeTypeArguments[delegateTypeTypeArguments.Length - 1] = returnTypeForFunction;
            var preparedDelegateType = delegateType.OriginalDefinition.Construct(delegateTypeTypeArguments);
            
            return factory((Outer: delegateType, Inner: preparedDelegateType), function, _typeParameterUtility.ExtractTypeParameters(returnType))
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, passedContext);
        }
    }

    /// <summary>
    /// Meant as entry point for mappings where concrete implementation type is already chosen.
    /// </summary>
    public IElementNode MapToImplementation(ImplementationMappingConfiguration config,
        INamedTypeSymbol? abstractionType,
        INamedTypeSymbol implementationType,
        PassedContext passedContext) =>
        SwitchImplementation(
            config,
            abstractionType,
            implementationType,
            passedContext,
            // Use NextForWraps, cause MapToImplementation is entry point
            NextForWraps);

    public IElementNode MapToOutParameter(ITypeSymbol type, PassedContext passedContext) => 
        _outParameterNodeFactory(type)
            .EnqueueBuildJobTo(_parentContainer.BuildQueue, passedContext);

    protected IElementNode SwitchImplementation(
        ImplementationMappingConfiguration config,
        INamedTypeSymbol? abstractionType,
        INamedTypeSymbol implementationType, 
        PassedContext passedContext,
        IElementNodeMapperBase nextMapper)
    {
        if (config.CheckForInitializedInstance && !ParentFunction.CheckIfReturnedType(implementationType))
        {
            if (ParentRange.GetInitializedNode(implementationType) is { } initializedInstanceNode)
            {
                ParentFunction.RegisterUsedInitializedInstance(initializedInstanceNode);
                return initializedInstanceNode;
            }
        }
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
            
            if (ParentFunction.TryGetReusedNode(implementationType, out var rn) && rn is not null)
                return rn;

            var ret = scopeLevel switch
            {
                ScopeLevel.Container => ParentRange.BuildContainerInstanceCall(implementationType, ParentFunction),
                ScopeLevel.TransientScope => ParentRange.BuildTransientScopeInstanceCall(implementationType, ParentFunction),
                ScopeLevel.Scope => ParentRange.BuildScopeInstanceCall(implementationType, ParentFunction),
                _ => null
            };
            if (ret is not null)
            {
                var reusedNode = _reusedNodeFactory(ret)
                    .EnqueueBuildJobTo(_parentContainer.BuildQueue, passedContext);
                ParentFunction.AddReusedNode(implementationType, reusedNode);
                return reusedNode;
            }
        }

        if (_checkTypeProperties.GetConstructorChoiceFor(implementationType) is { } constructor)
            return _implementationNodeFactory(
                    abstractionType,
                    implementationType, 
                    constructor, 
                    nextMapper)
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, passedContext);

        if (implementationType.NullableAnnotation != NullableAnnotation.Annotated)
            return _errorNodeFactory(implementationType.InstanceConstructors.Length switch
                {
                    0 => $"Class.Constructor: No constructor found for implementation {implementationType.FullName()}",
                    > 1 =>
                        $"Class.Constructor: More than one constructor found for implementation {implementationType.FullName()}",
                    _ => $"Class.Constructor: {implementationType.InstanceConstructors[0].Name} is not a method symbol"
                },
                implementationType).EnqueueBuildJobTo(_parentContainer.BuildQueue, passedContext);
            
        _localDiagLogger.Warning(WarningLogData.NullResolutionWarning(
            $"Class: Multiple or no implementations where a single is required for \"{implementationType.FullName()}\", but injecting null instead."),
            Location.None);
        return _nullNodeFactory(implementationType)
            .EnqueueBuildJobTo(_parentContainer.BuildQueue, passedContext);
    }
    
    private IElementNode SwitchInterface(INamedTypeSymbol interfaceType, PassedContext passedContext)
    {
        if (_checkTypeProperties.ShouldBeComposite(interfaceType)
            && _checkTypeProperties.GetCompositeFor(interfaceType) is {} compositeImplementationType)
            return SwitchInterfaceWithPotentialDecoration(interfaceType, compositeImplementationType, passedContext, Next);
        if (_checkTypeProperties.MapToSingleFittingImplementation(interfaceType, passedContext.InjectionKeyModification) is not { } impType)
        {
            if (interfaceType.NullableAnnotation == NullableAnnotation.Annotated)
            {
                _localDiagLogger.Warning(WarningLogData.NullResolutionWarning(
                    $"Interface: Multiple or no implementations where a single is required for \"{interfaceType.FullName()}\", but injecting null instead."),
                    Location.None);
                return _nullNodeFactory(interfaceType)
                    .EnqueueBuildJobTo(_parentContainer.BuildQueue, passedContext);
            }
            return _errorNodeFactory(
                    $"Interface: Multiple or no implementations where a single is required for \"{interfaceType.FullName()}\".",
                    interfaceType)
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, passedContext);
        }

        return SwitchInterfaceWithPotentialDecoration(interfaceType, impType, passedContext, this);
    }

    protected IElementNode SwitchInterfaceWithPotentialDecoration(
        INamedTypeSymbol interfaceType,
        INamedTypeSymbol implementationType, 
        PassedContext passedContext,
        IElementNodeMapperBase mapper)
    {
        var shouldBeDecorated = _checkTypeProperties.ShouldBeDecorated(interfaceType);
        if (!shouldBeDecorated)
            return SwitchImplementation(
                new(true, true, true),
                interfaceType,
                implementationType,
                passedContext,
                mapper);

        var decoratorSequence = _checkTypeProperties.GetDecorationSequenceFor(interfaceType, implementationType)
            .Reverse()
            .Append(implementationType)
            .ToList();
        
        var decoratorTypes = ImmutableQueue.CreateRange(decoratorSequence
            .Select(t => (interfaceType, t))
            .Append((interfaceType, implementationType)));
            
        var overridingMapper = _overridingElementNodeMapperFactory(this, decoratorTypes);
        return overridingMapper.Map(interfaceType, passedContext);
    }
}