using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.UserDefinedElements.FactoryMethod.WithParameterInScope;

internal sealed class ScopeRoot : IScopeRoot
{
    public Uri Property { get; }

    internal ScopeRoot(Uri property) => Property = property;
}

[CreateFunction(typeof(ScopeRoot), "Create")]
internal sealed partial class Container
{


    // ReSharper disable once InconsistentNaming
    private sealed partial class DIE_DefaultScope
    {
        // ReSharper disable once InconsistentNaming
        private string DIE_Factory_Url => "https://example.com/path";
        private Uri DIE_Factory(string url) => new (url);
    }
}

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var scopeRoot = container.Create();
        Assert.Equal("https://example.com/path", scopeRoot.Property.OriginalString);
    }
}