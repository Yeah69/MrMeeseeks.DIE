using Microsoft.CodeAnalysis;
using System;

namespace MrMeeseeks.DIE.Spy
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            Func<ISyntaxReceiver> syntaxReceiverFactory = () => new SyntaxReceiver();
            new InitializeImpl(context, syntaxReceiverFactory).Initialize();
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var diagLogger = new DiagLogger(context);
            var getAllImplementations = new GetAllImplementations(context);
            var containerGenerator = new ContainerGenerator(context, getAllImplementations);
            new ExecuteImpl(context, containerGenerator, diagLogger).Execute();
        }
    }
}
