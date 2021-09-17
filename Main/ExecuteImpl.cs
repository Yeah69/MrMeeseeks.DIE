using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        private readonly GeneratorExecutionContext _context;
        private readonly WellKnownTypes _wellKnownTypes;
        private readonly IContainerGenerator _containerGenerator;
        private readonly IDiagLogger _diagLogger;

        public ExecuteImpl(
            GeneratorExecutionContext context,
            WellKnownTypes wellKnownTypes,
            IContainerGenerator containerGenerator,
            IDiagLogger diagLogger)
        {
            _context = context;
            _wellKnownTypes = wellKnownTypes;
            _containerGenerator = containerGenerator;
            _diagLogger = diagLogger;
        }

        public void Execute()
        {
            _diagLogger.Log("Start Execute");
            if (_context
                .Compilation
                .GetTypeByMetadataName(typeof(ContainerAttribute).FullName ?? "") is not { } attributeType)
                return;
            foreach (var syntaxTree in _context.Compilation.SyntaxTrees)
            {
                var semanticModel = _context.Compilation.GetSemanticModel(syntaxTree);
                var containerClasses = syntaxTree
                    .GetRoot()
                    .DescendantNodesAndSelf()
                    .OfType<ClassDeclarationSyntax>()
                    .Select(x => semanticModel.GetDeclaredSymbol(x))
                    .Where(x => x != null)
                    .OfType<INamedTypeSymbol>()
                    .Where(x => x.AllInterfaces.Any(x => x.OriginalDefinition.Equals(_wellKnownTypes.Container, SymbolEqualityComparer.Default)));
                foreach (var namedTypeSymbol in containerClasses)
                {
                    _diagLogger.Log($"Container type {namedTypeSymbol.Name}");
                    _containerGenerator.Generate(namedTypeSymbol);
                }
            }
            
            _diagLogger.Log("End Execute");
        }
    }
}
