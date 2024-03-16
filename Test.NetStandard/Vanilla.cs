using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.NetStandard.Vanilla;

internal class Dependency;

internal class Parent : IScopeRoot
{
    internal Parent(
        Dependency dependency)
    {
        
    }
}

[CreateFunction(typeof(Parent), "Create")]
internal sealed partial class Container
{
    [CustomScopeForRootTypes(typeof(Parent))]
    private sealed partial class DIE_Scope_Parent;
}

public class Tests
{
    [Fact]
    public void Test()
    {
        //using var container = Container.DIE_CreateContainer();
        //var instance = container.Create();
    }
}