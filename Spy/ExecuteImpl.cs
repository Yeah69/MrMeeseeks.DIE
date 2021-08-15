using Microsoft.CodeAnalysis;

namespace MrMeeseeks.DIE.Spy
{
    internal interface IExecute
    {
        void Execute();
    }

    internal class ExecuteImpl : IExecute
    {
        private readonly GeneratorExecutionContext context;
        private readonly IContainerGenerator containerGenerator;
        private readonly IDiagLogger diagLogger;

        public ExecuteImpl(
            GeneratorExecutionContext context,
            IContainerGenerator containerGenerator,
            IDiagLogger diagLogger)
        {
            this.context = context;
            this.containerGenerator = containerGenerator;
            this.diagLogger = diagLogger;
        }

        public void Execute()
        {
            diagLogger.Log(0, "Start Execute");

            containerGenerator.Generate(
                context.Compilation.Assembly.Name);

            diagLogger.Log(2, "End Execute");
        }
    }
}
