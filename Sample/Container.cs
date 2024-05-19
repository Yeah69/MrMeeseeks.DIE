using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

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
    [GenericParameterMapping(typeof(Dependency<,,>), "T0")]
    [GenericParameterMapping(typeof(Dependency<,,>), "T1")]
    [GenericParameterMapping(typeof(Dependency<,,>), "T2")] TA>
{
    [UserDefinedPropertiesInjection(typeof(Dependency<,,>))]
    private void DIE_Props_Value0(out TA Value0) => Value0 = (TA)(object)69;
    [UserDefinedConstructorParametersInjection(typeof(Dependency<,,>))]
    private void DIE_ConstrParams_Value1(out TA value1) => value1 = (TA)(object)3;
    [UserDefinedInitializerParametersInjection(typeof(Dependency<,,>))]
    private void DIE_InitParams_Value2(out TA value2) => value2 = (TA)(object)23;
}//*/
