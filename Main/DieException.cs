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
}

public class ImplementationCycleDieException(IImmutableStack<INamedTypeSymbol> cycle) : DieException
{
    public override DieExceptionKind Kind => DieExceptionKind.ImplementationCycle;
    public IImmutableStack<INamedTypeSymbol> Cycle { get; } = cycle;
}

public class FunctionCycleDieException(IImmutableStack<string> cycleFunctionDescriptions) : DieException
{
    public override DieExceptionKind Kind => DieExceptionKind.FunctionCycle;
    public IImmutableStack<string> Cycle { get; } = cycleFunctionDescriptions;
}

public class InitializedInstanceCycleDieException(IImmutableStack<string> cycleFunctionDescriptions) : DieException
{
    public override DieExceptionKind Kind => DieExceptionKind.InitializedInstanceCycle;
    public IImmutableStack<string> Cycle { get; } = cycleFunctionDescriptions;
}

public class ValidationDieException : DieException
{
    public override DieExceptionKind Kind => DieExceptionKind.Validation;
}

public class ResolutionDieException(string errorMessage) : DieException
{
    public string ErrorMessage { get; } = errorMessage;

    public override DieExceptionKind Kind => DieExceptionKind.Resolution;
}

public class CompilationDieException(Diagnostic diagnostic) : DieException
{
    public Diagnostic Diagnostic { get; } = diagnostic;

    public override DieExceptionKind Kind => DieExceptionKind.Compilation;
}

public class ImpossibleDieException : DieException
{
    public override DieExceptionKind Kind => DieExceptionKind.Impossible;
}