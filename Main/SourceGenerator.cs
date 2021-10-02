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
            var getAssemblyAttributes = new GetAssemblyAttributes(context);
            var _ = WellKnownTypes.TryCreate(context.Compilation, out var wellKnownTypes);
            var typeToImplementationMapper = new TypeToImplementationsMapper(wellKnownTypes, diagLogger, getAllImplementations, getAssemblyAttributes);
            var containerGenerator = new ContainerGenerator(context, diagLogger);
            var resolutionTreeFactory = new ResolutionTreeFactory(typeToImplementationMapper);
            new ExecuteImpl(context, wellKnownTypes, containerGenerator, resolutionTreeFactory, ContainerInfoFactory, diagLogger).Execute();
            
            IContainerInfo ContainerInfoFactory(INamedTypeSymbol type) => new ContainerInfo(type, wellKnownTypes);
        }
    }
}
