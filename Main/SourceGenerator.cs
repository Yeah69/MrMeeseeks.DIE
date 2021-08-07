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
            var getAllImplementations = new GetAllImplementations(context);
            var typeToImplementationMapper = new TypeToImplementationsMapper(getAllImplementations);
            new ExecuteImpl(context, typeToImplementationMapper).Execute();
        }
    }
}
