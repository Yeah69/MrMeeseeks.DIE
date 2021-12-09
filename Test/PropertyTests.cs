using Xunit;

namespace MrMeeseeks.DIE.Test;

internal interface IPropertyClass
{
    string Dependency { get; }
}

internal class PropertyClass : IPropertyClass
{
    public string Dependency { get; }

    public PropertyClass(string dependency)
    {
        Dependency = dependency;
    }
}

internal partial class PropertyContainer : IContainer<IPropertyClass>
{
    private string DIE_Dependency { get; }

    public PropertyContainer(string dieDependency) => DIE_Dependency = dieDependency;
}

public partial class PropertyTests
{
    [Fact]
    public void ResolveExternalType()
    {
        var check = "Hello, Property!";
        using var container = new PropertyContainer(check);
        var propertyClass = ((IContainer<IPropertyClass>) container).Resolve();
        Assert.Equal(check, propertyClass.Dependency);
    }
}