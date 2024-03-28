namespace MrMeeseeks.DIE;

internal static class Constants
{
    // General
    internal const string DieAbbreviation = "DIE";
    internal const string ThisKeyword = "this";
    internal const string PublicKeyword = "public";
    internal const string InternalKeyword = "internal";
    internal const string PrivateKeyword = "private";
    
    // Ranges
    internal const string ScopeName = "Scope";
    internal const string DefaultScopeName = $"{DieAbbreviation}_Default{ScopeName}";
    internal const string CustomScopeName = $"{DieAbbreviation}_{ScopeName}";
    internal const string TransientScopeName = "TransientScope";
    internal const string DefaultTransientScopeName = $"{DieAbbreviation}_Default{TransientScopeName}";
    internal const string CustomTransientScopeName = $"{DieAbbreviation}_{TransientScopeName}";
    internal const string CreateContainerFunctionName = $"{DieAbbreviation}_CreateContainer";
    
    // User-defined scope elements
    internal const string UserDefinedFactory = $"{DieAbbreviation}_Factory";
    internal const string UserDefinedConstrParams = $"{DieAbbreviation}_ConstrParams";
    internal const string UserDefinedProps = $"{DieAbbreviation}_Props";
    internal const string UserDefinedInitParams = $"{DieAbbreviation}_InitParams";
    internal const string UserDefinedAddForDisposal = $"{DieAbbreviation}_AddForDisposal";
    internal const string UserDefinedAddForDisposalAsync = $"{DieAbbreviation}_AddForDisposalAsync";
    
    // Create Functions
    internal const string CreateFunctionSuffix = "";
    internal const string CreateFunctionSuffixAsync = "Async";
    internal const string CreateFunctionSuffixValueAsync = "ValueAsync";

    // ReSharper disable once InconsistentNaming
    internal const string IAsyncDisposableDisposeAsync = "DisposeAsync"; // Needed because .NET Standard 2.0 doesn't have IAsyncDisposable
    internal const string NamespaceForGeneratedStatics = "MrMeeseeks.DIE";
}