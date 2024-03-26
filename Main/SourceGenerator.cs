using MrMeeseeks.DIE.Logging;
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
            var wellKnownTypes = WellKnownTypes.Create(context.Compilation);
            var wellKnownTypesMiscellaneous = WellKnownTypesMiscellaneous.Create(context.Compilation);
            var rangeUtility = new RangeUtility(wellKnownTypesMiscellaneous);
            var requiredKeywordUtility = new RequiredKeywordUtility(context, new CheckInternalsVisible(context));
            var referenceGeneratorCounter = new ReferenceGeneratorCounter();
            var singularDisposeFunctionUtility = new SingularDisposeFunctionUtility(
                new ReferenceGenerator(
                    referenceGeneratorCounter, 
                    new LocalDiagLogger(
                        new FunctionLevelLogMessageEnhancerForSourceGenerator(), 
                        new DiagLogger(new GeneratorConfiguration(context, wellKnownTypesMiscellaneous), context))), 
                wellKnownTypes);

            var execute = new ExecuteImpl(
                context,
                rangeUtility,
                requiredKeywordUtility,
                singularDisposeFunctionUtility,
                referenceGeneratorCounter,
                ContainerInfoFactory);
            execute.Execute();
                
            ContainerInfo ContainerInfoFactory(INamedTypeSymbol type) => 
                new(type, wellKnownTypesMiscellaneous, rangeUtility);
        }
        catch (ValidationDieException)
        {
            // nothing to do here
        }
    }
}