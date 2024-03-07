using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.Tuples;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Mappers.MappingParts;

internal interface ITupleMappingPart : IMappingPart;

internal sealed class TupleMappingPart : ITupleMappingPart, IScopeInstance
{
    private readonly IContainerNode _parentContainer;
    private readonly IUserDefinedElementsMappingPart _userDefinedElementsMappingPart;
    private readonly Func<INamedTypeSymbol, IElementNodeMapperBase, IValueTupleNode> _valueTupleNodeFactory;
    private readonly Func<INamedTypeSymbol, IElementNodeMapperBase, IValueTupleSyntaxNode> _valueTupleSyntaxNodeFactory;
    private readonly Func<INamedTypeSymbol, IElementNodeMapperBase, ITupleNode> _tupleNodeFactory;

    internal TupleMappingPart(
        IContainerNode parentContainer,
        IUserDefinedElementsMappingPart userDefinedElementsMappingPart,
        Func<INamedTypeSymbol, IElementNodeMapperBase, IValueTupleNode> valueTupleNodeFactory, 
        Func<INamedTypeSymbol, IElementNodeMapperBase, IValueTupleSyntaxNode> valueTupleSyntaxNodeFactory, 
        Func<INamedTypeSymbol, IElementNodeMapperBase, ITupleNode> tupleNodeFactory)
    {
        _parentContainer = parentContainer;
        _userDefinedElementsMappingPart = userDefinedElementsMappingPart;
        _valueTupleNodeFactory = valueTupleNodeFactory;
        _valueTupleSyntaxNodeFactory = valueTupleSyntaxNodeFactory;
        _tupleNodeFactory = tupleNodeFactory;
    }
    
    public IElementNode? Map(MappingPartData data)
    {
        if (data.Type.FullName().StartsWith("global::System.ValueTuple<", StringComparison.Ordinal) && data.Type is INamedTypeSymbol valueTupleType)
            return _userDefinedElementsMappingPart.Map(data) 
                   ?? _valueTupleNodeFactory(valueTupleType, data.NextForWraps)
                       .EnqueueBuildJobTo(_parentContainer.BuildQueue, data.PassedContext);
        
        if (data.Type.FullName().StartsWith("(", StringComparison.Ordinal) && data.Type.FullName().EndsWith(")", StringComparison.Ordinal) && data.Type is INamedTypeSymbol syntaxValueTupleType)
            return _userDefinedElementsMappingPart.Map(data) 
                   ?? _valueTupleSyntaxNodeFactory(syntaxValueTupleType, data.NextForWraps)
                       .EnqueueBuildJobTo(_parentContainer.BuildQueue, data.PassedContext);

        if (data.Type.FullName().StartsWith("global::System.Tuple<", StringComparison.Ordinal) && data.Type is INamedTypeSymbol tupleType)
            return _userDefinedElementsMappingPart.Map(data) 
                   ?? _tupleNodeFactory(tupleType, data.NextForWraps)
                       .EnqueueBuildJobTo(_parentContainer.BuildQueue, data.PassedContext);
        
        return null;
    }
} 