using Microsoft.CodeAnalysis;

namespace MrMeeseeks.DIE.Spy
{
    internal interface IExecute
    {
        void Execute();
    }

    internal class ExecuteImpl : IExecute
    {
        private readonly GeneratorExecutionContext _context;
        private readonly IContainerGenerator _containerGenerator;
        private readonly IDiagLogger _diagLogger;

        public ExecuteImpl(
            GeneratorExecutionContext context,
            IContainerGenerator containerGenerator,
            IDiagLogger diagLogger)
        {
            _context = context;
            _containerGenerator = containerGenerator;
            _diagLogger = diagLogger;
        }

        public void Execute()
        {
            _diagLogger.Log(0, "Start Execute");

            _containerGenerator.Generate(
                _context.Compilation.Assembly.Name);

            _diagLogger.Log(2, "End Execute");
        }
    }
}
