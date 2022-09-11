namespace GettingStarted;

internal interface IMrMeeseeks
{
    void Greet();
}

internal class MrMeeseeks : IMrMeeseeks
{
    private readonly ILogger _logger;

    internal MrMeeseeks(ILogger logger) => 
        _logger = logger;

    public void Greet() => 
        _logger.Log("I'm MrMeeseeks! Look at me!");
}