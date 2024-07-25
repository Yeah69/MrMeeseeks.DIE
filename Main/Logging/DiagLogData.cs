using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Logging;

internal record struct DiagLogData(
    int MajorNumber,
    int MinorNumber,
    string Title,
    string Message,
    DieExceptionKind? Kind);

internal static class ErrorLogData
{
    internal static DiagLogData CircularReferenceInsideFactory(IImmutableStack<INamedTypeSymbol> cycle)
    {
        var cycleText = string.Join(" --> ", cycle.Select(nts => nts.FullName()));

        return new DiagLogData(
            68,
            0,
            "Circular Reference Exception (inside factory)",
            $"This container and/or its configuration lead to a circular reference inside one of its factory functions, which need to be generated. The implementations involved in the cycle are: {cycleText}",
            DieExceptionKind.ImplementationCycle);
    }
    
    internal static DiagLogData CircularReferenceAmongFactories(IImmutableStack<string> cycle)
    {
        var cycleText = string.Join(" --> ", cycle);

        return new DiagLogData(
            68,
            1,
            "Circular Reference Exception (among factories)",
            $"This container and/or its configuration lead to a circular reference among factory functions, which need to be generated. The involved factory functions are: {cycleText}",
            DieExceptionKind.FunctionCycle);
    }
    
    internal static DiagLogData CircularReferenceAmongInitializedInstances(IImmutableStack<string> cycle)
    {
        var cycleText = string.Join(" --> ", cycle);

        return new DiagLogData(
            68,
            2,
            "Circular Reference Exception (among initialized instances)",
            $"This container and/or its configuration lead to a circular resolution of initialized instances, which need to be generated. The involved initialized instances are: {cycleText}",
            DieExceptionKind.InitializedInstanceCycle);
    }
    
    internal static DiagLogData ValidationContainer(INamedTypeSymbol container, string specification) =>
        new(67,
            0,
            "Validation (Container)",
            $"The container \"{container.Name}\" isn't validly defined: {specification}",
            DieExceptionKind.Validation);
    
    internal static DiagLogData ValidationTransientScope(INamedTypeSymbol transientScope, INamedTypeSymbol parentContainer, string specification) =>
        new(67,
            1,
            "Validation (Transient Scope)",
            $"The transient scope \"{transientScope.Name}\" (of parent-container \"{parentContainer.Name}\") isn't validly defined: {specification}",
            DieExceptionKind.Validation);
    
    internal static DiagLogData ValidationScope(INamedTypeSymbol scope, INamedTypeSymbol parentContainer, string specification) =>
        new(67,
            2,
            "Validation (Scope)",
            $"The scope \"{scope.Name}\" (of parent-container \"{parentContainer.Name}\") isn't validly defined: {specification}",
            DieExceptionKind.Validation);
    
    internal static DiagLogData ValidationBaseClass(INamedTypeSymbol baseType, INamedTypeSymbol range, string specification) =>
        new(67,
            2,
            "Validation (Scope)",
            $"The scope \"{baseType.Name}\" (of container/(transient) scope \"{range.Name}\") isn't validly defined: {specification}",
            DieExceptionKind.Validation);

    internal static DiagLogData ValidationUserDefinedElement(
        ISymbol userDefinedElement, 
        INamedTypeSymbol parentRange,
        INamedTypeSymbol parentContainer,
        string specification) =>
        ValidationUserDefinedElementInner(userDefinedElement, parentRange.Name, parentContainer.Name, specification);

    internal static DiagLogData ValidationUserDefinedElement(
        ISymbol userDefinedElement, 
        IRangeNode parentRange,
        IContainerNode parentContainer,
        string specification) =>
        ValidationUserDefinedElementInner(userDefinedElement, parentRange.Name, parentContainer.Name, specification);

    internal static DiagLogData ValidationUserDefinedElementInner(
        ISymbol userDefinedElement, 
        string parentRange,
        string parentContainer,
        string specification)
    {
        var rangeDescription = Equals(parentRange, parentContainer)
            ? $"parent-Container \"{parentContainer}\""
            : $"Range \"{parentRange}\" in parent-Container \"{parentContainer}\"";
        return new(67,
            3,
            "Validation (User-Defined Element)",
            $"The user-defined element \"{userDefinedElement.Name}\" (of \"{rangeDescription}\") isn't validly defined: {specification}",
            DieExceptionKind.Validation);
    }

    internal static DiagLogData ValidationConfigurationAttribute(
        AttributeData attributeData, 
        INamedTypeSymbol? parentRange, 
        INamedTypeSymbol? parentContainer, 
        string specification)
    {
        var rangeDescription = parentRange is null && parentContainer is null 
            ? "assembly level"
            : parentRange is null || parentContainer is null 
                ? "default Range"
                : CustomSymbolEqualityComparer.Default.Equals(parentRange, parentContainer)
                    ? $"parent-Container \"{parentContainer.Name}\""
                    : $"Range \"{parentRange.Name}\" in parent-Container \"{parentContainer.Name}\"";
        return new(67,
            4,
            "Validation (Configuration Attribute)",
            $"The configuration attribute \"{attributeData.AttributeClass?.Name}\" (of \"{rangeDescription}\") isn't validly defined: {specification}",
            DieExceptionKind.Validation);
    }
    
    internal static DiagLogData ValidationGeneral(string message) =>
        new(67,
            5,
            "Validation (General)",
            message,
            DieExceptionKind.Validation);
    
    internal static DiagLogData ValidationDescriptionType(INamedTypeSymbol methodDescriptionType, string specification) =>
        new(67,
            6,
            "Validation (Description Type)",
            $"Description type \"{methodDescriptionType.Name}\" isn't validly defined: {specification}",
            DieExceptionKind.Validation);
    
    internal static DiagLogData UnexpectedException(Exception exception) =>
        new(66,
            1,
            "Unexpected Exception (General)",
            exception.ToString(),
            DieExceptionKind.NoneDIE);
    
    internal static DiagLogData ImpossibleException(Guid code) =>
        new(66,
            2,
            "Impossible Exception",
            $"You've run into an exception which should be impossible. Please create an issue - if none exists for this code hint yet - at https://github.com/Yeah69/MrMeeseeks.DIE/issues/new with code hint \"{code.ToString()}\".",
            DieExceptionKind.Impossible);
    
    internal static DiagLogData CompilationError(string message) =>
        new(65,
            0,
            "Error During Compilation",
            message,
            DieExceptionKind.Compilation);

    internal static DiagLogData SyncToAsyncCallCompilationError(string message) =>
        new(65,
            1,
            "Sync Call To Async Function Error",
            message,
            DieExceptionKind.Resolution);

    internal static DiagLogData SyncDisposalInAsyncContainerCompilationError(string message) =>
        new(65,
            2,
            "Sync Disposal In Async Container Error",
            message,
            DieExceptionKind.Resolution);

    internal static DiagLogData ResolutionException(string message, ITypeSymbol currentType, ImmutableStack<INamedTypeSymbol> implementationStack)
    {
        var enhancedMessage = 
            $"[TS:{(implementationStack.IsEmpty ? "empty" : implementationStack.Peek().FullName())
            }][CT:{currentType.FullName()}] {message} [S:{
                (implementationStack.IsEmpty ? "empty" : string.Join("<==", implementationStack.Select(t => t.FullName())))}]";
        return new(64,
            0,
            "Exception During Resolution",
            enhancedMessage,
            DieExceptionKind.Resolution);
    }
}


internal static class WarningLogData
{
    internal static DiagLogData NullResolutionWarning(string message) =>
        new(70,
            0,
            "Null Resolution Warning",
            message,
            null);
    
    internal static DiagLogData EmptyReferenceNameWarning(string message) =>
        new(70,
            1,
            "Empty Reference Name Warning",
            message,
            null);
    
    internal static DiagLogData ImplementationHasMultipleInjectionsOfSameTypeWarning(string message) =>
        new(70,
            2,
            "Implementation Has Multiple Injections Of Same Type",
            message,
            null);

    internal static DiagLogData ValidationConfigurationAttribute(
        AttributeData attributeData, 
        INamedTypeSymbol? parentRange, 
        INamedTypeSymbol? parentContainer, 
        string specification)
    {
        var rangeDescription = parentRange is null && parentContainer is null 
            ? "assembly level"
            : parentRange is null || parentContainer is null 
                ? "default Range"
                : CustomSymbolEqualityComparer.Default.Equals(parentRange, parentContainer)
                    ? $"parent-Container \"{parentContainer.Name}\""
                    : $"Range \"{parentRange.Name}\" in parent-Container \"{parentContainer.Name}\"";
        return new(71,
            0,
            "Validation (Configuration Attribute)",
            $"The configuration attribute \"{attributeData.AttributeClass?.Name}\" (of \"{rangeDescription}\") isn't validly defined: {specification}",
            null);
    }

    internal static DiagLogData ValidationUserDefinedElement(
        ISymbol userDefinedElement, 
        IRangeNode parentRange,
        IContainerNode parentContainer,
        string specification) =>
        ValidationUserDefinedElementInner(userDefinedElement, parentRange.Name, parentContainer.Name, specification);

    internal static DiagLogData ValidationUserDefinedElementInner(
        ISymbol userDefinedElement, 
        string parentRange,
        string parentContainer,
        string specification)
    {
        var rangeDescription = Equals(parentRange, parentContainer)
            ? $"parent-Container \"{parentContainer}\""
            : $"Range \"{parentRange}\" in parent-Container \"{parentContainer}\"";
        return new(71,
            1,
            "Validation (User-Defined Element)",
            $"The user-defined element \"{userDefinedElement.Name}\" (of \"{rangeDescription}\") isn't validly defined: {specification}",
            DieExceptionKind.Validation);
    }
    
    internal static DiagLogData Logging(string message) =>
        new(0,
            0,
            "Logging",
            message,
            null);
}