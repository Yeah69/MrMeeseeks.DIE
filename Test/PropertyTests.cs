using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration;
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

[CreateFunction(typeof(IPropertyClass), "Create")]
internal partial class PropertyContainer
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
        var propertyClass = container.Create();
        Assert.Equal(check, propertyClass.Dependency);
    }
}