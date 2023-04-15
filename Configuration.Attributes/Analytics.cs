namespace MrMeeseeks.DIE.Configuration.Attributes;

/// <summary>
/// Supported kinds of analytics.
/// </summary>
[Flags]
public enum Analytics
{
    None = 0,
    ResolutionGraph = 1<<0
}