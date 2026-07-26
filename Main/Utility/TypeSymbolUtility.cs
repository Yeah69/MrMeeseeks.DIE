using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Utility;

internal sealed class TypeSymbolUtility(WellKnownTypes wellKnownTypes) : IContainerInstance
{
    internal ITypeSymbol GetUnwrappedType(ITypeSymbol type)
    {
        if (IsOtherWrapType(type)
            && type is INamedTypeSymbol namedType)
            return GetUnwrappedType(namedType.TypeArguments.First());

        if (IsFuncDelegate(type) && type is INamedTypeSymbol func)
            return GetUnwrappedType(func.TypeArguments.Last());

        return type;
    }
    internal bool IsWrapType(ITypeSymbol type) =>
        IsFuncDelegate(type) || IsTaskType(type) || IsOtherWrapType(type);

    public bool IsTaskType(ITypeSymbol type) =>
        wellKnownTypes.ValueTask1 is not null && CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, wellKnownTypes.ValueTask1)
        || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, wellKnownTypes.Task1);

    public bool IsFuncDelegate(ITypeSymbol type) =>
        type.TypeKind == TypeKind.Delegate 
        && (CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, wellKnownTypes.Func1)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, wellKnownTypes.Func2)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, wellKnownTypes.Func3)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, wellKnownTypes.Func4)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, wellKnownTypes.Func5)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, wellKnownTypes.Func6)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, wellKnownTypes.Func7)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, wellKnownTypes.Func8)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, wellKnownTypes.Func9)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, wellKnownTypes.Func10)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, wellKnownTypes.Func11)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, wellKnownTypes.Func12)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, wellKnownTypes.Func13)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, wellKnownTypes.Func14)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, wellKnownTypes.Func15)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, wellKnownTypes.Func16)
            || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, wellKnownTypes.Func17));

    private bool IsOtherWrapType(ITypeSymbol type) =>
        CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, wellKnownTypes.Lazy1)
        || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, wellKnownTypes.ThreadLocal1);
}