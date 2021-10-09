using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace MrMeeseeks.DIE
{
    internal interface IResolutionTreeFactory
    {
        ResolutionBase Create(ITypeSymbol root);
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

        public ResolutionBase Create(ITypeSymbol type) => Create(
            type,
            _referenceGeneratorFactory.Create(), 
            Array.Empty<(ITypeSymbol Type, FuncParameterResolution Resolution)>());
        
        private ResolutionBase Create(ITypeSymbol type, IReferenceGenerator referenceGenerator, IReadOnlyList<(ITypeSymbol Type, FuncParameterResolution Resolution)> currentFuncParameters)
        {
            if (currentFuncParameters.FirstOrDefault(t => SymbolEqualityComparer.Default.Equals(t.Type.OriginalDefinition, type.OriginalDefinition)) is { Type: not null, Resolution: not null } funcParameter)
            {
                return funcParameter.Resolution;
            }

            if (type.OriginalDefinition.Equals(_wellKnownTypes.Lazy1, SymbolEqualityComparer.Default)
                && type is INamedTypeSymbol namedTypeSymbol)
            {
                var genericType = namedTypeSymbol.TypeArguments.SingleOrDefault() as INamedTypeSymbol ??
                                  throw new NotImplementedException("What if not castable?");

                var dependency = Create(
                    genericType, 
                    _referenceGeneratorFactory.Create(), 
                    Array.Empty<(ITypeSymbol Type, FuncParameterResolution Resolution)>());
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
                                    Array.Empty<FuncParameterResolution>(),
                                    dependency)
                            )
                        }));
            }

            if (type.OriginalDefinition.Equals(_wellKnownTypes.Enumerable1, SymbolEqualityComparer.Default)
            || type.OriginalDefinition.Equals(_wellKnownTypes.ReadOnlyCollection1, SymbolEqualityComparer.Default)
            || type.OriginalDefinition.Equals(_wellKnownTypes.ReadOnlyList1, SymbolEqualityComparer.Default))
            {
                var itemType = (type as INamedTypeSymbol)?.TypeArguments.SingleOrDefault() ?? throw new Exception();
                var itemFullName = itemType.FullName();
                var items = _typeToImplementationsMapper
                    .Map(itemType)
                    .Select(i => Create(i, referenceGenerator, currentFuncParameters))
                    .ToList();
                return new CollectionResolution(
                    referenceGenerator.Generate(type),
                    type.FullName(),
                    itemFullName,
                    items);
            }

            if (type.TypeKind == TypeKind.Interface)
            {
                var implementationType = _typeToImplementationsMapper
                    .Map(type)
                    .SingleOrDefault() ?? throw new NotImplementedException($"What if several possible implementations exist (interface;{type.FullName()})");
                return new InterfaceResolution(
                    referenceGenerator.Generate(type),
                    type.FullName(),
                    Create(implementationType, referenceGenerator, currentFuncParameters));
            }

            if (type.TypeKind == TypeKind.Class)
            {
                var implementationType = _typeToImplementationsMapper
                    .Map(type)
                    .SingleOrDefault() as INamedTypeSymbol ?? throw new NotImplementedException($"What if several possible implementations exist (class;{type.FullName()})");
                var constructor = implementationType.Constructors.SingleOrDefault()
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
                                referenceGenerator,
                                currentFuncParameters)))
                        .ToList()));
            }

            if (type.TypeKind == TypeKind.Delegate 
                && type.FullName().StartsWith("global::System.Func<")
                && type is INamedTypeSymbol namedTypeSymbol0)
            {
                var returnType = namedTypeSymbol0.TypeArguments.Last();
                var innerReferenceGenerator = _referenceGeneratorFactory.Create();
                var parameterTypes = namedTypeSymbol0
                    .TypeArguments
                    .Take(namedTypeSymbol0.TypeArguments.Length - 1)
                    .Select(ts => (Type: ts, Resolution: new FuncParameterResolution(innerReferenceGenerator.Generate(ts), ts.FullName())))
                    .ToArray();

                var dependency = Create(
                    returnType, 
                    innerReferenceGenerator, 
                    parameterTypes);
                return new FuncResolution(
                    referenceGenerator.Generate(type),
                    type.FullName(),
                    parameterTypes.Select(t => t.Resolution).ToArray(),
                    dependency);
            }

            throw new NotImplementedException("What if type neither interface nor class nor delegate");
        }
    }
}