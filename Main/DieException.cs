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
    Validation,
    Resolution,
    CycleDetection,
    ResolutionBuilding,
    CodeGeneration
}

public abstract class DieException : Exception
{
    public abstract DieExceptionKind Kind { get; }
}

public class ImplementationCycleDieException : DieException
{
    public ImplementationCycleDieException(IImmutableStack<INamedTypeSymbol> cycle) => Cycle = cycle;

    public override DieExceptionKind Kind => DieExceptionKind.ImplementationCycle;
    public IImmutableStack<INamedTypeSymbol> Cycle { get; }
}

public class FunctionCycleDieException : DieException
{
    public FunctionCycleDieException(IImmutableStack<string> cycleFunctionDescriptions) => Cycle = cycleFunctionDescriptions;

    public override DieExceptionKind Kind => DieExceptionKind.FunctionCycle;
    public IImmutableStack<string> Cycle { get; }
}

public class InitializedInstanceCycleDieException : DieException
{
    public InitializedInstanceCycleDieException(IImmutableStack<string> cycleFunctionDescriptions) => Cycle = cycleFunctionDescriptions;

    public override DieExceptionKind Kind => DieExceptionKind.InitializedInstanceCycle;
    public IImmutableStack<string> Cycle { get; }
}

public class ValidationDieException : DieException
{
    public IImmutableList<Diagnostic> Diagnostics { get; }

    public ValidationDieException(IImmutableList<Diagnostic> diagnostics) => Diagnostics = diagnostics;

    public override DieExceptionKind Kind => DieExceptionKind.Validation;
}

public class ResolutionDieException : DieException
{
    public string ErrorMessage { get; }
    public ResolutionDieException(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }

    public override DieExceptionKind Kind => DieExceptionKind.Resolution;
}

public class CompilationDieException : DieException
{
    public Diagnostic Diagnostic { get; }

    public CompilationDieException(Diagnostic diagnostic) => Diagnostic = diagnostic;
    public override DieExceptionKind Kind => DieExceptionKind.Compilation;
}

public class ImpossibleDieException : DieException
{
    public Guid Code { get; }

    public ImpossibleDieException(Guid code) => Code = code;
    public override DieExceptionKind Kind => DieExceptionKind.Impossible;
}