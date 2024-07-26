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
        try
        {
            using var executeLevelContainer = ExecuteLevelContainer.DIE_CreateContainer(context);
            var execute = executeLevelContainer.Create();
            execute.Execute();
        }
        catch (ValidationDieException)
        {
            // nothing to do here
        }
    }
}