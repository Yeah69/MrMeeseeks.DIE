using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.Nodes;

internal interface ITaskBasedQueue
{
    void EnqueueTaskBasedOnlyFunction(IFunctionNode function);
    void Process();
}

internal class TaskBasedQueue : ITaskBasedQueue, IContainerInstance
{
    private readonly HashSet<IFunctionNode> _taskBasedOnlyFunctions = [];
    private readonly Queue<IFunctionNode> _taskBasedOnlyQueue = new();
    private readonly HashSet<IFunctionNode> _taskBasedTooFunctions = [];
    private readonly Queue<IFunctionNode> _taskBasedTooQueue = new();
    
    public void EnqueueTaskBasedOnlyFunction(IFunctionNode function)
    {
        if (_taskBasedOnlyFunctions.Add(function))
            _taskBasedOnlyQueue.Enqueue(function);
    }
    
    private void EnqueueTaskBasedTooFunction(IFunctionNode function)
    {
        if (!_taskBasedOnlyFunctions.Contains(function) && _taskBasedTooFunctions.Add(function))
            _taskBasedTooQueue.Enqueue(function);
    }

    public void Process()
    {
        while (_taskBasedOnlyQueue.Count > 0)
        {
            var function = _taskBasedOnlyQueue.Dequeue();
            var (callingFunctions, calledFunctions) = function.MakeTaskBasedOnly();
            foreach (var callingFunction in callingFunctions)
                EnqueueTaskBasedOnlyFunction(callingFunction);
            foreach (var calledFunction in calledFunctions)
                EnqueueTaskBasedTooFunction(calledFunction);
        }
        
        while (_taskBasedTooQueue.Count > 0)
        {
            var function = _taskBasedTooQueue.Dequeue();
            var calledFunctions = function.MakeTaskBasedToo();
            foreach (var calledFunction in calledFunctions)
                EnqueueTaskBasedTooFunction(calledFunction);
        }
    }
}