using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Logging;
using MrMeeseeks.DIE.Mappers;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Elements;

internal interface IImplementationNode : IElementNode
{
    string ConstructorCallName { get; }
    ImplementationNode.UserDefinedInjection? UserDefinedInjectionConstructor { get; }
    IReadOnlyList<(string Name, IElementNode Element)> ConstructorParameters { get; }
    ImplementationNode.UserDefinedInjection? UserDefinedInjectionProperties { get; }
    IReadOnlyList<(string Name, IElementNode Element)> Properties { get; }
    ImplementationNode.Initialization? Initializer { get; }
    string SubDisposalReference { get; }
    bool AggregateForDisposal { get; }
    
    bool InitializerReturnsSomeTask { get; }
    string? AsyncReference { get; }
    string? AsyncTypeFullName { get; }
    
    string ImplementationTypeFullName { get; }
    
    void AppendAdditionalProperties((string Name, IElementNode Element)[] properties);
}

internal sealed partial class ImplementationNode : IImplementationNode
{
    internal sealed record UserDefinedInjection(
        string Name, 
        IReadOnlyList<(string Name, IElementNode Element, bool IsOut)> Parameters,
        IReadOnlyList<string> TypeParameters);

    internal sealed record Initialization(
        string TypeFullName,
        string MethodName,
        UserDefinedInjection? UserDefinedInjection,
        IReadOnlyList<(string Name, IElementNode Element)> Parameters);
    
    private readonly INamedTypeSymbol _implementationType;
    private readonly IMethodSymbol? _constructor;
    private readonly IFunctionNode _parentFunction;
    private readonly IContainerNode _parentContainer;
    private readonly IRangeNode _parentRange;
    private readonly IElementNodeMapperBase _elementNodeMapper;
    private readonly ICheckTypeProperties _checkTypeProperties;
    private readonly IUserDefinedElements _userDefinedElements;
    private readonly IReferenceGenerator _referenceGenerator;
    private readonly ILocalDiagLogger _localDiagLogger;
    private readonly ITaskBasedQueue _taskBasedQueue;
    private readonly IInjectablePropertyExtractor _injectablePropertyExtractor;

    private readonly List<(string Name, IElementNode Element)> _constructorParameters = [];
    private readonly List<(string Name, IElementNode Element)> _properties = [];
    private (string Name, IElementNode Element)[] _additionalProperties = [];

    internal ImplementationNode(
        // parameters
        INamedTypeSymbol? abstractionType,
        INamedTypeSymbol implementationType,
        IMethodSymbol? constructor, // assume empty constructor if null
        
        // dependencies
        IFunctionNode parentFunction,
        IContainerNode parentContainer,
        IRangeNode parentRange,
        IElementNodeMapperBase elementNodeMapper,
        IUserDefinedElements userDefinedElements,
        ICheckTypeProperties checkTypeProperties,
        IReferenceGenerator referenceGenerator,
        ILocalDiagLogger localDiagLogger,
        ITaskBasedQueue taskBasedQueue,
        IInjectablePropertyExtractor injectablePropertyExtractor)
    {
        _implementationType = implementationType;
        _constructor = constructor;
        _parentFunction = parentFunction;
        _parentContainer = parentContainer;
        _parentRange = parentRange;
        _elementNodeMapper = elementNodeMapper;
        _checkTypeProperties = checkTypeProperties;
        _userDefinedElements = userDefinedElements;
        _referenceGenerator = referenceGenerator;
        _localDiagLogger = localDiagLogger;
        _taskBasedQueue = taskBasedQueue;
        _injectablePropertyExtractor = injectablePropertyExtractor;
        TypeFullName = abstractionType?.FullName() ?? implementationType.FullName();
        ImplementationTypeFullName = implementationType.FullName();
        // The constructor call shouldn't contain nullable annotations
        ConstructorCallName = implementationType.FullName(SymbolDisplayMiscellaneousOptions.None);
        Reference = referenceGenerator.Generate(implementationType);
        SubDisposalReference = parentFunction.SubDisposalNode.Reference;
    }

    public void Build(PassedContext passedContext)
    {
        var implementationCycle = passedContext.ImplementationStack.Contains(_implementationType, CustomSymbolEqualityComparer.Default);

        if (implementationCycle)
        {
            var cycleStack = ImmutableStack.Create(_implementationType);
            var stack = passedContext.ImplementationStack;
            INamedTypeSymbol i;
            do
            {
                stack = stack.Pop(out var popped);
                cycleStack = cycleStack.Push(popped);
                i = popped;
            } while (!CustomSymbolEqualityComparer.Default.Equals(_implementationType, i));
            
            _localDiagLogger.Error(
                ErrorLogData.CircularReferenceInsideFactory(cycleStack), 
                _implementationType.Locations.FirstOrDefault() ?? Location.None);
            
            throw new ImplementationCycleDieException(cycleStack);
        }

        passedContext = passedContext with
        {
            ImplementationStack = passedContext.ImplementationStack.Push(_implementationType)
        };
        
        var (userDefinedInjectionConstructor, outParamsConstructor) = 
            GetUserDefinedInjection(
                _userDefinedElements.GetConstructorParametersInjectionFor(_implementationType),
                name => _constructor?.Parameters.FirstOrDefault(p => p.Name == name)?.Type);
        var (userDefinedInjectionProperties, outParamsProperties) = 
            GetUserDefinedInjection(
                _userDefinedElements.GetPropertiesInjectionFor(_implementationType),
                name => _implementationType.GetMembers().OfType<IPropertySymbol>().FirstOrDefault(p => p.Name == name)?.Type);

        UserDefinedInjectionConstructor = userDefinedInjectionConstructor;
        UserDefinedInjectionProperties = userDefinedInjectionProperties;
        
        _constructorParameters.AddRange(_constructor
            ?.Parameters
            .Select(p => (p.Name, MapToInjection(p.Name, p.Type, p, outParamsConstructor))) ?? []);

        IReadOnlyList<IPropertySymbol> properties;
        if (_checkTypeProperties.GetPropertyChoicesFor(_implementationType) is { } propertyChoice)
            properties = propertyChoice;
        // Automatic property injection is disabled for record types, but property choices are still allowed
        else if (!_implementationType.IsRecord)
            properties = _injectablePropertyExtractor
                .GetInjectableProperties(_implementationType)
                // Check whether property is settable
                .Where(p => p.IsRequired || (p.SetMethod?.IsInitOnly ?? false))
                .ToList();
        else 
            properties = [];
        _properties.AddRange(properties
            .Select(p => (p.Name, MapToInjection(p.Name, p.Type, p, outParamsProperties))));

        var injectionsAnalysisGathering = _constructor
            ?.Parameters
            .Select(ps => (ps.Type, $"\"{ps.Name}\" (constructor parameter)"))
            .Concat(properties
                .Select(ps => (ps.Type, $"\"{ps.Name}\" (property)")))
            .ToList() ?? [];

        if (_checkTypeProperties.GetInitializerFor(_implementationType) is { Type: {} initializerType, Initializer: {} initializerMethod })
        {
            var (userDefinedInjectionInitializer, outParamsInitializer) =
                GetUserDefinedInjection(
                    _userDefinedElements.GetInitializerParametersInjectionFor(_implementationType),
                    name => initializerMethod.Parameters.FirstOrDefault(p => p.Name == name)?.Type);

            var initializerParameters = initializerMethod.Parameters
                .Select(p => (p.Name, MapToInjection(p.Name, p.Type, p, outParamsInitializer)))
                .ToList();

            Initializer = new Initialization(
                initializerType.FullName(SymbolDisplayMiscellaneousOptions.None),
                initializerMethod.Name,
                userDefinedInjectionInitializer,
                initializerParameters);

            // if not void then the initializer return either ValueTask or Task (meaning it is async)
            if (!initializerMethod.ReturnsVoid)
            {
                InitializerReturnsSomeTask = true;
                AsyncReference = _referenceGenerator.Generate("task");
                AsyncTypeFullName = initializerMethod.ReturnType.FullName(); // ReturnType can only be either ValueTask or Task at this point
                _taskBasedQueue.EnqueueTaskBasedOnlyFunction(_parentFunction);
            }
            
            injectionsAnalysisGathering.AddRange(initializerMethod
                .Parameters
                .Select(ps => (ps.Type, $"\"{ps.Name}\" (initialize method parameter)")));
        }

        var disposalType = _checkTypeProperties.ShouldDisposalBeManaged(_implementationType);
        if (disposalType is not DisposalType.None)
        {
            _parentRange.RegisterTypeForDisposal(_implementationType);
            _parentFunction.AddOneToSubDisposalCount();
            if (disposalType.HasFlag(DisposalType.Sync))
                _parentRange.DisposalHandling.RegisterSyncDisposal();
            if (disposalType.HasFlag(DisposalType.Async))
                _parentRange.DisposalHandling.RegisterAsyncDisposal();
        }
        AggregateForDisposal = disposalType is not DisposalType.None;
        
        foreach (var sameTypeInjections in injectionsAnalysisGathering
                     .GroupBy(t => t.Type, CustomSymbolEqualityComparer.IncludeNullability)
                     .Where(g => g.Count() > 1))
            if (sameTypeInjections.Key is ITypeSymbol type)
                _localDiagLogger.Warning(WarningLogData.ImplementationHasMultipleInjectionsOfSameTypeWarning(
                    $"Implementation has multiple injections of same type \"{type.FullName()}\": {string.Join(", ", sameTypeInjections.Select(t => t.Item2))}"),
                    _implementationType.Locations.FirstOrDefault() ?? Location.None);
        return;


        (UserDefinedInjection? UserdefinedInjection, IReadOnlyDictionary<string, IElementNode>) GetUserDefinedInjection(
            IMethodSymbol? method,
            Func<string, ITypeSymbol?> typeOfMemberSelector)
        {
            if (method is null) return (null, new Dictionary<string, IElementNode>());
            var injectionParameters = method
                .Parameters
                .Select(p =>
                {
                    var isOut = p.RefKind == RefKind.Out;
                    var element = isOut
                        ? _elementNodeMapper.MapToOutParameter(p.Type, passedContext)
                        : _elementNodeMapper.Map(p.Type, passedContext);
                    return (p.Type, p.Name, Element: element, IsOut: isOut);
                })
                .ToArray();
            
            var typeParametersMap = new Dictionary<ITypeParameterSymbol, ITypeParameterSymbol>();
            foreach (var valueTuple in injectionParameters.Where(ip => ip.IsOut))
            {
                var (type, name, _, _) = valueTuple;

                if (typeOfMemberSelector(name) is not { } at)
                {
                    _localDiagLogger.Warning(
                        WarningLogData.ValidationUserDefinedElement(
                            method,
                            _parentRange,
                            _parentContainer,
                            $"User-defined injection name mismatch. An element called \"{name}\" couldn't be found."),
                        method.Locations.FirstOrDefault() ?? Location.None);
                    continue;
                }
                
                if (!CheckTypeMatches(type, at))
                    _localDiagLogger.Error(
                        ErrorLogData.ValidationUserDefinedElement(
                            method,
                            _parentRange,
                            _parentContainer,
                            $"User-defined injection type mismatch for \"{name}\". Expected \"{type.FullName()}\", but found \"{at.FullName()}\""),
                        method.Locations.FirstOrDefault() ?? Location.None);

                bool CheckTypeMatches(ITypeSymbol userDefinedType, ITypeSymbol actualType)
                {
                    switch (userDefinedType)
                    {
                        case IArrayTypeSymbol userDefinedArray:
                            return actualType is IArrayTypeSymbol actualArray 
                                   && CheckTypeMatches(userDefinedArray.ElementType, actualArray.ElementType);
                        case IDynamicTypeSymbol:
                            return actualType is IDynamicTypeSymbol;
                        case IErrorTypeSymbol:
                            return false;
                        case IFunctionPointerTypeSymbol userDefinedFunctionPointer:
                            if (actualType is not IFunctionPointerTypeSymbol actualFunctionPointer)
                                return false;
                            return userDefinedFunctionPointer.Signature.Parameters.Length != actualFunctionPointer.Signature.Parameters.Length
                                   && (userDefinedFunctionPointer.Signature.ReturnsVoid && actualFunctionPointer.Signature.ReturnsVoid
                                       || CheckTypeMatches(userDefinedFunctionPointer.Signature.ReturnType, actualFunctionPointer.Signature.ReturnType))
                                   && userDefinedFunctionPointer.Signature.Parameters.Select(p => p.Type)
                                       .Zip(actualFunctionPointer.Signature.Parameters.Select(p => p.Type), CheckTypeMatches)
                                       .All(b => b);
                        case INamedTypeSymbol userDefinedNamedType:
                            if (actualType is not INamedTypeSymbol actualNamedType)
                                return false;
                            if (userDefinedNamedType.Arity != actualNamedType.Arity
                                || userDefinedNamedType.ToDisplayString(SymbolDisplayFormatPicks.FullNameExceptTypeParameters) 
                                != actualNamedType.ToDisplayString(SymbolDisplayFormatPicks.FullNameExceptTypeParameters))
                                return false;
                            return userDefinedNamedType.TypeArguments.Zip(actualNamedType.TypeArguments, CheckTypeMatches)
                                .All(b => b);
                        case IPointerTypeSymbol userDefinedPointer:
                            return actualType is IPointerTypeSymbol actualPointer 
                                   && CheckTypeMatches(userDefinedPointer.PointedAtType, actualPointer.PointedAtType);
                        case ITypeParameterSymbol userDefinedTypeParameter:
                            if (actualType is not ITypeParameterSymbol actualTypeParameter)
                                return false;
                            if (typeParametersMap.TryGetValue(userDefinedTypeParameter, out var foundActualTypeParameter))
                                return SymbolEqualityComparer.Default.Equals(foundActualTypeParameter, actualTypeParameter);
                        
                            typeParametersMap[userDefinedTypeParameter] = actualTypeParameter;
                            
                            if (userDefinedTypeParameter.HasValueTypeConstraint && !actualTypeParameter.HasValueTypeConstraint)
                                return false;
                            
                            if (userDefinedTypeParameter.HasReferenceTypeConstraint && !actualTypeParameter.HasReferenceTypeConstraint)
                                return false;
                            
                            if (userDefinedTypeParameter.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated 
                                && actualTypeParameter.ReferenceTypeConstraintNullableAnnotation != NullableAnnotation.Annotated)
                                return false;
                            
                            if (userDefinedTypeParameter.HasNotNullConstraint && !actualTypeParameter.HasNotNullConstraint)
                                return false;
                            
                            if (userDefinedTypeParameter.HasUnmanagedTypeConstraint && !actualTypeParameter.HasUnmanagedTypeConstraint)
                                return false;
                            
                            if (userDefinedTypeParameter.HasConstructorConstraint && !actualTypeParameter.HasConstructorConstraint)
                                return false;
                            
                            if (userDefinedTypeParameter.ConstraintTypes.Length > actualTypeParameter.ConstraintTypes.Length)
                                return false;
                        
                            for (int u = 0; u < userDefinedTypeParameter.ConstraintTypes.Length; u++)
                            {
                                var check = false;
                                for (int a = 0; a < actualTypeParameter.ConstraintTypes.Length; a++)
                                {
                                    if (CheckTypeMatches(userDefinedTypeParameter.ConstraintTypes[u], actualTypeParameter.ConstraintTypes[a])
                                        && (userDefinedTypeParameter.ConstraintNullableAnnotations[u] != NullableAnnotation.Annotated || actualTypeParameter.ConstraintNullableAnnotations[a] == NullableAnnotation.Annotated))
                                    {
                                        check = true;
                                        break;
                                    }
                                }
                                if (!check) 
                                    return false;
                            }
                        
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(userDefinedType));
                    }

                    return true;
                }
            }

            var unassignedTypeParameters = method.TypeParameters.Where(tp => !typeParametersMap.ContainsKey(tp)).ToArray();
            
            if (unassignedTypeParameters.Length != 0)
                _localDiagLogger.Error(
                    ErrorLogData.ValidationUserDefinedElement(
                        method,
                        _parentRange,
                        _parentContainer,
                        $"Unassigned type parameters: {string.Join(", ", unassignedTypeParameters.Select(tp => tp.FullName()))}"),
                    method.Locations.FirstOrDefault() ?? Location.None);

            var typeParameters = method.TypeParameters.Select(tp => typeParametersMap.TryGetValue(tp, out var actualTypeParameter)
                    ? actualTypeParameter.Name
                    : "")
                .ToArray();

            return (
                new UserDefinedInjection(
                    method.Name, 
                    injectionParameters.Select(ip => (ip.Name, ip.Element, ip.IsOut)).ToArray(),
                    typeParameters),
                injectionParameters.Where(ip => ip.IsOut).ToDictionary(ip => ip.Name, ip => ip.Element));
        }

        IElementNode MapToInjection(
            string key,
            ITypeSymbol typeParam,
            ISymbol parameterOrProperty,
            IReadOnlyDictionary<string, IElementNode> outElementsCache) =>
            outElementsCache.TryGetValue(key, value: out var element)
                ? element
                : _elementNodeMapper.Map(typeParam,
                    passedContext with
                    {
                        InjectionKeyModification = 
                        _checkTypeProperties.IdentifyInjectionKeyModification(parameterOrProperty)
                    });
    }

    public string TypeFullName { get; }
    public string Reference { get; }
    public string ConstructorCallName { get; }
    public UserDefinedInjection? UserDefinedInjectionConstructor { get; private set; }
    public IReadOnlyList<(string Name, IElementNode Element)> ConstructorParameters => _constructorParameters;
    public UserDefinedInjection? UserDefinedInjectionProperties { get; private set; }
    public IReadOnlyList<(string Name, IElementNode Element)> Properties => 
        _properties.Concat(_additionalProperties).ToList();
    public Initialization? Initializer 
    {
        get;
        private set;
    }

    public string SubDisposalReference { get; }
    public bool AggregateForDisposal { get; private set; }

    public bool InitializerReturnsSomeTask { get; private set; }
    public string? AsyncReference { get; private set; }
    public string? AsyncTypeFullName { get; private set; }
    public string ImplementationTypeFullName { get; }
    public void AppendAdditionalProperties((string Name, IElementNode Element)[] properties) => 
        _additionalProperties = properties;
}