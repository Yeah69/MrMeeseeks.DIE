using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Abstraction.ArrayType;

internal interface IInterface {}

internal class DependencyA : IInterface {}

internal class DependencyB : IInterface {}

internal class DependencyC : IInterface {}

[CreateFunction(typeof(IInterface[]), "Create")]
internal sealed partial class Container {}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var instance = container.Create();
        Assert.Equal(3, instance.Length);
        Assert.IsAssignableFrom<IInterface>(instance[0]);
        Assert.IsAssignableFrom<IInterface>(instance[0]);
        Assert.IsAssignableFrom<IInterface>(instance[0]);
    }
}