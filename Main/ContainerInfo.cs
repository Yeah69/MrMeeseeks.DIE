using System.Linq;
using Microsoft.CodeAnalysis;

namespace MrMeeseeks.DIE
{
    public interface IContainerInfo
    {
        string Name { get; }
        string Namespace { get; }
        bool IsValid { get; }
        INamedTypeSymbol? ResolutionRootType { get; }
    }

    internal class ContainerInfo : IContainerInfo
    {
        public ContainerInfo(
            // parameters
            INamedTypeSymbol containerClass,
            
            // dependencies
            WellKnownTypes wellKnowTypes)
        {
            Name = containerClass.Name;
            Namespace = containerClass.ContainingNamespace.FullName();
            
            var namedTypeSymbol = containerClass.AllInterfaces.Single(x => x.OriginalDefinition.Equals(wellKnowTypes.Container, SymbolEqualityComparer.Default));
            var typeParameterSymbol = namedTypeSymbol.TypeArguments.Single();
            if (typeParameterSymbol is INamedTypeSymbol { IsUnboundGenericType: false } type && type.IsAccessibleInternally())
            {
                ResolutionRootType = type;
                IsValid = true;
            }
            else
            {
                IsValid = false;
            }
        }

        public string Name { get; }
        public string Namespace { get; }
        public bool IsValid { get; }
        public INamedTypeSymbol? ResolutionRootType { get; }
    }
}