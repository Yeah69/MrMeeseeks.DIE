using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration;

internal sealed class SharedNameRegistry : IContainerInstance
{
    private readonly Lazy<Dictionary<OverrideContext, string>> _overrideContextNameMap;
    private readonly Dictionary<ITypeSymbol, string> _entryFunctionsForFunctors = [];
    
    internal SharedNameRegistry(
        OverrideContextManager overrideContextManager,
        ReferenceGenerator referenceGenerator)
    {
        IOverrideInterfaceName = referenceGenerator.Generate("IOverride");
        _overrideContextNameMap = new Lazy<Dictionary<OverrideContext, string>>(() => overrideContextManager.AllOverrideContexts
            .ToDictionary(o => o, o => referenceGenerator.Generate(o is OverrideContext.Any ? "Overrides" : "NoOverrides")));
    }

    internal string IOverrideInterfaceName { get; }
    
    internal void AddEntryFunctionsForFunctorsMapping(ITypeSymbol type, string entryFunctionName) =>
        _entryFunctionsForFunctors[type] = entryFunctionName;
    
    internal string GetOverrideContextName(OverrideContext overrideContext) =>
        _overrideContextNameMap.Value[overrideContext];
    internal bool TryGetOverrideContextName(OverrideContext overrideContext, out string overrideContextName) =>
        _overrideContextNameMap.Value.TryGetValue(overrideContext, out overrideContextName);
    internal string GetEntryFunctionName(ITypeSymbol type) =>
        _entryFunctionsForFunctors[type];
}
