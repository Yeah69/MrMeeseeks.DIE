using System;
using Microsoft.CodeAnalysis;

namespace MrMeeseeks.DIE
{
    public interface IReferenceGenerator
    {
        string Generate(INamedTypeSymbol type);
        string Generate(string hardcodedName);
    }

    internal class ReferenceGenerator : IReferenceGenerator
    {
        private int _i = -1;
        private readonly int _j;

        public ReferenceGenerator(int j) => _j = j;

        public string Generate(INamedTypeSymbol type) => 
            Generate($"{char.ToLower(type.Name[0])}{type.Name.Substring(1)}");

        public string Generate(string hardcodedName) => 
            $"{hardcodedName}_{_j}_{++_i}";
    }
    
    public interface IReferenceGeneratorFactory
    {
        IReferenceGenerator Create();
    }

    internal class ReferenceGeneratorFactory : IReferenceGeneratorFactory
    {
        private readonly Func<int, IReferenceGenerator> _referenceGeneratorFactory;
        private int _j = -1;
        
        public ReferenceGeneratorFactory(Func<int, IReferenceGenerator> referenceGeneratorFactory) => _referenceGeneratorFactory = referenceGeneratorFactory;

        public IReferenceGenerator Create() => _referenceGeneratorFactory(++_j);
    }
}