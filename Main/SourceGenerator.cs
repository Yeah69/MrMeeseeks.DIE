using System.Threading;
using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE;

[Generator]
public class SourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // no initialization required
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var executeLevelContainer = ExecuteLevelContainer.DIE_CreateContainer(context);
        try
        {
            var execute = executeLevelContainer.Create();
            execute.Execute();
        }
        catch (ValidationDieException)
        {
            // nothing to do here
        }
        finally
        {
            var disposeAsync = executeLevelContainer.DisposeAsync();
            SpinWait.SpinUntil(() => disposeAsync.IsCompleted);
        }
    }
}