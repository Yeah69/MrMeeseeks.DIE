using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Analytics;

internal interface IAnalyticsFlags
{
    bool ResolutionGraph { get; }
}

internal class AnalyticsFlags : IAnalyticsFlags, IContainerInstance
{
    private readonly Configuration.Attributes.Analytics? _analytics;
    
    internal AnalyticsFlags(
        IContainerInfoContext containerInfoContext,
        IContainerWideContext containerWideContext,
        GeneratorExecutionContext context)
    {
        var attributeData = containerInfoContext.ContainerInfo.ContainerType
            .GetAttributes()
            .FirstOrDefault(CheckAttribute) 
            ?? context.Compilation.Assembly.GetAttributes().FirstOrDefault(CheckAttribute);

        _analytics = attributeData?.ConstructorArguments[0].Value is int analytics
            ? (Configuration.Attributes.Analytics) analytics
            : null;
        
        bool CheckAttribute(AttributeData ad) =>
            CustomSymbolEqualityComparer.Default.Equals(ad.AttributeClass, containerWideContext.WellKnownTypesMiscellaneous.AnalyticsAttribute) 
            && ad.ConstructorArguments.Length == 1 
            && ad.ConstructorArguments[0].Value is int;
    }

    public bool ResolutionGraph => _analytics?.HasFlag(Configuration.Attributes.Analytics.ResolutionGraph) ?? false;
}