using MrMeeseeks.DIE.InjectionGraph.CodeGeneration.ConcreteNodeGenerators;
using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.Logging;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration;

internal interface IInjectionGraphCodeGenerator
{
    string Generate();
}

internal sealed partial class InjectionGraphCodeGenerator : IInjectionGraphCodeGenerator
{
    private readonly StringBuilder _code = new();
    private readonly ContainerInfo _containerInfo;
    private readonly IInjectionGraphBuilder _injectionGraphBuilder;
    private readonly ScopeNodeBaseCodeGenerator _scopeNodeBaseCodeGenerator;
    private readonly ScopeNodeManager _scopeNodeManager;
    private readonly FunctionUtility _functionUtility;
    private readonly OverrideContextManager _overrideContextManager;
    private readonly ConcreteFunctorNodeManager _concreteFunctorNodeManager;
    private readonly ContextGenerator _contextGenerator;
    private readonly ReferenceGenerator _referenceGenerator;
    private readonly LocalDiagLogger _logger;
    private readonly InjectionNodeGenerator _injectionNodeGenerator;
    private readonly SharedNameRegistry _sharedNameRegistry;

    public InjectionGraphCodeGenerator(
        ContainerInfo containerInfo,
        IInjectionGraphBuilder injectionGraphBuilder,
        ScopeNodeBaseCodeGenerator scopeNodeBaseCodeGenerator,
        ScopeNodeManager scopeNodeManager,
        FunctionUtility functionUtility,
        OverrideContextManager overrideContextManager,
        ConcreteFunctorNodeManager concreteFunctorNodeManager,
        ContextGenerator contextGenerator,
        ReferenceGenerator referenceGenerator,
        LocalDiagLogger logger,
        InjectionNodeGenerator injectionNodeGenerator,
        SharedNameRegistry sharedNameRegistry)
    {
        _containerInfo = containerInfo;
        _injectionGraphBuilder = injectionGraphBuilder;
        _scopeNodeBaseCodeGenerator = scopeNodeBaseCodeGenerator;
        _scopeNodeManager = scopeNodeManager;
        _functionUtility = functionUtility;
        _overrideContextManager = overrideContextManager;
        _concreteFunctorNodeManager = concreteFunctorNodeManager;
        _contextGenerator = contextGenerator;
        _referenceGenerator = referenceGenerator;
        _logger = logger;
        _injectionNodeGenerator = injectionNodeGenerator;
        _sharedNameRegistry = sharedNameRegistry;
    }

    public string Generate()
    {
        _code.AppendLine(
        $$"""
          #nullable enable
          namespace {{_containerInfo.Namespace}}
          {
          """);
        
        /*var genericParameters = _rangeNode is IContainerNode containerNode && containerNode.TypeParameters.Any()
            ? $"<{string.Join(", ", containerNode.TypeParameters.Select(p => p.Name))}>"
            : ""; // ToDo generic types for the container */
        
        foreach (var nestingParentName in _containerInfo.ContainingTypeNames)
        {
            _code.AppendLine(
                $$"""
                  partial class {{nestingParentName}}
                  {
                  """);
        }

        var inheritanceElements = _scopeNodeBaseCodeGenerator.GetInheritanceHeaderElements(_scopeNodeManager.ContainerScopeNode, isContainer: true);

        var inheritance = inheritanceElements.Any()
            ? $" : {string.Join(", ", inheritanceElements)}"
            : "";

        _code.AppendLine(
            $$"""
              sealed partial class {{_containerInfo.Name}}{{inheritance}}
              {
              """);
        
        _contextGenerator.GenerateContextClass(_code);

        _scopeNodeBaseCodeGenerator.GenerateInterface(_code);

        var constructors = _containerInfo.ContainerType.GetMembers().OfType<IMethodSymbol>()
            .Where(ms => ms.MethodKind == MethodKind.Constructor);

        foreach (var constructor in constructors)
        {
            var containerReference = _referenceGenerator.Generate("container");
            _code.AppendLine(
                $$"""
                  public static {{_containerInfo.ContainerType.FullName()}} {{Constants.CreateContainerFunctionName}}({{string.Join(", ", constructor.Parameters.Select(p => $"{p.Type.FullName()} {p.Name}"))}})
                  {
                  {{_containerInfo.ContainerType.FullName()}} {{containerReference}} = new {{_containerInfo.ContainerType.FullName()}}({{string.Join(", ", constructor.Parameters.Select(p => p.Name))}});
                  """);
            // ToDo add initialized instances
            _code.AppendLine(
                $$"""
                  return {{containerReference}};
                  }
                  """);
        }
        
        _scopeNodeBaseCodeGenerator.GenerateScopedInstanceFunctions(_code, _scopeNodeManager.ContainerScopeNode, Constants.ThisKeyword);

        var typesGettingFunctorEntry = _concreteFunctorNodeManager.AllNodes
            .Select(n => n.ReturnedElement.Target)
            .Distinct();
        foreach (var typeNode in typesGettingFunctorEntry)
        {
            var function = new FunctorEntryFunction(typeNode.Type);
            var functionName = _functionUtility.GetName(function);
            _code.AppendLine(
                $$"""
                  {{_functionUtility.GenerateHeader(function)}}
                  {
                  """);
            if (typeNode.Incoming.Select(e => e.Type).OfType<FunctionEdgeType>().FirstOrDefault() is { } nextFunction)
            {
                _code.AppendLine($"return {_functionUtility.GenerateFunctionCall(nextFunction.Function, doScopedInstance: true, doScopeRoot: true)};");
            }
            else
            {
                _code.AppendLine($"throw new Exception(\"No function found for type {typeNode.Type.FullName()} during code generation.\");");
            }
            _code.AppendLine("}");
            _sharedNameRegistry.AddEntryFunctionsForFunctorsMapping(typeNode.Type, functionName);
        }

        var entryCreateFunctionsMap = new Dictionary<string, string>();
        foreach (var function in _injectionGraphBuilder.Functions)
        {
            var functionName = _functionUtility.GetName(function);

            _code.AppendLine(
                $$"""
                  {{_functionUtility.GenerateHeader(function)}}
                  {
                  """);
            
            var rootNode = function.RootNode;

            _scopeNodeBaseCodeGenerator.GenerateScopeRootEntry(_code, rootNode);
            _scopeNodeBaseCodeGenerator.GenerateScopedInstanceEntry(_code, rootNode);

            var rootReference = _injectionNodeGenerator.GenerateForInjectionNode(_code, rootNode);
            if (!rootNode.Outgoing.Any(e => e.Target is ConcreteEnumerableNode))
                _code.AppendLine($"return {rootReference};");
            _code.AppendLine("}");
            foreach (var entryCreateFunction in rootNode.Incoming.Select(e => e.Source).OfType<ConcreteEntryFunctionNode>())
                entryCreateFunctionsMap[entryCreateFunction.Data.Name] = functionName;
        }

        foreach (var (rootType, name, parameters, _) in _containerInfo.CreateFunctionData)
        {
            if (entryCreateFunctionsMap.TryGetValue(name, out var innerFunctionName)
                && _overrideContextManager.TryGetContext(parameters, out var overrideContext))
            {
                var parametersWithName = parameters.Select(p => (Type: p, Name: _referenceGenerator.Generate(p))).ToArray();
                var parametersOnDeclaration = string.Join(", ", parametersWithName.Select(t => $"{t.Type.FullName()} {t.Name}"));
                var overridesName = _sharedNameRegistry.GetOverrideContextName(overrideContext);
                var overridesAssignment = overrideContext is OverrideContext.Any any 
                    ? string.Join(", ", any.Overrides.Select(p => parametersWithName.First(t => t.Type.Equals(p)).Name))
                    : "";
                _code.AppendLine(
                    $$"""
                      internal {{rootType.FullName()}} {{name}}({{parametersOnDeclaration}})
                      {
                      return {{innerFunctionName}}({{_contextGenerator.GenerateInstanceCreation(overrideInstanceCreation: $"new {overridesName}({overridesAssignment})", outwardFacingTypeNumber: "0", caseNumber: "0", key: "null", containerNode: Constants.ThisKeyword, transientScopeNode: Constants.ThisKeyword, scopeNode: Constants.ThisKeyword, scopeNodeName: $"\"{_containerInfo.Name}\"")}}, {{_functionUtility.DoScopedInstanceParameterName}}: {{Constants.TrueKeyword}}, {{_functionUtility.DoScopeRootParameterName}}: {{Constants.TrueKeyword}});
                      }
                      """);
            }
        }

        var overrideGenericTypeName = _referenceGenerator.Generate("TValue");
        if (_overrideContextManager.AllOverrideContexts.Any(o => o is OverrideContext.Any))
            _code.AppendLine(
                $$"""
                  private interface {{_sharedNameRegistry.IOverrideInterfaceName}}<{{overrideGenericTypeName}}>
                  {
                  {{overrideGenericTypeName}} Value();
                  }
                  """);
        
        foreach (var overrideContext in _overrideContextManager.AllOverrideContexts)
        {
            switch (overrideContext)
            {
                case OverrideContext.None0:
                    var noneTypeName = _sharedNameRegistry.GetOverrideContextName(overrideContext);
                    _code.AppendLine($"private record {noneTypeName};");
                    break;
                case OverrideContext.Any any:
                    var anyTypeName = _sharedNameRegistry.GetOverrideContextName(overrideContext);
                    var properties = any.Overrides.Select((o, i) => $"{o.FullName()} Value{i}");
                    var interfaceAssignments = any.Overrides.Select(o => $"{_sharedNameRegistry.IOverrideInterfaceName}<{o.FullName()}>");
                    _code.AppendLine($"private record {anyTypeName}({string.Join(", ", properties)}) : {string.Join(", ", interfaceAssignments)}");
                    _code.AppendLine("{");
                    var i = 0;
                    foreach (var overrideType in any.Overrides)
                        _code.AppendLine($"{overrideType.FullName()} {_sharedNameRegistry.IOverrideInterfaceName}<{overrideType.FullName()}>.Value() => Value{i++};");
                    _code.AppendLine("}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        foreach (var scopeNode in _scopeNodeManager.TransientScopes)
        {
            var scopeInheritanceElements = _scopeNodeBaseCodeGenerator.GetInheritanceHeaderElements(scopeNode, isContainer: false);
            var scopeInheritance = scopeInheritanceElements.Any()
                ? $" : {string.Join(", ", scopeInheritanceElements)}"
                : "";
            var containerReference = _scopeNodeBaseCodeGenerator.ScopeNodeToContainerPropertyReference[scopeNode];
            
            _code.AppendLine($"{Constants.PrivateKeyword} partial class {scopeNode.Name}{scopeInheritance}");
            _code.AppendLine("{");
            _code.AppendLine($"{Constants.InternalKeyword} required {_containerInfo.FullName} {containerReference} {{ {Constants.PrivateKeyword} get; init; }}");
            
            _scopeNodeBaseCodeGenerator.GenerateScopeRootFunctions(_code, scopeNode, containerReference);
            _scopeNodeBaseCodeGenerator.GenerateScopedInstanceFunctions(_code, scopeNode, containerReference);
            
            _code.AppendLine("}");
        }
        
        foreach (var scopeNode in _scopeNodeManager.Scopes)
        {
            var scopeInheritanceElements = _scopeNodeBaseCodeGenerator.GetInheritanceHeaderElements(scopeNode, isContainer: false);
            var scopeInheritance = scopeInheritanceElements.Any()
                ? $" : {string.Join(", ", scopeInheritanceElements)}"
                : "";
            var containerReference = _scopeNodeBaseCodeGenerator.ScopeNodeToContainerPropertyReference[scopeNode];
            
            _code.AppendLine($"{Constants.PrivateKeyword} partial class {scopeNode.Name}{scopeInheritance}");
            _code.AppendLine("{");
            _code.AppendLine($"{Constants.InternalKeyword} required {_containerInfo.FullName} {containerReference} {{ {Constants.PrivateKeyword} get; init; }}");
            
            _scopeNodeBaseCodeGenerator.GenerateScopeRootFunctions(_code, scopeNode, containerReference);
            _scopeNodeBaseCodeGenerator.GenerateScopedInstanceFunctions(_code, scopeNode, containerReference);
            
            _code.AppendLine("}");
        }

        _code.AppendLine("}");

        _code.AppendLine(string.Join(Environment.NewLine, _containerInfo.ContainingTypeNames.Select(_ => "}")));

        _code.AppendLine(
            """
            }
            #nullable disable
            """);
        
        return _code.ToString();
    }
    
}