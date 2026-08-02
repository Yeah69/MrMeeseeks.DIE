using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration;

internal interface IInjectionGraphPlantUmlGenerator
{
    string Generate();
}

internal sealed class InjectionGraphPlantUmlGenerator(
    TypeNodeManager typeNodeManager,
    ConcreteEntryFunctionNodeManager concreteEntryFunctionNodeManager,
    ConcreteImplementationNodeManager concreteImplementationNodeManager,
    ConcreteInterfaceNodeManager concreteInterfaceNodeManager,
    ConcreteFunctorNodeManager concreteFunctorNodeManager,
    ConcreteEnumerableNodeManager concreteEnumerableNodeManager,
    ConcreteKeyValuePairNodeManager concreteKeyValuePairNodeManager,
    ConcreteOverrideNodeManager concreteOverrideNodeManager,
    ConcreteTaskNodeManager concreteTaskNodeManager,
    ConcreteExceptionNode concreteExceptionNode)
    : IInjectionGraphPlantUmlGenerator
{
    private readonly StringBuilder _diagram = new();

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
        _diagram.AppendLine("  BackgroundColor<<TypeNodeAsync>> CornflowerBlue");
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

        if (HasAnyNodes())
        {
            GenerateTypeNodes();
            GenerateConcreteNodes();
            GenerateEdges();
        }

        _diagram.AppendLine();
        _diagram.AppendLine("@enduml");

        return _diagram.ToString();
    }

    private bool HasAnyNodes() =>
        typeNodeManager.AllTypeNodes.Count != 0 ||
        concreteEntryFunctionNodeManager.AllNodes.Count != 0 ||
        concreteImplementationNodeManager.AllNodes.Count != 0 ||
        concreteInterfaceNodeManager.AllNodes.Count != 0 ||
        concreteFunctorNodeManager.AllNodes.Count != 0 ||
        concreteEnumerableNodeManager.AllNodes.Count != 0 ||
        concreteKeyValuePairNodeManager.AllNodes.Count != 0 ||
        concreteOverrideNodeManager.AllNodes.Count != 0 ||
        concreteTaskNodeManager.AllNodes.Count != 0;

    private void GenerateTypeNodes()
    {
        _diagram.AppendLine("' Type Nodes");
        foreach (var typeNode in typeNodeManager.AllTypeNodes)
        {
            var id = GetTypeNodeId(typeNode);
            var label = SanitizeLabel(GetTypeDisplayName(typeNode.Type));

            // Add function information if present
            var functionInfo = new List<string>();
            if (typeNode.SyncFunction is not null)
                functionInfo.Add($"Sync: {typeNode.SyncFunction.GetType().Name}");
            if (typeNode.AsyncFunction is not null)
                functionInfo.Add($"Async: {typeNode.AsyncFunction.GetType().Name}");

            if (functionInfo.Count > 0)
                label += "\\n" + string.Join("\\n", functionInfo);

            // Use different stereotype based on whether the node has async function
            var stereotype = typeNode.AsyncFunction is not null ? "TypeNodeAsync" : "TypeNode";
            _diagram.AppendLine($"rectangle \"{label}\" as {id} <<{stereotype}>>");
        }
        _diagram.AppendLine();
    }

    private void GenerateConcreteNodes()
    {
        _diagram.AppendLine("' Concrete Nodes");

        foreach (var node in concreteEntryFunctionNodeManager.AllNodes)
        {
            var id = GetConcreteNodeId(node);
            var label = SanitizeLabel($"Entry: {node.Data.Name}");
            _diagram.AppendLine($"rectangle \"{label}\" as {id} <<EntryFunction>>");
        }

        foreach (var node in concreteImplementationNodeManager.AllNodes)
        {
            var id = GetConcreteNodeId(node);
            var label = SanitizeLabel($"Impl: {GetTypeDisplayName(node.Data.Implementation)}");
            _diagram.AppendLine($"rectangle \"{label}\" as {id} <<Implementation>>");
        }

        foreach (var node in concreteInterfaceNodeManager.AllNodes)
        {
            var id = GetConcreteNodeId(node);
            var label = SanitizeLabel($"Iface: {GetTypeDisplayName(node.Data.Interface)}");
            _diagram.AppendLine($"rectangle \"{label}\" as {id} <<Interface>>");
        }

        foreach (var node in concreteFunctorNodeManager.AllNodes)
        {
            var id = GetConcreteNodeId(node);
            var label = SanitizeLabel($"Func: {GetTypeDisplayName(node.Data.Type)}");
            _diagram.AppendLine($"rectangle \"{label}\" as {id} <<Functor>>");
        }

        foreach (var node in concreteEnumerableNodeManager.AllNodes)
        {
            var id = GetConcreteNodeId(node);
            var label = SanitizeLabel($"Enum: {GetTypeDisplayName(node.Data.EnumerableType)}");
            _diagram.AppendLine($"rectangle \"{label}\" as {id} <<Enumerable>>");
        }

        foreach (var node in concreteKeyValuePairNodeManager.AllNodes)
        {
            var id = GetConcreteNodeId(node);
            var label = SanitizeLabel($"KVP: {GetTypeDisplayName(node.Data.KeyValuePairType)}");
            _diagram.AppendLine($"rectangle \"{label}\" as {id} <<KeyValuePair>>");
        }

        foreach (var node in concreteOverrideNodeManager.AllNodes)
        {
            var id = GetConcreteNodeId(node);
            var label = SanitizeLabel($"Override: {GetTypeDisplayName(node.Data.Type)}");
            _diagram.AppendLine($"rectangle \"{label}\" as {id} <<Override>>");
        }

        foreach (var node in concreteTaskNodeManager.AllNodes)
        {
            var id = GetConcreteNodeId(node);
            var label = SanitizeLabel($"Task: {GetTypeDisplayName(node.Data.TaskType)}");
            _diagram.AppendLine($"rectangle \"{label}\" as {id} <<Task>>");
        }

        if (HasExceptionNodeConnections())
        {
            var id = GetConcreteNodeId(concreteExceptionNode);
            _diagram.AppendLine($"rectangle \"Exception\" as {id} <<Exception>>");
        }

        _diagram.AppendLine();
    }

    private void GenerateEdges()
    {
        _diagram.AppendLine("' Edges (TypeNode -> ConcreteNode via ConcreteEdge)");
        foreach (var typeNode in typeNodeManager.AllTypeNodes)
        {
            var typeNodeId = _typeNodeIds[typeNode];
            foreach (var concreteEdge in typeNode.Outgoing)
            {
                if (!_concreteNodeIds.TryGetValue(concreteEdge.Target, out var targetId))
                    continue;

                // Indicate sync vs async edge
                var edgeStyle = concreteEdge is ConcreteAsyncEdge ? "[#blue,dashed]" : "";

                foreach (var context in concreteEdge.Contexts)
                {
                    var contextLabel = FormatEdgeContext(context);
                    _diagram.AppendLine($"{typeNodeId} -{edgeStyle}-> {targetId} : \"{contextLabel}\"");
                }
            }
        }

        _diagram.AppendLine();
        _diagram.AppendLine("' Edges (ConcreteNode -> TypeNode via TypeEdge)");
        foreach (var typeNode in typeNodeManager.AllTypeNodes)
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

    private string GetTypeNodeId(TypeNode typeNode)
    {
        if (_typeNodeIds.TryGetValue(typeNode, out var id))
            return id;
        id = $"TN{_nodeIdCounter++}";
        _typeNodeIds[typeNode] = id;
        return id;
    }

    private string GetConcreteNodeId(IConcreteNode concreteNode)
    {
        if (_concreteNodeIds.TryGetValue(concreteNode, out var id))
            return id;
        id = $"CN{_nodeIdCounter++}";
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

    private bool HasExceptionNodeConnections()
    {
        foreach (var typeNode in typeNodeManager.AllTypeNodes)
        {
            foreach (var concreteEdge in typeNode.Outgoing)
            {
                if (Equals(concreteEdge.Target, concreteExceptionNode))
                    return true;
            }
        }
        return false;
    }
}
