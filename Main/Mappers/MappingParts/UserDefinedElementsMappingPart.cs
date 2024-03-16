using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.Factories;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Mappers.MappingParts;

internal interface IUserDefinedElementsMappingPart : IMappingPart;

internal sealed class UserDefinedElementsMappingPart : IUserDefinedElementsMappingPart, IScopeInstance
{
    private readonly IUserDefinedElements _userDefinedElements;
    private readonly IContainerNode _parentContainer;
    private readonly Func<IFieldSymbol, IFactoryFieldNode> _factoryFieldNodeFactory;
    private readonly Func<IPropertySymbol, IFactoryPropertyNode> _factoryPropertyNodeFactory;
    private readonly Func<IMethodSymbol, IElementNodeMapperBase, IFactoryFunctionNode> _factoryFunctionNodeFactory;
    
    internal UserDefinedElementsMappingPart(
        IContainerNode parentContainer,
        IUserDefinedElements userDefinedElements,
        Func<IFieldSymbol, IFactoryFieldNode> factoryFieldNodeFactory,
        Func<IPropertySymbol, IFactoryPropertyNode> factoryPropertyNodeFactory,
        Func<IMethodSymbol, IElementNodeMapperBase, IFactoryFunctionNode> factoryFunctionNodeFactory)
    {
        _userDefinedElements = userDefinedElements;
        _parentContainer = parentContainer;
        _factoryFieldNodeFactory = factoryFieldNodeFactory;
        _factoryPropertyNodeFactory = factoryPropertyNodeFactory;
        _factoryFunctionNodeFactory = factoryFunctionNodeFactory;
    }
    
    
    public IElementNode? Map(MappingPartData data)
    {
        if (_userDefinedElements.GetFactoryFieldFor(data.Type) is { } instance)
            return _factoryFieldNodeFactory(instance)
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, data.PassedContext);

        if (_userDefinedElements.GetFactoryPropertyFor(data.Type) is { } property)
            return _factoryPropertyNodeFactory(property)
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, data.PassedContext);

        if (_userDefinedElements.GetFactoryMethodFor(data.Type) is { } method)
            return _factoryFunctionNodeFactory(method, data.Next)
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, data.PassedContext);

        return null;
    }
}