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
    private readonly Func<IElementNodeMapperBase, ImmutableQueue<(INamedTypeSymbol, Override)>, IOverridingElementNodeMapper> _overridingElementNodeMapperFactory;

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
        Func<IElementNodeMapperBase, ImmutableQueue<(INamedTypeSymbol, Override)>, IOverridingElementNodeMapper> overridingElementNodeMapperFactory)
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

            var implementationResult = _checkTypeProperties.MapToSingleFittingImplementation(implementationType, data.PassedContext.InjectionKeyModification);
            
            if (implementationResult is ImplementationResult.Single { Implementation: { } chosenImplementationType })
                return SwitchImplementation(
                    new(true, true, true),
                    null,
                    chosenImplementationType,
                    data.PassedContext,
                    data.Next,
                    data);

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

            var errorMessage = implementationResult switch
            {
                ImplementationResult.None => $"Class: No implementation registered for \"{classOrStructType.FullName()}\".",
                ImplementationResult.Multiple { Implementations: var implementations} => $"Class: Multiple implementations registered for \"{classOrStructType.FullName()}\": {string.Join(", ", implementations.Select(i => i.FullName()))}.",
                _ => throw new InvalidOperationException("Unexpected ImplementationResult")
            };

            return _errorNodeFactory(errorMessage, classOrStructType).EnqueueBuildJobTo(_parentContainer.BuildQueue, data.PassedContext);
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
        if (config.CheckForInitializedInstance)
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

        var constructorResult = _checkTypeProperties.GetConstructorChoiceFor(implementationType);

        if (constructorResult is ConstructorResult.Single { Constructor: {} constructor })
            return _implementationNodeFactory(
                    abstractionType,
                    implementationType, 
                    constructor, 
                    nextMapper)
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, passedContext);

        if (implementationType.NullableAnnotation != NullableAnnotation.Annotated)
            return _errorNodeFactory(constructorResult switch
                {
                    ConstructorResult.None => $"Class.Constructor: No visible constructor found for implementation {implementationType.FullName()}",
                    ConstructorResult.Multiple => $"Class.Constructor: More than one visible constructor found for implementation {implementationType.FullName()}",
                    ConstructorResult.ChoiceFailedNone => $"Class.Constructor: Constructor choice didn't match with any constructor for implementation {implementationType.FullName()}",
                    ConstructorResult.ChoiceFailedMultiple => $"Class.Constructor: Constructor choice matched with multiple constructors for implementation {implementationType.FullName()}",
                    _ => throw new InvalidOperationException("Unexpected ConstructorResult")
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
        var implementationResult = _checkTypeProperties.MapToSingleFittingImplementation(interfaceType, passedContext.InjectionKeyModification);
        if (implementationResult is ImplementationResult.Single { Implementation: { } impType })
            return SwitchInterfaceWithPotentialDecoration(interfaceType, impType, passedContext, data.Current, data.Current);
        
        if (interfaceType.NullableAnnotation == NullableAnnotation.Annotated)
        {
            _localDiagLogger.Warning(WarningLogData.NullResolutionWarning(
                    $"Interface: Multiple or no implementations where a single is required for \"{interfaceType.FullName()}\", but injecting null instead."),
                Location.None);
            return _nullNodeFactory(interfaceType)
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, passedContext);
        }
        var errorMessage = implementationResult switch
        {
            ImplementationResult.None => $"Interface: No implementation registered for \"{interfaceType.FullName()}\".",
            ImplementationResult.Multiple { Implementations: var implementations } => $"Interface: Multiple implementations registered for \"{interfaceType.FullName()}\": {string.Join(", ", implementations.Select(i => i.FullName()))}.",
            _ => throw new InvalidOperationException("Unexpected SingleImplementationResult")
        };
        return _errorNodeFactory(errorMessage, interfaceType).EnqueueBuildJobTo(_parentContainer.BuildQueue, passedContext);
    }

    public IElementNode SwitchInterfaceWithPotentialDecoration(
        INamedTypeSymbol interfaceType,
        INamedTypeSymbol implementationType, 
        PassedContext passedContext,
        IElementNodeMapperBase mapper,
        IElementNodeMapperBase current)
    {
        var decoratorSequence = _checkTypeProperties.GetDecorationSequenceFor(interfaceType, implementationType)
            .Select(t => t switch
            {
                Decoration.Decorator {Type: var type} => new Override.Implementation(type),
                Decoration.Interceptor {Type: var type} => new Override.Interceptor(type),
                _ => (Override?) null
            })
            .OfType<Override>()
            .Reverse()
            .Append(new Override.Implementation(implementationType))
            .ToList();
        
        if (decoratorSequence.Count == 1)
            return SwitchImplementation(
                new(true, true, true),
                interfaceType,
                implementationType,
                passedContext,
                mapper);
        
        var decoratorTypes = ImmutableQueue.CreateRange(decoratorSequence
            .Select(t => (interfaceType, t)));
            
        var overridingMapper = _overridingElementNodeMapperFactory(current, decoratorTypes);
        return overridingMapper.Map(interfaceType, passedContext);
    }

    public IElementNode ForScopeWithImplementationType(
        INamedTypeSymbol implementationType,
        (string Name, string Reference)[] additionalProperties, 
        PassedContext passedContext,
        IElementNodeMapperBase nextMapper)
    {
        var constructorResult = _checkTypeProperties.GetConstructorChoiceFor(implementationType);
        
        var implementationNode = _implementationNodeFactory(
                null,
                implementationType,
                constructorResult is ConstructorResult.Single { Constructor: {} constructor } ? constructor : null,
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