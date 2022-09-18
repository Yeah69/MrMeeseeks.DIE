namespace MrMeeseeks.DIE.ResolutionBuilding.Function;

internal interface IFunctionCycleTracker
{
    void TrackFunctionCall(FunctionResolutionBuilderHandle from, FunctionResolutionBuilderHandle to);

    void DetectCycle();
}

internal class FunctionCycleTracker : IFunctionCycleTracker
{
    private readonly Dictionary<FunctionResolutionBuilderHandle, IList<FunctionResolutionBuilderHandle>> _adjacencyMap = new ();

    public void TrackFunctionCall(FunctionResolutionBuilderHandle from, FunctionResolutionBuilderHandle to)
    {
        if (_adjacencyMap.TryGetValue(from, out var neighbors))
            neighbors.Add(to);
        else
            _adjacencyMap[from] = new List<FunctionResolutionBuilderHandle> { to };
    }

    public void DetectCycle()
    {
        foreach (var function in _adjacencyMap.Keys)
            DetectCycleInner(function, new Stack<FunctionResolutionBuilderHandle>(), new HashSet<object>());

        void DetectCycleInner(FunctionResolutionBuilderHandle current, Stack<FunctionResolutionBuilderHandle> stack, HashSet<object> visited)
        {
            if (visited.Contains(current.Identity))
                throw new FunctionCycleDieException();
            if (_adjacencyMap.TryGetValue(current, out var neighbors))
            {
                stack.Push(current);
                visited.Add(current.Identity);
                foreach (var neighbor in neighbors)
                    DetectCycleInner(neighbor, stack, visited);
                visited.Remove(current.Identity);
                stack.Pop();
            }
        }
    }
}