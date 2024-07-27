using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE;

internal interface IContainerInfo
{
    string Name { get; }
    string Namespace { get; }
    string FullName { get; }
    INamedTypeSymbol ContainerType { get; }
    IReadOnlyList<(ITypeSymbol, string, IReadOnlyList<ITypeSymbol>, Location)> CreateFunctionData { get; }
    ImmutableArray<string> ContainingTypeNames { get; }
    string GenerateHintPath(string suffix = "");
}

internal sealed class ContainerInfo : IContainerLevelOnlyContainerInstance
{
    private readonly string _hintPathPrefix;
    internal ContainerInfo(
        // parameters
        INamedTypeSymbol containerClass,
            
        // dependencies
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous,
        RangeUtility rangeUtility)
    {
        Name = containerClass.Name;
        Namespace = containerClass.ContainingNamespace.FullName();
        ContainerType = containerClass;

        CreateFunctionData = rangeUtility.GetRangeAttributes(containerClass)
            .Where(ad => CustomSymbolEqualityComparer.Default.Equals(wellKnownTypesMiscellaneous.CreateFunctionAttribute, ad.AttributeClass))
            .Select(ad => ad.ConstructorArguments is [
                              { Kind: TypedConstantKind.Type, Value: ITypeSymbol type },
                              { Kind: TypedConstantKind.Primitive, Value: string methodNamePrefix }, 
                              { Kind: TypedConstantKind.Array }]
                          && ad.ConstructorArguments[2].Values.Select(v => v.Value).All(v => v is ITypeSymbol)
                ? (
                    type, 
                    methodNamePrefix,
                    ad.ConstructorArguments[2].Values.Select(v => v.Value).OfType<ITypeSymbol>().ToList(),
                    ad.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? Location.None)
                : ((ITypeSymbol, string, IReadOnlyList<ITypeSymbol>, Location)?) null)
            .OfType<(ITypeSymbol, string, IReadOnlyList<ITypeSymbol>, Location)>()
            .ToList();

        var nestingStack = new Stack<INamedTypeSymbol>();
        var nestingParent = containerClass.ContainingType;
        while (nestingParent is not null)
        {
            nestingStack.Push(nestingParent);
            nestingParent = nestingParent.ContainingType;
        }

        var nesting = nestingStack.ToImmutableArray();

        ContainingTypeNames = [..nestingStack.Select(n => n.Name)];

        if (nesting is [var topmostAncestor, ..var remainingAncestors])
        {
            var fullNameBuilder =  new StringBuilder();
            fullNameBuilder.Append(topmostAncestor.FullName());
            foreach (var remainingAncestor in remainingAncestors)
            {
                fullNameBuilder.Append('.');
                fullNameBuilder.Append(remainingAncestor.Name);
            }
            fullNameBuilder.Append('.');
            fullNameBuilder.Append(containerClass.Name);
            FullName = fullNameBuilder.ToString();
        }
        else
            FullName = containerClass.FullName();
        
        var namespaceName = containerClass.ContainingNamespace.FullName();
        var nestingPart = ContainingTypeNames.Length > 0 ? $".{string.Join(".", ContainingTypeNames)}" : string.Empty;
        _hintPathPrefix = $"{namespaceName}{nestingPart}.{containerClass.Name}";
    }

    public string Name { get; }
    public string Namespace { get; }
    public string FullName { get; }
    public INamedTypeSymbol ContainerType { get; }
    public IReadOnlyList<(ITypeSymbol, string, IReadOnlyList<ITypeSymbol>, Location)> CreateFunctionData { get; }
    public ImmutableArray<string> ContainingTypeNames { get; }
    public string GenerateHintPath(string suffix = "") => 
        $"{_hintPathPrefix}{suffix}.g.cs";
}