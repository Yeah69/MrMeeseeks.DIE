namespace MrMeeseeks.DIE.Nodes.Ranges;

internal interface IScopeNodeBase : IRangeNode
{
    string ContainerFullName { get; }
    string ContainerParameterReference { get; }
}