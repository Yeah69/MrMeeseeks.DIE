using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Mappers;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Elements;

internal interface IImplementationNode : IElementNode, IAwaitableNode
{
    string ConstructorCallName { get; }
    ImplementationNode.UserDefinedInjection? UserDefinedInjectionConstructor { get; }
    IReadOnlyList<(string Name, IElementNode Element)> ConstructorParameters { get; }
    ImplementationNode.UserDefinedInjection? UserDefinedInjectionProperties { get; }
    IReadOnlyList<(string Name, IElementNode Element)> Properties { get; }
    ImplementationNode.Initialization? Initializer { get; }
    string? SyncDisposalCollectionReference { get; }
    string? AsyncDisposalCollectionReference { get; }
    
    string? AsyncReference { get; }
    string? AsyncTypeFullName { get; }
}

internal class ImplementationNode : IImplementationNode
{
    internal record UserDefinedInjection(
        string Name, 
        IReadOnlyList<(string Name, IElementNode Element, bool IsOut)> Parameters);

    internal record Initialization(
        string TypeFullName,
        string MethodName,
        UserDefinedInjection? UserDefinedInjection,
        IReadOnlyList<(string Name, IElementNode Element)> Parameters);
    
    private readonly INamedTypeSymbol _implementationType;
    private readonly IMethodSymbol _constructor;
    private readonly IFunctionNode _parentFunction;
    private readonly IRangeNode _parentRange;
    private readonly IElementNodeMapperBase _elementNodeMapper;
    private readonly ICheckTypeProperties _checkTypeProperties;
    private readonly IUserDefinedElements _userDefinedElements;
    private readonly IReferenceGenerator _referenceGenerator;
    private readonly IInjectablePropertyExtractor _injectablePropertyExtractor;

    private readonly List<(string Name, IElementNode Element)> _constructorParameters = new ();
    private readonly List<(string Name, IElementNode Element)> _properties = new ();

    internal ImplementationNode(
        INamedTypeSymbol implementationType,
        IMethodSymbol constructor,
        
        IFunctionNode parentFunction,
        IElementNodeMapperBase elementNodeMapper,
        ITransientScopeWideContext transientScopeWideContext,
        IReferenceGenerator referenceGenerator,
        IInjectablePropertyExtractor injectablePropertyExtractor)
    {
        _implementationType = implementationType;
        _constructor = constructor;
        _parentFunction = parentFunction;
        _parentRange = transientScopeWideContext.Range;
        _elementNodeMapper = elementNodeMapper;
        _checkTypeProperties = transientScopeWideContext.CheckTypeProperties;
        _userDefinedElements = transientScopeWideContext.UserDefinedElements;
        _referenceGenerator = referenceGenerator;
        _injectablePropertyExtractor = injectablePropertyExtractor;
        TypeFullName = implementationType.FullName();
        // The constructor call shouldn't contain nullable annotations
        ConstructorCallName = implementationType.FullName(SymbolDisplayMiscellaneousOptions.None);
        Reference = referenceGenerator.Generate(implementationType);
    }

    public void Build(ImmutableStack<INamedTypeSymbol> implementationStack)
    {
        _parentFunction.RegisterAwaitableNode(this);
        var implementationCycle = implementationStack.Contains(_implementationType, CustomSymbolEqualityComparer.Default);

        if (implementationCycle)
        {
            var cycleStack = ImmutableStack.Create(_implementationType);
            var stack = implementationStack;
            INamedTypeSymbol i;
            do
            {
                stack = stack.Pop(out var popped);
                cycleStack = cycleStack.Push(popped);
                i = popped;
            } while (!CustomSymbolEqualityComparer.Default.Equals(_implementationType, i));
            
            throw new ImplementationCycleDieException(cycleStack);
        }

        implementationStack = implementationStack.Push(_implementationType);
        
        var (userDefinedInjectionConstructor, outParamsConstructor) = GetUserDefinedInjection(_userDefinedElements.GetConstructorParametersInjectionFor(_implementationType));
        var (userDefinedInjectionProperties, outParamsProperties) = GetUserDefinedInjection(_userDefinedElements.GetPropertiesInjectionFor(_implementationType));

        UserDefinedInjectionConstructor = userDefinedInjectionConstructor;
        UserDefinedInjectionProperties = userDefinedInjectionProperties;
        
        _constructorParameters.AddRange(_constructor.Parameters
            .Select(p => (p.Name, MapToInjection(p.Name, p.Type, outParamsConstructor))));

        IEnumerable<IPropertySymbol> properties;
        if (_checkTypeProperties.GetPropertyChoicesFor(_implementationType) is { } propertyChoice)
            properties = propertyChoice;
        // Automatic property injection is disabled for record types, but property choices are still allowed
        else if (!_implementationType.IsRecord)
            properties = _injectablePropertyExtractor
                .GetInjectableProperties(_implementationType)
                // Check whether property is settable
                .Where(p => p.IsRequired || (p.SetMethod?.IsInitOnly ?? false));
        else 
            properties = Array.Empty<IPropertySymbol>();
        _properties.AddRange(properties
            .Select(p => (p.Name, MapToInjection(p.Name, p.Type, outParamsProperties))));

        if (_checkTypeProperties.GetInitializerFor(_implementationType) is { Type: {} initializerType, Initializer: {} initializerMethod })
        {
            var (userDefinedInjectionInitializer, outParamsInitializer) = GetUserDefinedInjection(_userDefinedElements.GetInitializerParametersInjectionFor(_implementationType));

            var initializerParameters = initializerMethod.Parameters
                .Select(p => (p.Name, MapToInjection(p.Name, p.Type, outParamsInitializer)))
                .ToList();

            Initializer = new Initialization(
                initializerType.FullName(SymbolDisplayMiscellaneousOptions.None),
                initializerMethod.Name,
                userDefinedInjectionInitializer,
                initializerParameters);

            // if not void then the initializer return either ValueTask or Task (meaning it is async)
            if (!initializerMethod.ReturnsVoid)
            {
                Awaited = true;
                AsyncReference = _referenceGenerator.Generate("task");
                AsyncTypeFullName = initializerMethod.ReturnType.FullName(); // ReturnType can only be either ValueTask or Task at this point
            }
        }

        var disposalType = _checkTypeProperties.ShouldDisposalBeManaged(_implementationType);
        if (disposalType.HasFlag(DisposalType.Sync))
            SyncDisposalCollectionReference = _parentRange.DisposalHandling.RegisterSyncDisposal();
        if (disposalType.HasFlag(DisposalType.Async))
            AsyncDisposalCollectionReference = _parentRange.DisposalHandling.RegisterAsyncDisposal();
            

        (UserDefinedInjection? UserdefinedInjection, IReadOnlyDictionary<string, IElementNode>) GetUserDefinedInjection(IMethodSymbol? method)
        {
            if (method is not { }) return (null, new Dictionary<string, IElementNode>());
            var injectionParameters = method
                .Parameters
                .Select(p =>
                {
                    var isOut = p.RefKind == RefKind.Out;
                    var element = isOut
                        ? _elementNodeMapper.MapToOutParameter(p.Type, implementationStack)
                        : _elementNodeMapper.Map(p.Type, implementationStack);
                    return (p.Type, p.Name, Element: element, IsOut: isOut);
                })
                .ToArray();
            return (
                new UserDefinedInjection(method.Name, injectionParameters.Select(ip => (ip.Name, ip.Element, ip.IsOut)).ToArray()),
                injectionParameters.Where(ip => ip.IsOut).ToDictionary(ip => ip.Name, ip => ip.Element));
        }

        IElementNode MapToInjection(
            string key,
            ITypeSymbol typeParam,
            IReadOnlyDictionary<string, IElementNode> outElementsCache) =>
            outElementsCache.TryGetValue(key, value: out var element)
                ? element
                : _elementNodeMapper.Map(typeParam, implementationStack);
    }

    public void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitImplementationNode(this);

    public string TypeFullName { get; }
    public string Reference { get; }
    public string ConstructorCallName { get; }
    public UserDefinedInjection? UserDefinedInjectionConstructor { get; private set; }
    public IReadOnlyList<(string Name, IElementNode Element)> ConstructorParameters => _constructorParameters;
    public UserDefinedInjection? UserDefinedInjectionProperties { get; private set; }
    public IReadOnlyList<(string Name, IElementNode Element)> Properties => _properties;
    public Initialization? Initializer 
    {
        get;
        private set;
    }

    public string? SyncDisposalCollectionReference { get; private set; }
    public string? AsyncDisposalCollectionReference { get; private set; }

    public bool Awaited { get; private set; }
    public string? AsyncReference { get; private set; }
    public string? AsyncTypeFullName { get; private set; }
}