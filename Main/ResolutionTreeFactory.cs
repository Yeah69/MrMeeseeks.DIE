using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace MrMeeseeks.DIE
{
    internal interface IResolutionTreeFactory
    {
        ResolutionBase Create(INamedTypeSymbol root);
    }

    internal class ResolutionTreeFactory : IResolutionTreeFactory
    {
        private readonly ITypeToImplementationsMapper _typeToImplementationsMapper;
        private readonly IReferenceGeneratorFactory _referenceGeneratorFactory;
        private readonly WellKnownTypes _wellKnownTypes;

        public ResolutionTreeFactory(
            ITypeToImplementationsMapper typeToImplementationsMapper,
            IReferenceGeneratorFactory referenceGeneratorFactory,
            WellKnownTypes wellKnownTypes)
        {
            _typeToImplementationsMapper = typeToImplementationsMapper;
            _referenceGeneratorFactory = referenceGeneratorFactory;
            _wellKnownTypes = wellKnownTypes;
        }

        public ResolutionBase Create(INamedTypeSymbol type) => Create(type, _referenceGeneratorFactory.Create());
        
        private ResolutionBase Create(INamedTypeSymbol type, IReferenceGenerator referenceGenerator)
        {
            if (type.TypeKind == TypeKind.Interface)
            {
                var implementationType = _typeToImplementationsMapper
                    .Map(type)
                    .FirstOrDefault() ?? throw new NotImplementedException($"What if several possible implementations exist (interface;{type.FullName()})");
                return new InterfaceResolution(
                    referenceGenerator.Generate(type),
                    type.FullName(),
                    Create(implementationType, referenceGenerator));
            }

            if (type.OriginalDefinition.Equals(_wellKnownTypes.Lazy1, SymbolEqualityComparer.Default))
            {
                var genericType = type.TypeArguments.FirstOrDefault() as INamedTypeSymbol ??
                                      throw new NotImplementedException("What if not castable?");

                var dependency = Create(genericType, _referenceGeneratorFactory.Create());
                return new ConstructorResolution(
                    referenceGenerator.Generate(type),
                    type.FullName(),
                    new ReadOnlyCollection<(string Name, ResolutionBase Dependency)>(
                        new List<(string Name, ResolutionBase Dependency)> 
                        { 
                            (
                                "valueFactory", 
                                new FuncResolution(
                                    referenceGenerator.Generate("func"),
                                    $"global::System.Func<{genericType.FullName()}>",
                                    dependency)
                            )
                        }));
            }

            if (type.TypeKind == TypeKind.Class)
            {
                var implementationType = _typeToImplementationsMapper
                    .Map(type)
                    .FirstOrDefault() ?? throw new NotImplementedException($"What if several possible implementations exist (class;{type.FullName()})");
                var constructor = implementationType.Constructors.FirstOrDefault()
                    ?? throw new NotImplementedException("What if no constructor exists or several possible constructors exist");
                
                return new ConstructorResolution(
                    referenceGenerator.Generate(implementationType),
                    implementationType.FullName(),
                    new ReadOnlyCollection<(string Name, ResolutionBase Dependency)>(constructor
                        .Parameters
                        .Select(p => (
                            p.Name, 
                            Create(p.Type as INamedTypeSymbol 
                                   ?? throw new NotImplementedException("What if parameter type is not INamedTypeSymbol?"),
                                referenceGenerator)))
                        .ToList()));
            }

            throw new NotImplementedException("What if type neither interface nor class");
        }
    }
}