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
            DetectCycleInner(function, new HashSet<object>(), new Stack<FunctionResolutionBuilderHandle>());

        void DetectCycleInner(FunctionResolutionBuilderHandle current, HashSet<object> visited, Stack<FunctionResolutionBuilderHandle> stack)
        {
            if (visited.Contains(current.Identity))
            {
                var cycleStack = ImmutableStack.Create(current);
                var i = current;
                do
                {
                    i = stack.Pop();
                    cycleStack = cycleStack.Push(i);
                } while (i.Identity != current.Identity && stack.Any());
                throw new FunctionCycleDieException(cycleStack);
            }
            if (_adjacencyMap.TryGetValue(current, out var neighbors))
            {
                visited.Add(current.Identity);
                stack.Push(current);
                foreach (var neighbor in neighbors)
                    DetectCycleInner(neighbor, visited, stack);
                stack.Pop();
                visited.Remove(current.Identity);
            }
        }
    }
}