namespace MrMeeseeks.DIE.ResolutionBuilding;

internal interface IErrorContext
{
    string Prefix { get; }
    Location Location { get; }
}

internal class ErrorContext : IErrorContext
{
    public ErrorContext(string prefix, Location location)
    {
        Prefix = prefix;
        Location = location;
    }

    public string Prefix { get; }
    public Location Location { get; }
}