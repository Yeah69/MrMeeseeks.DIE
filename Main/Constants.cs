namespace MrMeeseeks.DIE;

internal static class Constants
{
    // General
    internal const string DieAbbreviation = "DIE";
    internal const string ThisKeyword = "this";
    
    // Ranges
    internal const string ScopeName = "Scope";
    internal const string DefaultScopeName = $"{DieAbbreviation}_Default{ScopeName}";
    internal const string CustomScopeName = $"{DieAbbreviation}_{ScopeName}";
    internal const string TransientScopeName = "TransientScope";
    internal const string DefaultTransientScopeName = $"{DieAbbreviation}_Default{TransientScopeName}";
    internal const string CustomTransientScopeName = $"{DieAbbreviation}_{TransientScopeName}";
    
    // User-defined scope elements
    internal const string UserDefinedFactory = $"{DieAbbreviation}_Factory";
    internal const string UserDefinedConstructorParameters = $"{DieAbbreviation}_ConstrParam";
    internal const string UserDefinedAddForDisposal = $"{DieAbbreviation}_AddForDisposal";
    internal const string UserDefinedAddForDisposalAsync = $"{DieAbbreviation}_AddForDisposalAsync";
    
    // Create Functions
    internal const string CreateFunctionSuffix = "";
    internal const string CreateFunctionSuffixAsync = "Async";
    internal const string CreateFunctionSuffixValueAsync = "ValueAsync";
}