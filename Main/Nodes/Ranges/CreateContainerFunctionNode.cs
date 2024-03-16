using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Ranges;

internal interface ICreateContainerFunctionNode : INode
{
    string Name { get; }
    IReadOnlyList<(string TypeFullName, string Reference)> Parameters { get; }
    string ContainerTypeFullName { get; }
    string ContainerReference { get; }
    string? InitializationFunctionName { get; }
    bool InitializationAwaited { get; }
    string ReturnTypeFullName { get; }
}

internal sealed partial class CreateContainerFunctionNode : ICreateContainerFunctionNode
{
    private readonly IVoidFunctionNode? _initializationFunction;
    private readonly INamedTypeSymbol _containerType;
    private readonly WellKnownTypes _wellKnownTypes;

    internal CreateContainerFunctionNode(
        // parameters
        IMethodSymbol? constructor, // Container can have no user-defined constructors (null then; in which case it will be treated like an empty constructor)
        IVoidFunctionNode? initializationFunction,
        
        // dependencies
        WellKnownTypes wellKnownTypes,
        IReferenceGenerator referenceGenerator,
        IContainerInfo containerInfo)
    {
        _initializationFunction = initializationFunction;
        Name = containerInfo.Name;
        ContainerTypeFullName = containerInfo.FullName;
        ContainerReference = referenceGenerator.Generate(containerInfo.ContainerType);
        Parameters = constructor?.Parameters.Select(ps => (ps.Type.FullName(), ps.Name)).ToList()
            ?? new List<(string, string)>();
        InitializationFunctionName = initializationFunction?.Name;
        _containerType = containerInfo.ContainerType;
        _wellKnownTypes = wellKnownTypes;
    }
    
    public void Build(PassedContext passedContext) { }

    public string Name { get; }
    public IReadOnlyList<(string TypeFullName, string Reference)> Parameters { get; }
    public string ContainerTypeFullName { get; }
    public string ContainerReference { get; }
    public string? InitializationFunctionName { get; }

    public bool InitializationAwaited =>
        _initializationFunction?.SynchronicityDecision is {} synchronicityDecision
        && synchronicityDecision != SynchronicityDecision.Sync;

    public string ReturnTypeFullName => InitializationAwaited
        ? (_wellKnownTypes.ValueTask1 ?? _wellKnownTypes.Task1).Construct(_containerType).FullName()
        : ContainerTypeFullName;
}