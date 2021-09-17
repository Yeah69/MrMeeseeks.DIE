using Microsoft.CodeAnalysis;
using System;

namespace MrMeeseeks.DIE
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
            var _ = WellKnownTypes.TryCreate(context.Compilation, out var wellKnownTypes);
            var containerGenerator = new ContainerGenerator(context, diagLogger, wellKnownTypes, typeToImplementationMapper);
            new ExecuteImpl(context, wellKnownTypes, containerGenerator, diagLogger).Execute();
        }
    }
}
