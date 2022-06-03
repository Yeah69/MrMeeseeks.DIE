using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.CustomEmbedding.ConstructorParameterInScope;

internal class Dependency
{
    public int Number { get; }

    internal Dependency(int number) => Number = number;
}

internal class ScopeRoot : IScopeRoot
{
    public Dependency Dependency { get; }

    internal ScopeRoot(
        Dependency dependency) => Dependency = dependency;
}

[CreateFunction(typeof(ScopeRoot), "Create")]
internal partial class Container
{
    partial class DIE_DefaultScope
    {
        [CustomConstructorParameterChoice(typeof(Dependency))]
        private void DIE_ConstrParam_Dependency(out int number) => number = 69;
    }
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var instance = container.Create().Dependency;
        Assert.Equal(69, instance.Number);
    }
}