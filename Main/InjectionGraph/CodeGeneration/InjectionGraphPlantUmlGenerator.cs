using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration;

internal interface IInjectionGraphPlantUmlGenerator
{
    string Generate();
}

internal sealed class InjectionGraphPlantUmlGenerator(
    SyncGraphHolder syncGraphHolder,
    AsyncGraphHolder asyncGraphHolder) : IInjectionGraphPlantUmlGenerator
{
    private readonly StringBuilder _diagram = new();
    private readonly SyncGraphRoot _syncGraphRoot = syncGraphHolder.Value;
    private readonly AsyncGraphRoot _asyncGraphRoot = asyncGraphHolder.Value;

    private readonly Dictionary<TypeNode, string> _typeNodeIds = [];
    private readonly Dictionary<IConcreteNode, string> _concreteNodeIds = [];
    private int _nodeIdCounter;

    public string Generate()
    {
        _diagram.AppendLine("@startuml");
        _diagram.AppendLine("allowmixing");
        _diagram.AppendLine();
        _diagram.AppendLine("' Styling");
        _diagram.AppendLine("skinparam rectangle {");
        _diagram.AppendLine("  BackgroundColor<<TypeNode>> LightBlue");
        _diagram.AppendLine("  BackgroundColor<<Implementation>> LightGreen");
        _diagram.AppendLine("  BackgroundColor<<Interface>> LightYellow");
        _diagram.AppendLine("  BackgroundColor<<Functor>> LightCoral");
        _diagram.AppendLine("  BackgroundColor<<Enumerable>> LightPink");
        _diagram.AppendLine("  BackgroundColor<<KeyValuePair>> LightGray");
        _diagram.AppendLine("  BackgroundColor<<Override>> Wheat");
        _diagram.AppendLine("  BackgroundColor<<EntryFunction>> LightCyan");
        _diagram.AppendLine("  BackgroundColor<<Exception>> Salmon");
        _diagram.AppendLine("  BackgroundColor<<Task>> Orchid");
        _diagram.AppendLine("}");
        _diagram.AppendLine();

        // Generate sync graph in its own namespace
        if (HasAnyNodes(_syncGraphRoot))
        {
            _diagram.AppendLine("namespace Sync {");
            GenerateGraphContent(_syncGraphRoot, "S_");
            _diagram.AppendLine("}");
            _diagram.AppendLine();
        }

        // Clear the node ID mappings for the async graph
        _typeNodeIds.Clear();
        _concreteNodeIds.Clear();

        // Generate async graph in its own namespace
        if (HasAnyNodes(_asyncGraphRoot))
        {
            _diagram.AppendLine("namespace Async {");
            GenerateGraphContent(_asyncGraphRoot, "A_");
            _diagram.AppendLine("}");
        }

        _diagram.AppendLine();
        _diagram.AppendLine("@enduml");

        return _diagram.ToString();
    }

    private bool HasAnyNodes(IGraphRoot graphRoot) =>
        graphRoot.TypeNodeManager.AllTypeNodes.Count != 0 ||
        graphRoot.ConcreteEntryFunctionNodeManager.AllNodes.Count != 0 ||
        graphRoot.ConcreteImplementationNodeManager.AllNodes.Count != 0 ||
        graphRoot.ConcreteInterfaceNodeManager.AllNodes.Count != 0 ||
        graphRoot.ConcreteFunctorNodeManager.AllNodes.Count != 0 ||
        graphRoot.ConcreteEnumerableNodeManager.AllNodes.Count != 0 ||
        graphRoot.ConcreteKeyValuePairNodeManager.AllNodes.Count != 0 ||
        graphRoot.ConcreteOverrideNodeManager.AllNodes.Count != 0 ||
        graphRoot.ConcreteTaskNodeManager.AllNodes.Count != 0;

    private void GenerateGraphContent(IGraphRoot graphRoot, string nodePrefix)
    {
        GenerateTypeNodes(graphRoot, nodePrefix);
        GenerateConcreteNodes(graphRoot, nodePrefix);
        GenerateEdges(graphRoot);
    }

    private void GenerateTypeNodes(IGraphRoot graphRoot, string nodePrefix)
    {
        _diagram.AppendLine("' Type Nodes");
        foreach (var typeNode in graphRoot.TypeNodeManager.AllTypeNodes)
        {
            var id = GetTypeNodeId(typeNode, nodePrefix);
            var label = SanitizeLabel(GetTypeDisplayName(typeNode.Type));
            _diagram.AppendLine($"rectangle \"{label}\" as {id} <<TypeNode>>");
        }
        _diagram.AppendLine();
    }

    private void GenerateConcreteNodes(IGraphRoot graphRoot, string nodePrefix)
    {
        _diagram.AppendLine("' Concrete Nodes");

        foreach (var node in graphRoot.ConcreteEntryFunctionNodeManager.AllNodes)
        {
            var id = GetConcreteNodeId(node, nodePrefix);
            var label = SanitizeLabel($"Entry: {node.Data.Name}");
            _diagram.AppendLine($"rectangle \"{label}\" as {id} <<EntryFunction>>");
        }

        foreach (var node in graphRoot.ConcreteImplementationNodeManager.AllNodes)
        {
            var id = GetConcreteNodeId(node, nodePrefix);
            var label = SanitizeLabel($"Impl: {GetTypeDisplayName(node.Data.Implementation)}");
            _diagram.AppendLine($"rectangle \"{label}\" as {id} <<Implementation>>");
        }

        foreach (var node in graphRoot.ConcreteInterfaceNodeManager.AllNodes)
        {
            var id = GetConcreteNodeId(node, nodePrefix);
            var label = SanitizeLabel($"Iface: {GetTypeDisplayName(node.Data.Interface)}");
            _diagram.AppendLine($"rectangle \"{label}\" as {id} <<Interface>>");
        }

        foreach (var node in graphRoot.ConcreteFunctorNodeManager.AllNodes)
        {
            var id = GetConcreteNodeId(node, nodePrefix);
            var label = SanitizeLabel($"Func: {GetTypeDisplayName(node.Data.Type)}");
            _diagram.AppendLine($"rectangle \"{label}\" as {id} <<Functor>>");
        }

        foreach (var node in graphRoot.ConcreteEnumerableNodeManager.AllNodes)
        {
            var id = GetConcreteNodeId(node, nodePrefix);
            var label = SanitizeLabel($"Enum: {GetTypeDisplayName(node.Data.EnumerableType)}");
            _diagram.AppendLine($"rectangle \"{label}\" as {id} <<Enumerable>>");
        }

        foreach (var node in graphRoot.ConcreteKeyValuePairNodeManager.AllNodes)
        {
            var id = GetConcreteNodeId(node, nodePrefix);
            var label = SanitizeLabel($"KVP: {GetTypeDisplayName(node.Data.KeyValuePairType)}");
            _diagram.AppendLine($"rectangle \"{label}\" as {id} <<KeyValuePair>>");
        }

        foreach (var node in graphRoot.ConcreteOverrideNodeManager.AllNodes)
        {
            var id = GetConcreteNodeId(node, nodePrefix);
            var label = SanitizeLabel($"Override: {GetTypeDisplayName(node.Data.Type)}");
            _diagram.AppendLine($"rectangle \"{label}\" as {id} <<Override>>");
        }

        if (HasExceptionNodeConnections(graphRoot))
        {
            var id = GetConcreteNodeId(graphRoot.ConcreteExceptionNode, nodePrefix);
            _diagram.AppendLine($"rectangle \"Exception\" as {id} <<Exception>>");
        }

        _diagram.AppendLine();
    }

    private void GenerateEdges(IGraphRoot graphRoot)
    {
        _diagram.AppendLine("' Edges (TypeNode -> ConcreteNode via ConcreteEdge)");
        foreach (var typeNode in graphRoot.TypeNodeManager.AllTypeNodes)
        {
            var typeNodeId = _typeNodeIds[typeNode];
            foreach (var concreteEdge in typeNode.Outgoing)
            {
                if (!_concreteNodeIds.TryGetValue(concreteEdge.Target, out var targetId))
                    continue;

                foreach (var context in concreteEdge.Contexts)
                {
                    var contextLabel = FormatEdgeContext(context);
                    _diagram.AppendLine($"{typeNodeId} --> {targetId} : \"{contextLabel}\"");
                }
            }
        }

        _diagram.AppendLine();
        _diagram.AppendLine("' Edges (ConcreteNode -> TypeNode via TypeEdge)");
        foreach (var typeNode in graphRoot.TypeNodeManager.AllTypeNodes)
        {
            var typeNodeId = _typeNodeIds[typeNode];
            foreach (var typeEdge in typeNode.Incoming)
            {
                if (!_concreteNodeIds.TryGetValue(typeEdge.Source, out var sourceId))
                    continue;

                foreach (var context in typeEdge.Contexts)
                {
                    var contextLabel = FormatEdgeContext(context);
                    _diagram.AppendLine($"{sourceId} --> {typeNodeId} : \"{contextLabel}\"");
                }
            }
        }
    }

    private string GetTypeNodeId(TypeNode typeNode, string nodePrefix)
    {
        if (_typeNodeIds.TryGetValue(typeNode, out var id))
            return id;
        id = $"{nodePrefix}TN{_nodeIdCounter++}";
        _typeNodeIds[typeNode] = id;
        return id;
    }

    private string GetConcreteNodeId(IConcreteNode concreteNode, string nodePrefix)
    {
        if (_concreteNodeIds.TryGetValue(concreteNode, out var id))
            return id;
        id = $"{nodePrefix}CN{_nodeIdCounter++}";
        _concreteNodeIds[concreteNode] = id;
        return id;
    }

    private static string GetTypeDisplayName(ITypeSymbol type)
    {
        var fullName = type.FullName();
        // Remove global:: prefix if present
        if (fullName.StartsWith("global::", StringComparison.Ordinal))
            fullName = fullName.Substring(8);
        return fullName;
    }

    private static string SanitizeLabel(string label)
    {
        // Escape characters that could break PlantUML syntax
        return label
            .Replace("\"", "\\\"")
            .Replace("<", "\\<")
            .Replace(">", "\\>");
    }

    private static string FormatEdgeContext(EdgeContext context)
    {
        var parts = new List<string>();

        switch (context.ScopeNode)
        {
            case ScopeNodeContext.Container:
                parts.Add("Container");
                break;
            case ScopeNodeContext.Scope scope:
                var scopeLabel = scope.TransientScopeName is not null
                    ? $"Scope:{scope.ScopeName}@{scope.TransientScopeName}"
                    : $"Scope:{scope.ScopeName}";
                parts.Add(scopeLabel);
                break;
            case ScopeNodeContext.TransientScope transientScope:
                parts.Add($"TScope:{transientScope.TransientScopeName}");
                break;
        }

        switch (context.Override)
        {
            case OverrideContext.Any any:
                parts.Add($"Override:{any.Overrides.Length}");
                break;
        }

        switch (context.Key)
        {
            case KeyContext.Single single:
                parts.Add($"Key:{single.Value}");
                break;
        }

        switch (context.CaseChoice)
        {
            case CaseChoiceContext.Single single:
                parts.Add($"Case:{single.OutwardFacingTypeId}.{single.CaseId}");
                break;
        }

        return parts.Count > 0 ? string.Join(", ", parts) : "Default";
    }

    private static bool HasExceptionNodeConnections(IGraphRoot graphRoot)
    {
        foreach (var typeNode in graphRoot.TypeNodeManager.AllTypeNodes)
        {
            foreach (var concreteEdge in typeNode.Outgoing)
            {
                if (Equals(concreteEdge.Target, graphRoot.ConcreteExceptionNode))
                    return true;
            }
        }
        return false;
    }
}
