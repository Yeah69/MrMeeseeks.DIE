using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Logging;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Mappers.MappingParts;

internal interface IAbstractionImplementationMappingPart : IMappingPart
{
    IElementNode SwitchImplementation(
        ImplementationMappingConfiguration config,
        INamedTypeSymbol? abstractionType,
        INamedTypeSymbol implementationType,
        PassedContext passedContext,
        IElementNodeMapperBase nextMapper);

    IElementNode SwitchInterfaceWithPotentialDecoration(
        INamedTypeSymbol interfaceType,
        INamedTypeSymbol implementationType,
        PassedContext passedContext,
        IElementNodeMapperBase mapper,
        IElementNodeMapperBase current);
    
    IElementNode ForScopeWithImplementationType(
        INamedTypeSymbol implementationType,
        (string Name, string Reference)[] additionalProperties,
        PassedContext passedContext,
        IElementNodeMapperBase nextMapper); 
}

internal sealed class AbstractionImplementationMappingPart : IAbstractionImplementationMappingPart, IScopeInstance
{
    private readonly IContainerNode _parentContainer;
    private readonly IRangeNode _parentRange;
    private readonly IFunctionNode _parentFunction;
    private readonly ICheckTypeProperties _checkTypeProperties;
    private readonly ILocalDiagLogger _localDiagLogger;
    private readonly IUserDefinedElementsMappingPart _userDefinedElementsMappingPart;
    private readonly Func<INamedTypeSymbol?, INamedTypeSymbol, IMethodSymbol?, IElementNodeMapperBase, IImplementationNode> _implementationNodeFactory;
    private readonly Func<IElementNode, IReusedNode> _reusedNodeFactory;
    private readonly Func<string, IReferenceNode> _referenceNodeFactory;
    private readonly Func<string, ITypeSymbol, IErrorNode> _errorNodeFactory;
    private readonly Func<ITypeSymbol, INullNode> _nullNodeFactory;
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly Func<IElementNodeMapperBase, ImmutableQueue<(INamedTypeSymbol, INamedTypeSymbol)>, IOverridingElementNodeMapper> _overridingElementNodeMapperFactory;

    internal AbstractionImplementationMappingPart(
        IContainerNode parentContainer,
        IRangeNode parentRange,
        ICheckTypeProperties checkTypeProperties,
        IFunctionNode parentFunction,
        ILocalDiagLogger localDiagLogger,
        WellKnownTypes wellKnownTypes,
        IUserDefinedElementsMappingPart userDefinedElementsMappingPart,
        Func<string, ITypeSymbol, IErrorNode> errorNodeFactory,
        Func<ITypeSymbol, INullNode> nullNodeFactory, 
        Func<INamedTypeSymbol?, INamedTypeSymbol, IMethodSymbol?, IElementNodeMapperBase, IImplementationNode> implementationNodeFactory,
        Func<IElementNode, IReusedNode> reusedNodeFactory,
        Func<string, IReferenceNode> referenceNodeFactory,
        Func<IElementNodeMapperBase, ImmutableQueue<(INamedTypeSymbol, INamedTypeSymbol)>, IOverridingElementNodeMapper> overridingElementNodeMapperFactory)
    {
        _parentContainer = parentContainer;
        _parentRange = parentRange;
        _parentFunction = parentFunction;
        _checkTypeProperties = checkTypeProperties;
        _localDiagLogger = localDiagLogger;
        _userDefinedElementsMappingPart = userDefinedElementsMappingPart;
        _errorNodeFactory = errorNodeFactory;
        _nullNodeFactory = nullNodeFactory;
        _implementationNodeFactory = implementationNodeFactory;
        _reusedNodeFactory = reusedNodeFactory;
        _referenceNodeFactory = referenceNodeFactory;
        _overridingElementNodeMapperFactory = overridingElementNodeMapperFactory;
        _wellKnownTypes = wellKnownTypes;
    }

    public IElementNode? Map(MappingPartData data)
    {
        if (data.Type is ({ TypeKind: TypeKind.Interface } or { TypeKind: TypeKind.Class, IsAbstract: true })
            and INamedTypeSymbol interfaceOrAbstractType)
        {
            return _userDefinedElementsMappingPart.Map(data) 
                   ?? SwitchInterface(interfaceOrAbstractType, data.PassedContext, data);
        }

        if (data.Type is INamedTypeSymbol { TypeKind: TypeKind.Class or TypeKind.Struct } classOrStructType)
        {
            var implementationType = classOrStructType;
            var isNullableStruct = classOrStructType.TypeArguments.Length == 1
                                   && classOrStructType.TypeArguments.Single() is INamedTypeSymbol maybeInnerStruct
                                   && CustomSymbolEqualityComparer.Default.Equals(classOrStructType,
                                       _wellKnownTypes.Nullable1.Construct(maybeInnerStruct));
            if (isNullableStruct && classOrStructType.TypeArguments.Single() is INamedTypeSymbol innerType)
            {
                // Take inner type of Nullable<T>
                implementationType = innerType;
            }
            
            if (_checkTypeProperties.MapToSingleFittingImplementation(implementationType, data.PassedContext.InjectionKeyModification) is not { } chosenImplementationType)
            {
                // Check user defined elements as last resort
                if (_userDefinedElementsMappingPart.Map(data) is { } userDefinedNode)
                    return userDefinedNode;
                
                if (classOrStructType.NullableAnnotation == NullableAnnotation.Annotated || isNullableStruct)
                {
                    _localDiagLogger.Warning(WarningLogData.NullResolutionWarning(
                        $"Class: Multiple or no implementations where a single is required for \"{classOrStructType.FullName()}\", but injecting null instead."),
                        Location.None);
                    return _nullNodeFactory(classOrStructType)
                        .EnqueueBuildJobTo(_parentContainer.BuildQueue, data.PassedContext);
                }
                return _errorNodeFactory(
                        $"Class: Multiple or no implementations where a single is required for \"{classOrStructType.FullName()}\",",
                        classOrStructType)
                    .EnqueueBuildJobTo(_parentContainer.BuildQueue, data.PassedContext);
            }

            return SwitchImplementation(
                new(true, true, true),
                null,
                chosenImplementationType,
                data.PassedContext,
                data.Next,
                data);
        }

        return null;
    }

    public IElementNode SwitchImplementation(
        ImplementationMappingConfiguration config,
        INamedTypeSymbol? abstractionType,
        INamedTypeSymbol implementationType,
        PassedContext passedContext,
        IElementNodeMapperBase nextMapper) =>
        SwitchImplementation(config, abstractionType, implementationType, passedContext, nextMapper, null);

    private IElementNode SwitchImplementation(
        ImplementationMappingConfiguration config,
        INamedTypeSymbol? abstractionType,
        INamedTypeSymbol implementationType, 
        PassedContext passedContext,
        IElementNodeMapperBase nextMapper,
        MappingPartData? data)
    {
        if (config.CheckForInitializedInstance && !_parentFunction.CheckIfReturnedType(implementationType))
        {
            if (_parentRange.GetInitializedNode(implementationType) is { } initializedInstanceNode)
            {
                _parentFunction.RegisterUsedInitializedInstance(initializedInstanceNode);
                return initializedInstanceNode;
            }
        }
        if (config.CheckForScopeRoot)
        {
            var ret = _checkTypeProperties.ShouldBeScopeRoot(implementationType) switch
            {
                ScopeLevel.TransientScope => _parentRange.BuildTransientScopeCall(implementationType, _parentFunction, nextMapper),
                ScopeLevel.Scope => _parentRange.BuildScopeCall(implementationType, _parentFunction, nextMapper),
                _ => (IElementNode?) null
            };
            if (ret is not null)
                return ret;
        }
        
        if (config.CheckForRangedInstance)
        {
            var scopeLevel = _checkTypeProperties.GetScopeLevelFor(implementationType);
            
            if (_parentFunction.TryGetReusedNode(implementationType, out var rn) && rn is not null)
                return rn;

            var ret = scopeLevel switch
            {
                ScopeLevel.Container => _parentRange.BuildContainerInstanceCall(implementationType, _parentFunction),
                ScopeLevel.TransientScope => _parentRange.BuildTransientScopeInstanceCall(implementationType, _parentFunction),
                ScopeLevel.Scope => _parentRange.BuildScopeInstanceCall(implementationType, _parentFunction),
                _ => null
            };
            if (ret is not null)
            {
                var reusedNode = _reusedNodeFactory(ret)
                    .EnqueueBuildJobTo(_parentContainer.BuildQueue, passedContext);
                _parentFunction.AddReusedNode(implementationType, reusedNode);
                return reusedNode;
            }
        }
        
        if (data is not null && _userDefinedElementsMappingPart.Map(data) is { } userDefinedNode)
            return userDefinedNode;

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
    
    private IElementNode SwitchInterface(INamedTypeSymbol interfaceType, PassedContext passedContext, MappingPartData data)
    {
        if (_checkTypeProperties.ShouldBeComposite(interfaceType)
            && _checkTypeProperties.GetCompositeFor(interfaceType) is {} compositeImplementationType)
            return SwitchInterfaceWithPotentialDecoration(interfaceType, compositeImplementationType, passedContext, data.Next, data.Current);
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

        return SwitchInterfaceWithPotentialDecoration(interfaceType, impType, passedContext, data.Current, data.Current);
    }

    public IElementNode SwitchInterfaceWithPotentialDecoration(
        INamedTypeSymbol interfaceType,
        INamedTypeSymbol implementationType, 
        PassedContext passedContext,
        IElementNodeMapperBase mapper,
        IElementNodeMapperBase current)
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
            
        var overridingMapper = _overridingElementNodeMapperFactory(current, decoratorTypes);
        return overridingMapper.Map(interfaceType, passedContext);
    }

    public IElementNode ForScopeWithImplementationType(
        INamedTypeSymbol implementationType,
        (string Name, string Reference)[] additionalProperties, 
        PassedContext passedContext,
        IElementNodeMapperBase nextMapper)
    {
        var implementationNode = _implementationNodeFactory(
                null,
                implementationType,
                _checkTypeProperties.GetConstructorChoiceFor(implementationType),
                nextMapper)
            .EnqueueBuildJobTo(_parentContainer.BuildQueue, passedContext);

        var addProps = additionalProperties
            .Select(p => (p.Name, Element: (IElementNode) _referenceNodeFactory(p.Reference)
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, passedContext)))
            .ToArray();
        
        implementationNode.AppendAdditionalProperties(addProps);

        return implementationNode;
    }
}