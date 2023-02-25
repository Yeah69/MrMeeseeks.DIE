using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Mappers;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface ICreateFunctionNode : ISingleFunctionNode
{
}

internal class CreateFunctionNode : SingleFunctionNodeBase, ICreateFunctionNode
{
    private readonly IReferenceGenerator _referenceGenerator;
    private readonly Func<ISingleFunctionNode, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, IElementNodeMapperBase> _typeToElementNodeMapperFactory;

    public CreateFunctionNode(
        ITypeSymbol typeSymbol, 
        IReadOnlyList<ITypeSymbol> parameters,
        IRangeNode parentNode, 
        IContainerNode parentContainer, 
        IUserDefinedElements userDefinedElements, 
        ICheckTypeProperties checkTypeProperties,
        IReferenceGenerator referenceGenerator, 
        Func<ISingleFunctionNode, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, IElementNodeMapperBase> typeToElementNodeMapperFactory,
        Func<string?, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<string, string, IScopeNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, IScopeCallNode> scopeCallNodeFactory, 
        Func<string, ITransientScopeNode, IContainerNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, ITransientScopeCallNode> transientScopeCallNodeFactory,
        Func<ITypeSymbol, IReferenceGenerator, IParameterNode> parameterNodeFactory,
        WellKnownTypes wellKnownTypes) 
        : base(
            Microsoft.CodeAnalysis.Accessibility.Private,
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
        Name = referenceGenerator.Generate("Create", typeSymbol);
    }

    protected override IElementNodeMapperBase GetMapper(ISingleFunctionNode parentFunction, IRangeNode parentNode, IContainerNode parentContainer,
        IUserDefinedElements userDefinedElements, ICheckTypeProperties checkTypeProperties) =>
        _typeToElementNodeMapperFactory(parentFunction, parentNode, parentContainer, userDefinedElements, checkTypeProperties, _referenceGenerator);

    public override void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitCreateFunctionNode(this);
    public override string Name { get; protected set; }
}