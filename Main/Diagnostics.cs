namespace MrMeeseeks.DIE;

public static class Diagnostics
{
    public static Diagnostic CircularReferenceInsideFactory => 
        Diagnostic.Create(new DiagnosticDescriptor($"{Constants.DieAbbreviation}_68_00", 
            "Circular Reference Exception (inside factory)",
            "This container and/or its configuration lead to a circular reference inside one of its factory functions, which need to be generated.", 
            "Error", DiagnosticSeverity.Error, 
            true),
            Location.None);
    
    public static Diagnostic CircularReferenceAmongFactories => 
        Diagnostic.Create(new DiagnosticDescriptor($"{Constants.DieAbbreviation}_68_01", 
                "Circular Reference Exception (among factories)",
                "This container and/or its configuration lead to a circular reference among factory functions, which need to be generated.", 
                "Error", DiagnosticSeverity.Error, 
                true),
            Location.None);
    
    public static Diagnostic ValidationContainer(INamedTypeSymbol container, string specification) => 
        Diagnostic.Create(new DiagnosticDescriptor($"{Constants.DieAbbreviation}_67_00", 
                "Validation (Container)",
                $"The Container \"{container.Name}\" isn't validly defined: {specification}", 
                "Error", DiagnosticSeverity.Error, 
                true),
            container.Locations.FirstOrDefault() ?? Location.None);
    
    public static Diagnostic ValidationTransientScope(INamedTypeSymbol transientScope, INamedTypeSymbol parentContainer, string specification) => 
        Diagnostic.Create(new DiagnosticDescriptor($"{Constants.DieAbbreviation}_67_01", 
                "Validation (TransientScope)",
                $"The TransientScope \"{transientScope.Name}\" (of parent-Container \"{parentContainer.Name}\") isn't validly defined: {specification}", 
                "Error", DiagnosticSeverity.Error, 
                true),
            transientScope.Locations.FirstOrDefault() ?? Location.None);
    
    public static Diagnostic ValidationScope(INamedTypeSymbol scope, INamedTypeSymbol parentContainer, string specification) => 
        Diagnostic.Create(new DiagnosticDescriptor($"{Constants.DieAbbreviation}_67_02", 
                "Validation (Scope)",
                $"The Scope \"{scope.Name}\" (of parent-Container \"{parentContainer.Name}\") isn't validly defined: {specification}", 
                "Error", DiagnosticSeverity.Error, 
                true),
            scope.Locations.FirstOrDefault() ?? Location.None);
    
    public static Diagnostic ValidationUserDefinedElement(ISymbol userDefinedElement, INamedTypeSymbol parentRange, INamedTypeSymbol parentContainer, string specification)
    {
        var rangeDescription = SymbolEqualityComparer.Default.Equals(parentRange, parentContainer)
            ? $"parent-Container \"{parentContainer.Name}\""
            : $"Range \"{parentRange.Name}\" in parent-Container \"{parentContainer.Name}\"";
        return Diagnostic.Create(new DiagnosticDescriptor($"{Constants.DieAbbreviation}_67_03",
                "Validation (User-Defined Element)",
                $"The user-defined \"{userDefinedElement.Name}\" (of \"{rangeDescription}) isn't validly defined: {specification}",
                "Error", DiagnosticSeverity.Error,
                true),
            userDefinedElement.Locations.FirstOrDefault() ?? Location.None);
    }
}