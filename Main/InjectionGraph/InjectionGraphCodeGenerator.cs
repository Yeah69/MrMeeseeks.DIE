using Microsoft.CodeAnalysis.CSharp;
using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph;

internal interface IInjectionGraphCodeGenerator
{
    string Generate();
}

internal class InjectionGraphCodeGenerator : IInjectionGraphCodeGenerator
{
    private const string NotAvailable = "null!"; // ToDo change value to "not_available" as soon as correct behavior is required
    private readonly StringBuilder _code = new();
    private readonly Dictionary<IFunction, string> _functionNames = [];
    private readonly Dictionary<ITypeSymbol, string> _entryFunctionsForFunctors = [];
    private readonly string _overridesParameterName;
    private readonly IReadOnlyList<(ITypeSymbol Type, string Name)> _functionParameters;
    private readonly IContainerInfo _containerInfo;
    private readonly IInjectionGraphBuilder _injectionGraphBuilder;
    private readonly OverrideContextManager _overrideContextManager;
    private readonly ConcreteFunctorNodeManager _concreteFunctorNodeManager;
    private readonly IReferenceGenerator _referenceGenerator;
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly string _iOverrideInterfaceName;
    private Dictionary<OverrideContext, string> _overrideContextNameMap = [];

    public InjectionGraphCodeGenerator(IContainerInfo containerInfo,
        IInjectionGraphBuilder injectionGraphBuilder,
        OverrideContextManager overrideContextManager,
        ConcreteFunctorNodeManager concreteFunctorNodeManager,
        IReferenceGenerator referenceGenerator,
        WellKnownTypes wellKnownTypes)
    {
        _containerInfo = containerInfo;
        _injectionGraphBuilder = injectionGraphBuilder;
        _overrideContextManager = overrideContextManager;
        _concreteFunctorNodeManager = concreteFunctorNodeManager;
        _referenceGenerator = referenceGenerator;
        _wellKnownTypes = wellKnownTypes;
        _overridesParameterName = referenceGenerator.Generate("overrides");
        _iOverrideInterfaceName = _referenceGenerator.Generate("IOverride");
        _functionParameters = [(wellKnownTypes.Object, _overridesParameterName)];
    }

    public string Generate()
    {
        _overrideContextNameMap = _overrideContextManager.AllOverrideContexts
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

        var typesGettingFunctorEntry = _concreteFunctorNodeManager.AllNodes
            .Select(n => n.ReturnedElement.Target)
            .Distinct();
        foreach (var typeNode in typesGettingFunctorEntry)
        {
            var functionName = _referenceGenerator.Generate("Create", typeNode.Type);
            var function = new FunctorEntryFunction(typeNode.Type) { Accessibility = Accessibility.Private };
            _code.AppendLine(
                $$"""
                  {{GenerateMethodDeclaration(function, functionName, _functionParameters)}}
                  {
                  """);
            if (typeNode.Incoming.Select(e => e.Type).OfType<FunctionEdgeType>().FirstOrDefault() is { } nextFunction)
            {
                _code.AppendLine($"return {_functionNames[nextFunction.Function]}({_overridesParameterName});");
            }
            else
            {
                _code.AppendLine($"throw new Exception(\"No function found for type {typeNode.Type.FullName()} during code generation.\");");
            }
            _code.AppendLine("}");
            _entryFunctionsForFunctors[typeNode.Type] = functionName;
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
                var overridesName = _overrideContextNameMap[overrideContext];
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
        if (_overrideContextManager.AllOverrideContexts.Any(o => o is OverrideContext.Any))
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
                    var noneTypeName = _overrideContextNameMap[overrideContext];
                    _code.AppendLine($"private record {noneTypeName};");
                    break;
                case OverrideContext.Any any:
                    var anyTypeName = _overrideContextNameMap[overrideContext];
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
            return NotAvailable; // ToDo this is wrong, adjust as soon a correct behavior is required
        
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

            var implementationFullName = GetImplementationsFullName(implementationNode.Data.Implementation);
            _code.AppendLine($"{implementationFullName} {reference} = new {implementationFullName}({parameters}){objectInitializer};");
            
            return reference;

            static string GetImplementationsFullName(ITypeSymbol implementation)
            {
                var implementationFullName = implementation.FullName();
                if (!implementationFullName.StartsWith("(") || !implementationFullName.EndsWith(")") || implementation is not INamedTypeSymbol namedType)
                    return implementationFullName;
                var namespaceFullName = implementation.ContainingNamespace.FullName();
                var typeName = implementation.Name;
                var typeParameters = namedType.TypeArguments.Length > 0 
                    ? $"<{string.Join(", ", namedType.TypeArguments.Select(GetImplementationsFullName))}>"
                    : "";
                return $"{namespaceFullName}.{typeName}{typeParameters}";
            }
        }
        if (innerNode is ConcreteFunctorNode functorNode)
        {
            var reference = _referenceGenerator.Generate(functorNode.Data.Type);
            switch (functorNode.FunctorType)
            {
                case ConcreteFunctorNodeType.Func:
                    var parameterReferences = functorNode.FunctorParameterTypes.Select(_ => _referenceGenerator.Generate("p")).ToArray();
                    var parameterDeclaration = string.Join(", ", parameterReferences);
                    if (_overrideContextManager.TryGetContext(functorNode.FunctorParameterTypes, out var overrideContext)
                        && _overrideContextNameMap.TryGetValue(overrideContext, out var overrideContextName))
                    {
                        var overrideParameters = overrideContext is OverrideContext.Any any
                            ? string.Join(", ", any.Overrides.Select(o => parameterReferences[functorNode.FunctorParameterTypes.Select((t, i) => (t, i)).First(t => CustomSymbolEqualityComparer.IncludeNullability.Equals(t.t, o)).i]))
                            : "";
                        _code.AppendLine($"{functorNode.Data.Type.FullName()} {reference} = ({parameterDeclaration}) => {_entryFunctionsForFunctors[functorNode.ReturnedElement.Target.Type]}(new {overrideContextName}({overrideParameters}));");
                    }
                    break;
                case ConcreteFunctorNodeType.Lazy:
                case ConcreteFunctorNodeType.ThreadLocal:
                    _code.AppendLine($"{functorNode.Data.Type.FullName()} {reference} = new {functorNode.Data.Type.FullName()}(() => {_entryFunctionsForFunctors[functorNode.ReturnedElement.Target.Type]}(new {_overrideContextNameMap[new OverrideContext.None()]}()));");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return reference;
        }
        return NotAvailable;
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
        return $"{accessibility}{asyncModifier}{function.ReturnType.FullName()} {explicitInterfaceFullName}{functionName}{typeParameters}({parametersText}){typeParametersConstraints}";
    }
    
}