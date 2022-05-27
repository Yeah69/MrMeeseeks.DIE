using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.SampleChild;


namespace MrMeeseeks.DIE.Test.Implementation.Aggregation.AssemblyImplementationsAggregation;

[FilterAllImplementationsAggregation]
[AssemblyImplementationsAggregation(typeof(MrMeeseeks.DIE.SampleChild.AssemblyInfo))]
[ConstructorChoice(typeof(Parent.ClassToo))]
[CreateFunction(typeof(Parent.ClassToo), "Create")]
internal partial class Container {}