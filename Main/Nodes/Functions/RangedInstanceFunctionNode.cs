using MrMeeseeks.DIE.CodeGeneration.Nodes;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Mappers;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface IRangedInstanceFunctionNode : ISingleFunctionNode
{
    IRangedInstanceFunctionGroupNode? Group { get; set; }
}

internal interface IRangedInstanceFunctionNodeInitializer
{
    /// <summary>
    /// Only intended for transient scope instance function, cause they need to synchronize identifier with their interface function
    /// </summary>
    void Initialize(string namePrefix, string nameNumberSuffix, string explicitInterfaceFullName);
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
        IOuterFunctionSubDisposalNodeChooser subDisposalNodeChooser,
        IEntryTransientScopeDisposalNodeChooser transientScopeDisposalNodeChooser,
        AsynchronicityHandlingFactory asynchronicityHandlingFactory,
        Lazy<IFunctionNodeGenerator> functionNodeGenerator,
        Func<IElementNodeMapper> typeToElementNodeMapperFactory,
        Func<PlainFunctionCallNode.Params, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<WrappedAsyncFunctionCallNode.Params, IWrappedAsyncFunctionCallNode> asyncFunctionCallNodeFactory,
        Func<ScopeCallNode.Params, IScopeCallNode> scopeCallNodeFactory,
        Func<TransientScopeCallNode.Params, ITransientScopeCallNode> transientScopeCallNodeFactory,
        Func<ITypeSymbol, IParameterNode> parameterNodeFactory,
        ITypeParameterUtility typeParameterUtility) 
        : base(
            Microsoft.CodeAnalysis.Accessibility.Private,
            type, 
            parameters,
            ImmutableDictionary.Create<ITypeSymbol, IParameterNode>(CustomSymbolEqualityComparer.IncludeNullability), 
            parentRange, 
            parentContainer, 
            subDisposalNodeChooser,
            transientScopeDisposalNodeChooser,
            asynchronicityHandlingFactory,
            functionNodeGenerator,
            parameterNodeFactory,
            plainFunctionCallNodeFactory,
            asyncFunctionCallNodeFactory,
            scopeCallNodeFactory,
            transientScopeCallNodeFactory,
            typeParameterUtility)
    {
        _type = type;
        _typeToElementNodeMapperFactory = typeToElementNodeMapperFactory;
        NamePrefix = $"Get{level.ToString()}Instance{type.Name}";
        NameNumberSuffix = referenceGenerator.Generate("");
        AsynchronicityHandling.MakeAsyncYes(); // RangedInstanceFunctionNode is always async (in case it returns a Task), because it needs to await the semaphore
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

    protected override string NamePrefix { get; set; }
    protected override string NameNumberSuffix { get; set; }

    void IRangedInstanceFunctionNodeInitializer.Initialize(
        string namePrefix, 
        string nameNumberSuffix, 
        string explicitInterfaceFullName)
    {
        NamePrefix = namePrefix;
        NameNumberSuffix = nameNumberSuffix;
        ExplicitInterfaceFullName = explicitInterfaceFullName;
    }

    public IRangedInstanceFunctionGroupNode? Group { get; set; }
}