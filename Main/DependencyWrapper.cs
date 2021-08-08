﻿using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace MrMeeseeks.DIE
{
    internal enum ResolutionStage
    {
        Prefix,
        Postfix
    }

    internal record DependencyWrapper(
        ResolutionStage ResolutionStage, 
        int Id, 
        INamedTypeSymbol InjectedType, 
        INamedTypeSymbol ImplementationType,
        IReadOnlyList<int> ParameterIds);
}
