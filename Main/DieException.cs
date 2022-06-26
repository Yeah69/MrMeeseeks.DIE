namespace MrMeeseeks.DIE;

public enum DieExceptionKind
{
    ImplementationCycle,
    FunctionCycle,
    Validation,
    SlippedResolutionError
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
    public override DieExceptionKind Kind => DieExceptionKind.FunctionCycle;
}

public class ValidationDieException : DieException
{
    public IImmutableList<Diagnostic> Diagnostics { get; }

    public ValidationDieException(IImmutableList<Diagnostic> diagnostics) => Diagnostics = diagnostics;

    public override DieExceptionKind Kind => DieExceptionKind.Validation;
}

public class SlippedResolutionDieException : DieException
{
    public string ErrorMessage { get; }

    public SlippedResolutionDieException(string errorMessage) => ErrorMessage = errorMessage;
    public override DieExceptionKind Kind => DieExceptionKind.SlippedResolutionError;
}