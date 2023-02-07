namespace MrMeeseeks.DIE.Nodes.Ranges;

internal record UngenericToGenericData(
    string ReturnTypeFullName,
    string FunctionName,
    string UngenericParameterTypeFullName,
    string UngenericParameterName,
    string ResultParameterName);

internal record GenericToGenericData(
    string ReturnTypeFullName,
    string FunctionName,
    string ParameterTypeFullName,
    string ParameterName);

internal interface ITaskTransformationFunctions
{
    UngenericToGenericData UngenericValueTaskToGenericValueTask { get; }
    UngenericToGenericData UngenericValueTaskToGenericTask { get; }
    UngenericToGenericData UngenericTaskToGenericTask { get; }
    UngenericToGenericData UngenericTaskToGenericValueTask { get; }
    GenericToGenericData GenericValueTaskToGenericTask { get; }
    GenericToGenericData GenericTaskToGenericValueTask { get; }
}

internal class TaskTransformationFunctions : ITaskTransformationFunctions
{
    public TaskTransformationFunctions(
        IReferenceGenerator referenceGenerator,
        
        WellKnownTypes wellKnownTypes)
    {
        var genericValueTaskFullName = $"{wellKnownTypes.ValueTask.FullName()}<T>";
        var ungenericValueTaskFullName = wellKnownTypes.ValueTask.FullName();
        var genericTaskFullName = $"{wellKnownTypes.Task.FullName()}<T>";
        var ungenericTaskFullName = wellKnownTypes.Task.FullName();
        UngenericValueTaskToGenericValueTask = new UngenericToGenericData(
            genericValueTaskFullName,
            referenceGenerator.Generate(nameof(UngenericValueTaskToGenericValueTask)),
            ungenericValueTaskFullName,
            referenceGenerator.Generate("valueTask"),
            referenceGenerator.Generate("result"));
        UngenericValueTaskToGenericTask = new UngenericToGenericData(
            genericTaskFullName,
            referenceGenerator.Generate(nameof(UngenericValueTaskToGenericTask)),
            ungenericValueTaskFullName,
            referenceGenerator.Generate("valueTask"),
            referenceGenerator.Generate("result"));
        UngenericTaskToGenericTask = new UngenericToGenericData(
            genericTaskFullName,
            referenceGenerator.Generate(nameof(UngenericTaskToGenericTask)),
            ungenericTaskFullName,
            referenceGenerator.Generate("task"),
            referenceGenerator.Generate("result"));
        UngenericTaskToGenericValueTask = new UngenericToGenericData(
            genericValueTaskFullName,
            referenceGenerator.Generate(nameof(UngenericTaskToGenericValueTask)),
            ungenericTaskFullName,
            referenceGenerator.Generate("task"),
            referenceGenerator.Generate("result"));
        GenericValueTaskToGenericTask = new GenericToGenericData(
            genericTaskFullName,
            referenceGenerator.Generate(nameof(GenericValueTaskToGenericTask)),
            genericValueTaskFullName,
            referenceGenerator.Generate("valueTask"));
        GenericTaskToGenericValueTask = new GenericToGenericData(
            genericValueTaskFullName,
            referenceGenerator.Generate(nameof(GenericTaskToGenericValueTask)),
            genericTaskFullName,
            referenceGenerator.Generate("task"));
    }

    public UngenericToGenericData UngenericValueTaskToGenericValueTask { get; }
    public UngenericToGenericData UngenericValueTaskToGenericTask { get; }
    public UngenericToGenericData UngenericTaskToGenericTask { get; }
    public UngenericToGenericData UngenericTaskToGenericValueTask { get; }
    public GenericToGenericData GenericValueTaskToGenericTask { get; }
    public GenericToGenericData GenericTaskToGenericValueTask { get; }
}