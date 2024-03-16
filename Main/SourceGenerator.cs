using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility;

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
            var wellKnownTypesMiscellaneous = WellKnownTypesMiscellaneous.Create(context.Compilation);
            var rangeUtility = new RangeUtility(wellKnownTypesMiscellaneous);
            var requiredKeywordUtility = new RequiredKeywordUtility(new ContainerWideContext(context.Compilation), new CheckInternalsVisible(context));

            var execute = new ExecuteImpl(
                context,
                rangeUtility,
                requiredKeywordUtility,
                ContainerInfoFactory);
            execute.Execute();
                
            IContainerInfo ContainerInfoFactory(INamedTypeSymbol type) => 
                new ContainerInfo(type, wellKnownTypesMiscellaneous, rangeUtility);
        }
        catch (ValidationDieException)
        {
            // nothing to do here
        }
    }
}