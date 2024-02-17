

using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE.Contexts;

internal interface IContainerWideContext
{
    WellKnownTypes WellKnownTypes { get; }
    WellKnownTypesAggregation WellKnownTypesAggregation { get; }
    WellKnownTypesChoice WellKnownTypesChoice { get; }
    WellKnownTypesCollections WellKnownTypesCollections { get; }
    WellKnownTypesMiscellaneous WellKnownTypesMiscellaneous { get; }
    Compilation Compilation { get; }
}

internal sealed class ContainerWideContext : IContainerWideContext, IContainerInstance
{
    public ContainerWideContext(
        WellKnownTypes wellKnownTypes, 
        WellKnownTypesAggregation wellKnownTypesAggregation, 
        WellKnownTypesChoice wellKnownTypesChoice,
        WellKnownTypesCollections wellKnownTypesCollections, 
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous,
        Compilation compilation)
    {
        WellKnownTypes = wellKnownTypes;
        WellKnownTypesAggregation = wellKnownTypesAggregation;
        WellKnownTypesChoice = wellKnownTypesChoice;
        WellKnownTypesCollections = wellKnownTypesCollections;
        WellKnownTypesMiscellaneous = wellKnownTypesMiscellaneous;
        Compilation = compilation;
    }

    public WellKnownTypes WellKnownTypes { get; }
    public WellKnownTypesAggregation WellKnownTypesAggregation { get; }
    public WellKnownTypesChoice WellKnownTypesChoice { get; }
    public WellKnownTypesCollections WellKnownTypesCollections { get; }
    public WellKnownTypesMiscellaneous WellKnownTypesMiscellaneous { get; }
    public Compilation Compilation { get; }
}