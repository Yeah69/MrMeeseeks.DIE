

using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE.Contexts;

internal interface IContainerWideContext
{
    WellKnownTypes WellKnownTypes { get; }
    WellKnownTypesAggregation WellKnownTypesAggregation { get; }
    WellKnownTypesChoice WellKnownTypesChoice { get; }
    WellKnownTypesCollections WellKnownTypesCollections { get; }
    WellKnownTypesMiscellaneous WellKnownTypesMiscellaneous { get; }
}

internal class ContainerWideContext : IContainerWideContext, IContainerInstance
{
    public ContainerWideContext(
        WellKnownTypes wellKnownTypes, 
        WellKnownTypesAggregation wellKnownTypesAggregation, 
        WellKnownTypesChoice wellKnownTypesChoice,
        WellKnownTypesCollections wellKnownTypesCollections, 
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous)
    {
        WellKnownTypes = wellKnownTypes;
        WellKnownTypesAggregation = wellKnownTypesAggregation;
        WellKnownTypesChoice = wellKnownTypesChoice;
        WellKnownTypesCollections = wellKnownTypesCollections;
        WellKnownTypesMiscellaneous = wellKnownTypesMiscellaneous;
    }

    public WellKnownTypes WellKnownTypes { get; }
    public WellKnownTypesAggregation WellKnownTypesAggregation { get; }
    public WellKnownTypesChoice WellKnownTypesChoice { get; }
    public WellKnownTypesCollections WellKnownTypesCollections { get; }
    public WellKnownTypesMiscellaneous WellKnownTypesMiscellaneous { get; }
}