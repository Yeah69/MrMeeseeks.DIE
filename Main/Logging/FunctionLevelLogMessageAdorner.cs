using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.Logging;

internal interface IFunctionLevelLogMessageEnhancer
{
    string Enhance(string message);
}

internal class FunctionLevelLogMessageEnhancer : IFunctionLevelLogMessageEnhancer, IScopeInstance
{
    private readonly IRangeLevelLogMessageEnhancer _parentEnhancer;
    private readonly Lazy<string> _prefix;

    internal FunctionLevelLogMessageEnhancer(
        Lazy<IFunctionNode> parentFunction,
        IRangeLevelLogMessageEnhancer parentEnhancer)
    {
        _parentEnhancer = parentEnhancer;
        _prefix = parentFunction.Select(f => $"[RT:{f.ReturnedTypeNameNotWrapped}]");
    }
    
    public string Enhance(string message) => 
        _parentEnhancer.Enhance($"{_prefix.Value} {message}");
}

internal class FunctionLevelLogMessageEnhancerForRanges : IFunctionLevelLogMessageEnhancer, IScopeInstance
{
    private readonly IRangeLevelLogMessageEnhancer _parentEnhancer;

    internal FunctionLevelLogMessageEnhancerForRanges(
        IRangeLevelLogMessageEnhancer parentEnhancer) =>
        _parentEnhancer = parentEnhancer;

    public string Enhance(string message) => 
        _parentEnhancer.Enhance(message);
}