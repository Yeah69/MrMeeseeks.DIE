using MrMeeseeks.DIE.Contexts;
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
        IContainerWideContext containerWideContext) =>
        ErrorDescriptionInsteadOfBuildFailure = context.Compilation.Assembly.GetAttributes()
            .Any(ad => CustomSymbolEqualityComparer.Default.Equals(
                containerWideContext.WellKnownTypesMiscellaneous.ErrorDescriptionInsteadOfBuildFailureAttribute, 
                ad.AttributeClass));

    public bool ErrorDescriptionInsteadOfBuildFailure { get; }
}