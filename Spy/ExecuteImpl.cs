namespace MrMeeseeks.DIE.Spy;

internal interface IExecute
{
    void Execute();
}

internal class ExecuteImpl : IExecute
{
    private readonly GeneratorExecutionContext _context;
    private readonly ITypeReportGenerator _typeReportGenerator;
    private readonly IDiagLogger _diagLogger;

    public ExecuteImpl(
        GeneratorExecutionContext context,
        ITypeReportGenerator typeReportGenerator,
        IDiagLogger diagLogger)
    {
        _context = context;
        _typeReportGenerator = typeReportGenerator;
        _diagLogger = diagLogger;
    }

    public void Execute()
    {
        _diagLogger.Log("Start Execute");

        _typeReportGenerator.Generate(
            _context.Compilation.Assembly.Name);

        _diagLogger.Log("End Execute");
    }
}