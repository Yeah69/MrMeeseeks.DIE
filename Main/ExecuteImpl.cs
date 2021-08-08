using Microsoft.CodeAnalysis;
using MrMeeseeks.StaticDelegateGenerator;
using System.Linq;

namespace MrMeeseeks.DIE
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
            if (context
                .Compilation
                .GetTypeByMetadataName(typeof(ContainerAttribute).FullName ?? "") is not { } attributeType)
                return;


            foreach (var attributeData in context
                .Compilation
                .Assembly
                .GetAttributes()
                .Where(ad => ad.AttributeClass?.Equals(attributeType, SymbolEqualityComparer.Default) ?? false))
                containerGenerator.Generate(attributeData);
            diagLogger.Log(2, "End Execute");
        }
    }
}
