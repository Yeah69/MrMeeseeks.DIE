using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE;

internal static class Diagnostics
{
    private static string PhaseToString(ExecutionPhase phase) =>
        $"[{phase switch { ExecutionPhase.Validation => "Validation", ExecutionPhase.Resolution => "Resolution", ExecutionPhase.CycleDetection => "Cycle Detection", ExecutionPhase.CodeGeneration => "Code Generation", ExecutionPhase.ResolutionBuilding => "Resolution Building", _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, null) }}]";

    internal static Diagnostic CircularReferenceInsideFactory(ImplementationCycleDieException exception, ExecutionPhase phase)
    {
        var cycleText = string.Join(" --> ", exception.Cycle.Select(nts => nts.FullName()));
        
        return Diagnostic.Create(new DiagnosticDescriptor($"{Constants.DieAbbreviation}_68_00", 
                "Circular Reference Exception (inside factory)",
                $"[DIE] {PhaseToString(phase)} This container and/or its configuration lead to a circular reference inside one of its factory functions, which need to be generated. The implementations involved in the cycle are: {cycleText}", 
                "Error", 
                DiagnosticSeverity.Error, 
                true),
            Location.None);
    }
        
    
    internal static Diagnostic CircularReferenceAmongFactories(FunctionCycleDieException exception, ExecutionPhase phase)
    {
        var cycleText = string.Join(" --> ", exception.Cycle);
        
        return Diagnostic.Create(new DiagnosticDescriptor($"{Constants.DieAbbreviation}_68_01",
                "Circular Reference Exception (among factories)",
                $"[DIE] {PhaseToString(phase)} This container and/or its configuration lead to a circular reference among factory functions, which need to be generated. The involved factory functions are: {cycleText}",
                "Error",
                DiagnosticSeverity.Error,
                true),
            Location.None);
    }

    internal static Diagnostic ValidationContainer(INamedTypeSymbol container, string specification, ExecutionPhase phase) => 
        Diagnostic.Create(new DiagnosticDescriptor($"{Constants.DieAbbreviation}_67_00", 
                "Validation (Container)",
                $"[DIE] {PhaseToString(phase)} The Container \"{container.Name}\" isn't validly defined: {specification}", 
                "Error", 
                DiagnosticSeverity.Error, 
                true),
            container.Locations.FirstOrDefault() ?? Location.None);
    
    internal static Diagnostic ValidationContainer(INamedTypeSymbol container, string specification, Location location, ExecutionPhase phase) => 
        Diagnostic.Create(new DiagnosticDescriptor($"{Constants.DieAbbreviation}_67_00", 
                "Validation (Container)",
                $"[DIE] {PhaseToString(phase)} The Container \"{container.Name}\" isn't validly defined: {specification}", 
                "Error", 
                DiagnosticSeverity.Error, 
                true),
            location);
    
    internal static Diagnostic ValidationTransientScope(INamedTypeSymbol transientScope, INamedTypeSymbol parentContainer, string specification, ExecutionPhase phase) => 
        Diagnostic.Create(new DiagnosticDescriptor($"{Constants.DieAbbreviation}_67_01", 
                "Validation (TransientScope)",
                $"[DIE] {PhaseToString(phase)} The TransientScope \"{transientScope.Name}\" (of parent-Container \"{parentContainer.Name}\") isn't validly defined: {specification}", 
                "Error", 
                DiagnosticSeverity.Error, 
                true),
            transientScope.Locations.FirstOrDefault() ?? Location.None);
    
    internal static Diagnostic ValidationScope(INamedTypeSymbol scope, INamedTypeSymbol parentContainer, string specification, ExecutionPhase phase) => 
        Diagnostic.Create(new DiagnosticDescriptor($"{Constants.DieAbbreviation}_67_02", 
                "Validation (Scope)",
                $"[DIE] {PhaseToString(phase)} The Scope \"{scope.Name}\" (of parent-Container \"{parentContainer.Name}\") isn't validly defined: {specification}", 
                "Error", 
                DiagnosticSeverity.Error, 
                true),
            scope.Locations.FirstOrDefault() ?? Location.None);
    
    internal static Diagnostic ValidationUserDefinedElement(ISymbol userDefinedElement, INamedTypeSymbol parentRange, INamedTypeSymbol parentContainer, string specification, ExecutionPhase phase)
    {
        var rangeDescription = SymbolEqualityComparer.Default.Equals(parentRange, parentContainer)
            ? $"parent-Container \"{parentContainer.Name}\""
            : $"Range \"{parentRange.Name}\" in parent-Container \"{parentContainer.Name}\"";
        return Diagnostic.Create(new DiagnosticDescriptor($"{Constants.DieAbbreviation}_67_03",
                "Validation (User-Defined Element)",
                $"[DIE] {PhaseToString(phase)} The user-defined \"{userDefinedElement.Name}\" (of {rangeDescription}) isn't validly defined: {specification}",
                "Error", 
                DiagnosticSeverity.Error,
                true),
            userDefinedElement.Locations.FirstOrDefault() ?? Location.None);
    }
    
    internal static Diagnostic ValidationConfigurationAttribute(AttributeData attributeData, INamedTypeSymbol? parentRange, INamedTypeSymbol? parentContainer, string specification, ExecutionPhase phase)
    {
        var rangeDescription = parentRange is null && parentContainer is null 
            ? "assembly level"
            : parentRange is null || parentContainer is null 
                ? "default Range"
                : SymbolEqualityComparer.Default.Equals(parentRange, parentContainer)
                    ? $"parent-Container \"{parentContainer.Name}\""
                    : $"Range \"{parentRange.Name}\" in parent-Container \"{parentContainer.Name}\"";
        return Diagnostic.Create(new DiagnosticDescriptor($"{Constants.DieAbbreviation}_67_03",
                "Validation (Configuration Attribute)",
                $"[DIE] {PhaseToString(phase)} The configuration attribute \"{attributeData.AttributeClass?.Name}\" (of {rangeDescription}) isn't validly defined: {specification}",
                "Error", 
                DiagnosticSeverity.Error,
                true),
            attributeData.GetLocation());
    }
    
    internal static Diagnostic ValidationGeneral(string message) => 
        Diagnostic.Create(new DiagnosticDescriptor($"{Constants.DieAbbreviation}_67_02", 
                "Validation (General)",
                $"[DIE] {PhaseToString(ExecutionPhase.Validation)} {message}", 
                "Error", 
                DiagnosticSeverity.Error, 
                true),
            Location.None);
    
    internal static Diagnostic UnexpectedDieException(DieException exception, ExecutionPhase phase)
    {
        return Diagnostic.Create(new DiagnosticDescriptor($"{Constants.DieAbbreviation}_66_00", 
                "Unexpected Exception (DIE)",
                $"[DIE] {PhaseToString(phase)} {exception}", 
                "Error",
                DiagnosticSeverity.Error, 
                true),
            Location.None);
    }
    
    internal static Diagnostic UnexpectedException(Exception exception, ExecutionPhase phase)
    {
        return Diagnostic.Create(new DiagnosticDescriptor($"{Constants.DieAbbreviation}_66_01", 
                "Unexpected Exception (General)",
                $"[DIE] {PhaseToString(phase)} {exception}", 
                "Error", 
                DiagnosticSeverity.Error, 
                true),
            Location.None);
    }
    
    internal static Diagnostic ImpossibleException(ImpossibleDieException exception, ExecutionPhase phase)
    {
        return Diagnostic.Create(new DiagnosticDescriptor($"{Constants.DieAbbreviation}_66_02", 
                "Impossible Exception",
                $"[DIE] {PhaseToString(phase)} You've run into an impossible exception. Please make a issue at https://github.com/Yeah69/MrMeeseeks.DIE/issues/new with code hint \"{exception.Code.ToString()}\".", 
                "Error", 
                DiagnosticSeverity.Error, 
                true),
            Location.None);
    }
    
    internal static Diagnostic CompilationError(string message, Location location, ExecutionPhase phase)
    {
        return Diagnostic.Create(new DiagnosticDescriptor($"{Constants.DieAbbreviation}_65_00", 
                "Error During Compilation",
                $"[DIE] {PhaseToString(phase)} {message}", 
                "Error",
                DiagnosticSeverity.Error, 
                true),
            location);
    }
    
    internal static Diagnostic SyncToAsyncCallCompilationError(string message, ExecutionPhase phase)
    {
        return Diagnostic.Create(new DiagnosticDescriptor($"{Constants.DieAbbreviation}_65_01", 
                "Sync Call To Async Function Error",
                $"[DIE] {PhaseToString(phase)} {message}", 
                "Error",
                DiagnosticSeverity.Error, 
                true),
            Location.None);
    }
    
    internal static Diagnostic SyncDisposalInAsyncContainerCompilationError(string message, ExecutionPhase phase)
    {
        return Diagnostic.Create(new DiagnosticDescriptor($"{Constants.DieAbbreviation}_65_02", 
                "Sync Disposal In Async Container Error",
                $"[DIE] {PhaseToString(phase)} {message}", 
                "Error",
                DiagnosticSeverity.Error, 
                true),
            Location.None);
    }
    
    internal static Diagnostic IncompleteCompilationProcessingError(string message, ExecutionPhase phase)
    {
        return Diagnostic.Create(new DiagnosticDescriptor($"{Constants.DieAbbreviation}_65_02", 
                "Incomplete Compilation Processing Error",
                $"[DIE] {PhaseToString(phase)} {message}", 
                "Error",
                DiagnosticSeverity.Error, 
                true),
            Location.None);
    }
    
    internal static Diagnostic NullResolutionWarning(string message, ExecutionPhase phase) =>
        Diagnostic.Create(new DiagnosticDescriptor($"{Constants.DieAbbreviation}_70_00", 
                "Null Resolution Warning",
                $"[DIE] {PhaseToString(phase)} {message}", 
                "Warning",
                DiagnosticSeverity.Warning, 
                true),
            Location.None);
    
    internal static Diagnostic EmptyReferenceNameWarning(string message, ExecutionPhase phase) =>
        Diagnostic.Create(new DiagnosticDescriptor($"{Constants.DieAbbreviation}_70_01", 
                "Empty Reference Name Warning",
                $"[DIE] {PhaseToString(phase)} {message}", 
                "Warning",
                DiagnosticSeverity.Warning, 
                true),
            Location.None);

    internal static Diagnostic Logging(string message, ExecutionPhase phase) =>
        Diagnostic.Create(new DiagnosticDescriptor($"{Constants.DieAbbreviation}_00_00", 
                "Logging",
                $"[DIE] {PhaseToString(phase)} {message}", 
                "Log",
                DiagnosticSeverity.Warning, 
                true),
            Location.None);
}