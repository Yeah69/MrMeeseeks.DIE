using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Scoping.SameScopeDifferentParameters;

internal class ScopeRoot : IScopeRoot
{
}

[CreateFunction(typeof(Func<string, ScopeRoot>), "Create0")]
[CreateFunction(typeof(Func<int, ScopeRoot>), "Create1")]
internal partial class Container
{
    
}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
    }
}