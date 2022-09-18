namespace MrMeeseeks.DIE.ResolutionBuilding.Function;

internal interface IFunctionCycleTracker
{
    void RegisterRootHandle(FunctionResolutionBuilderHandle root);
    
    void TrackFunctionCall(FunctionResolutionBuilderHandle from, FunctionResolutionBuilderHandle to);

    void DetectCycle();
}

internal class FunctionCycleTracker : IFunctionCycleTracker
{
    private readonly Dictionary<FunctionResolutionBuilderHandle, IList<FunctionResolutionBuilderHandle>> _adjacencyMap = new ();
    private readonly List<FunctionResolutionBuilderHandle> _roots = new();

    public void RegisterRootHandle(FunctionResolutionBuilderHandle root) => _roots.Add(root);

    public void TrackFunctionCall(FunctionResolutionBuilderHandle from, FunctionResolutionBuilderHandle to)
    {
        if (_adjacencyMap.TryGetValue(from, out var neighbors))
            neighbors.Add(to);
        else
            _adjacencyMap[from] = new List<FunctionResolutionBuilderHandle> { to };
    }

    public void DetectCycle()
    {
        foreach (var function in _roots)
            DetectCycleInner(function, new HashSet<object>());

        void DetectCycleInner(FunctionResolutionBuilderHandle current, HashSet<object> visited)
        {
            if (visited.Contains(current.Identity))
                throw new FunctionCycleDieException();
            if (_adjacencyMap.TryGetValue(current, out var neighbors))
            {
                visited.Add(current.Identity);
                foreach (var neighbor in neighbors)
                    DetectCycleInner(neighbor, visited);
                visited.Remove(current.Identity);
            }
        }
    }
}