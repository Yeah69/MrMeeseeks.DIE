namespace MrMeeseeks.DIE.ResolutionBuilding;

internal abstract class RangeResolutionBaseBuilder
{

    internal abstract record InterfaceExtension(
        INamedTypeSymbol InterfaceType)
    {
        internal string KeySuffix() =>
            this switch
            {
                DecorationInterfaceExtension decoration => $":::{decoration.ImplementationType.FullName()}",
                CompositionInterfaceExtension composition => string.Join(":::", composition.ImplementationTypes.Select(i => i.FullName())),
                _ => throw new ArgumentException()
            };
        
        internal string RangedNameSuffix() =>
            this switch
            {
                DecorationInterfaceExtension decoration => $"_{decoration.ImplementationType.Name}",
                CompositionInterfaceExtension =>  "_Composite",
                _ => throw new ArgumentException()
            };
    }
    internal record DecorationInterfaceExtension(
        INamedTypeSymbol InterfaceType,
        INamedTypeSymbol ImplementationType,
        INamedTypeSymbol DecoratorType,
        InterfaceResolution CurrentInterfaceResolution)
        : InterfaceExtension(InterfaceType);
    internal record CompositionInterfaceExtension(
        INamedTypeSymbol InterfaceType,
        IReadOnlyList<INamedTypeSymbol> ImplementationTypes,
        INamedTypeSymbol CompositeType,
        IReadOnlyList<InterfaceResolution> InterfaceResolutionComposition)
        : InterfaceExtension(InterfaceType);
    
    protected readonly WellKnownTypes WellKnownTypes;
    protected readonly ITypeToImplementationsMapper TypeToImplementationsMapper;
    protected readonly ICheckTypeProperties CheckTypeProperties;
    protected readonly ICheckDecorators CheckDecorators;

    protected readonly IReferenceGenerator RootReferenceGenerator;
    protected readonly IDictionary<string, RangedInstanceFunction> RangedInstanceReferenceResolutions =
        new Dictionary<string, RangedInstanceFunction>();
    protected readonly HashSet<(RangedInstanceFunction, string)> RangedInstanceQueuedOverloads = new ();
    protected readonly Queue<(RangedInstanceFunction, IReadOnlyList<(ITypeSymbol, ParameterResolution)>, INamedTypeSymbol, InterfaceExtension?)> RangedInstanceResolutionsQueue = new();
    
    protected readonly List<(RangedInstanceFunction, RangedInstanceFunctionOverload)> RangedInstances = new ();
    protected readonly DisposableCollectionResolution DisposableCollectionResolution;
    protected readonly IUserProvidedScopeElements UserProvidedScopeElements;
    protected readonly DisposalHandling DisposalHandling;
    protected readonly string Name;

    protected RangeResolutionBaseBuilder(
        // parameters
        (string, bool) name,
        
        // dependencies
        WellKnownTypes wellKnownTypes,
        ITypeToImplementationsMapper typeToImplementationsMapper,
        IReferenceGeneratorFactory referenceGeneratorFactory,
        ICheckTypeProperties checkTypeProperties,
        ICheckDecorators checkDecorators, 
        IUserProvidedScopeElements userProvidedScopeElements)
    {
        WellKnownTypes = wellKnownTypes;
        TypeToImplementationsMapper = typeToImplementationsMapper;
        CheckTypeProperties = checkTypeProperties;
        CheckDecorators = checkDecorators;
        UserProvidedScopeElements = userProvidedScopeElements;

        RootReferenceGenerator = referenceGeneratorFactory.Create();
        DisposableCollectionResolution = new DisposableCollectionResolution(
            RootReferenceGenerator.Generate(WellKnownTypes.ConcurrentBagOfDisposable),
            WellKnownTypes.ConcurrentBagOfDisposable.FullName());
        
        Name = name.Item2 ? RootReferenceGenerator.Generate(name.Item1) : name.Item1;
        DisposalHandling = new DisposalHandling(
            DisposableCollectionResolution,
            Name,
            RootReferenceGenerator.Generate("_disposed"),
            RootReferenceGenerator.Generate("disposed"),
            RootReferenceGenerator.Generate("Disposed"),
            RootReferenceGenerator.Generate("disposable"));
    }

    protected abstract RangedInstanceReferenceResolution CreateSingleInstanceReferenceResolution(ForConstructorParameter parameter);
    
    protected abstract ScopeRootResolution CreateScopeRootResolution(
        IScopeRootParameter parameter,
        INamedTypeSymbol rootType,
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters);

    internal record Parameter;

    internal record SwitchTypeParameter(
        ITypeSymbol Type,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentFuncParameters)
        : Parameter;
    
    protected Resolvable SwitchType(SwitchTypeParameter parameter)
    {
        var (type, currentFuncParameters) = parameter;
        if (currentFuncParameters.FirstOrDefault(t => SymbolEqualityComparer.Default.Equals(t.Type.OriginalDefinition, type.OriginalDefinition)) is { Type: not null, Resolution: not null } funcParameter)
            return funcParameter.Resolution;

        if (UserProvidedScopeElements.GetInstanceFor(type) is { } instance)
            return new FieldResolution(
                RootReferenceGenerator.Generate(instance.Type),
                instance.Type.FullName(),
                instance.Name);
        
        if (UserProvidedScopeElements.GetPropertyFor(type) is { } property)
            return new FieldResolution(
                RootReferenceGenerator.Generate(property.Type),
                property.Type.FullName(),
                property.Name);

        if (UserProvidedScopeElements.GetFactoryFor(type) is { } factory)
            return new FactoryResolution(
                RootReferenceGenerator.Generate(factory.ReturnType),
                factory.ReturnType.FullName(),
                factory.Name,
                factory
                    .Parameters
                    .Select(p => (p.Name, SwitchType(new SwitchTypeParameter(p.Type, currentFuncParameters))))
                    .ToList());

        if (type.OriginalDefinition.Equals(WellKnownTypes.Lazy1, SymbolEqualityComparer.Default)
            && type is INamedTypeSymbol namedTypeSymbol)
        {
            if (namedTypeSymbol.TypeArguments.SingleOrDefault() is not INamedTypeSymbol genericType)
            {
                return new ErrorTreeItem(namedTypeSymbol.TypeArguments.Length switch
                {
                    0 => $"[{namedTypeSymbol.FullName()}] Lazy: No type argument",
                    > 1 => $"[{namedTypeSymbol.FullName()}] Lazy: more than one type argument",
                    _ => $"[{namedTypeSymbol.FullName()}] Lazy: {namedTypeSymbol.TypeArguments[0].FullName()} is not a named type symbol"
                });
            }
            
            var dependency = SwitchType(new SwitchTypeParameter(
                genericType, 
                Array.Empty<(ITypeSymbol Type, ParameterResolution Resolution)>()));
            return new ConstructorResolution(
                RootReferenceGenerator.Generate(namedTypeSymbol),
                namedTypeSymbol.FullName(),
                ImplementsIDisposable(namedTypeSymbol, WellKnownTypes, DisposableCollectionResolution, CheckTypeProperties),
                new ReadOnlyCollection<(string Name, Resolvable Dependency)>(
                    new List<(string Name, Resolvable Dependency)> 
                    { 
                        (
                            "valueFactory", 
                            new FuncResolution(
                                RootReferenceGenerator.Generate("func"),
                                $"global::System.Func<{genericType.FullName()}>",
                                Array.Empty<ParameterResolution>(),
                                dependency)
                        )
                    }),
                Array.Empty<(string Name, Resolvable Dependency)>());
        }

        if (type.OriginalDefinition.Equals(WellKnownTypes.Enumerable1, SymbolEqualityComparer.Default)
            || type.OriginalDefinition.Equals(WellKnownTypes.ReadOnlyCollection1, SymbolEqualityComparer.Default)
            || type.OriginalDefinition.Equals(WellKnownTypes.ReadOnlyList1, SymbolEqualityComparer.Default))
        {
            if (type is not INamedTypeSymbol collectionType)
            {
                return new ErrorTreeItem($"[{type.FullName()}] Collection: Collection is not a named type symbol");
            }
            if (collectionType.TypeArguments.SingleOrDefault() is not INamedTypeSymbol itemType)
            {
                return new ErrorTreeItem(collectionType.TypeArguments.Length switch
                {
                    0 => $"[{type.FullName()}] Collection: No item type argument",
                    > 1 => $"[{type.FullName()}] Collection: More than one item type argument",
                    _ => $"[{type.FullName()}] Collection: {collectionType.TypeArguments[0].FullName()} is not a named type symbol"
                });
            }
            var itemFullName = itemType.FullName();
            var itemTypeIsInterface = itemType.TypeKind == TypeKind.Interface;
            var items = TypeToImplementationsMapper
                .Map(itemType)
                .Select(i => itemTypeIsInterface
                    ? SwitchInterfaceForSpecificImplementation(new SwitchInterfaceForSpecificImplementationParameter(itemType, i, currentFuncParameters))
                    : SwitchClass(new SwitchClassParameter(i, currentFuncParameters)))
                .ToList();

            return new CollectionResolution(
                RootReferenceGenerator.Generate(type),
                type.FullName(),
                itemFullName,
                items);
        }

        if (type.TypeKind == TypeKind.Interface)
            return SwitchInterface(new SwitchInterfaceParameter(type, currentFuncParameters));

        if (type.TypeKind == TypeKind.Class || type.TypeKind == TypeKind.Struct)
            return SwitchClass(new SwitchClassParameter(type, currentFuncParameters));

        if (type.TypeKind == TypeKind.Delegate 
            && type.FullName().StartsWith("global::System.Func<")
            && type is INamedTypeSymbol namedTypeSymbol0)
        {
            var returnType = namedTypeSymbol0.TypeArguments.Last();
            var parameterTypes = namedTypeSymbol0
                .TypeArguments
                .Take(namedTypeSymbol0.TypeArguments.Length - 1)
                .Select(ts => (Type: ts, Resolution: new ParameterResolution(RootReferenceGenerator.Generate(ts), ts.FullName())))
                .ToArray();

            var dependency = SwitchType(new SwitchTypeParameter(
                returnType, 
                parameterTypes));
            return new FuncResolution(
                RootReferenceGenerator.Generate(type),
                type.FullName(),
                parameterTypes.Select(t => t.Resolution).ToArray(),
                dependency);
        }

        return new ErrorTreeItem($"[{type.FullName()}] Couldn't process in resolution tree creation.");
    }

    internal record SwitchInterfaceParameter(
        ITypeSymbol Type,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentFuncParameters)
        : Parameter;

    private Resolvable SwitchInterface(SwitchInterfaceParameter parameter)
    {
        var (typeSymbol, currentParameters) = parameter;
        var interfaceType = (INamedTypeSymbol) typeSymbol;
        var implementations = TypeToImplementationsMapper
            .Map(typeSymbol);
        var shouldBeScopeRoot = implementations.Any(i => CheckTypeProperties.ShouldBeScopeRoot(i));

        var nextParameter = new SwitchInterfaceAfterScopeRootParameter(
            interfaceType,
            implementations,
            currentParameters);
        
        if (shouldBeScopeRoot)
        {
            return CreateScopeRootResolution(
                nextParameter,
                interfaceType,
                DisposableCollectionResolution,
                currentParameters);
        }

        return SwitchInterfaceAfterScopeRoot(nextParameter);
    }

    internal interface IScopeRootParameter
    {
        string KeySuffix();
        string RootFunctionSuffix();
    }

    internal record SwitchInterfaceAfterScopeRootParameter(
        INamedTypeSymbol InterfaceType,
        IList<INamedTypeSymbol> ImplementationTypes,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentParameters) 
        : IScopeRootParameter
    {
        public string KeySuffix() => ":::InterfaceAfterRoot";

        public string RootFunctionSuffix() => "_InterfaceAfterRoot";
    }

    protected Resolvable SwitchInterfaceAfterScopeRoot(
        SwitchInterfaceAfterScopeRootParameter parameter)
    {
        var (interfaceType, implementations, currentParameters) = parameter;
        if (CheckTypeProperties.ShouldBeComposite(interfaceType))
        {
            var compositeImplementationType = CheckTypeProperties.GetCompositeFor(interfaceType);
            var interfaceResolutions = implementations.Select(i => CreateInterface(new CreateInterfaceParameter(
                interfaceType,
                i,
                currentParameters))).ToList();
            var composition = new CompositionInterfaceExtension(
                interfaceType,
                implementations.ToList(),
                compositeImplementationType,
                interfaceResolutions);
            return CreateInterface(new CreateInterfaceParameterAsComposition(
                interfaceType, 
                compositeImplementationType,
                currentParameters, 
                composition));
        }
        if (implementations.SingleOrDefault() is not { } implementationType)
        {
            return new ErrorTreeItem(implementations.Count switch
            {
                0 => $"[{interfaceType.FullName()}] Interface: No implementation found",
                > 1 => $"[{interfaceType.FullName()}] Interface: more than one implementation found",
                _ =>
                    $"[{interfaceType.FullName()}] Interface: Found single implementation {implementations[0].FullName()} is not a named type symbol"
            });
        }

        return CreateInterface(new CreateInterfaceParameter(
            interfaceType,
            implementationType,
            currentParameters));
    }


    internal record SwitchInterfaceForSpecificImplementationParameter(
        INamedTypeSymbol InterfaceType,
        INamedTypeSymbol ImplementationType,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentParameters);

    private Resolvable SwitchInterfaceForSpecificImplementation(
        SwitchInterfaceForSpecificImplementationParameter parameter)
    {
        var (interfaceType, implementationType, currentParameters) = parameter;
        
        var nextParameter = new CreateInterfaceParameter(
            interfaceType,
            implementationType,
            currentParameters);
        
        if (CheckTypeProperties.ShouldBeScopeRoot(implementationType))
        {
            return CreateScopeRootResolution(
                nextParameter,
                interfaceType,
                DisposableCollectionResolution,
                currentParameters);
        }

        return CreateInterface(nextParameter);
    }

    internal record CreateInterfaceParameter(
        INamedTypeSymbol InterfaceType,
        INamedTypeSymbol ImplementationType,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentParameters)
        : IScopeRootParameter
    {
        public virtual string KeySuffix() => ":::NormalInterface";

        public virtual string RootFunctionSuffix() => "_NormalInterface";
    }

    internal record CreateInterfaceParameterAsComposition(
        INamedTypeSymbol InterfaceType,
        INamedTypeSymbol ImplementationType,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentParameters,
        CompositionInterfaceExtension Composition)
        : CreateInterfaceParameter(InterfaceType, ImplementationType, CurrentParameters)
    {
        public override string KeySuffix() => ":::CompositeInterface";

        public override string RootFunctionSuffix() => "_CompositeInterface";
    }

    internal InterfaceResolution CreateInterface(CreateInterfaceParameter parameter)
    {
        var (interfaceType, implementationType, currentParameters) = parameter;
        var shouldBeDecorated = CheckDecorators.ShouldBeDecorated(interfaceType);

        var nextParameter = parameter switch
        {
            CreateInterfaceParameterAsComposition asComposition => new SwitchImplementationParameterWithComposition(
                asComposition.Composition.CompositeType,
                currentParameters,
                asComposition.Composition),
            _ => new SwitchImplementationParameter(
                implementationType,
                currentParameters)
        };

        var currentInterfaceResolution = new InterfaceResolution(
            RootReferenceGenerator.Generate(interfaceType),
            interfaceType.FullName(),
            SwitchImplementation(nextParameter));

        if (shouldBeDecorated)
        {
            var decorators = new Queue<INamedTypeSymbol>(CheckDecorators.GetSequenceFor(interfaceType, implementationType));
            while (decorators.Any())
            {
                var decorator = decorators.Dequeue();
                var decoration = new DecorationInterfaceExtension(interfaceType, implementationType, decorator,
                    currentInterfaceResolution);
                var decoratorResolution = SwitchImplementation(new SwitchImplementationParameterWithDecoration(
                    decorator,
                    currentParameters,
                    decoration));
                currentInterfaceResolution = new InterfaceResolution(
                    RootReferenceGenerator.Generate(interfaceType),
                    interfaceType.FullName(),
                    decoratorResolution);
            }
        }
        
        return currentInterfaceResolution;
    }

    internal record SwitchClassParameter(
        ITypeSymbol TypeSymbol,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentParameters);

    protected Resolvable SwitchClass(SwitchClassParameter parameter)
    {
        var (typeSymbol, currentParameters) = parameter;
        var implementations = TypeToImplementationsMapper
            .Map(typeSymbol);
        var implementationType = implementations.SingleOrDefault();
        if (implementationType is not { })
        {
            return new ErrorTreeItem(implementations.Count switch
            {
                0 => $"[{typeSymbol.FullName()}] Class: No implementation found",
                > 1 => $"[{typeSymbol.FullName()}] Class: more than one implementation found",
                _ =>
                    $"[{typeSymbol.FullName()}] Class: Found single implementation{implementations[0].FullName()} is not a named type symbol"
            });
        }

        var nextParameter = new SwitchImplementationParameter(
            implementationType,
            currentParameters);

        if (CheckTypeProperties.ShouldBeScopeRoot(implementationType))
            return CreateScopeRootResolution(nextParameter, implementationType, DisposableCollectionResolution, currentParameters);

        return SwitchImplementation(nextParameter);
    }
    
    internal record SwitchImplementationParameter(
        INamedTypeSymbol ImplementationType,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentParameters)
        : IScopeRootParameter
    {
        public virtual string KeySuffix() => ":::Implementation";

        public virtual string RootFunctionSuffix() => "";
    }
    
    internal record SwitchImplementationParameterWithDecoration(
        INamedTypeSymbol ImplementationType,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentParameters,
        DecorationInterfaceExtension Decoration)
        : SwitchImplementationParameter(ImplementationType, CurrentParameters);
    
    internal record SwitchImplementationParameterWithComposition(
            INamedTypeSymbol ImplementationType,
            IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentParameters,
            CompositionInterfaceExtension Composition)
        : SwitchImplementationParameter(ImplementationType, CurrentParameters);

    protected Resolvable SwitchImplementation(SwitchImplementationParameter parameter)
    {
        var (implementationType, currentParameters) = parameter;
        var scopeLevel = parameter switch
        {
            SwitchImplementationParameterWithComposition withComposition =>
                withComposition.Composition.ImplementationTypes.Select(i => CheckTypeProperties.GetScopeLevelFor(i))
                    .Min(),
            SwitchImplementationParameterWithDecoration withDecoration => CheckTypeProperties.GetScopeLevelFor(
                withDecoration.Decoration.ImplementationType),
            _ => CheckTypeProperties.GetScopeLevelFor(parameter.ImplementationType)
        };
        var nextParameter = parameter switch
        {
            SwitchImplementationParameterWithComposition withComposition => new ForConstructorParameterWithComposition(
                withComposition.Composition.CompositeType, 
                currentParameters,
                withComposition.Composition),
            SwitchImplementationParameterWithDecoration withDecoration => new ForConstructorParameterWithDecoration(
                withDecoration.Decoration.DecoratorType,
                currentParameters,
                withDecoration.Decoration),
            _ => new ForConstructorParameter(implementationType, currentParameters)
        };
        return scopeLevel switch
        {
            ScopeLevel.SingleInstance => CreateSingleInstanceReferenceResolution(nextParameter),
            ScopeLevel.Scope => CreateScopedInstanceReferenceResolution(nextParameter),
            _ => CreateConstructorResolution(nextParameter)
        };
    }

    internal record ForConstructorParameter(
        INamedTypeSymbol ImplementationType,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentFuncParameters);

    internal abstract record ForConstructorParameterWithInterfaceExtension(
        INamedTypeSymbol ImplementationType,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentFuncParameters,
        InterfaceExtension InterfaceExtension)
        : ForConstructorParameter(ImplementationType, CurrentFuncParameters);

    internal record ForConstructorParameterWithDecoration(
        INamedTypeSymbol ImplementationType,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentFuncParameters,
        DecorationInterfaceExtension Decoration)
        : ForConstructorParameterWithInterfaceExtension(ImplementationType, CurrentFuncParameters, Decoration);

    internal record ForConstructorParameterWithComposition(
            INamedTypeSymbol ImplementationType,
            IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentFuncParameters,
            CompositionInterfaceExtension Composition)
        : ForConstructorParameterWithInterfaceExtension(ImplementationType, CurrentFuncParameters, Composition);

    protected Resolvable CreateConstructorResolution(ForConstructorParameter parameter)
    {
        var (implementationType, currentParameters) = parameter;
        
        if (CheckTypeProperties.GetConstructorChoiceFor(implementationType) is not { } constructor)
        {
            return new ErrorTreeItem(implementationType.Constructors.Length switch
            {
                0 => $"[{implementationType.FullName()}] Class.Constructor: No constructor found for implementation {implementationType.FullName()}",
                > 1 => $"[{implementationType.FullName()}] Class.Constructor: More than one constructor found for implementation {implementationType.FullName()}",
                _ => $"[{implementationType.FullName()}] Class.Constructor: {implementationType.Constructors[0].Name} is not a method symbol"
            });
        }

        var checkForDecoration = false;
        DecorationInterfaceExtension? decoration = null;
        
        if (parameter is ForConstructorParameterWithDecoration withDecoration)
        {
            checkForDecoration = true;
            decoration = withDecoration.Decoration;
        }
        
        var checkForComposition = false;
        CompositionInterfaceExtension? composition = null;
        
        if (parameter is ForConstructorParameterWithComposition withComposition)
        {
            checkForComposition = true;
            composition = withComposition.Composition;
        }
        
        return new ConstructorResolution(
            RootReferenceGenerator.Generate(implementationType),
            implementationType.FullName(),
            ImplementsIDisposable(
                implementationType, 
                WellKnownTypes, 
                DisposableCollectionResolution,
                CheckTypeProperties),
            new ReadOnlyCollection<(string Name, Resolvable Dependency)>(constructor
                .Parameters
                .Select(p => ProcessChildType(p.Type, p.Name, implementationType, currentParameters))
                .ToList()),
            new ReadOnlyCollection<(string Name, Resolvable Dependency)>(implementationType
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.SetMethod?.IsInitOnly ?? false)
                .Select(p => ProcessChildType(p.Type, p.Name, implementationType, currentParameters))
                .ToList()));

        (string Name, Resolvable Dependency) ProcessChildType(ITypeSymbol typeSymbol, string parameterName, INamedTypeSymbol impType, IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currParameter)
        {
            if (checkForDecoration && typeSymbol.Equals(decoration?.InterfaceType, SymbolEqualityComparer.Default))
            {
                return (parameterName, decoration.CurrentInterfaceResolution);
            }
            if (checkForComposition 
                && composition is {} 
                && (typeSymbol.Equals(WellKnownTypes.Enumerable1.Construct(composition.InterfaceType), SymbolEqualityComparer.Default)
                    || typeSymbol.Equals(WellKnownTypes.ReadOnlyCollection1.Construct(composition.InterfaceType), SymbolEqualityComparer.Default)
                    || typeSymbol.Equals(WellKnownTypes.ReadOnlyList1.Construct(composition.InterfaceType), SymbolEqualityComparer.Default)))
            {
                        
                return (parameterName, new CollectionResolution(
                    RootReferenceGenerator.Generate(typeSymbol),
                    typeSymbol.FullName(),
                    composition.InterfaceType.FullName(),
                    composition.InterfaceResolutionComposition));
            }
            if (typeSymbol is not INamedTypeSymbol parameterType)
            {
                return ("",
                    new ErrorTreeItem(
                        $"[{impType.FullName()}] Class.Constructor.Parameter: Parameter type {typeSymbol.FullName()} is not a named type symbol"));
            }

            return (
                parameterName,
                SwitchType(new SwitchTypeParameter(
                    parameterType,
                    currParameter)));
        }
    }


    private RangedInstanceReferenceResolution CreateScopedInstanceReferenceResolution(
        ForConstructorParameter parameter) =>
        CreateRangedInstanceReferenceResolution(
            parameter,
            "Scoped",
            "this");

    protected RangedInstanceReferenceResolution CreateRangedInstanceReferenceResolution(
        ForConstructorParameter parameter,
        string label,
        string owningObjectReference)
    {
        var (implementationType, currentParameters) = parameter;
        InterfaceExtension? interfaceExtension = parameter switch
        {
            ForConstructorParameterWithComposition withComposition => withComposition.Composition,
            ForConstructorParameterWithDecoration withDecoration => withDecoration.Decoration,
            _ => null
        };
        var key = $"{implementationType.FullName()}{interfaceExtension?.KeySuffix() ?? ""}";
        if (!RangedInstanceReferenceResolutions.TryGetValue(
                key,
                out RangedInstanceFunction function))
        {
            var decorationSuffix = interfaceExtension?.RangedNameSuffix() ?? "";
            function = new RangedInstanceFunction(
                RootReferenceGenerator.Generate($"Get{label}Instance", implementationType, decorationSuffix),
                implementationType.FullName(),
                RootReferenceGenerator.Generate($"_{label.ToLower()}InstanceField", implementationType, decorationSuffix),
                RootReferenceGenerator.Generate($"_{label.ToLower()}InstanceLock{decorationSuffix}"));
            RangedInstanceReferenceResolutions[key] = function;
        }

        var listedParameterTypes = string.Join(",", currentParameters.Select(p => p.Item2.TypeFullName));
        if (!RangedInstanceQueuedOverloads.Contains((function, listedParameterTypes)))
        {
            var tempParameter = currentParameters
                .Select(t => (t.Type, new ParameterResolution(RootReferenceGenerator.Generate(t.Type), t.Type.FullName())))
                .ToList();
            RangedInstanceResolutionsQueue.Enqueue((function, tempParameter, implementationType, interfaceExtension));
            RangedInstanceQueuedOverloads.Add((function, listedParameterTypes));
        }

        return new RangedInstanceReferenceResolution(
            RootReferenceGenerator.Generate(implementationType),
            function,
            currentParameters.Select(t => t.Resolution).ToList(),
            owningObjectReference);
    }

    protected void DoRangedInstancesWork()
    {
        while (RangedInstanceResolutionsQueue.Any())
        {
            var (scopedInstanceFunction, parameter, type, interfaceExtension) = RangedInstanceResolutionsQueue.Dequeue();
            var resolvable = interfaceExtension switch
            {
                DecorationInterfaceExtension decoration => CreateConstructorResolution(new ForConstructorParameterWithDecoration(
                    decoration.DecoratorType, parameter, decoration)),
                CompositionInterfaceExtension composition => CreateConstructorResolution(new ForConstructorParameterWithComposition(
                    composition.CompositeType, parameter, composition)),
                _ => CreateConstructorResolution(new ForConstructorParameter(type, parameter))
            };
            RangedInstances.Add((
                scopedInstanceFunction, 
                new RangedInstanceFunctionOverload(
                    resolvable, 
                    parameter.Select(t => t.Item2).ToList())));
        }
    }

    private static DisposableCollectionResolution? ImplementsIDisposable(
        INamedTypeSymbol type, 
        WellKnownTypes wellKnownTypes, 
        DisposableCollectionResolution disposableCollectionResolution,
        ICheckTypeProperties checkDisposalManagement) =>
        type.AllInterfaces.Contains(wellKnownTypes.Disposable) && checkDisposalManagement.ShouldBeManaged(type) 
            ? disposableCollectionResolution 
            : null;
}