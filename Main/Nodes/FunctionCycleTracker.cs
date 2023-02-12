using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.ResolutionBuilding.Function;

namespace MrMeeseeks.DIE.Nodes;

internal interface IFunctionCycleTracker
{
    void DetectCycle(IContainerNode containerNode);
}

internal class FunctionCycleTracker : IFunctionCycleTracker
{
    public void DetectCycle(IContainerNode containerNode)
    {
        Queue<IFunctionNode> roots = new(containerNode.RootFunctions);
        HashSet<IFunctionNode> v = new();
        HashSet<IFunctionNode> cf = new();
        Stack<IFunctionNode> s = new();

        while (roots.Any() && roots.Dequeue() is {} next)
            DetectCycleInner(
                next, 
                v, 
                s,
                cf);

        void DetectCycleInner(
            IFunctionNode current, 
            ISet<IFunctionNode> visited, 
            Stack<IFunctionNode> stack,
            ISet<IFunctionNode> cycleFree)
        {
            if (cycleFree.Contains(current))
                return; // one of the previous roots checked this node already
            if (visited.Contains(current))
            {
                var cycleStack = ImmutableStack.Create(new FunctionResolutionBuilderHandle(current, current.Description));
                IFunctionNode i;
                do
                {
                    i = stack.Pop();
                    cycleStack = cycleStack.Push(new FunctionResolutionBuilderHandle(i, current.Description));
                } while (i != current && stack.Any());
                throw new FunctionCycleDieException(cycleStack);
            }
            visited.Add(current);
            stack.Push(current);
            foreach (var neighbor in current.CalledFunctions)
                DetectCycleInner(neighbor, visited, stack, cycleFree);
            cycleFree.Add(current);
            stack.Pop();
            visited.Remove(current);
            foreach (var localFunction in current.LocalFunctions)
                roots.Enqueue(localFunction);
        }
    }
}