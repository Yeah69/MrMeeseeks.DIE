using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Logging;

internal interface ILogEnhancer
{
    string Enhance(string message);
}

internal sealed class BaseLogEnhancer : ILogEnhancer, IScopeInstance
{
    public string Enhance(string message) => message;
}

internal sealed class FunctionLevelLogEnhancerDecorator : ILogEnhancer, IDecorator<ILogEnhancer>
{
    private readonly ILogEnhancer _decoratedEnhancer;
    private readonly Lazy<string> _prefix;

    internal FunctionLevelLogEnhancerDecorator(
        Lazy<IFunctionNode> parentFunction,
        ILogEnhancer decoratedEnhancer)
    {
        _decoratedEnhancer = decoratedEnhancer;
        _prefix = parentFunction.Select(f => f.ReturnedTypeNameNotWrapped);
    }
    
    public string Enhance(string message) => $"[RT:{_prefix.Value}] {_decoratedEnhancer.Enhance(message)}";
}

internal sealed class ContainerLevelLogEnhancerDecorator : ILogEnhancer, IDecorator<ILogEnhancer>
{
    private readonly ICurrentExecutionPhase _currentExecutionPhase;
    private readonly ILogEnhancer _decoratedEnhancer;
    private readonly Lazy<string> _containerPart;

    internal ContainerLevelLogEnhancerDecorator(
        Lazy<IContainerNode> parentContainer,
        ICurrentExecutionPhase currentExecutionPhase,
        ILogEnhancer decoratedEnhancer)
    {
        _currentExecutionPhase = currentExecutionPhase;
        _decoratedEnhancer = decoratedEnhancer;
        _containerPart = parentContainer.Select(c => c.Name);
    }

    public string Enhance(string message) => 
        $"[P:{_currentExecutionPhase.Value.ToString()}][C:{_containerPart.Value}]{_decoratedEnhancer.Enhance(message)}";
}

internal sealed class ExecuteLevelLogEnhancerDecorator : ILogEnhancer, IDecorator<ILogEnhancer>
{
    private readonly ILogEnhancer _decoratedEnhancer;

    internal ExecuteLevelLogEnhancerDecorator(ILogEnhancer decoratedEnhancer) => _decoratedEnhancer = decoratedEnhancer;

    public string Enhance(string message) => $"[{Constants.DieAbbreviation}]{_decoratedEnhancer.Enhance(message)}";
}