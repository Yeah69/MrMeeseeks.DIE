using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Mappers;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface IEntryFunctionNode : ISingleFunctionNode
{
}

internal class EntryFunctionNode : SingleFunctionNodeBase, IEntryFunctionNode
{
    private readonly IReferenceGenerator _referenceGenerator;
    private readonly Func<ISingleFunctionNode, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, IElementNodeMapperBase> _typeToElementNodeMapperFactory;
    private readonly Func<IElementNodeMapperBase, ElementNodeMapperBase.PassedDependencies, INonWrapToCreateElementNodeMapper> _nonWrapToCreateElementNodeMapperFactory;

    public EntryFunctionNode(
        ITypeSymbol typeSymbol, 
        string prefix, 
        IReadOnlyList<ITypeSymbol> parameters,
        IRangeNode parentNode, 
        IContainerNode parentContainer, 
        IUserDefinedElements userDefinedElements, 
        ICheckTypeProperties checkTypeProperties,
        IReferenceGenerator referenceGenerator,
        WellKnownTypes wellKnownTypes,
        Func<ISingleFunctionNode, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, IElementNodeMapperBase> typeToElementNodeMapperFactory, 
        Func<IElementNodeMapperBase, ElementNodeMapperBase.PassedDependencies, INonWrapToCreateElementNodeMapper> nonWrapToCreateElementNodeMapperFactory,
        Func<string?, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<string, string, IScopeNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, IScopeCallNode> scopeCallNodeFactory,
        Func<string, ITransientScopeNode, IContainerNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, ITransientScopeCallNode> transientScopeCallNodeFactory,
        Func<ITypeSymbol, IReferenceGenerator, IParameterNode> parameterNodeFactory) 
        : base(
            Microsoft.CodeAnalysis.Accessibility.Internal,
            typeSymbol, 
            parameters,
            ImmutableDictionary.Create<ITypeSymbol, IParameterNode>(CustomSymbolEqualityComparer.IncludeNullability), 
            parentNode, 
            parentContainer, 
            userDefinedElements, 
            checkTypeProperties,
            referenceGenerator, 
            parameterNodeFactory,
            plainFunctionCallNodeFactory,
            scopeCallNodeFactory,
            transientScopeCallNodeFactory,
            wellKnownTypes)
    {
        _referenceGenerator = referenceGenerator;
        _typeToElementNodeMapperFactory = typeToElementNodeMapperFactory;
        _nonWrapToCreateElementNodeMapperFactory = nonWrapToCreateElementNodeMapperFactory;
        Name = prefix;
    }

    protected override IElementNodeMapperBase GetMapper(ISingleFunctionNode parentFunction, IRangeNode parentNode, IContainerNode parentContainer,
        IUserDefinedElements userDefinedElements, ICheckTypeProperties checkTypeProperties)
    {
        var dummyMapper = _typeToElementNodeMapperFactory(parentFunction, parentNode, parentContainer,
            userDefinedElements, checkTypeProperties, _referenceGenerator);

        return _nonWrapToCreateElementNodeMapperFactory(dummyMapper, dummyMapper.MapperDependencies);
    }

    public override void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitEntryFunctionNode(this);
    public override string Name { get; protected set; }
}