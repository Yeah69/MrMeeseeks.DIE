using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration;

internal interface IInjectionGraphPlantUmlGenerator
{
    string Generate();
}

internal sealed class InjectionGraphPlantUmlGenerator : IInjectionGraphPlantUmlGenerator
{
    private readonly StringBuilder _diagram = new();
    private readonly TypeNodeManager _typeNodeManager;
    private readonly ConcreteImplementationNodeManager _concreteImplementationNodeManager;
    private readonly ConcreteInterfaceNodeManager _concreteInterfaceNodeManager;
    private readonly ConcreteFunctorNodeManager _concreteFunctorNodeManager;
    private readonly ConcreteEnumerableNodeManager _concreteEnumerableNodeManager;
    private readonly ConcreteKeyValuePairNodeManager _concreteKeyValuePairNodeManager;
    private readonly ConcreteOverrideNodeManager _concreteOverrideNodeManager;
    private readonly ConcreteEntryFunctionNodeManager _concreteEntryFunctionNodeManager;
    private readonly ConcreteExceptionNode _concreteExceptionNode;

    private readonly Dictionary<TypeNode, string> _typeNodeIds = [];
    private readonly Dictionary<IConcreteNode, string> _concreteNodeIds = [];
    private int _nodeIdCounter;

    public InjectionGraphPlantUmlGenerator(
        TypeNodeManager typeNodeManager,
        ConcreteImplementationNodeManager concreteImplementationNodeManager,
        ConcreteInterfaceNodeManager concreteInterfaceNodeManager,
        ConcreteFunctorNodeManager concreteFunctorNodeManager,
        ConcreteEnumerableNodeManager concreteEnumerableNodeManager,
        ConcreteKeyValuePairNodeManager concreteKeyValuePairNodeManager,
        ConcreteOverrideNodeManager concreteOverrideNodeManager,
        ConcreteEntryFunctionNodeManager concreteEntryFunctionNodeManager,
        ConcreteExceptionNode concreteExceptionNode)
    {
        _typeNodeManager = typeNodeManager;
        _concreteImplementationNodeManager = concreteImplementationNodeManager;
        _concreteInterfaceNodeManager = concreteInterfaceNodeManager;
        _concreteFunctorNodeManager = concreteFunctorNodeManager;
        _concreteEnumerableNodeManager = concreteEnumerableNodeManager;
        _concreteKeyValuePairNodeManager = concreteKeyValuePairNodeManager;
        _concreteOverrideNodeManager = concreteOverrideNodeManager;
        _concreteEntryFunctionNodeManager = concreteEntryFunctionNodeManager;
        _concreteExceptionNode = concreteExceptionNode;
    }

    public string Generate()
    {
        _diagram.AppendLine("@startuml");
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
        _diagram.AppendLine("}");
        _diagram.AppendLine();

        GenerateTypeNodes();
        GenerateConcreteNodes();
        GenerateEdges();

        _diagram.AppendLine();
        _diagram.AppendLine("@enduml");

        return _diagram.ToString();
    }

    private void GenerateTypeNodes()
    {
        _diagram.AppendLine("' Type Nodes");
        foreach (var typeNode in _typeNodeManager.AllTypeNodes)
        {
            var id = GetTypeNodeId(typeNode);
            var label = SanitizeLabel(GetTypeDisplayName(typeNode.Type));
            _diagram.AppendLine($"rectangle \"{label}\" as {id} <<TypeNode>>");
        }
        _diagram.AppendLine();
    }

    private void GenerateConcreteNodes()
    {
        _diagram.AppendLine("' Concrete Nodes");

        foreach (var node in _concreteEntryFunctionNodeManager.AllNodes)
        {
            var id = GetConcreteNodeId(node);
            var label = SanitizeLabel($"Entry: {node.Data.Name}");
            _diagram.AppendLine($"rectangle \"{label}\" as {id} <<EntryFunction>>");
        }

        foreach (var node in _concreteImplementationNodeManager.AllNodes)
        {
            var id = GetConcreteNodeId(node);
            var label = SanitizeLabel($"Impl: {GetTypeDisplayName(node.Data.Implementation)}");
            _diagram.AppendLine($"rectangle \"{label}\" as {id} <<Implementation>>");
        }

        foreach (var node in _concreteInterfaceNodeManager.AllNodes)
        {
            var id = GetConcreteNodeId(node);
            var label = SanitizeLabel($"Iface: {GetTypeDisplayName(node.Data.Interface)}");
            _diagram.AppendLine($"rectangle \"{label}\" as {id} <<Interface>>");
        }

        foreach (var node in _concreteFunctorNodeManager.AllNodes)
        {
            var id = GetConcreteNodeId(node);
            var label = SanitizeLabel($"Func: {GetTypeDisplayName(node.Data.Type)}");
            _diagram.AppendLine($"rectangle \"{label}\" as {id} <<Functor>>");
        }

        foreach (var node in _concreteEnumerableNodeManager.AllNodes)
        {
            var id = GetConcreteNodeId(node);
            var label = SanitizeLabel($"Enum: {GetTypeDisplayName(node.Data.Enumerable)}");
            _diagram.AppendLine($"rectangle \"{label}\" as {id} <<Enumerable>>");
        }

        foreach (var node in _concreteKeyValuePairNodeManager.AllNodes)
        {
            var id = GetConcreteNodeId(node);
            var label = SanitizeLabel($"KVP: {GetTypeDisplayName(node.Data.KeyValuePairType)}");
            _diagram.AppendLine($"rectangle \"{label}\" as {id} <<KeyValuePair>>");
        }

        foreach (var node in _concreteOverrideNodeManager.AllNodes)
        {
            var id = GetConcreteNodeId(node);
            var label = SanitizeLabel($"Override: {GetTypeDisplayName(node.Data.Type)}");
            _diagram.AppendLine($"rectangle \"{label}\" as {id} <<Override>>");
        }

        if (HasExceptionNodeConnections())
        {
            var id = GetConcreteNodeId(_concreteExceptionNode);
            _diagram.AppendLine($"rectangle \"Exception\" as {id} <<Exception>>");
        }

        _diagram.AppendLine();
    }

    private void GenerateEdges()
    {
        _diagram.AppendLine("' Edges (TypeNode -> ConcreteNode via ConcreteEdge)");
        foreach (var typeNode in _typeNodeManager.AllTypeNodes)
        {
            var typeNodeId = GetTypeNodeId(typeNode);
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
        foreach (var typeNode in _typeNodeManager.AllTypeNodes)
        {
            var typeNodeId = GetTypeNodeId(typeNode);
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
                parts.Add($"Scope:{scope.ScopeName}");
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
        foreach (var typeNode in _typeNodeManager.AllTypeNodes)
        {
            foreach (var concreteEdge in typeNode.Outgoing)
            {
                if (concreteEdge.Target == _concreteExceptionNode)
                    return true;
            }
        }
        return false;
    }
}
