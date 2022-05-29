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
            DetectCycleInner(function, ImmutableStack<FunctionResolutionBuilderHandle>.Empty);

        void DetectCycleInner(FunctionResolutionBuilderHandle current, IImmutableStack<FunctionResolutionBuilderHandle> stack)
        {
            if (stack.Any(h => h.Identity == current.Identity))
                throw new FunctionCycleDieException();
            if (_adjacencyMap.TryGetValue(current, out var neighbors))
            {
                stack = stack.Push(current);
                foreach (var neighbor in neighbors)
                    DetectCycleInner(neighbor, stack);
            }
        }
    }
}