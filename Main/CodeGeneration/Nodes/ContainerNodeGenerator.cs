using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.CodeGeneration.Nodes;

internal interface IContainerNodeGenerator : IRangeNodeGenerator;

internal sealed class ContainerNodeGenerator : RangeNodeGenerator, IContainerNodeGenerator
{
    private readonly IContainerNode _containerNode;
    private readonly WellKnownTypes _wellKnownTypes;

    internal ContainerNodeGenerator(
        IContainerNode containerNode,
        IDisposeUtility disposeUtility,
        WellKnownTypes wellKnownTypes,
        WellKnownTypesCollections wellKnownTypesCollections) 
        : base(
            containerNode,
            containerNode,
            disposeUtility,
            wellKnownTypes,
            wellKnownTypesCollections)
    {
        _containerNode = containerNode;
        _wellKnownTypes = wellKnownTypes;
    }

    protected override void PreClass(StringBuilder code)
    {
        base.PreClass(code);

        code.AppendLine(
            $$"""
              #nullable enable
              namespace {{_containerNode.Namespace}}
              {
              """);
    }

    protected override string InterfaceAssignment => _containerNode.TransientScopeInterface.FullName;

    protected override void PreGeneralContent(StringBuilder code, ICodeGenerationVisitor visitor)
    {
        foreach (var containerCreateContainerFunction in _containerNode.CreateContainerFunctions)
            visitor.VisitICreateContainerFunctionNode(containerCreateContainerFunction);

        foreach (var entryFunctionNode in _containerNode.RootFunctions)
            visitor.VisitIEntryFunctionNode(entryFunctionNode);
    }

    protected override void PostGeneralContent(StringBuilder code, ICodeGenerationVisitor visitor)
    {
        base.PostGeneralContent(code, visitor);
        
        code.AppendLine(
            $$"""
              private interface {{_containerNode.ScopeInterface}} : {{GenerateDisposalInterfaceAssignments()}} {}

              private {{_wellKnownTypes.ListOfObject.FullName()}} {{_containerNode.TransientScopeDisposalReference}} = new {{_wellKnownTypes.ListOfObject.FullName()}}();
              """);
        
        visitor.VisitITransientScopeInterfaceNode(_containerNode.TransientScopeInterface);
        
        foreach (var scope in _containerNode.Scopes)
            visitor.VisitIScopeNode(scope);
        
        foreach (var transientScope in _containerNode.TransientScopes)
            visitor.VisitITransientScopeNode(transientScope);
    }

    protected override void PostClass(StringBuilder code)
    {
        base.PostClass(code);

        code.AppendLine(
            """
            }
            #nullable disable
            """);
    }
}