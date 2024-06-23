using MrMeeseeks.DIE.Mappers;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;

internal interface IScopeCallNodeBase : IFunctionCallNode
{
    IFunctionCallNode? Initialization { get; }
    IElementNode ScopeConstruction { get; }
}

internal sealed record ScopeCallNodeOuterMapperParam(IElementNodeMapperBase Mapper);

internal abstract class ScopeCallNodeBase : FunctionCallNode, IScopeCallNodeBase
{
    private readonly IScopeNodeBase _scope;
    private readonly IElementNodeMapperBase _scopeImplementationMapper;
    
    protected ScopeCallNodeBase(
        // parameters
        ITypeSymbol callSideType,
        IScopeNodeBase scope,
        IReadOnlyList<(IParameterNode, IParameterNode)> parameters, 
        IReadOnlyList<ITypeSymbol> typeParameters,
        IFunctionCallNode? initialization,
        ScopeCallNodeOuterMapperParam outerMapperParam,
        IElementNode callingTransientScopeDisposal,
        
        // dependencies
        IFunctionNode calledFunction, 
        IReferenceGenerator referenceGenerator) 
        : base(
            null,
            callSideType,
            parameters,
            typeParameters,
            null,
            callingTransientScopeDisposal,
            calledFunction,
            referenceGenerator)
    {
        _scope = scope;
        Initialization = initialization;
        _scopeImplementationMapper = outerMapperParam.Mapper;
    }
    
    protected abstract (string Name, string Reference)[] AdditionalPropertiesForConstruction { get; }

    public override void Build(PassedContext passedContext)
    {
        base.Build(passedContext);

        ScopeConstruction = _scope.ImplementationType is not null
            ? _scopeImplementationMapper.MapToScopeWithImplementationType(
                _scope.ImplementationType,
                AdditionalPropertiesForConstruction,
                passedContext)
            : _scopeImplementationMapper.MapToImplicitScope(
                _scope.Name, // Using name instead of full name to prevent compile error as implicit scopes are not defined before code generation
                AdditionalPropertiesForConstruction,
                passedContext);
    }

    public override string OwnerReference => ScopeConstruction.Reference;

    public IFunctionCallNode? Initialization { get; }
    public IElementNode ScopeConstruction { get; private set; } = null!;
}