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
    string ReturnTypeFullName { get; }
    ReturnTypeStatus ReturnTypeStatus { get; }
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
        Parameters = constructor?.Parameters.Select(ps => (ps.Type.FullName(), ps.Name)).ToList() ?? [];
        _containerType = containerInfo.ContainerType;
        _wellKnownTypes = wellKnownTypes;
    }
    
    public void Build(PassedContext passedContext) { }

    public string Name { get; }
    public IReadOnlyList<(string TypeFullName, string Reference)> Parameters { get; }
    public string ContainerTypeFullName { get; }
    public string ContainerReference { get; }

    public string? InitializationFunctionName
    {
        get
        {
            if (_initializationFunction is null)
                return null;
            var statuses = Enum.GetValues(typeof(ReturnTypeStatus)).OfType<ReturnTypeStatus>();
            foreach (var status in statuses)
                if (_initializationFunction.ReturnTypeStatus.HasFlag(status))
                    return _initializationFunction.Name(status);
            throw new InvalidOperationException("No valid return type status.");
        }
    }

    public string ReturnTypeFullName => _initializationFunction switch
    {
        { ReturnTypeStatus: var returnTypeStatus0 } 
            when returnTypeStatus0.HasFlag(ReturnTypeStatus.ValueTask) && _wellKnownTypes.ValueTask1 is not null => 
            _wellKnownTypes.ValueTask1.Construct(_containerType).FullName(),
        { ReturnTypeStatus: var returnTypeStatus1 } 
            when returnTypeStatus1.HasFlag(ReturnTypeStatus.Task) => 
            _wellKnownTypes.Task1.Construct(_containerType).FullName(),
        _ => ContainerTypeFullName
    };
    public ReturnTypeStatus ReturnTypeStatus => _initializationFunction?.ReturnTypeStatus ?? ReturnTypeStatus.Ordinary;
}