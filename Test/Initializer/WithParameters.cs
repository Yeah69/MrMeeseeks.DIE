using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Initializer.WithParameters;

internal sealed class Dependency
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
    // ReSharper disable once InconsistentNaming
    private readonly int DIE_Factory_Number = 69;
    // ReSharper disable once InconsistentNaming
    private readonly string DIE_Factory_Text = "foo";
    
}

public sealed class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.True(instance.IsInitialized);
        Assert.Equal(69, instance.Number);
        Assert.Equal("foo", instance.Text);
    }
}