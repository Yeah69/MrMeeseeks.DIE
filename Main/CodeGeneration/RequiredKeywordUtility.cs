using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.CodeGeneration;

internal interface IRequiredKeywordUtility
{
    void SetRequiredKeywordAsRequired();
    string? GenerateRequiredKeywordTypesFile();
}

internal sealed class RequiredKeywordUtility : IRequiredKeywordUtility, IContainerInstance
{
    private readonly Compilation _compilation;
    private readonly ICheckInternalsVisible _checkInternalsVisible;
    private bool _isRequiredKeywordRequired;

    internal RequiredKeywordUtility(
        GeneratorExecutionContext generatorExecutionContext,
        ICheckInternalsVisible checkInternalsVisible)
    {
        _compilation = generatorExecutionContext.Compilation;
        _checkInternalsVisible = checkInternalsVisible;
    }

    public void SetRequiredKeywordAsRequired() => _isRequiredKeywordRequired = true;
    public string? GenerateRequiredKeywordTypesFile()
    {
        var isExternalInit = CheckWhetherTypeIsAccessible("System.Runtime.CompilerServices.IsExternalInit");
        var requiredMemberAttribute = CheckWhetherTypeIsAccessible("System.Runtime.CompilerServices.RequiredMemberAttribute");
        var compilerFeatureRequiredAttribute = CheckWhetherTypeIsAccessible("System.Runtime.CompilerServices.CompilerFeatureRequiredAttribute");
        var setsRequiredMembersAttribute = CheckWhetherTypeIsAccessible("System.Diagnostics.CodeAnalysis.SetsRequiredMembersAttribute");

        if (!_isRequiredKeywordRequired 
            || isExternalInit && requiredMemberAttribute && compilerFeatureRequiredAttribute && setsRequiredMembersAttribute)
            return null;
        var code = new StringBuilder();
        if (!isExternalInit || !requiredMemberAttribute || !compilerFeatureRequiredAttribute)
        {
            code.AppendLine(
                """
                #nullable enable
                namespace System.Runtime.CompilerServices
                {
                """);

            if (!isExternalInit)
            {
                code.AppendLine(
                    """
                    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
                    internal static class IsExternalInit {}
                    """);
            }
            
            if (!requiredMemberAttribute)
            {
                code.AppendLine(
                    """
                    [global::System.AttributeUsage(global::System.AttributeTargets.Class | global::System.AttributeTargets.Struct | global::System.AttributeTargets.Field | global::System.AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
                    internal sealed class RequiredMemberAttribute : global::System.Attribute {}
                    """);
            }
            
            if (!compilerFeatureRequiredAttribute)
            {
                code.AppendLine(
                    """
                    [global::System.AttributeUsage(global::System.AttributeTargets.All, AllowMultiple = true, Inherited = false)]
                    internal sealed class CompilerFeatureRequiredAttribute : global::System.Attribute
                    {
                        public CompilerFeatureRequiredAttribute(string featureName)
                        {
                            FeatureName = featureName;
                        }
                    
                        public string FeatureName { get; }
                        public bool IsOptional { get; init; }
                    
                        public const string RefStructs = nameof(RefStructs);
                        public const string RequiredMembers = nameof(RequiredMembers);
                    }
                    """);
            }

            code.AppendLine("}");
        }
        
        if (!setsRequiredMembersAttribute)
        {
            code.AppendLine(
                """
                namespace System.Diagnostics.CodeAnalysis
                {
                [global::System.AttributeUsage(global::System.AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
                internal sealed class SetsRequiredMembersAttribute : global::System.Attribute {}
                }
                """);
        }
        
        code.AppendLine("#nullable disable");
        
        return code.ToString();

        bool CheckWhetherTypeIsAccessible(string fullyQualifiedName) =>
            _compilation.GetTypesByMetadataName(fullyQualifiedName).Any(t =>
                CustomSymbolEqualityComparer.Default.Equals(t.ContainingAssembly, _compilation.Assembly) &&
                t.DeclaredAccessibility is Accessibility.Internal or Accessibility.Public
                || t.DeclaredAccessibility is Accessibility.Public
                || t.DeclaredAccessibility is Accessibility.Internal && _checkInternalsVisible.Check(t.ContainingAssembly));
    }
}