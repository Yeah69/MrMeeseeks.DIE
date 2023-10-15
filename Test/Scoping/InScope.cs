using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Scoping.SameScopeDifferentParameters;

internal class ScopeRoot : IScopeRoot
{
}

[CreateFunction(typeof(Func<string, ScopeRoot>), "Create0")]
[CreateFunction(typeof(Func<int, ScopeRoot>), "Create1")]
internal sealed partial class Container
{
    private Container() {}
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var _ = container.Create0();
        var __ = container.Create1();
    }
}