

using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE.Contexts;

internal interface IContainerWideContext
{
    WellKnownTypes WellKnownTypes { get; }
    WellKnownTypesAggregation WellKnownTypesAggregation { get; }
    WellKnownTypesChoice WellKnownTypesChoice { get; }
    WellKnownTypesCollections WellKnownTypesCollections { get; }
    WellKnownTypesMiscellaneous WellKnownTypesMiscellaneous { get; }
    WellKnownTypesMapping WellKnownTypesMapping { get; }
}

internal class ContainerWideContext(WellKnownTypes wellKnownTypes,
        WellKnownTypesAggregation wellKnownTypesAggregation,
        WellKnownTypesChoice wellKnownTypesChoice,
        WellKnownTypesCollections wellKnownTypesCollections,
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous,
        WellKnownTypesMapping wellKnownTypesMapping)
    : IContainerWideContext, IContainerInstance
{
    public WellKnownTypes WellKnownTypes { get; } = wellKnownTypes;
    public WellKnownTypesAggregation WellKnownTypesAggregation { get; } = wellKnownTypesAggregation;
    public WellKnownTypesChoice WellKnownTypesChoice { get; } = wellKnownTypesChoice;
    public WellKnownTypesCollections WellKnownTypesCollections { get; } = wellKnownTypesCollections;
    public WellKnownTypesMiscellaneous WellKnownTypesMiscellaneous { get; } = wellKnownTypesMiscellaneous;
    public WellKnownTypesMapping WellKnownTypesMapping { get; } = wellKnownTypesMapping;
}