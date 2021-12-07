using System;
using MrMeeseeks.DIE;
//using SampleChild;

[assembly:ImplementationAggregation(typeof(DateTime))]
[assembly:ConstructorChoice(typeof(DateTime))]
//[assembly:SpyAggregation(typeof(IPublicTypeReport))]
//[assembly:SpyConstructorChoiceAggregationAttribute( PublicConstructorReport.ClassToo, PublicConstructorReport.Class_Int32)]

namespace MrMeeseeks.DIE.Sample;

internal partial class ConstructorChoiceContainer : IContainer<DateTime>
{
}