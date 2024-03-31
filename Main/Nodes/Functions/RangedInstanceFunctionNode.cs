using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Mappers;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface IRangedInstanceFunctionNode : ISingleFunctionNode;

internal interface IRangedInstanceFunctionNodeInitializer
{
    /// <summary>
    /// Only intended for transient scope instance function, cause they need to synchronize identifier with their interface function
    /// </summary>
    void Initialize(string name, string explicitInterfaceFullName);
}

internal sealed partial class RangedInstanceFunctionNode : SingleFunctionNodeBase, IRangedInstanceFunctionNode, IRangedInstanceFunctionNodeInitializer, IScopeInstance
{
    private readonly INamedTypeSymbol _type;
    private readonly Func<IElementNodeMapper> _typeToElementNodeMapperFactory;

    public RangedInstanceFunctionNode(
        // parameters
        ScopeLevel level,
        INamedTypeSymbol type, 
        IReadOnlyList<ITypeSymbol> parameters,
        
        // dependencies
        IRangeNode parentRange,
        IContainerNode parentContainer, 
        IReferenceGenerator referenceGenerator, 
        IInnerFunctionSubDisposalNodeChooser subDisposalNodeChooser,
        IInnerTransientScopeDisposalNodeChooser transientScopeDisposalNodeChooser,
        Func<IElementNodeMapper> typeToElementNodeMapperFactory,
        Func<PlainFunctionCallNode.Params, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<WrappedAsyncFunctionCallNode.Params, IWrappedAsyncFunctionCallNode> asyncFunctionCallNodeFactory,
        Func<ScopeCallNode.Params, IScopeCallNode> scopeCallNodeFactory,
        Func<TransientScopeCallNode.Params, ITransientScopeCallNode> transientScopeCallNodeFactory,
        Func<ITypeSymbol, IParameterNode> parameterNodeFactory,
        ITypeParameterUtility typeParameterUtility,
        WellKnownTypes wellKnownTypes) 
        : base(
            Microsoft.CodeAnalysis.Accessibility.Private,
            type, 
            parameters,
            ImmutableDictionary.Create<ITypeSymbol, IParameterNode>(CustomSymbolEqualityComparer.IncludeNullability), 
            parentRange, 
            parentContainer, 
            subDisposalNodeChooser,
            transientScopeDisposalNodeChooser,
            parameterNodeFactory,
            plainFunctionCallNodeFactory,
            asyncFunctionCallNodeFactory,
            scopeCallNodeFactory,
            transientScopeCallNodeFactory,
            typeParameterUtility,
            wellKnownTypes)
    {
        _type = type;
        _typeToElementNodeMapperFactory = typeToElementNodeMapperFactory;
        Name = referenceGenerator.Generate($"Get{level.ToString()}Instance", _type);
    }

    protected override IElementNodeMapperBase GetMapper() =>
        _typeToElementNodeMapperFactory();

    protected override IElementNode MapToReturnedElement(IElementNodeMapperBase mapper) => 
        // "MapToImplementation" instead of "Map", because latter would cause an infinite recursion ever trying to create a new ranged instance function
        mapper.MapToImplementation(
            new(CheckForScopeRoot: true, CheckForRangedInstance: false, CheckForInitializedInstance: true), 
            null, 
            _type,
            new(ImmutableStack<INamedTypeSymbol>.Empty, null)); 

    public override string Name { get; protected set; }

    void IRangedInstanceFunctionNodeInitializer.Initialize(
        string name, 
        string explicitInterfaceFullName)
    {
        Name = name;
        ExplicitInterfaceFullName = explicitInterfaceFullName;
    }
}