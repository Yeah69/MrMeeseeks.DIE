using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE.Utility;

internal interface IRequiredKeywordUtility
{
    void SetRequiredKeywordAsRequired();
    string? GenerateRequiredKeywordTypesFile();
}

internal sealed class RequiredKeywordUtility : IRequiredKeywordUtility, IContainerInstance
{
    private readonly WellKnownTypesMiscellaneous _wellKnownTypesMiscellaneous;
    private bool _isRequiredKeywordRequired;
    
    internal RequiredKeywordUtility(
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous)
    {
        _wellKnownTypesMiscellaneous = wellKnownTypesMiscellaneous;
    }

    public void SetRequiredKeywordAsRequired() => _isRequiredKeywordRequired = true;
    public string? GenerateRequiredKeywordTypesFile()
    {
        if (!_isRequiredKeywordRequired 
            || _wellKnownTypesMiscellaneous.IsExternalInit is not null 
            && _wellKnownTypesMiscellaneous.RequiredMemberAttribute is not null
            && _wellKnownTypesMiscellaneous.CompilerFeatureRequiredAttribute is not null
            && _wellKnownTypesMiscellaneous.SetsRequiredMembersAttribute is not null)
            return null;
        var code = new StringBuilder();
        if (_wellKnownTypesMiscellaneous.IsExternalInit is null
            || _wellKnownTypesMiscellaneous.RequiredMemberAttribute is null
            || _wellKnownTypesMiscellaneous.CompilerFeatureRequiredAttribute is null)
        {
            code.AppendLine(
                """
                namespace System.Runtime.CompilerServices
                {
                """);

            if (_wellKnownTypesMiscellaneous.IsExternalInit is null)
            {
                code.AppendLine(
                    """
                    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
                    internal static class IsExternalInit {}
                    """);
            }
            
            if (_wellKnownTypesMiscellaneous.RequiredMemberAttribute is null)
            {
                code.AppendLine(
                    """
                    [global::System.AttributeUsage(global::System.AttributeTargets.Class | global::System.AttributeTargets.Struct | global::System.AttributeTargets.Field | global::System.AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
                    internal sealed class RequiredMemberAttribute : global::System.Attribute {}
                    """);
            }
            
            if (_wellKnownTypesMiscellaneous.CompilerFeatureRequiredAttribute is null)
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
        
        if (_wellKnownTypesMiscellaneous.SetsRequiredMembersAttribute is null)
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
        
        return code.ToString();
    }
}