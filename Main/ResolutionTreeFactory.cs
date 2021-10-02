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
        private int _id = -1;

        public ResolutionTreeFactory(
            ITypeToImplementationsMapper typeToImplementationsMapper)
        {
            _typeToImplementationsMapper = typeToImplementationsMapper;
        }
        
        public ResolutionBase Create(INamedTypeSymbol type)
        {
            if (type.TypeKind == TypeKind.Interface)
            {
                var implementationType = _typeToImplementationsMapper
                    .Map(type)
                    .FirstOrDefault() ?? throw new NotImplementedException("What if several possible implementations exist");
                return new InterfaceResolution(
                    $"_{++_id}",
                    type.FullName(),
                    Create(implementationType));
            }
            if (type.TypeKind == TypeKind.Class)
            {
                var implementationType = _typeToImplementationsMapper
                    .Map(type)
                    .FirstOrDefault() ?? throw new NotImplementedException("What if several possible implementations exist");
                var constructor = implementationType.Constructors.FirstOrDefault()
                    ?? throw new NotImplementedException("What if no constructor exists or several possible constructors exist");
                return new ConstructorResolution(
                    $"_{++_id}",
                    implementationType.FullName(),
                    new ReadOnlyCollection<ResolutionBase>(
                        constructor.Parameters.Select(p => Create(p.Type as INamedTypeSymbol ?? throw new NotImplementedException("What if parameter type is not INamedTypeSymbol?"))).ToList()));
            }

            throw new NotImplementedException("What if type neither interface nor class");
        }
    }
}