namespace MrMeeseeks.DIE;

public enum DieExceptionKind
{
    ImplementationCycle,
    FunctionCycle,
    Validation
}

public abstract class DieException : Exception
{
    public abstract DieExceptionKind Kind { get; }
}

public class ImplementationCycleDieException : DieException
{
    public override DieExceptionKind Kind => DieExceptionKind.ImplementationCycle;
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