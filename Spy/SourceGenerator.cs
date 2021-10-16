using Microsoft.CodeAnalysis;

namespace MrMeeseeks.DIE.Spy
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            IDiagLogger diagLogger = new DiagLogger(context);
            IGetAllImplementations getAllImplementations = new GetAllImplementations(context);
            ITypeReportGenerator typeReportGenerator = new TypeReportGenerator(context, getAllImplementations);
            IExecute execute = new ExecuteImpl(context, typeReportGenerator, diagLogger);
            
            execute.Execute();
        }
    }
}
