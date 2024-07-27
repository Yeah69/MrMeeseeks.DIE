using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE;

[Generator]
#pragma warning disable RS1042
public class SourceGenerator : ISourceGenerator
#pragma warning restore RS1042
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