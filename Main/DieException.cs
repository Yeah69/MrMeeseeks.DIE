using MrMeeseeks.DIE.ResolutionBuilding.Function;

namespace MrMeeseeks.DIE;

public enum DieExceptionKind
{
    NoneDIE,
    ImplementationCycle,
    FunctionCycle,
    Validation,
    Compilation
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
    public FunctionCycleDieException(IImmutableStack<FunctionResolutionBuilderHandle> cycle) => Cycle = cycle;

    public override DieExceptionKind Kind => DieExceptionKind.FunctionCycle;
    public IImmutableStack<FunctionResolutionBuilderHandle> Cycle { get; }
}

public class ValidationDieException : DieException
{
    public IImmutableList<Diagnostic> Diagnostics { get; }

    public ValidationDieException(IImmutableList<Diagnostic> diagnostics) => Diagnostics = diagnostics;

    public override DieExceptionKind Kind => DieExceptionKind.Validation;
}

public class CompilationDieException : DieException
{
    public Diagnostic Diagnostic { get; }

    public CompilationDieException(Diagnostic diagnostic) => Diagnostic = diagnostic;
    public override DieExceptionKind Kind => DieExceptionKind.Compilation;
}