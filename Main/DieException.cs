namespace MrMeeseeks.DIE;

public enum DieExceptionKind
{
    ImplementationCycle,
    FunctionCycle
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