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
            WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous = WellKnownTypesMiscellaneous.Create(context.Compilation);
        
            new ExecuteImpl(
                context,
                wellKnownTypesMiscellaneous,
                ContainerInfoFactory)
                .Execute();
                
            IContainerInfo ContainerInfoFactory(INamedTypeSymbol type) => new ContainerInfo(type, wellKnownTypesMiscellaneous);
        }
        catch (ValidationDieException)
        {
            // nothing to do here
        }
    }
}