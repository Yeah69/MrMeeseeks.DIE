using System.Threading.Tasks;
using Xunit;
// ReSharper disable once CheckNamespace
using MrMeeseeks.DIE.Configuration.Attributes;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.TypeParameterMapping.CustomInjections;

internal sealed class Dependency<T0, T1, T2>
{
    private T1 _value1;
    private T2? _value2;
    internal required T0 Value0 { get; init; }
    internal T1 Value1 => _value1;
    internal T2? Value2 => _value2;

    internal Dependency(T1 value1) => _value1 = value1;
    public void Initialize(T2 value2) => _value2 = value2;
}

[Initializer(typeof(Dependency<,,>), "Initialize")]
[CreateFunction(typeof(Dependency<,,>), "Create")]
internal sealed partial class Container<
    [GenericParameterMapping(typeof(Dependency<,,>), "T0")] TA, 
    [GenericParameterMapping(typeof(Dependency<,,>), "T1")] TB,
    [GenericParameterMapping(typeof(Dependency<,,>), "T2")] TC>
{
    [UserDefinedPropertiesInjection(typeof(Dependency<,,>))]
    private void DIE_Props_Value0(out TA Value0) => Value0 = (TA)(object)69;
    [UserDefinedConstructorParametersInjection(typeof(Dependency<,,>))]
    private void DIE_ConstrParams_Value1(out TB value1) => value1 = (TB)(object)3L;
    [UserDefinedInitializerParametersInjection(typeof(Dependency<,,>))]
    private void DIE_InitParams_Value2(out TC value2) => value2 = (TC)(object)23.0;
}

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container<int, long, double>.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<Dependency<int, long, double>>(instance);
        Assert.Equal(69, instance.Value0);
        Assert.Equal(3L, instance.Value1);
        Assert.Equal(23.0, instance.Value2);
    }
}