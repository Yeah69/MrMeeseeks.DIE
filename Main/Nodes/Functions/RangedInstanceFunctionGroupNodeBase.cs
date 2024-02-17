using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface IRangedInstanceFunctionGroupNodeBase : INode
{
    ScopeLevel Level { get; }
    string TypeFullName { get; }
    string FieldReference { get; }
    string LockReference { get; }
    string? IsCreatedForStructs { get; }
    IRangedInstanceFunctionNode BuildFunction(IFunctionNode callingFunction);
}

internal abstract class RangedInstanceFunctionGroupNodeBase : IRangedInstanceFunctionGroupNodeBase
{
    internal RangedInstanceFunctionGroupNodeBase(
        // parameters
        ScopeLevel level,
        INamedTypeSymbol type,
        
        // dependencies
        IReferenceGenerator referenceGenerator)
    {
        Level = level;
        TypeFullName = type.FullName();
        var label = Level.ToString();
        FieldReference =
            referenceGenerator.Generate($"_{label.ToLowerInvariant()}InstanceField", type);
        LockReference = referenceGenerator.Generate($"_{label.ToLowerInvariant()}InstanceLock");
        IsCreatedForStructs = type.IsValueType ? referenceGenerator.Generate("isCreated") : null;
    }

    public void Build(PassedContext passedContext) { }

    public abstract void Accept(INodeVisitor nodeVisitor);

    public ScopeLevel Level { get; }
    public string TypeFullName { get; }
    public string FieldReference { get; }
    public string LockReference { get; }
    public string? IsCreatedForStructs { get; }

    public abstract IRangedInstanceFunctionNode BuildFunction(IFunctionNode callingFunction);
}