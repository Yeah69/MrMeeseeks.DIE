using System;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.SampleChild;
using SampleChild;

namespace MrMeeseeks.DIE.Test.Spy.Implementations;

[SpyAggregation(typeof(IPublicTypeReport))]
[SpyConstructorChoiceAggregation(
    typeof(PublicConstructorReport.global__MrMeeseeks_DIE_SampleChild_Class._),
    typeof(PublicConstructorReport.global__MrMeeseeks_DIE_SampleChild_ClassToo.Int32))]
[CreateFunction(typeof(Class), "CreateClass")]
[CreateFunction(typeof(Func<int, ClassToo>), "CreateClassToo")]
internal partial class Container
{
    
}
