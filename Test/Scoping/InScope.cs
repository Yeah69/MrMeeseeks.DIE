using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Scoping.SameScopeDifferentParameters;

internal sealed class ScopeRoot : IScopeRoot;

[CreateFunction(typeof(Func<string, ScopeRoot>), "Create0")]
[CreateFunction(typeof(Func<int, ScopeRoot>), "Create1")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        _ = container.Create0();
        _ = container.Create1();
    }
}