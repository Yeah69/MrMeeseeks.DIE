using MrMeeseeks.DIE.ResolutionBuilding.Function;

namespace MrMeeseeks.DIE.CodeBuilding;

internal interface IRangeCodeBaseBuilder
{
    StringBuilder Build(StringBuilder stringBuilder);
}

internal abstract class RangeCodeBaseBuilder : IRangeCodeBaseBuilder
{
    private readonly RangeResolution _rangeResolution;
    private readonly ContainerResolution _containerResolution;
    protected readonly WellKnownTypes WellKnownTypes;
    private bool _isDisposalHandlingRequired;

    internal RangeCodeBaseBuilder(
        RangeResolution rangeResolution,
        ContainerResolution containerResolution,
        WellKnownTypes wellKnownTypes)
    {
        _rangeResolution = rangeResolution;
        _containerResolution = containerResolution;
        WellKnownTypes = wellKnownTypes;
    }
    
    protected StringBuilder GenerateResolutionRange(
        StringBuilder stringBuilder,
        RangeResolution rangeResolution)
    {
        stringBuilder = rangeResolution.RangedInstanceFunctionGroups.Aggregate(stringBuilder, GenerateRangedInstanceFunction);

        stringBuilder =  rangeResolution.RootResolutions.Aggregate(stringBuilder, GenerateResolutionFunction);
        
        return GenerateContainerDisposalFunction(
            stringBuilder,
            rangeResolution);
    }

    private StringBuilder GenerateContainerDisposalFunction(
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

        if (_isDisposalHandlingRequired)
        {
            stringBuilder = rangeResolution.RangedInstanceFunctionGroups.Aggregate(
                stringBuilder, 
                (current, containerInstanceResolution) => current.AppendLine($"this.{containerInstanceResolution.LockReference}.Wait();"));

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

            stringBuilder = rangeResolution.RangedInstanceFunctionGroups.Aggregate(
                stringBuilder, 
                (current, containerInstanceResolution) => current.AppendLine($"this.{containerInstanceResolution.LockReference}.Release();"));

            return stringBuilder
                .AppendLine($"}}")
                .AppendLine($"}}");
        }

        return stringBuilder
            .AppendLine($"}}");
    }

    private StringBuilder GenerateResolutionFunction(
        StringBuilder stringBuilder,
        FunctionResolution resolution)
    {
        if (resolution is RootResolutionFunction rootResolutionFunction)
        {
            var parameter = string.Join(",", resolution.Parameter.Select(r => $"{r.TypeFullName} {r.Reference}"));
            stringBuilder = stringBuilder
                .AppendLine($"{rootResolutionFunction.AccessModifier} {(rootResolutionFunction.SynchronicityDecision is SynchronicityDecision.AsyncTask or SynchronicityDecision.AsyncValueTask ? "async " : "")}{resolution.TypeFullName} {resolution.Reference}({parameter})")
                .AppendLine($"{{")
                .AppendLine($"if (this.{rootResolutionFunction.DisposalHandling.DisposedPropertyReference}) throw new {WellKnownTypes.ObjectDisposedException}(\"\");");

            stringBuilder = GenerateResolutionFunctionContent(stringBuilder, resolution.Resolvable)
                .AppendLine($"return {resolution.Resolvable.Reference};");

            stringBuilder = resolution.LocalFunctions.Aggregate(stringBuilder, GenerateResolutionFunction)
                .AppendLine($"}}");
            
            return stringBuilder;
        }
        else if (resolution is LocalFunctionResolution localFunctionResolution)
        {
            var parameter = string.Join(",", resolution.Parameter.Select(r => $"{r.TypeFullName} {r.Reference}"));
            stringBuilder = stringBuilder
                .AppendLine($"{(localFunctionResolution.SynchronicityDecision is SynchronicityDecision.AsyncTask or SynchronicityDecision.AsyncValueTask ? "async " : "")}{resolution.TypeFullName} {resolution.Reference}({parameter})")
                .AppendLine($"{{")
                .AppendLine($"if (this.{localFunctionResolution.DisposalHandling.DisposedPropertyReference}) throw new {WellKnownTypes.ObjectDisposedException}(\"\");");

            stringBuilder = GenerateResolutionFunctionContent(stringBuilder, resolution.Resolvable)
                .AppendLine($"return {resolution.Resolvable.Reference};");

            stringBuilder = resolution.LocalFunctions.Aggregate(stringBuilder, GenerateResolutionFunction)
                .AppendLine($"}}");

            
        
            return stringBuilder;
        }

        throw new Exception();
    }

    private StringBuilder GenerateResolutionFunctionContent(
        StringBuilder stringBuilder,
        Resolvable resolution)
    {
        stringBuilder = GenerateFields(stringBuilder, resolution);
        return GenerateResolutions(stringBuilder, resolution);
    }

    private static StringBuilder GenerateFields(
        StringBuilder stringBuilder,
        Resolvable resolution)
    {
        switch (resolution)
        {
            case LazyResolution(var reference, var typeFullName, _):
                stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};");
                break;
            case FunctionCallResolution(var reference, _, _, _, _, _) functionCallResolution:
                stringBuilder = stringBuilder.AppendLine($"{functionCallResolution.SelectedTypeFullName} {reference};");
                break;
            case DeferringResolvable { Resolvable: {} resolvable}:
                stringBuilder = GenerateFields(stringBuilder, resolvable);
                break;
            case NewReferenceResolvable(var reference, var typeFullName, var resolvable):
                stringBuilder = GenerateFields(stringBuilder, resolvable);
                stringBuilder = stringBuilder
                    .AppendLine($"{typeFullName} {reference};");
                break;
            case MultiTaskResolution { SelectedResolvable: {} resolvable}:
                stringBuilder = GenerateFields(stringBuilder, resolvable);
                break;
            case MultiSynchronicityFunctionCallResolution { SelectedFunctionCall: {} selectedFunctionCall}:
                stringBuilder = GenerateFields(stringBuilder, selectedFunctionCall);
                break;
            case ValueTaskFromWrappedTaskResolution(var resolvable, var reference, var fullName):
                stringBuilder = GenerateFields(stringBuilder, resolvable);
                stringBuilder = stringBuilder
                    .AppendLine($"{fullName} {reference};");
                break;
            case TaskFromWrappedValueTaskResolution(var resolvable, var reference, var fullName):
                stringBuilder = GenerateFields(stringBuilder, resolvable);
                stringBuilder = stringBuilder
                    .AppendLine($"{fullName} {reference};");
                break;
            case TaskFromTaskResolution(var wrappedResolvable, _, var taskReference, var taskFullName):
                stringBuilder = GenerateFields(stringBuilder, wrappedResolvable);
                stringBuilder = stringBuilder
                    .AppendLine($"{taskFullName} {taskReference};");
                break;
            case TaskFromValueTaskResolution(var wrappedResolvable, _, var taskReference, var taskFullName):
                stringBuilder = GenerateFields(stringBuilder, wrappedResolvable);
                stringBuilder = stringBuilder
                    .AppendLine($"{taskFullName} {taskReference};");
                break;
            case TaskFromSyncResolution(var wrappedResolvable, var taskReference, var taskFullName):
                stringBuilder = GenerateFields(stringBuilder, wrappedResolvable);
                stringBuilder = stringBuilder
                    .AppendLine($"{taskFullName} {taskReference};");
                break;
            case ValueTaskFromTaskResolution(var wrappedResolvable, _, var valueTaskReference, var valueTaskFullName):
                stringBuilder = GenerateFields(stringBuilder, wrappedResolvable);
                stringBuilder = stringBuilder
                    .AppendLine($"{valueTaskFullName} {valueTaskReference};");
                break;
            case ValueTaskFromValueTaskResolution(var wrappedResolvable, _, var valueTaskReference, var valueTaskFullName):
                stringBuilder = GenerateFields(stringBuilder, wrappedResolvable);
                stringBuilder = stringBuilder
                    .AppendLine($"{valueTaskFullName} {valueTaskReference};");
                break;
            case ValueTaskFromSyncResolution(var wrappedResolvable, var valueTaskReference, var valueTaskFullName):
                stringBuilder = GenerateFields(stringBuilder, wrappedResolvable);
                stringBuilder = stringBuilder
                    .AppendLine($"{valueTaskFullName} {valueTaskReference};");
                break;
            case TransientScopeRootResolution(var transientScopeReference, var transientScopeTypeFullName, _, _, var functionCallResolution):
                stringBuilder = stringBuilder.AppendLine($"{transientScopeTypeFullName} {transientScopeReference};");
                stringBuilder = GenerateFields(stringBuilder, functionCallResolution);
                break;
            case ScopeRootResolution(var scopeReference, var scopeTypeFullName, _, _, _, var functionCallResolution):
                stringBuilder = stringBuilder.AppendLine($"{scopeTypeFullName} {scopeReference};");  
                stringBuilder = GenerateFields(stringBuilder, functionCallResolution);
                break;
            case TransientScopeAsDisposableResolution(var reference, var typeFullName):
                stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};");              
                break;
            case InterfaceResolution(var reference, var typeFullName, Resolvable resolutionBase):
                stringBuilder = GenerateFields(stringBuilder, resolutionBase);
                stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};");              
                break;
            case ConstructorResolution(var reference, var typeFullName, _, var parameters, var initializedProperties, var initialization):
                stringBuilder = parameters.Aggregate(stringBuilder,
                    (builder, tuple) => GenerateFields(builder, tuple.Dependency));
                stringBuilder = initializedProperties.Aggregate(stringBuilder,
                    (builder, tuple) => GenerateFields(builder, tuple.Dependency));
                stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};");
                if (initialization is TaskBaseTypeInitializationResolution { Await: false } taskInit)
                    stringBuilder = stringBuilder.AppendLine($"{taskInit.TaskTypeFullName} {taskInit.TaskReference};");
                break;
            case SyntaxValueTupleResolution(var reference, var typeFullName, var items):
                stringBuilder = items.Aggregate(stringBuilder, GenerateFields);
                stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};");
                break;
            case FuncResolution(var reference, var typeFullName, _):
                stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};");
                break;
            case ParameterResolution:
                break; // the parameter is the field
            case MethodGroupResolution:
                break; // referenced directly
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

    private StringBuilder GenerateResolutions(
        StringBuilder stringBuilder,
        Resolvable resolution)
    {
        switch (resolution)
        {
            case DeferringResolvable { Resolvable: {} resolvable}:
                stringBuilder = GenerateResolutions(stringBuilder, resolvable);
                break;
            case MultiTaskResolution { SelectedResolvable: {} resolvable}:
                stringBuilder = GenerateResolutions(stringBuilder, resolvable);
                break;
            case NewReferenceResolvable(var reference, var typeFullName, var resolvable):
                stringBuilder = GenerateResolutions(stringBuilder, resolvable);
                stringBuilder = stringBuilder
                    .AppendLine($"{reference} = ({typeFullName}) {resolvable.Reference};");
                break;
            case MultiSynchronicityFunctionCallResolution { SelectedFunctionCall: {} selectedFunctionCall}:
                stringBuilder = GenerateResolutions(stringBuilder, selectedFunctionCall);
                break;
            case LazyResolution(var reference, var typeFullName, var methodGroup):
                string owner = "";
                if (methodGroup.OwnerReference is { } explicitOwner)
                    owner = $"{explicitOwner}.";
                stringBuilder = stringBuilder
                    .AppendLine($"{reference} = new {typeFullName}({owner}{methodGroup.Reference});");
                break;
            case FunctionCallResolution(var reference, _, _, var functionReference, var functionOwner, var parameters) functionCallResolution:
                string owner2 = "";
                if (functionOwner is { } explicitOwner2)
                    owner2 = $"{explicitOwner2}.";
                if (functionCallResolution.Await)
                    stringBuilder = stringBuilder
                        .AppendLine($"{reference} = ({functionCallResolution.SelectedTypeFullName})(await {owner2}{functionReference}({string.Join(", ", parameters.Select(p => $"{p.Name}: {p.Reference}"))}));");
                else
                    stringBuilder = stringBuilder
                        .AppendLine($"{reference} = ({functionCallResolution.SelectedTypeFullName}){owner2}{functionReference}({string.Join(", ", parameters.Select(p => $"{p.Name}: {p.Reference}"))});");
                break;
            case ValueTaskFromWrappedTaskResolution(var resolvable, var reference, var fullName):
                stringBuilder = GenerateResolutions(stringBuilder, resolvable);
                stringBuilder = stringBuilder
                    .AppendLine($"{reference} = new {fullName}({resolvable.Reference});");
                break;
            case TaskFromWrappedValueTaskResolution(var resolvable, var reference, var fullName):
                stringBuilder = GenerateResolutions(stringBuilder, resolvable);
                stringBuilder = stringBuilder
                    .AppendLine($"{reference} = {resolvable.Reference}.AsTask();");
                break;
            case TaskFromTaskResolution(var wrappedResolvable, var initialization, var taskReference, _):
                stringBuilder = GenerateResolutions(stringBuilder, wrappedResolvable);
                stringBuilder = stringBuilder
                    .AppendLine($"{taskReference} = {initialization.TaskReference}.ContinueWith(t =>")
                    .AppendLine("{")
                    .AppendLine("if (t.IsCompletedSuccessfully)")
                    .AppendLine($"return {wrappedResolvable.Reference};")
                    .AppendLine("if (t.IsFaulted && t.Exception is { })")
                    .AppendLine("throw t.Exception;")
                    .AppendLine("if (t.IsCanceled)")
                    .AppendLine($"throw new {WellKnownTypes.TaskCanceledException.FullName()}(t);")
                    .AppendLine($"throw new {WellKnownTypes.Exception.FullName()}(\"Something unexpected.\");")
                    .AppendLine("});");        
                break;
            case TaskFromValueTaskResolution(var wrappedResolvable, var initialization, var taskReference, _):
                stringBuilder = GenerateResolutions(stringBuilder, wrappedResolvable);
                stringBuilder = stringBuilder
                    .AppendLine($"{taskReference} = {initialization.TaskReference}.AsTask().ContinueWith(t =>")
                    .AppendLine("{")
                    .AppendLine("if (t.IsCompletedSuccessfully)")
                    .AppendLine($"return {wrappedResolvable.Reference};")
                    .AppendLine("if (t.IsFaulted && t.Exception is { })")
                    .AppendLine("throw t.Exception;")
                    .AppendLine("if (t.IsCanceled)")
                    .AppendLine($"throw new {WellKnownTypes.TaskCanceledException.FullName()}(t);")
                    .AppendLine($"throw new {WellKnownTypes.Exception.FullName()}(\"Something unexpected.\");")
                    .AppendLine("});");
                break;
            case TaskFromSyncResolution(var wrappedResolvable, var taskReference, _):
                stringBuilder = GenerateResolutions(stringBuilder, wrappedResolvable);
                stringBuilder = stringBuilder.AppendLine($"{taskReference} = {WellKnownTypes.Task.FullName()}.FromResult({wrappedResolvable.Reference});");      
                break;
            case ValueTaskFromTaskResolution(var wrappedResolvable, var initialization, var valueTaskReference, var valueTaskFullName):
                stringBuilder = GenerateResolutions(stringBuilder, wrappedResolvable);
                stringBuilder = stringBuilder
                    .AppendLine($"{valueTaskReference} = new {valueTaskFullName}({initialization.TaskReference}.ContinueWith(t =>")
                    .AppendLine("{")
                    .AppendLine("if (t.IsCompletedSuccessfully)")
                    .AppendLine($"return {wrappedResolvable.Reference};")
                    .AppendLine("if (t.IsFaulted && t.Exception is { })")
                    .AppendLine("throw t.Exception;")
                    .AppendLine("if (t.IsCanceled)")
                    .AppendLine($"throw new {WellKnownTypes.TaskCanceledException.FullName()}(t);")
                    .AppendLine($"throw new {WellKnownTypes.Exception.FullName()}(\"Something unexpected.\");")
                    .AppendLine("}));");
                break;
            case ValueTaskFromValueTaskResolution(var wrappedResolvable, var initialization, var valueTaskReference, var valueTaskFullName):
                stringBuilder = GenerateResolutions(stringBuilder, wrappedResolvable);
                stringBuilder = stringBuilder
                    .AppendLine($"{valueTaskReference} = new {valueTaskFullName}({initialization.TaskReference}.AsTask().ContinueWith(t =>")
                    .AppendLine("{")
                    .AppendLine("if (t.IsCompletedSuccessfully)")
                    .AppendLine($"return {wrappedResolvable.Reference};")
                    .AppendLine("if (t.IsFaulted && t.Exception is { })")
                    .AppendLine("throw t.Exception;")
                    .AppendLine("if (t.IsCanceled)")
                    .AppendLine($"throw new {WellKnownTypes.TaskCanceledException.FullName()}(t);")
                    .AppendLine($"throw new {WellKnownTypes.Exception.FullName()}(\"Something unexpected.\");")
                    .AppendLine("}));");
                break;
            case ValueTaskFromSyncResolution(var wrappedResolvable, var valueTaskReference, _):
                stringBuilder = GenerateResolutions(stringBuilder, wrappedResolvable);
                stringBuilder = stringBuilder.AppendLine($"{valueTaskReference} = {WellKnownTypes.ValueTask.FullName()}.FromResult({wrappedResolvable.Reference});");      
                break;
            case TransientScopeRootResolution(var transientScopeReference, var transientScopeTypeFullName, var containerInstanceScopeReference, var (_, _, _, _, _, _), var createFunctionCall):
                stringBuilder = stringBuilder
                    .AppendLine($"{transientScopeReference} = new {transientScopeTypeFullName}({containerInstanceScopeReference});")
                    .AppendLine($"{_rangeResolution.ContainerReference}.{_containerResolution.DisposalHandling.DisposableCollection.Reference}.Add(({WellKnownTypes.Disposable.FullName()}) {transientScopeReference});");
                stringBuilder = GenerateResolutions(stringBuilder, createFunctionCall);
                _isDisposalHandlingRequired = true;
                break;
            case ScopeRootResolution(var scopeReference, var scopeTypeFullName, var containerInstanceScopeReference, var transientInstanceScopeReference, var (disposableCollectionReference, _, _, _, _, _), var createFunctionCall):
                stringBuilder = stringBuilder
                    .AppendLine($"{scopeReference} = new {scopeTypeFullName}({containerInstanceScopeReference}, {transientInstanceScopeReference});")
                    .AppendLine($"{disposableCollectionReference}.Add(({WellKnownTypes.Disposable.FullName()}) {scopeReference});");
                stringBuilder = GenerateResolutions(stringBuilder, createFunctionCall);
                _isDisposalHandlingRequired = true;
                break;
            case InterfaceResolution(var reference, var typeFullName, Resolvable resolutionBase):
                stringBuilder = GenerateResolutions(stringBuilder, resolutionBase);
                stringBuilder = stringBuilder.AppendLine(
                    $"{reference} = ({typeFullName}) {resolutionBase.Reference};");              
                break;
            case TransientScopeAsDisposableResolution(var reference, var typeFullName):
                stringBuilder = stringBuilder.AppendLine($"{reference} = ({typeFullName}) this;");              
                break;
            case ConstructorResolution(var reference, var typeFullName, var disposableCollectionResolution, var parameters, var initializedProperties, var initialization):
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
                    _isDisposalHandlingRequired = true;
                }

                if (initialization is {} init)
                {
                    stringBuilder = init switch
                    {
                        SyncTypeInitializationResolution (var initInterfaceTypeName, var initMethodName) => 
                            stringBuilder.AppendLine($"(({initInterfaceTypeName}) {reference}).{initMethodName}();"),
                        TaskBaseTypeInitializationResolution { Await: true, TypeFullName: {} initInterfaceTypeName, MethodName: {} initMethodName} => 
                            stringBuilder.AppendLine($"await (({initInterfaceTypeName}) {reference}).{initMethodName}();"),
                        TaskBaseTypeInitializationResolution { Await: false, TypeFullName: {} initInterfaceTypeName, MethodName: {} initMethodName, TaskReference: {} taskReference} => 
                            stringBuilder.AppendLine($"{taskReference} = (({initInterfaceTypeName}) {reference}).{initMethodName}();"),
                        _ => stringBuilder
                    };
                }
                break;
            case SyntaxValueTupleResolution(var reference, _, var items):
                stringBuilder = items.Aggregate(stringBuilder, GenerateResolutions);
                stringBuilder = stringBuilder.AppendLine($"{reference} = ({string.Join(", ", items.Select(d => d.Reference))});");
                break;
            case FuncResolution(var reference, _, var methodGroup):
                string owner1 = "";
                if (methodGroup.OwnerReference is { } explicitOwner1)
                    owner1 = $"{explicitOwner1}.";
                stringBuilder = stringBuilder
                    .AppendLine($"{reference} = {owner1}{methodGroup.Reference};");
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

    private StringBuilder GenerateRangedInstanceFunction(StringBuilder stringBuilder, RangedInstanceFunctionGroupResolution rangedInstanceFunctionGroupResolution)
    {
        stringBuilder = stringBuilder
            .AppendLine(
                $"private {rangedInstanceFunctionGroupResolution.TypeFullName}? {rangedInstanceFunctionGroupResolution.FieldReference};")
            .AppendLine(
                $"private {WellKnownTypes.SemaphoreSlim.FullName()} {rangedInstanceFunctionGroupResolution.LockReference} = new {WellKnownTypes.SemaphoreSlim.FullName()}(1);");
            
        foreach (var overload in rangedInstanceFunctionGroupResolution.Overloads)
        {
            var isAsync =
                overload.SynchronicityDecision is SynchronicityDecision.AsyncTask or SynchronicityDecision.AsyncValueTask;
            var parameters = string.Join(", ",
                overload.Parameter.Select(p => $"{p.TypeFullName} {p.Reference}"));
            stringBuilder = stringBuilder.AppendLine(
                    $"public {(isAsync ? "async " : "")}{overload.TypeFullName} {overload.Reference}({parameters})")
                .AppendLine($"{{").AppendLine(
                    $"if (!object.ReferenceEquals({rangedInstanceFunctionGroupResolution.FieldReference}, null)) return {rangedInstanceFunctionGroupResolution.FieldReference};")
                .AppendLine($"{(isAsync ? "await " : "")}this.{rangedInstanceFunctionGroupResolution.LockReference}.Wait{(isAsync ? "Async" : "")}();")
                .AppendLine($"try")
                .AppendLine($"{{")
                .AppendLine(
                    $"if (this.{overload.DisposalHandling.DisposedPropertyReference}) throw new {WellKnownTypes.ObjectDisposedException}(nameof({overload.DisposalHandling.RangeName}));")
                .AppendLine(
                    $"if (!object.ReferenceEquals({rangedInstanceFunctionGroupResolution.FieldReference}, null)) return {rangedInstanceFunctionGroupResolution.FieldReference};");

            stringBuilder = GenerateResolutionFunctionContent(stringBuilder, overload.Resolvable);

            stringBuilder = stringBuilder
                .AppendLine(
                    $"this.{rangedInstanceFunctionGroupResolution.FieldReference} = {overload.Resolvable.Reference};")
                .AppendLine($"}}")
                .AppendLine($"finally")
                .AppendLine($"{{")
                .AppendLine($"this.{rangedInstanceFunctionGroupResolution.LockReference}.Release();")
                .AppendLine($"}}")
                .AppendLine($"return this.{rangedInstanceFunctionGroupResolution.FieldReference};")
                .AppendLine($"}}");
        }
        
        return stringBuilder;
    }

    public abstract StringBuilder Build(StringBuilder stringBuilder);
}