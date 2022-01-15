namespace MrMeeseeks.DIE.CodeBuilding;

internal interface IRangeCodeBaseBuilder
{
    StringBuilder Build(StringBuilder stringBuilder);
}

internal abstract class RangeCodeBaseBuilder : IRangeCodeBaseBuilder
{
    protected readonly WellKnownTypes WellKnownTypes;
    protected bool IsDisposalHandlingRequired = false;

    internal RangeCodeBaseBuilder(
        WellKnownTypes wellKnownTypes)
    {
        WellKnownTypes = wellKnownTypes;
    }
    
    protected StringBuilder GenerateResolutionRange(
        StringBuilder stringBuilder,
        RangeResolution rangeResolution)
    {
        stringBuilder = rangeResolution.RangedInstances.Aggregate(stringBuilder, GenerateRangedInstanceFunction);

        stringBuilder =  rangeResolution.RootResolutions.Aggregate(stringBuilder, GenerateResolutionFunction);
        
        return GenerateContainerDisposalFunction(
            stringBuilder,
            rangeResolution);
    }
    
    protected StringBuilder GenerateContainerDisposalFunction(
        StringBuilder stringBuilder,
        RangeResolution rangeResolution)
    {
        var disposalHandling = rangeResolution.DisposalHandling;
        stringBuilder = stringBuilder
            .AppendLine($"private {disposalHandling.DisposableCollection.TypeFullName} {disposalHandling.DisposableCollection.Reference} = new {disposalHandling.DisposableCollection.TypeFullName}();")
            .AppendLine($"private int {disposalHandling.DisposedFieldReference} = 0;")
            .AppendLine($"private bool {disposalHandling.DisposedPropertyReference} => {disposalHandling.DisposedFieldReference} != 0;")
            .AppendLine($"public void Dispose()")
            .AppendLine($"{{")
            .AppendLine($"var {disposalHandling.DisposedLocalReference} = global::System.Threading.Interlocked.Exchange(ref this.{disposalHandling.DisposedFieldReference}, 1);")
            .AppendLine($"if ({disposalHandling.DisposedLocalReference} != 0) return;");
        
        if (IsDisposalHandlingRequired)
        {
            stringBuilder = rangeResolution.RangedInstances.Aggregate(
                stringBuilder, 
                (current, containerInstanceResolution) => current.AppendLine($"this.{containerInstanceResolution.Function.LockReference}.Wait();"));

            stringBuilder = stringBuilder
                .AppendLine($"try")
                .AppendLine($"{{")
                .AppendLine($"foreach(var {disposalHandling.DisposableLocalReference} in {disposalHandling.DisposableCollection.Reference})")
                .AppendLine($"{{")
                .AppendLine($"try")
                .AppendLine($"{{")
                .AppendLine($"{disposalHandling.DisposableLocalReference}.Dispose();")
                .AppendLine($"}}")
                .AppendLine($"catch({WellKnownTypes.Exception.FullName()})")
                .AppendLine($"{{")
                .AppendLine($"// catch and ignore exceptions of individual disposals so the other disposals are triggered")
                .AppendLine($"}}")
                .AppendLine($"}}")
                .AppendLine($"}}")
                .AppendLine($"finally")
                .AppendLine($"{{");

            stringBuilder = rangeResolution.RangedInstances.Aggregate(
                stringBuilder, 
                (current, containerInstanceResolution) => current.AppendLine($"this.{containerInstanceResolution.Function.LockReference}.Release();"));

            return stringBuilder
                .AppendLine($"}}")
                .AppendLine($"}}");
        }

        return stringBuilder
            .AppendLine($"}}");
    }

    protected StringBuilder GenerateResolutionFunction(
        StringBuilder stringBuilder,
        RootResolutionFunction resolution)
    {
        var parameter = string.Join(",", resolution.Parameter.Select(r => $"{r.TypeFullName} {r.Reference}"));
        stringBuilder = stringBuilder
            .AppendLine($"{resolution.AccessModifier} {resolution.TypeFullName} {resolution.ExplicitImplementationFullName}{(string.IsNullOrWhiteSpace(resolution.ExplicitImplementationFullName) ? "" : ".")}{resolution.Reference}({parameter})")
            .AppendLine($"{{")
            .AppendLine($"if (this.{resolution.DisposalHandling.DisposedPropertyReference}) throw new {WellKnownTypes.ObjectDisposedException}(nameof({resolution.RangeName}));");
        
        return GenerateResolutionFunctionContent(stringBuilder, resolution.Resolvable)
            .AppendLine($"return {resolution.Resolvable.Reference};")
            .AppendLine($"}}");
    }

    protected StringBuilder GenerateResolutionFunctionContent(
        StringBuilder stringBuilder,
        Resolvable resolution)
    {
        stringBuilder = GenerateFields(stringBuilder, resolution);
        return GenerateResolutions(stringBuilder, resolution);
    }

    protected StringBuilder GenerateFields(
        StringBuilder stringBuilder,
        Resolvable resolution)
    {
        switch (resolution)
        {
            case ScopeRootResolution(var reference, var typeFullName, var scopeReference, var scopeTypeFullName, _, _, _, _):
                stringBuilder = stringBuilder
                    .AppendLine($"{scopeTypeFullName} {scopeReference};")
                    .AppendLine($"{typeFullName} {reference};");  
                break;
            case InterfaceResolution(var reference, var typeFullName, Resolvable resolutionBase):
                stringBuilder = GenerateFields(stringBuilder, resolutionBase);
                stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};");              
                break;
            case ConstructorResolution(var reference, var typeFullName, _, var parameters, var initializedProperties):
                stringBuilder = parameters.Aggregate(stringBuilder,
                    (builder, tuple) => GenerateFields(builder, tuple.Dependency));
                stringBuilder = initializedProperties.Aggregate(stringBuilder,
                    (builder, tuple) => GenerateFields(builder, tuple.Dependency));
                stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};");
                break;
            case SyntaxValueTupleResolution(var reference, var typeFullName, var items):
                stringBuilder = items.Aggregate(stringBuilder, GenerateFields);
                stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};");
                break;
            case FuncResolution(var reference, var typeFullName, _, _):
                stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};");
                break;
            case ParameterResolution:
                break; // the parameter is the field
            case FieldResolution(var reference, var typeFullName, _):
                stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};");
                break;
            case FactoryResolution(var reference, var typeFullName, _, var parameters):
                stringBuilder = parameters.Aggregate(stringBuilder,
                    (builder, tuple) => GenerateFields(builder, tuple.Dependency));
                stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};");
                break;
            case CollectionResolution(var reference, var typeFullName, _, var items):
                stringBuilder = items.OfType<Resolvable>().Aggregate(stringBuilder, GenerateFields);
                stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};");
                break;
            case var (reference, typeFullName):
                stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};"); 
                break;
            default:
                throw new Exception("Unexpected case or not implemented.");
        }

        return stringBuilder;
    }

    protected StringBuilder GenerateResolutions(
        StringBuilder stringBuilder,
        Resolvable resolution)
    {
        switch (resolution)
        {
            case ScopeRootResolution(var reference, var typeFullName, var scopeReference, var scopeTypeFullName, var containerInstanceScopeReference, var parameter, var (disposableCollectionReference, _, _, _, _), var (createFunctionReference, _)):
                stringBuilder = stringBuilder
                    .AppendLine($"{scopeReference} = new {scopeTypeFullName}({containerInstanceScopeReference});")
                    .AppendLine($"{disposableCollectionReference}.Add(({WellKnownTypes.Disposable.FullName()}) {scopeReference});")
                    .AppendLine($"{reference} = ({typeFullName}) {scopeReference}.{createFunctionReference}({string.Join(", ", parameter.Select(p => p.Reference))});");
                IsDisposalHandlingRequired = true;
                break;
            case RangedInstanceReferenceResolution(var reference, { Reference: {} functionReference}, var parameter, var owningObjectReference):
                stringBuilder = stringBuilder.AppendLine($"{reference} = {owningObjectReference}.{functionReference}({string.Join(", ", parameter.Select(p => p.Reference))});"); 
                break;
            case InterfaceResolution(var reference, var typeFullName, Resolvable resolutionBase):
                stringBuilder = GenerateResolutions(stringBuilder, resolutionBase);
                stringBuilder = stringBuilder.AppendLine(
                    $"{reference} = ({typeFullName}) {resolutionBase.Reference};");              
                break;
            case ConstructorResolution(var reference, var typeFullName, var disposableCollectionResolution, var parameters, var initializedProperties):
                stringBuilder = parameters.Aggregate(stringBuilder,
                    (builder, tuple) => GenerateResolutions(builder, tuple.Dependency));
                stringBuilder = initializedProperties.Aggregate(stringBuilder,
                    (builder, tuple) => GenerateResolutions(builder, tuple.Dependency));
                var constructorParameter =
                    string.Join(", ", parameters.Select(d => $"{d.Name}: {d.Dependency.Reference}"));
                var objectInitializerParameter = initializedProperties.Any()
                    ? $" {{ {string.Join(", ", initializedProperties.Select(p => $"{p.Name} = {p.Dependency.Reference}"))} }}"
                    : "";
                stringBuilder = stringBuilder.AppendLine(
                    $"{reference} = new {typeFullName}({constructorParameter}){objectInitializerParameter};");
                if (disposableCollectionResolution is {})
                {
                    stringBuilder = stringBuilder.AppendLine(
                        $"{disposableCollectionResolution.Reference}.Add(({WellKnownTypes.Disposable.FullName()}) {reference});");
                    IsDisposalHandlingRequired = true;
                }
                break;
            case SyntaxValueTupleResolution(var reference, var typeFullName, var items):
                stringBuilder = items.Aggregate(stringBuilder, GenerateResolutions);
                stringBuilder = stringBuilder.AppendLine($"{reference} = ({string.Join(", ", items.Select(d => d.Reference))});");
                break;
            case FuncResolution(var reference, _, var parameter, Resolvable resolutionBase):
                stringBuilder = stringBuilder.AppendLine($"{reference} = ({string.Join(", ", parameter.Select(fpr => fpr.Reference))}) =>");
                stringBuilder = stringBuilder.AppendLine($"{{");
                GenerateResolutionFunctionContent(stringBuilder, resolutionBase);
                stringBuilder = stringBuilder.AppendLine($"return {resolutionBase.Reference};");
                stringBuilder = stringBuilder.AppendLine($"}};");
                break;
            case ParameterResolution:
                break; // parameter exists already
            case FieldResolution(var reference, _, var fieldName):
                stringBuilder = stringBuilder.AppendLine($"{reference} = this.{fieldName};");
                break;
            case FactoryResolution(var reference, _, var functionName, var parameters):
                stringBuilder = parameters.Aggregate(stringBuilder,
                    (builder, tuple) => GenerateResolutions(builder, tuple.Dependency ?? throw new Exception()));
                stringBuilder = stringBuilder.AppendLine($"{reference} = this.{functionName}({string.Join(", ", parameters.Select(t => $"{t.Name}: {t.Dependency.Reference}"))});");
                break;
            case CollectionResolution(var reference, _, var itemFullName, var items):
                stringBuilder = items.OfType<Resolvable>().Aggregate(stringBuilder, GenerateResolutions);
                stringBuilder = stringBuilder.AppendLine(
                    $"{reference} = new {itemFullName}[]{{{string.Join(", ", items.Select(d => $"({itemFullName}) {(d as Resolvable)?.Reference}"))}}};");
                break;
            default:
                throw new Exception("Unexpected case or not implemented.");
        }

        return stringBuilder;
    }

    protected StringBuilder GenerateRangedInstanceFunction(StringBuilder stringBuilder, RangedInstance rangedInstance)
    {
        stringBuilder = stringBuilder
            .AppendLine(
                $"private {rangedInstance.Function.TypeFullName} {rangedInstance.Function.FieldReference};")
            .AppendLine(
                $"private {WellKnownTypes.SemaphoreSlim.FullName()} {rangedInstance.Function.LockReference} = new {WellKnownTypes.SemaphoreSlim.FullName()}(1);");
            
        foreach (var (resolvable, funcParameterResolutions) in rangedInstance.Overloads)
        {
            var parameters = string.Join(", ",
                funcParameterResolutions.Select(p => $"{p.TypeFullName} {p.Reference}"));
            stringBuilder = stringBuilder.AppendLine(
                    $"public {rangedInstance.Function.TypeFullName} {rangedInstance.Function.Reference}({parameters})")
                .AppendLine($"{{")
                .AppendLine($"this.{rangedInstance.Function.LockReference}.Wait();")
                .AppendLine($"try")
                .AppendLine($"{{")
                .AppendLine(
                    $"if (this.{rangedInstance.DisposalHandling.DisposedPropertyReference}) throw new {WellKnownTypes.ObjectDisposedException}(nameof({rangedInstance.DisposalHandling.RangeName}));")
                .AppendLine(
                    $"if (!object.ReferenceEquals({rangedInstance.Function.FieldReference}, null)) return {rangedInstance.Function.FieldReference};");

            stringBuilder = GenerateResolutionFunctionContent(stringBuilder, resolvable);

            stringBuilder = stringBuilder
                .AppendLine(
                    $"this.{rangedInstance.Function.FieldReference} = {resolvable.Reference};")
                .AppendLine($"}}")
                .AppendLine($"finally")
                .AppendLine($"{{")
                .AppendLine($"this.{rangedInstance.Function.LockReference}.Release();")
                .AppendLine($"}}")
                .AppendLine($"return this.{rangedInstance.Function.FieldReference};")
                .AppendLine($"}}");
        }
        
        return stringBuilder;
    }

    public abstract StringBuilder Build(StringBuilder stringBuilder);
}