using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Utility;

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
            var rangeUtility = new RangeUtility(
                new ContainerWideContext(
                    WellKnownTypes.Create(context.Compilation),
                    WellKnownTypesAggregation.Create(context.Compilation),
                    WellKnownTypesChoice.Create(context.Compilation),
                    WellKnownTypesCollections.Create(context.Compilation), 
                    wellKnownTypesMiscellaneous,
                    context.Compilation));
        
            var execute = new ExecuteImpl(
                context,
                rangeUtility,
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