using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Configuration.Interception;
using MrMeeseeks.DIE.Mappers.MappingParts;
using MrMeeseeks.DIE.Nodes;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Mappers;

internal abstract record Override
{
    internal sealed record Implementation(INamedTypeSymbol ImplementationType) : Override;
    internal sealed record Interceptor(INamedTypeSymbol InterceptorType) : Override;
}

internal interface IOverridingElementNodeMapper : IElementNodeMapperBase;

internal sealed class OverridingElementNodeMapper : ElementNodeMapperBase, IOverridingElementNodeMapper
{
    private ImmutableQueue<(INamedTypeSymbol InterfaceType, Override Override)> _override;
    private readonly IInvocationTypeManager _invocationTypeManager;
    private readonly Func<string, (IElementNode, IElementNode), IInterceptionElementNode> _interceptionNodeFactory;
    private readonly Func<IElementNodeMapperBase, ImmutableQueue<(INamedTypeSymbol, Override)>, IOverridingElementNodeMapper> _overridingElementNodeMapperFactory;

    internal OverridingElementNodeMapper(
        IElementNodeMapperBase parentElementNodeMapper,
        ImmutableQueue<(INamedTypeSymbol, Override)> @override,
        
        IContainerNode parentContainer,
        IOverridesMappingPart overridesMappingPart,
        IUserDefinedElementsMappingPart userDefinedElementsMappingPart,
        IAsyncWrapperMappingPart asyncWrapperMappingPart,
        ITupleMappingPart tupleMappingPart,
        IDelegateMappingPart delegateMappingPart,
        ICollectionMappingPart collectionMappingPart,
        IAbstractionImplementationMappingPart abstractionImplementationMappingPart,
        IInvocationTypeManager invocationTypeManager,
        Func<ITypeSymbol, IOutParameterNode> outParameterNodeFactory,
        Func<string, (string Name, IElementNode Element)[], IImplicitScopeImplementationNode> implicitScopeImplementationNodeFactory,
        Func<string, IReferenceNode> referenceNodeFactory,
        Func<string, (IElementNode, IElementNode), IInterceptionElementNode> interceptionNodeFactory,
        Func<string, ITypeSymbol, IErrorNode> errorNodeFactory,
        Func<IElementNodeMapperBase, ImmutableQueue<(INamedTypeSymbol, Override)>, IOverridingElementNodeMapper> overridingElementNodeMapperFactory) 
        : base(
            parentContainer,
            overridesMappingPart,
            userDefinedElementsMappingPart,
            asyncWrapperMappingPart,
            tupleMappingPart,
            delegateMappingPart,
            collectionMappingPart,
            abstractionImplementationMappingPart,
            outParameterNodeFactory,
            implicitScopeImplementationNodeFactory,
            referenceNodeFactory,
            errorNodeFactory)
    {
        Next = parentElementNodeMapper;
        _override = @override;
        _invocationTypeManager = invocationTypeManager;
        _interceptionNodeFactory = interceptionNodeFactory;
        _overridingElementNodeMapperFactory = overridingElementNodeMapperFactory;
    }

    protected override IElementNodeMapperBase NextForWraps => this;

    protected override IElementNodeMapperBase Next { get; }

    public override IElementNode Map(ITypeSymbol type, PassedContext passedContext)
    {
        if (!_override.IsEmpty
            && type is INamedTypeSymbol abstraction 
            && CustomSymbolEqualityComparer.Default.Equals(_override.Peek().InterfaceType, type))
        {
            var nextOverride = _override.Dequeue(out var currentOverride);
            switch (currentOverride.Override)
            {
                case Override.Implementation {ImplementationType: var implementationType}:
                    var mapper = _overridingElementNodeMapperFactory(this, nextOverride);
                    return SwitchImplementation(
                        new(true, true, true),
                        abstraction,
                        implementationType,
                        passedContext,
                        mapper);
                case Override.Interceptor {InterceptorType: var interceptorType}:
                    var interceptorImplementationNode = SwitchImplementation(
                        new(true, true, true),
                        null,
                        interceptorType,
                        passedContext,
                        Next); // ToDo maybe a completely new vanilla mapper would be more correct
                    var mapperForInterception = _overridingElementNodeMapperFactory(this, nextOverride);
                    var innerElementNode = mapperForInterception.Map(abstraction, passedContext);
                    return _interceptionNodeFactory(
                        _invocationTypeManager.GetInterceptorBasedDecoratorTypeFullName(interceptorType, abstraction), 
                        (interceptorImplementationNode, innerElementNode));
            }
            
        }
        return base.Map(type, passedContext);
    }

    protected override MapperData GetMapperDataForAsyncWrapping() => new OverridingMapperData(_override);
}