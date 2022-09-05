using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Test.Initializer.WithParameters;

internal class Dependency
{
    public bool IsInitialized { get; private set; }

    public int Number { get; private set; }

    public string Text { get; private set; } = "";
    
    internal void Initialize(int number, string text)
    {
        IsInitialized = true;
        Number = number;
        Text = text;
    }
}

[Initializer(typeof(Dependency), nameof(Dependency.Initialize))]
[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container
{
    private readonly int DIE_Factory_Number = 69;
    private readonly string DIE_Factory_Text = "foo";
}