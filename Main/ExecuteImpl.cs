using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        private readonly IResolutionTreeFactory _resolutionTreeFactory;
        private readonly Func<INamedTypeSymbol, IContainerInfo> _containerInfoFactory;
        private readonly IDiagLogger _diagLogger;

        public ExecuteImpl(
            GeneratorExecutionContext context,
            WellKnownTypes wellKnownTypes,
            IContainerGenerator containerGenerator,
            IResolutionTreeFactory resolutionTreeFactory,
            Func<INamedTypeSymbol, IContainerInfo> containerInfoFactory,
            IDiagLogger diagLogger)
        {
            _context = context;
            _wellKnownTypes = wellKnownTypes;
            _containerGenerator = containerGenerator;
            _resolutionTreeFactory = resolutionTreeFactory;
            _containerInfoFactory = containerInfoFactory;
            _diagLogger = diagLogger;
        }

        public void Execute()
        {
            _diagLogger.Log("Start Execute");
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
                    var containerInfo = _containerInfoFactory(namedTypeSymbol);
                    if (containerInfo.IsValid && containerInfo.ResolutionRootType is { })
                    {
                        var resolutionRoot = _resolutionTreeFactory.Create(containerInfo.ResolutionRootType);
                        _containerGenerator.Generate(containerInfo, resolutionRoot);
                    }
                    else throw new NotImplementedException("Handle non-valid container information");
                }
            }
            
            _diagLogger.Log("End Execute");
        }
    }
}
