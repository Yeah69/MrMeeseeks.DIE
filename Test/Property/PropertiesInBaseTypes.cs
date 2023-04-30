using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Property.PropertiesInBaseTypes;

internal abstract class Base
{
    public int PropProtectedInit { get; protected init; }
    public virtual required string PropVirtualOverriden { get; init; }
    public virtual required string PropVirtualNotOverriden { get; init; }
    public abstract required string PropAbstractOverriden { get; init; }
    public string? PropNewed { get; init; }
    public string? PropNewedRequired { get; init; }
}

internal abstract class IntermediateBase : Base
{
    public override required string PropVirtualOverriden { get; init; }
    public new string? PropNewed { get; init; }
    public new required string PropNewedRequired { get; init; }
}

internal class Dependency : IntermediateBase
{
    internal Dependency() => PropProtectedInit = 9;
    public override required string PropAbstractOverriden { get; init; }
    public required string PropString { get; init; }
}

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container
{
    // ReSharper disable once InconsistentNaming
    private readonly string DIE_Factory_Yeah = "Yeah";
    // ReSharper disable once InconsistentNaming
    private readonly string? DIE_Factory_YeahNullable = "YeahNullable";
    
    private Container() {}
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var _ = container.Create();
    }
}