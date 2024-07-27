using MrMeeseeks.DIE.InjectionGraph.CodeGeneration;
using MrMeeseeks.DIE.InjectionGraph.Nodes;

namespace MrMeeseeks.DIE.InjectionGraph;

internal interface IFunction
{
    Accessibility? Accessibility { get; }
    ExplicitInterfaceDescription ExplicitInterface { get; }
    ITypeParameterSymbol[] TypeParameters { get; }
    ITypeSymbol SyncReturnType { get; }
    ITypeSymbol? AsyncReturnType { get; }
    bool Sync { get; }
}

internal interface ITypeNodeFunction : IFunction
{
    TypeNode RootNode { get; }
}

internal abstract record ExplicitInterfaceDescription
{
    internal sealed record None4 : ExplicitInterfaceDescription
    {
        private None4() {}
        internal static None4 Instance { get; } = new();
    }
    internal sealed record KnownType(INamedTypeSymbol Type) : ExplicitInterfaceDescription;
    internal sealed record Generated(string TypeFullName) : ExplicitInterfaceDescription;
}

internal abstract class FunctionBase(
    Accessibility? accessibility,
    ExplicitInterfaceDescription explicitInterface,
    ITypeParameterSymbol[] typeParameters,
    ITypeSymbol syncReturnType,
    bool sync, 
    
    FunctionUtility functionUtility) 
    : IFunction
{
    public Accessibility? Accessibility { get; } = accessibility;
    public ExplicitInterfaceDescription ExplicitInterface { get; } = explicitInterface;
    public ITypeParameterSymbol[] TypeParameters { get; } = typeParameters;
    public ITypeSymbol SyncReturnType { get; } = syncReturnType;
    public ITypeSymbol? AsyncReturnType { get; } = sync ? null : functionUtility.MakeItAnAsyncReturnType(syncReturnType);
    public bool Sync => 
        AsyncReturnType is null;
}

internal sealed class TypeNodeFunction(
    TypeNode rootElement,
    bool sync,
    
    FunctionUtility functionUtility) 
    : FunctionBase(Microsoft.CodeAnalysis.Accessibility.Private, ExplicitInterfaceDescription.None4.Instance, [], rootElement.Type, sync, functionUtility), 
        ITypeNodeFunction
{
    public TypeNode RootNode { get; } = rootElement;
}

internal sealed class FunctorEntryFunction(
    ITypeSymbol returnType,

    FunctionUtility functionUtility)
    : FunctionBase(Microsoft.CodeAnalysis.Accessibility.Private, ExplicitInterfaceDescription.None4.Instance, [], returnType, sync: true, functionUtility);

internal sealed class ScopedInstanceFunction(
    TypeNode scopedInstanceElement, 
    ExplicitInterfaceDescription explicitInterfaceDescription,
    bool sync,
    
    FunctionUtility functionUtility) 
    : FunctionBase(accessibility: null, explicitInterfaceDescription, [], scopedInstanceElement.Type, sync, functionUtility),
        ITypeNodeFunction
{
    public TypeNode RootNode { get; } = scopedInstanceElement;
}

internal sealed class ScopeRootFunction(
    TypeNode scopeRootElement,
    bool sync,
    
    FunctionUtility functionUtility)
    : FunctionBase(Microsoft.CodeAnalysis.Accessibility.Internal, ExplicitInterfaceDescription.None4.Instance, [], scopeRootElement.Type, sync, functionUtility), 
        ITypeNodeFunction
{
    public TypeNode RootNode { get; } = scopeRootElement;
}