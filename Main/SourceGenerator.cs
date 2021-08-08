using Microsoft.CodeAnalysis;
using MrMeeseeks.DIE;
using System;

namespace MrMeeseeks.StaticDelegateGenerator
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
            var typeToImplementationMapper = new TypeToImplementationsMapper(getAllImplementations);
            var containerGenerator = new ContainerGenrator(context, diagLogger, typeToImplementationMapper);
            new ExecuteImpl(context, containerGenerator, diagLogger).Execute();
        }
    }
}
