using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Validation.Attributes;
using MrMeeseeks.DIE.Validation.Range;
using MrMeeseeks.DIE.Validation.Range.UserDefined;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE;

[Generator]
public class SourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        new InitializeImpl(context, SyntaxReceiverFactory).Initialize();
            
        ISyntaxReceiver SyntaxReceiverFactory() => new SyntaxReceiver();
        //if (!Debugger.IsAttached)
        {
            //Debugger.Launch();
        }
    }

    public void Execute(GeneratorExecutionContext context)
    {
        try
        {
            WellKnownTypesCollections wellKnownTypesCollections = WellKnownTypesCollections.Create(context.Compilation);
            WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous = WellKnownTypesMiscellaneous.Create(context.Compilation);
            var errorDescriptionInsteadOfBuildFailure = context.Compilation.Assembly.GetAttributes()
                .Any(ad => CustomSymbolEqualityComparer.Default.Equals(wellKnownTypesMiscellaneous.ErrorDescriptionInsteadOfBuildFailureAttribute, ad.AttributeClass));

            WellKnownTypes wellKnownTypes = WellKnownTypes.Create(context.Compilation);
            WellKnownTypesAggregation wellKnownTypesAggregation = WellKnownTypesAggregation.Create(context.Compilation);
            WellKnownTypesChoice wellKnownTypesChoice = WellKnownTypesChoice.Create(context.Compilation);
            IContainerWideContext containerWideContext = new ContainerWideContext(
                wellKnownTypes,
                wellKnownTypesAggregation,
                wellKnownTypesChoice,
                wellKnownTypesCollections,
                wellKnownTypesMiscellaneous);
            DiagLogger diagLogger = new DiagLogger(errorDescriptionInsteadOfBuildFailure, context);
        
            var containerDieExceptionGenerator = new ContainerDieExceptionGenerator(context, containerWideContext);
            new ExecuteImpl(
                errorDescriptionInsteadOfBuildFailure,
                context,
                wellKnownTypesMiscellaneous,
                containerDieExceptionGenerator,
                ContainerInfoFactory, 
                diagLogger).Execute();
                
            IContainerInfo ContainerInfoFactory(INamedTypeSymbol type) => new ContainerInfo(type, containerWideContext);
        }
        catch (ValidationDieException)
        {
            // nothing to do here
        }
    }
}