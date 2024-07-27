using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE;

internal sealed class GeneratorConfiguration : IContainerInstance
{
    public GeneratorConfiguration(
        GeneratorExecutionContext context,
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous) =>
        ErrorDescriptionInsteadOfBuildFailure = context.Compilation.Assembly.GetAttributes()
            .Any(ad => CustomSymbolEqualityComparer.Default.Equals(
                wellKnownTypesMiscellaneous.ErrorDescriptionInsteadOfBuildFailureAttribute, 
                ad.AttributeClass));

    public bool ErrorDescriptionInsteadOfBuildFailure { get; }
}