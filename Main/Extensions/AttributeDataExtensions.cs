namespace MrMeeseeks.DIE.Extensions;

internal static class AttributeDataExtensions
{
    internal static Location GetLocation(this AttributeData attributeData) =>
        attributeData.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? Location.None;
}