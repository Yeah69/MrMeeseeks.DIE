

using MrMeeseeks.DIE.Test.Struct.RecordNoExplicitConstructor;

internal class Program
{
    private static void Main()
    {
        var container = new Container();
        var dependency = container.Create();
        
    }
}