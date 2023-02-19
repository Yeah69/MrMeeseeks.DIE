using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Collection.Injection.IAsyncEnumerable;

internal interface IInterface {}

internal class ClassA : IInterface, IValueTaskInitializer
{
    public ValueTask InitializeAsync() => new();
}

internal class ClassB : IInterface, ITaskInitializer
{
    public Task InitializeAsync() => Task.CompletedTask;
}

internal class ClassC : IInterface {}

[CreateFunction(typeof(IAsyncEnumerable<IInterface>), "Create")]
internal sealed partial class Container {}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var _ = container.Create();
    }
}