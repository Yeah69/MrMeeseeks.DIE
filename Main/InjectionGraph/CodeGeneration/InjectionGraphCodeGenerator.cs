using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration;

internal sealed class InjectionGraphCodeGenerator(
    ContainerInfo containerInfo,
    SyncGraphHolder syncGraphHolder,
    AsyncGraphHolder asyncGraphHolder,
    ScopeNodeBaseCodeGenerator scopeNodeBaseCodeGenerator,
    ScopeNodeManager scopeNodeManager,
    FunctionUtility functionUtility,
    OverrideContextManager overrideContextManager,
    ContextGenerator contextGenerator,
    TypeSymbolUtility typeSymbolUtility,
    ReferenceGenerator referenceGenerator,
    SharedNameRegistry sharedNameRegistry)
{
    private readonly StringBuilder _code = new();
    private readonly SyncGraphRoot _syncGraphRoot = syncGraphHolder.Value;
    private readonly AsyncGraphRoot _asyncGraphRoot = asyncGraphHolder.Value;

    public string Generate()
    {
        _code.AppendLine(
        $$"""
          #nullable enable
          namespace {{containerInfo.Namespace}}
          {
          """);
        
        /*var genericParameters = _rangeNode is IContainerNode containerNode && containerNode.TypeParameters.Any()
            ? $"<{string.Join(", ", containerNode.TypeParameters.Select(p => p.Name))}>"
            : ""; // ToDo generic types for the container */
        
        foreach (var nestingParentName in containerInfo.ContainingTypeNames)
        {
            _code.AppendLine(
                $$"""
                  partial class {{nestingParentName}}
                  {
                  """);
        }

        var inheritanceElements = scopeNodeBaseCodeGenerator.GetInheritanceHeaderElements(scopeNodeManager.ContainerScopeNode, isContainer: true);

        var inheritance = inheritanceElements.Any()
            ? $" : {string.Join(", ", inheritanceElements)}"
            : "";

        _code.AppendLine(
            $$"""
              sealed partial class {{containerInfo.Name}}{{inheritance}}
              {
              """);
        
        contextGenerator.GenerateContextClass(_code);

        scopeNodeBaseCodeGenerator.GenerateInterface(_code);

        var constructors = containerInfo.ContainerType.GetMembers().OfType<IMethodSymbol>()
            .Where(ms => ms.MethodKind == MethodKind.Constructor);

        foreach (var constructor in constructors)
        {
            var containerReference = referenceGenerator.Generate("container");
            _code.AppendLine(
                $$"""
                  public static {{containerInfo.ContainerType.FullName()}} {{Constants.CreateContainerFunctionName}}({{string.Join(", ", constructor.Parameters.Select(p => $"{p.Type.FullName()} {p.Name}"))}})
                  {
                  {{containerInfo.ContainerType.FullName()}} {{containerReference}} = new {{containerInfo.ContainerType.FullName()}}({{string.Join(", ", constructor.Parameters.Select(p => p.Name))}});
                  """);
            // ToDo add initialized instances
            _code.AppendLine(
                $$"""
                  return {{containerReference}};
                  }
                  """);
        }
        
        scopeNodeBaseCodeGenerator.GenerateScopedInstanceFunctions(_code, scopeNodeManager.ContainerScopeNode, Constants.ThisKeyword);

        var typesGettingFunctorEntry = _syncGraphRoot.ConcreteFunctorNodeManager.AllNodes
            .Concat(_asyncGraphRoot.ConcreteFunctorNodeManager.AllNodes)
            .Select(n => n.ReturnedElement.Target)
            .Distinct();
        foreach (var typeNode in typesGettingFunctorEntry)
        {
            var function = new FunctorEntryFunction(typeNode.Type);
            var functionName = functionUtility.GetName(function);
            _code.AppendLine(
                $$"""
                  {{functionUtility.GenerateHeader(function)}}
                  {
                  """);
            if (typeNode.Incoming.Select(e => e.Type).OfType<FunctionEdgeType>().FirstOrDefault() is { } nextFunction)
                _code.AppendLine($"return {functionUtility.GenerateFunctionCall(nextFunction.Function, doScopedInstance: true, doScopeRoot: true)};");
            else if (typeSymbolUtility.IsTaskType(typeNode.Type) && typeNode.Outgoing is [{ Target: ConcreteTaskNode { InnerEdge.Type: FunctionEdgeType nextFunction0 } }])
                _code.AppendLine($"return {functionUtility.GenerateFunctionCall(nextFunction0.Function, doScopedInstance: true, doScopeRoot: true)};");
            else
                _code.AppendLine($"throw new Exception(\"No function found for type {typeNode.Type.FullName()} during code generation.\");");
            
            _code.AppendLine("}");
            sharedNameRegistry.AddEntryFunctionsForFunctorsMapping(typeNode.Type, functionName);
        }

        var functionsAndGenerators = Enumerable.Empty<IGraphRoot>().Append(_syncGraphRoot).Append(_asyncGraphRoot)
            .SelectMany(r => r.GraphBuilder.Functions.Select(f => (Function: f, Generator: r.InjectionNodeGenerator)));
        foreach (var (function, injectionNodeGenerator) in functionsAndGenerators)
        {
            _code.AppendLine(
                $$"""
                  {{functionUtility.GenerateHeader(function)}}
                  {
                  """);
            
            var rootNode = function.RootNode;

            scopeNodeBaseCodeGenerator.GenerateScopeRootEntry(_code, rootNode);
            scopeNodeBaseCodeGenerator.GenerateScopedInstanceEntry(_code, rootNode);

            var rootReference = injectionNodeGenerator.GenerateForInjectionNode(_code, rootNode);
            if (!rootNode.Outgoing.Any(e => e.Target is ConcreteEnumerableNode))
                _code.AppendLine($"return {rootReference};");
            _code.AppendLine("}");
        }

        var entryCreateFunctionsMap = Enumerable.Empty<IGraphRoot>().Append(_syncGraphRoot).Append(_asyncGraphRoot)
            .SelectMany(gr =>
                gr.ConcreteEntryFunctionNodeManager.AllNodes.Select(n => (ConcreteEntryFunctionNode: n, gr.InjectionNodeGenerator, GraphType: gr.GraphTypeHolder.Type)))
            .ToImmutableDictionary(t => t.ConcreteEntryFunctionNode.Data.Name, t => t);

        foreach (var (rootType, name, parameters, _) in containerInfo.CreateFunctionData)
        {
            if (entryCreateFunctionsMap.TryGetValue(name, out var tuple)
                && overrideContextManager.TryGetContext(parameters, out var overrideContext))
            {
                var (entryFunctionNode, injectionNodeGenerator, graphType) = tuple;
                var parametersWithName = parameters.Select(p => (Type: p, Name: referenceGenerator.Generate(p))).ToArray();
                var parametersOnDeclaration = string.Join(", ", parametersWithName.Select(t => $"{t.Type.FullName()} {t.Name}"));
                var overridesName = sharedNameRegistry.GetOverrideContextName(overrideContext);
                var overridesAssignment = overrideContext is OverrideContext.Any any 
                    ? string.Join(", ", any.Overrides.Select(p => parametersWithName.First(t => t.Type.Equals(p)).Name))
                    : "";
                var maybeAsync = graphType is GraphType.Async ? "async " : "";
                var maybeAwait = graphType is GraphType.Async ? "await " : "";
                _code.AppendLine(
                    $$"""
                      internal {{maybeAsync}}{{rootType.FullName()}} {{name}}({{parametersOnDeclaration}})
                      {
                      {{contextGenerator.FullNameAndParameterName}} = {{contextGenerator.GenerateInstanceCreation(overrideInstanceCreation: $"new {overridesName}({overridesAssignment})", outwardFacingTypeNumber: "0", caseNumber: "0", key: "null", containerNode: Constants.ThisKeyword, transientScopeNode: Constants.ThisKeyword, scopeNode: Constants.ThisKeyword, scopeNodeName: $"\"{containerInfo.Name}\"")}};
                      """);
                var reference = injectionNodeGenerator.CallFunctionOrGenerateForInjectionNode(_code, entryFunctionNode.ReturnType, entryFunctionNode.ReturnType.Target);
                _code.AppendLine($"return {maybeAwait}{reference};");
                _code.AppendLine("}");
                
            }
        }

        var overrideGenericTypeName = referenceGenerator.Generate("TValue");
        if (overrideContextManager.AllOverrideContexts.Any(o => o is OverrideContext.Any))
            _code.AppendLine(
                $$"""
                  private interface {{sharedNameRegistry.IOverrideInterfaceName}}<{{overrideGenericTypeName}}>
                  {
                  {{overrideGenericTypeName}} Value();
                  }
                  """);
        
        foreach (var overrideContext in overrideContextManager.AllOverrideContexts)
        {
            switch (overrideContext)
            {
                case OverrideContext.None0:
                    var noneTypeName = sharedNameRegistry.GetOverrideContextName(overrideContext);
                    _code.AppendLine($"private record {noneTypeName};");
                    break;
                case OverrideContext.Any any:
                    var anyTypeName = sharedNameRegistry.GetOverrideContextName(overrideContext);
                    var properties = any.Overrides.Select((o, i) => $"{o.FullName()} Value{i}");
                    var interfaceAssignments = any.Overrides.Select(o => $"{sharedNameRegistry.IOverrideInterfaceName}<{o.FullName()}>");
                    _code.AppendLine($"private record {anyTypeName}({string.Join(", ", properties)}) : {string.Join(", ", interfaceAssignments)}");
                    _code.AppendLine("{");
                    var i = 0;
                    foreach (var overrideType in any.Overrides)
                        _code.AppendLine($"{overrideType.FullName()} {sharedNameRegistry.IOverrideInterfaceName}<{overrideType.FullName()}>.Value() => Value{i++};");
                    _code.AppendLine("}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        foreach (var scopeNode in scopeNodeManager.TransientScopes)
        {
            var scopeInheritanceElements = scopeNodeBaseCodeGenerator.GetInheritanceHeaderElements(scopeNode, isContainer: false);
            var scopeInheritance = scopeInheritanceElements.Any()
                ? $" : {string.Join(", ", scopeInheritanceElements)}"
                : "";
            var containerReference = scopeNodeBaseCodeGenerator.ScopeNodeToContainerPropertyReference[scopeNode];
            
            _code.AppendLine($"{Constants.PrivateKeyword} partial class {scopeNode.Name}{scopeInheritance}");
            _code.AppendLine("{");
            _code.AppendLine($"{Constants.InternalKeyword} required {containerInfo.FullName} {containerReference} {{ {Constants.PrivateKeyword} get; init; }}");
            
            scopeNodeBaseCodeGenerator.GenerateScopeRootFunctions(_code, scopeNode, containerReference);
            scopeNodeBaseCodeGenerator.GenerateScopedInstanceFunctions(_code, scopeNode, containerReference);
            
            _code.AppendLine("}");
        }
        
        foreach (var scopeNode in scopeNodeManager.Scopes)
        {
            var scopeInheritanceElements = scopeNodeBaseCodeGenerator.GetInheritanceHeaderElements(scopeNode, isContainer: false);
            var scopeInheritance = scopeInheritanceElements.Any()
                ? $" : {string.Join(", ", scopeInheritanceElements)}"
                : "";
            var containerReference = scopeNodeBaseCodeGenerator.ScopeNodeToContainerPropertyReference[scopeNode];
            
            _code.AppendLine($"{Constants.PrivateKeyword} partial class {scopeNode.Name}{scopeInheritance}");
            _code.AppendLine("{");
            _code.AppendLine($"{Constants.InternalKeyword} required {containerInfo.FullName} {containerReference} {{ {Constants.PrivateKeyword} get; init; }}");
            
            scopeNodeBaseCodeGenerator.GenerateScopeRootFunctions(_code, scopeNode, containerReference);
            scopeNodeBaseCodeGenerator.GenerateScopedInstanceFunctions(_code, scopeNode, containerReference);
            
            _code.AppendLine("}");
        }

        _code.AppendLine("}");

        _code.AppendLine(string.Join(Environment.NewLine, containerInfo.ContainingTypeNames.Select(_ => "}")));

        _code.AppendLine(
            """
            }
            #nullable disable
            """);
        
        return _code.ToString();
    }
    
}