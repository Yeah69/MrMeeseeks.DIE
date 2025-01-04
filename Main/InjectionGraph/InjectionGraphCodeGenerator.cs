using Microsoft.CodeAnalysis.CSharp;
using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph;

internal interface IInjectionGraphCodeGenerator
{
    string Generate();
}

internal class InjectionGraphCodeGenerator : IInjectionGraphCodeGenerator
{
    private readonly StringBuilder _code = new();
    private readonly Dictionary<IFunction, string> _functionNames = new();
    private readonly string _overridesParameterName;
    private readonly IReadOnlyList<(ITypeSymbol Type, string Name)> _functionParameters;
    private readonly IContainerInfo _containerInfo;
    private readonly IInjectionGraphBuilder _injectionGraphBuilder;
    private readonly OverrideContextManager _overrideContextManager;
    private readonly IReferenceGenerator _referenceGenerator;
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly string _iOverrideInterfaceName;

    public InjectionGraphCodeGenerator(IContainerInfo containerInfo,
        IInjectionGraphBuilder injectionGraphBuilder,
        OverrideContextManager overrideContextManager,
        IReferenceGenerator referenceGenerator,
        WellKnownTypes wellKnownTypes)
    {
        _containerInfo = containerInfo;
        _injectionGraphBuilder = injectionGraphBuilder;
        _overrideContextManager = overrideContextManager;
        _referenceGenerator = referenceGenerator;
        _wellKnownTypes = wellKnownTypes;
        _overridesParameterName = referenceGenerator.Generate("overrides");
        _iOverrideInterfaceName = _referenceGenerator.Generate("IOverride");
        _functionParameters = [(wellKnownTypes.Object, _overridesParameterName)];
    }

    public string Generate()
    {
        var overrideContextNameMap = _overrideContextManager.AllOverrideContexts
            .ToDictionary(o => o, o => _referenceGenerator.Generate(o is OverrideContext.Any ? "Overrides" : "NoOverrides"));
        foreach (var function in _injectionGraphBuilder.Functions)
        {
            if (_functionNames.ContainsKey(function))
                continue;
            _functionNames[function] = _referenceGenerator.Generate("Create", function.RootNode.Type);
        }
        
        _code.AppendLine(
        $$"""
          #nullable enable
          namespace {{_containerInfo.Namespace}}
          {
          """);
        
        /*var genericParameters = _rangeNode is IContainerNode containerNode && containerNode.TypeParameters.Any()
            ? $"<{string.Join(", ", containerNode.TypeParameters.Select(p => p.Name))}>"
            : ""; // ToDo generic types for the container */

        _code.AppendLine(
            $$"""
              sealed partial class {{_containerInfo.Name}}
              {
              """);

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

        var entryCreateFunctionsMap = new Dictionary<string, string>();
        foreach (var function in _injectionGraphBuilder.Functions)
        {
            var functionName = _functionNames[function];

            _code.AppendLine(
                $$"""
                  {{GenerateMethodDeclaration(function, functionName, _functionParameters)}}
                  {
                  """);

            var rootReference = GenerateForInjectionNode(function.RootNode);
            _code.AppendLine($"return {rootReference};");
            _code.AppendLine("}");
            foreach (var entryCreateFunction in function.RootNode.Incoming.Select(e => e.Source).OfType<ConcreteEntryFunctionNode>())
                entryCreateFunctionsMap[entryCreateFunction.Data.Name] = functionName;
        }
        
        foreach (var (rootType, name, parameters) in _containerInfo.CreateFunctionData)
        {
            if (entryCreateFunctionsMap.TryGetValue(name, out var innerFunctionName)
                && _overrideContextManager.TryGetContext(parameters, out var overrideContext))
            {
                var parametersWithName = parameters.Select(p => (Type: p, Name: _referenceGenerator.Generate(p))).ToArray();
                var parametersOnDeclaration = string.Join(", ", parametersWithName.Select(t => $"{t.Type.FullName()} {t.Name}"));
                var overridesName = overrideContextNameMap[overrideContext];
                var overridesReference = _referenceGenerator.Generate("overrides");
                var overridesAssignment = overrideContext is OverrideContext.Any any 
                    ? string.Join(", ", any.Overrides.Select(p => parametersWithName.First(t => t.Type.Equals(p)).Name))
                    : "";
                //var overrides 
                _code.AppendLine(
                    $$"""
                      internal {{rootType.FullName()}} {{name}}({{parametersOnDeclaration}})
                      {
                      {{_wellKnownTypes.Object.FullName()}} {{overridesReference}} = new {{overridesName}}({{overridesAssignment}});
                      return {{innerFunctionName}}({{overridesReference}});
                      }
                      """);
            }
        }

        var overrideGenericTypeName = _referenceGenerator.Generate("TValue");
        _code.AppendLine(
            $$"""
              private interface {{_iOverrideInterfaceName}}<{{overrideGenericTypeName}}>
              {
              {{overrideGenericTypeName}} Value();
              }
              """);
        
        foreach (var overrideContext in _overrideContextManager.AllOverrideContexts)
        {
            switch (overrideContext)
            {
                case OverrideContext.None:
                    var noneTypeName = overrideContextNameMap[overrideContext];
                    _code.AppendLine($"private record {noneTypeName};");
                    break;
                case OverrideContext.Any any:
                    var anyTypeName = overrideContextNameMap[overrideContext];
                    var properties = any.Overrides.Select((o, i) => $"{o.FullName()} Value{i}");
                    var interfaceAssignments = any.Overrides.Select(o => $"{_iOverrideInterfaceName}<{o.FullName()}>");
                    _code.AppendLine($"private record {anyTypeName}({string.Join(", ", properties)}) : {string.Join(", ", interfaceAssignments)}");
                    _code.AppendLine("{");
                    var i = 0;
                    foreach (var overrideType in any.Overrides)
                        _code.AppendLine($"{overrideType.FullName()} {_iOverrideInterfaceName}<{overrideType.FullName()}>.Value() => Value{i++};");
                    _code.AppendLine("}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        _code.AppendLine("}");

        _code.AppendLine(
            """
            }
            #nullable disable
            """);
        
        return _code.ToString();
    }
    
    private string GenerateForInjectionNode(TypeNode node)
    {
        if (node.Outgoing.Count > 1)
            return "not_available"; // ToDo this is wrong, adjust as soon a correct behavior is required
        
        var innerNode = node.Outgoing.Select(e => e.Target).FirstOrDefault();
        
        if (innerNode is ConcreteOverrideNode overrideNode)
        {
            var reference = _referenceGenerator.Generate(overrideNode.Data.Type);
            _code.AppendLine($"{overrideNode.Data.Type.FullName()} {reference} = (({_iOverrideInterfaceName}<{overrideNode.Data.Type.FullName()}>) {_overridesParameterName}).Value();");
            return reference;
        }
        if (innerNode is ConcreteImplementationNode implementationNode)
        {
            var reference = _referenceGenerator.Generate(implementationNode.Data.Implementation);
            
            // Constructor
            var parameterNodes = implementationNode.ConstructorParameters.Select(t => t.Edge);
            var parameters = string.Join(", ", parameterNodes.Select(e => CallFunctionOrGenerateForInjectionNode(e, e.Target)));
            
            // Object initializer
            var objectInitializer = ""; 
            if (implementationNode.ObjectInitializerAssignments.Length > 0)
            {
                var propertyNodeAssignments = implementationNode.ObjectInitializerAssignments;
                objectInitializer = $" {{ {string.Join(", ", propertyNodeAssignments.Select(t => $"{t.Name} = {CallFunctionOrGenerateForInjectionNode(t.Edge, t.Edge.Target)}"))} }}";
            }
            
            _code.AppendLine($"{implementationNode.Data.Implementation.FullName()} {reference} = new {implementationNode.Data.Implementation.FullName()}({parameters}){objectInitializer};");
            
            return reference;
        }
        
        return "not_available";
    }

    private string CallFunctionOrGenerateForInjectionNode(TypeEdge edge, TypeNode node)
    {
        if (edge.Type is FunctionEdgeType functionEdgeType)
        {
            var function = functionEdgeType.Function;
            var resultReference = _referenceGenerator.Generate(function.RootNode.Type);
            _code.AppendLine($"{function.RootNode.Type.FullName()} {resultReference} = {_functionNames[function]}({_overridesParameterName});");
            return resultReference;
        }
        return GenerateForInjectionNode(node);
    }
    
    private static string GenerateMethodDeclaration(IFunction function, string functionName, IReadOnlyList<(ITypeSymbol Type, string Name)> parameters)
    {
        var accessibility = function is { Accessibility: { } acc, ExplicitInterface: null }
            ? $"{SyntaxFacts.GetText(acc)} "  
            : "";
        var asyncModifier = function.IsAsync
            ? "async "
            : "";
        var explicitInterfaceFullName = function.ExplicitInterface is { } explicitInterface
            ? $"{explicitInterface.FullName()}."
            : "";
        var typeParameters = "";
        var typeParametersConstraints = "";
        if (function.TypeParameters.Any())
        {
            typeParameters = $"<{string.Join(", ", function.TypeParameters.Select(p => p.Name))}>";
            typeParametersConstraints = string.Join("", function
                .TypeParameters
                .Where(p => p.HasValueTypeConstraint 
                            || p.HasReferenceTypeConstraint
                            || p.HasNotNullConstraint 
                            || p.HasUnmanagedTypeConstraint
                            || p.HasConstructorConstraint
                            || p.ConstraintTypes.Length > 0)
                .Select(p =>
                {
                    var constraints = new List<string>();
                    if (p.HasUnmanagedTypeConstraint)
                        constraints.Add("unmanaged");
                    else if (p.HasValueTypeConstraint)
                        constraints.Add("struct");
                    else if (p.HasReferenceTypeConstraint)
                        constraints.Add($"class{(p.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated ? "?" : "")}");
                    if (p.HasNotNullConstraint)
                        constraints.Add("notnull");
                    constraints.AddRange(p.ConstraintTypes.Select((t, i) => t.WithNullableAnnotation(p.ConstraintNullableAnnotations[i]).FullName()));
                    if (p.HasConstructorConstraint)
                        constraints.Add("new()");
                    return $"{Environment.NewLine}where {p.Name} : {string.Join(", ", constraints)}";
                }));
        }

        var parametersText = string.Join(", ", parameters.Select(p => $"{p.Type.FullName()} {p.Name}"));
        return $"{accessibility}{asyncModifier}{function.RootNode.Type.FullName()} {explicitInterfaceFullName}{functionName}{typeParameters}({parametersText}){typeParametersConstraints}";
    }
    
}