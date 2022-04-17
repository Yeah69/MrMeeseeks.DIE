using System;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.TestChild;
using TestChild;
using Xunit;

namespace MrMeeseeks.DIE.Test.Spy.Implementations;

[SpyAggregation(typeof(IPublicTypeReport))]
[SpyConstructorChoiceAggregation(
    typeof(PublicConstructorReport.global__MrMeeseeks_DIE_TestChild_Class._),
    typeof(PublicConstructorReport.global__MrMeeseeks_DIE_TestChild_ClassToo.Int32))]
[CreateFunction(typeof(Class), "CreateClass")]
[CreateFunction(typeof(Func<int, ClassToo>), "CreateClassToo")]
internal partial class Container
{
    
}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        var _ = container.CreateClass();
        var __ = container.CreateClassToo()(69);
    }
}