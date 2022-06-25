using System;
using System.Threading.Tasks;
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
    public async ValueTask Test()
    {
        await using var container = new Container();
    }
}