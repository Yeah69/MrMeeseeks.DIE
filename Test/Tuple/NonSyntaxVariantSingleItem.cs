using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Tuple.NonSyntaxVariantSingleItem;

internal sealed class Wrapper
{
    public Wrapper(
        Tuple<int>
            dependency) =>
        Dependency = dependency;

    public Tuple<int>
        Dependency { get; }
}

[CreateFunction(typeof(Wrapper), "Create")]
internal sealed partial class Container
{
    private int _i;

    private int DIE_Factory_Counter() => _i++;
    
}

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var wrapper = container.Create();
        Assert.Equal(0, wrapper.Dependency.Item1);
    }
}