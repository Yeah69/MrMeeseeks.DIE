namespace MrMeeseeks.DIE;

public enum DieExceptionKind
{
    // ReSharper disable once InconsistentNaming
    NoneDIE,
    ImplementationCycle,
    FunctionCycle,
    InitializedInstanceCycle,
    Validation,
    Resolution,
    Compilation,
    Impossible
}

internal enum ExecutionPhase
{
    ContainerValidation,
    Resolution,
    ResolutionValidation,
    CodeGeneration,
    Analytics
}

public abstract class DieException : Exception
{
    public abstract DieExceptionKind Kind { get; }

    protected DieException() { }
    protected DieException(string message) : base(message) { }
    protected DieException(string message, Exception innerException) : base(message, innerException) { }
}

#pragma warning disable CA1032 // This exception class needs to be initiliazed with a cycle. The standard exception constructors would not be suitable for this class.
public sealed class ImplementationCycleDieException : DieException
#pragma warning restore CA1032
{
    public override DieExceptionKind Kind => DieExceptionKind.ImplementationCycle;
    public IImmutableStack<INamedTypeSymbol> Cycle { get; }
    
    public ImplementationCycleDieException(IImmutableStack<INamedTypeSymbol> cycle) => Cycle = cycle;
}

#pragma warning disable CA1032 // This exception class needs to be initiliazed with a cycle. The standard exception constructors would not be suitable for this class.
public sealed class FunctionCycleDieException : DieException
#pragma warning restore CA1032
{
    public override DieExceptionKind Kind => DieExceptionKind.FunctionCycle;
    public IImmutableStack<string> Cycle { get; }
    
    public FunctionCycleDieException(IImmutableStack<string> cycleFunctionDescriptions) => Cycle = cycleFunctionDescriptions;
}

#pragma warning disable CA1032 // This exception class needs to be initiliazed with a cycle. The standard exception constructors would not be suitable for this class.
public class InitializedInstanceCycleDieException : DieException
#pragma warning restore CA1032
{
    public override DieExceptionKind Kind => DieExceptionKind.InitializedInstanceCycle;
    public IImmutableStack<string> Cycle { get; }
    
    public InitializedInstanceCycleDieException(IImmutableStack<string> cycleFunctionDescriptions) => Cycle = cycleFunctionDescriptions;
}

public sealed class ValidationDieException : DieException
{
    public override DieExceptionKind Kind => DieExceptionKind.Validation;

    public ValidationDieException() { }
    public ValidationDieException(string message) : base(message) { }
    public ValidationDieException(string message, Exception innerException) : base(message, innerException) { }
}

public class ImpossibleDieException : DieException
{
    public override DieExceptionKind Kind => DieExceptionKind.Impossible;
    
    public ImpossibleDieException() { }
    public ImpossibleDieException(string message) : base(message) { }
    public ImpossibleDieException(string message, Exception innerException) : base(message, innerException) { }
}