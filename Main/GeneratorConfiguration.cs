using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE;

internal interface IGeneratorConfiguration
{
    bool ErrorDescriptionInsteadOfBuildFailure { get; }
}

internal sealed class GeneratorConfiguration : IGeneratorConfiguration, IContainerInstance
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