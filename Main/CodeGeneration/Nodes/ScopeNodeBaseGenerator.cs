using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.CodeGeneration.Nodes;

internal interface IScopeNodeBaseGenerator : IRangeNodeGenerator
{
    
}

internal abstract class ScopeNodeBaseGenerator : RangeNodeGenerator, IScopeNodeBaseGenerator
{
    protected ScopeNodeBaseGenerator(
        IRangeNode rangeNode,
        IContainerNode containerNode,
        IDisposeUtility disposeUtility,
        WellKnownTypes wellKnownTypes,
        WellKnownTypesCollections wellKnownTypesCollections)
        : base(
            rangeNode,
            containerNode,
            disposeUtility,
            wellKnownTypes,
            wellKnownTypesCollections)
    {
    }

    protected override string ClassDeclaredAccessibility => "private ";

    protected override string DefaultConstructorDeclaredAccessibility => "internal ";
}