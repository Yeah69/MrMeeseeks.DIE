using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Scoping.SameScopeDifferentParameters;

internal class ScopeRoot : IScopeRoot
{
}

[CreateFunction(typeof(Func<string, ScopeRoot>), "Create0")]
[CreateFunction(typeof(Func<int, ScopeRoot>), "Create1")]
internal sealed partial class Container
{
    
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
    }
}