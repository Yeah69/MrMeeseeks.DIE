using MrMeeseeks.DIE.Configuration.Interception;
using MrMeeseeks.DIE.Nodes.Interception;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.CodeGeneration;

internal abstract class ImplementationBodyGeneratorBase
{
    private readonly SyncImplementationBase _syncImplementation;
    private readonly InterceptionDecoratorData _interceptionDecoratorData;
    public required IReferenceGenerator ReferenceGenerator { protected get; init; }
    public required WellKnownTypes WellKnownTypes { protected get; init; }
    public required WellKnownTypesCollections WellKnownTypesCollections { protected get; init; }
    public required IInvocationTypeManager InvocationTypeManager { protected get; init; }
    
    internal ImplementationBodyGeneratorBase(
        SyncImplementationBase syncImplementation,
        InterceptionDecoratorData interceptionDecoratorData)
    {
        _syncImplementation = syncImplementation;
        _interceptionDecoratorData = interceptionDecoratorData;
    }
    
    protected abstract void GenerateReturnVariableInitialization(StringBuilder code);
    protected abstract ImmutableArray<string> ArgumentNames { get; }
    protected abstract ImmutableArray<ITypeParameterSymbol> TypeParameters { get; }
    protected abstract void GenerateProceedMethod(
        StringBuilder code, 
        string invocationReference, 
        string argumentsReference,
        IInvocationDescriptionNode invocationDescriptionNode);
    protected abstract string GenerateMethodSeeking(StringBuilder code);
    protected abstract string ReturnValue { get; }
    protected abstract void GenerateReturnStatement(StringBuilder code);

    internal void Generate(StringBuilder code)
    {
        if (_syncImplementation.InterceptMethod is not { Parameters: [{ Type: INamedTypeSymbol { TypeKind: TypeKind.Interface } interceptionDescriptionType}] }
            || InvocationTypeManager.GetInvocationDescriptionNode(interceptionDescriptionType) is not { } invocationDescriptionNode)
            return;
        var declaringInterface = _syncImplementation.DeclaringInterface;
        GenerateReturnVariableInitialization(code);
        
        var argumentsReference = ReferenceGenerator.Generate("arguments");
        var argumentsNullability = invocationDescriptionNode.ArgumentsItemTypeNullable || !invocationDescriptionNode.ArgumentsProperty ? "?" : "";
        code.AppendLine($"var {argumentsReference} = new {WellKnownTypes.Object.FullName()}{argumentsNullability}{(invocationDescriptionNode.ArgumentsItemTypeNullable ? "?" : "")}[] {{ {string.Join(", ", ArgumentNames.Select(an => $"{an}{(invocationDescriptionNode.ArgumentsItemTypeNullable ? "" : "!")}"))} }};");
        
        var methodReference = invocationDescriptionNode.MethodProperty || invocationDescriptionNode.MethodInvocationTargetProperty 
            ? GenerateMethodSeeking(code)
            : "";
        
        var invocationReference = ReferenceGenerator.Generate("invocation");
        code.AppendLine($"var {invocationReference} = new {invocationDescriptionNode.ImplementationFullName}");
        code.AppendLine("{");
        code.AppendLine(string.Join($",{Environment.NewLine}", InvocationPropertyInitializations()));
        code.AppendLine("};");
        
        GenerateProceedMethod(code, invocationReference, argumentsReference, invocationDescriptionNode);
        
        code.AppendLine($"{_interceptionDecoratorData.InterceptorFieldReference}.{_syncImplementation.InterceptMethod.Name}({invocationReference});");
        
        GenerateReturnStatement(code);
        return;

        IEnumerable<string> InvocationPropertyInitializations()
        {
            if (invocationDescriptionNode.ArgumentsProperty)
                yield return $"Arguments = {argumentsReference}";
            if (invocationDescriptionNode.GenericArgumentsProperty)
                yield return $"GenericArguments = new {WellKnownTypes.Type.FullName()}[] {{ {string.Join(", ", TypeParameters.Select(tp => $"typeof({tp.FullName()})"))} }}";
            if (invocationDescriptionNode.InvocationTargetProperty)
                yield return $"InvocationTarget = {_interceptionDecoratorData.InterfaceFieldReference}";
            if (invocationDescriptionNode.MethodProperty)
                yield return $"Method = {methodReference}";
            if (invocationDescriptionNode.MethodInvocationTargetProperty)
                yield return $"MethodInvocationTarget = {methodReference}";
            if (invocationDescriptionNode.ProxyProperty)
                yield return "Proxy = this";
            if (invocationDescriptionNode.ReturnValueProperty)
                yield return $"ReturnValue = {ReturnValue}{(invocationDescriptionNode.ReturnValueTypeNullable ? "" : "!")}";
            if (invocationDescriptionNode.TargetTypeProperty)
                yield return $"TargetType = typeof({declaringInterface.FullName()})";
        }
    }
}

internal sealed class MethodImplementationBodyGenerator : ImplementationBodyGeneratorBase
{
    private readonly SyncMethodImplementation _syncImplementation;
    private readonly InterceptionDecoratorData _interceptionDecoratorData;
    private readonly string? _returnType;
    private string? _returnValueVariableReference;
    private string? _assignedReturnValueReference;

    internal MethodImplementationBodyGenerator(
        SyncMethodImplementation syncImplementation,
        InterceptionDecoratorData interceptionDecoratorData)
        : base(syncImplementation, interceptionDecoratorData)
    {
        _syncImplementation = syncImplementation;
        _interceptionDecoratorData = interceptionDecoratorData;
        _returnType = _syncImplementation.Method.ReturnsVoid ? null : _syncImplementation.Method.ReturnType.FullName();
    }

    protected override void GenerateReturnVariableInitialization(StringBuilder code)
    {
        _returnValueVariableReference = _syncImplementation.Method.ReturnsVoid ? null : ReferenceGenerator.Generate("returnValue");
        if (_returnType is not null && _returnValueVariableReference is not null)
            code.AppendLine($"var {_returnValueVariableReference} = default({_returnType});");
    }

    protected override ImmutableArray<string> ArgumentNames => _syncImplementation.Method.Parameters.Select(p => p.Name).ToImmutableArray();
    protected override ImmutableArray<ITypeParameterSymbol> TypeParameters => _syncImplementation.Method.TypeParameters;
    protected override void GenerateProceedMethod(
        StringBuilder code,
        string invocationReference,
        string argumentsReference,
        IInvocationDescriptionNode invocationDescriptionNode)
    {
        var genericsPart = _syncImplementation.Method.TypeParameters.Length == 0
            ? string.Empty
            : $"<{string.Join(", ", TypeParameters.Select(tp => tp.FullName()))}>";
        _assignedReturnValueReference = (_syncImplementation.Method.ReturnsVoid, invocationDescriptionNode.ReturnValueProperty) switch
        {
            (true, _) => "",
            (_, true) => $"{invocationReference}.ReturnValue",
            (_, false) => $"{_returnValueVariableReference}"
        };
        if (invocationDescriptionNode.ProceedMethod)
        {
            var assignment = _syncImplementation.Method.ReturnsVoid ? "" : $"{_assignedReturnValueReference} = ";
            var argumentsAssignment = string.Join(", ", _syncImplementation.Method.Parameters.Select((p, i) => $"({p.Type.FullName()}) {argumentsReference}[{i}]"));
            code.AppendLine($"{invocationReference}.ProceedAction = () => {assignment}{_interceptionDecoratorData.InterfaceFieldReference}.{_syncImplementation.Method.Name}{genericsPart}({argumentsAssignment});");
        }
    }

    protected override string GenerateMethodSeeking(StringBuilder code)
    {
        var methodRef = ReferenceGenerator.Generate("method");

        var membersWithTheSameName = _syncImplementation.DeclaringInterface.GetMembers(_syncImplementation.Method.Name);
        if (membersWithTheSameName.Length == 1) // Simple case where there is only one method with the same name
            code.AppendLine($"var {methodRef} = typeof({_syncImplementation.DeclaringInterface.FullName()}).GetMethod(\"{_syncImplementation.Method.Name}\")!;");
        else
        {
            if (_syncImplementation.Method.Parameters.Length == 0)
                code.AppendLine($"var {methodRef} = {WellKnownTypesCollections.Enumerable.FullName()}.First(typeof({_syncImplementation.DeclaringInterface.FullName()}).GetMethods(), m => m.Name == nameof({_syncImplementation.DeclaringInterface.FullName()}.{_syncImplementation.Method.Name}) && m.GetGenericArguments().Length == {_syncImplementation.Method.TypeParameters.Length} && m.GetParameters().Length == 0);");
            else
            {
                var genericParamsRef = ReferenceGenerator.Generate("genericParams");
                var paramsRef = ReferenceGenerator.Generate("params");
                code.AppendLine($"var {methodRef} = {WellKnownTypesCollections.Enumerable.FullName()}.First(typeof({_syncImplementation.DeclaringInterface.FullName()}).GetMethods(), m =>");
                code.AppendLine("{");
                code.AppendLine($"var {genericParamsRef} = m.GetGenericArguments();");
                code.AppendLine($"var {paramsRef} = m.GetParameters();");
                code.AppendLine($"return {string.Join(" && ", MethodSignatureCheckingConditions())};");
                code.AppendLine("});");

                IEnumerable<string> MethodSignatureCheckingConditions()
                {
                    yield return $"m.Name == nameof({_syncImplementation.DeclaringInterface.FullName()}.{_syncImplementation.Method.Name})";
                    yield return $"{genericParamsRef}.Length == {_syncImplementation.Method.TypeParameters.Length}";
                    yield return $"{paramsRef}.Length == {_syncImplementation.Method.Parameters.Length}";
                    for (var i = 0; i < _syncImplementation.Method.TypeParameters.Length; i++)
                    {
                        yield return $"{genericParamsRef}[{i}].Name == \"{_syncImplementation.Method.TypeParameters[i].Name}\"";
                    }
                    for (var i = 0; i < _syncImplementation.Method.Parameters.Length; i++)
                    {
                        yield return _syncImplementation.Method.Parameters[i].Type is ITypeParameterSymbol
                            ? $"{paramsRef}[{i}].ParameterType.Name == \"{_syncImplementation.Method.Parameters[i].Type.Name}\""
                            : $"{paramsRef}[{i}].ParameterType == typeof({_syncImplementation.Method.Parameters[i].Type.FullName()})";
                    }
                }
            }
        }
        
        return methodRef;
    }

    protected override string ReturnValue => _syncImplementation.Method.ReturnsVoid || _returnValueVariableReference is null 
        ? "default"
        : _returnValueVariableReference;

    protected override void GenerateReturnStatement(StringBuilder code)
    {
        if (!_syncImplementation.Method.ReturnsVoid && _returnType is not null && _assignedReturnValueReference is not null)
            code.AppendLine($"return ({_returnType}) {_assignedReturnValueReference};");
    }
}

internal sealed class PropertyGetImplementationBodyGenerator : ImplementationBodyGeneratorBase
{
    private readonly SyncPropertyImplementation _syncImplementation;
    private readonly InterceptionDecoratorData _interceptionDecoratorData;
    private readonly string _returnType;
    private string? _returnValueVariableReference;
    private string _assignedReturnValueReference = "";

    internal PropertyGetImplementationBodyGenerator(
        SyncPropertyImplementation syncImplementation,
        InterceptionDecoratorData interceptionDecoratorData)
        : base(syncImplementation, interceptionDecoratorData)
    {
        _syncImplementation = syncImplementation;
        _interceptionDecoratorData = interceptionDecoratorData;
        _returnType = _syncImplementation.Property.Type.FullName();
    }

    protected override void GenerateReturnVariableInitialization(StringBuilder code)
    {
        _returnValueVariableReference = ReferenceGenerator.Generate("returnValue");
        if (_returnValueVariableReference is not null)
            code.AppendLine($"var {_returnValueVariableReference} = default({_returnType});");
    }

    protected override ImmutableArray<string> ArgumentNames => ImmutableArray<string>.Empty;
    protected override ImmutableArray<ITypeParameterSymbol> TypeParameters => ImmutableArray<ITypeParameterSymbol>.Empty;
    protected override void GenerateProceedMethod(
        StringBuilder code,
        string invocationReference,
        string argumentsReference,
        IInvocationDescriptionNode invocationDescriptionNode)
    {
        _assignedReturnValueReference = invocationDescriptionNode.ReturnValueProperty
            ? $"{invocationReference}.ReturnValue"
            : $"{_returnValueVariableReference}";
        if (invocationDescriptionNode.ProceedMethod)
        {
            code.AppendLine($"{invocationReference}.ProceedAction = () => {_assignedReturnValueReference} = {_interceptionDecoratorData.InterfaceFieldReference}.{_syncImplementation.Property.Name};");
        }
    }

    protected override string GenerateMethodSeeking(StringBuilder code)
    {
        var methodRef = ReferenceGenerator.Generate("method");
        code.AppendLine($"var {methodRef} = typeof({_syncImplementation.DeclaringInterface.FullName()}).GetProperty(\"{_syncImplementation.Property.Name}\")!.GetMethod!;");
        return methodRef;
    }

    protected override string ReturnValue => _returnValueVariableReference ?? "default";

    protected override void GenerateReturnStatement(StringBuilder code)
    {
        code.AppendLine($"return ({_returnType}) {_assignedReturnValueReference};");
    }
}

internal sealed class PropertySetImplementationBodyGenerator : ImplementationBodyGeneratorBase
{
    private readonly SyncPropertyImplementation _syncImplementation;
    private readonly InterceptionDecoratorData _interceptionDecoratorData;

    internal PropertySetImplementationBodyGenerator(
        SyncPropertyImplementation syncImplementation,
        InterceptionDecoratorData interceptionDecoratorData)
        : base(syncImplementation, interceptionDecoratorData)
    {
        _syncImplementation = syncImplementation;
        _interceptionDecoratorData = interceptionDecoratorData;
    }

    protected override void GenerateReturnVariableInitialization(StringBuilder code) { }

    protected override ImmutableArray<string> ArgumentNames => ImmutableArray.Create("value");
    protected override ImmutableArray<ITypeParameterSymbol> TypeParameters => ImmutableArray<ITypeParameterSymbol>.Empty;
    protected override void GenerateProceedMethod(
        StringBuilder code,
        string invocationReference,
        string argumentsReference,
        IInvocationDescriptionNode invocationDescriptionNode)
    {
        if (invocationDescriptionNode.ProceedMethod)
        {
            code.AppendLine($"{invocationReference}.ProceedAction = () => {_interceptionDecoratorData.InterfaceFieldReference}.{_syncImplementation.Property.Name} = ({_syncImplementation.Property.Type.FullName()}) {argumentsReference}[0];");
        }
    }

    protected override string GenerateMethodSeeking(StringBuilder code)
    {
        var methodRef = ReferenceGenerator.Generate("method");
        code.AppendLine($"var {methodRef} = typeof({_syncImplementation.DeclaringInterface.FullName()}).GetProperty(\"{_syncImplementation.Property.Name}\")!.SetMethod!;");
        return methodRef;
    }

    protected override string ReturnValue => "default";

    protected override void GenerateReturnStatement(StringBuilder code) { }
}

internal sealed class EventAddImplementationBodyGenerator : ImplementationBodyGeneratorBase
{
    private readonly SyncEventImplementation _syncImplementation;
    private readonly InterceptionDecoratorData _interceptionDecoratorData;

    internal EventAddImplementationBodyGenerator(
        SyncEventImplementation syncImplementation,
        InterceptionDecoratorData interceptionDecoratorData)
        : base(syncImplementation, interceptionDecoratorData)
    {
        _syncImplementation = syncImplementation;
        _interceptionDecoratorData = interceptionDecoratorData;
    }

    protected override void GenerateReturnVariableInitialization(StringBuilder code) { }

    protected override ImmutableArray<string> ArgumentNames => ImmutableArray.Create("value");
    protected override ImmutableArray<ITypeParameterSymbol> TypeParameters => ImmutableArray<ITypeParameterSymbol>.Empty;
    protected override void GenerateProceedMethod(
        StringBuilder code,
        string invocationReference,
        string argumentsReference,
        IInvocationDescriptionNode invocationDescriptionNode)
    {
        if (invocationDescriptionNode.ProceedMethod)
        {
            code.AppendLine($"{invocationReference}.ProceedAction = () => {_interceptionDecoratorData.InterfaceFieldReference}.{_syncImplementation.Event.Name} += ({_syncImplementation.Event.Type.FullName()}) {argumentsReference}[0];");
        }
    }

    protected override string GenerateMethodSeeking(StringBuilder code)
    {
        var methodRef = ReferenceGenerator.Generate("method");
        code.AppendLine($"var {methodRef} = typeof({_syncImplementation.DeclaringInterface.FullName()}).GetEvent(\"{_syncImplementation.Event.Name}\")!.AddMethod!;");
        return methodRef;
    }

    protected override string ReturnValue => "default";

    protected override void GenerateReturnStatement(StringBuilder code) { }
}

internal sealed class EventRemoveImplementationBodyGenerator : ImplementationBodyGeneratorBase
{
    private readonly SyncEventImplementation _syncImplementation;
    private readonly InterceptionDecoratorData _interceptionDecoratorData;

    internal EventRemoveImplementationBodyGenerator(
        SyncEventImplementation syncImplementation,
        InterceptionDecoratorData interceptionDecoratorData)
        : base(syncImplementation, interceptionDecoratorData)
    {
        _syncImplementation = syncImplementation;
        _interceptionDecoratorData = interceptionDecoratorData;
    }

    protected override void GenerateReturnVariableInitialization(StringBuilder code) { }

    protected override ImmutableArray<string> ArgumentNames => ImmutableArray.Create("value");
    protected override ImmutableArray<ITypeParameterSymbol> TypeParameters => ImmutableArray<ITypeParameterSymbol>.Empty;
    protected override void GenerateProceedMethod(
        StringBuilder code,
        string invocationReference,
        string argumentsReference,
        IInvocationDescriptionNode invocationDescriptionNode)
    {
        if (invocationDescriptionNode.ProceedMethod)
        {
            code.AppendLine($"{invocationReference}.ProceedAction = () => {_interceptionDecoratorData.InterfaceFieldReference}.{_syncImplementation.Event.Name} += ({_syncImplementation.Event.Type.FullName()}) {argumentsReference}[0];");
        }
    }

    protected override string GenerateMethodSeeking(StringBuilder code)
    {
        var methodRef = ReferenceGenerator.Generate("method");
        code.AppendLine($"var {methodRef} = typeof({_syncImplementation.DeclaringInterface.FullName()}).GetEvent(\"{_syncImplementation.Event.Name}\")!.RemoveMethod!;");
        return methodRef;
    }

    protected override string ReturnValue => "default";

    protected override void GenerateReturnStatement(StringBuilder code) { }
}

internal sealed class IndexerGetImplementationBodyGenerator : ImplementationBodyGeneratorBase
{
    private readonly SyncIndexerImplementation _syncImplementation;
    private readonly InterceptionDecoratorData _interceptionDecoratorData;
    private readonly string _returnType;
    private string? _returnValueVariableReference;
    private string _assignedReturnValueReference = "";

    internal IndexerGetImplementationBodyGenerator(
        SyncIndexerImplementation syncImplementation,
        InterceptionDecoratorData interceptionDecoratorData)
        : base(syncImplementation, interceptionDecoratorData)
    {
        _syncImplementation = syncImplementation;
        _interceptionDecoratorData = interceptionDecoratorData;
        _returnType = _syncImplementation.Indexer.Type.FullName();
    }

    protected override void GenerateReturnVariableInitialization(StringBuilder code)
    {
        _returnValueVariableReference = ReferenceGenerator.Generate("returnValue");
        if (_returnValueVariableReference is not null)
            code.AppendLine($"var {_returnValueVariableReference} = default({_returnType});");
    }

    protected override ImmutableArray<string> ArgumentNames => _syncImplementation.Indexer.Parameters.Select(p => p.Name).ToImmutableArray();
    protected override ImmutableArray<ITypeParameterSymbol> TypeParameters => ImmutableArray<ITypeParameterSymbol>.Empty;
    protected override void GenerateProceedMethod(
        StringBuilder code,
        string invocationReference,
        string argumentsReference,
        IInvocationDescriptionNode invocationDescriptionNode)
    {
        _assignedReturnValueReference = invocationDescriptionNode.ReturnValueProperty
            ? $"{invocationReference}.ReturnValue"
            : $"{_returnValueVariableReference}";
        if (invocationDescriptionNode.ProceedMethod)
        {
            var argumentsAssignment = string.Join(", ", _syncImplementation.Indexer.Parameters.Select((p, i) => $"({p.Type.FullName()}) {argumentsReference}[{i}]"));
            code.AppendLine($"{invocationReference}.ProceedAction = () => {_assignedReturnValueReference} = {_interceptionDecoratorData.InterfaceFieldReference}[{argumentsAssignment}];");
        }
    }

    protected override string GenerateMethodSeeking(StringBuilder code)
    {
        var methodRef = ReferenceGenerator.Generate("method");

        var membersWithTheSameName = _syncImplementation.DeclaringInterface.GetMembers("Item");
        if (membersWithTheSameName.Length == 1) // Simple case where there is only one method with the same name
            code.AppendLine($"var {methodRef} = typeof({_syncImplementation.DeclaringInterface.FullName()}).GetProperty(\"Item\")!.GetMethod!;");
        else
        {
            var paramsRef = ReferenceGenerator.Generate("params");
            code.AppendLine($"var {methodRef} = {WellKnownTypesCollections.Enumerable.FullName()}.First(typeof({_syncImplementation.DeclaringInterface.FullName()}).GetProperties(), p =>");
            code.AppendLine("{");
            code.AppendLine($"var {paramsRef} = p.GetIndexParameters();");
            code.AppendLine($"return {string.Join(" && ", MethodSignatureCheckingConditions())};");
            code.AppendLine("}).GetMethod!;");

            IEnumerable<string> MethodSignatureCheckingConditions()
            {
                yield return "p.Name == \"Item\"";
                yield return $"{paramsRef}.Length == {_syncImplementation.Indexer.Parameters.Length}";
                for (var i = 0; i < _syncImplementation.Indexer.Parameters.Length; i++)
                {
                    yield return _syncImplementation.Indexer.Parameters[i].Type is ITypeParameterSymbol
                        ? $"{paramsRef}[{i}].ParameterType.Name == \"{_syncImplementation.Indexer.Parameters[i].Type.Name}\""
                        : $"{paramsRef}[{i}].ParameterType == typeof({_syncImplementation.Indexer.Parameters[i].Type.FullName()})";
                }
            }
        }
        
        return methodRef;
    }

    protected override string ReturnValue => _returnValueVariableReference ?? "default";

    protected override void GenerateReturnStatement(StringBuilder code)
    {
        code.AppendLine($"return ({_returnType}) {_assignedReturnValueReference};");
    }
}

internal sealed class IndexerSetImplementationBodyGenerator : ImplementationBodyGeneratorBase
{
    private readonly SyncIndexerImplementation _syncImplementation;
    private readonly InterceptionDecoratorData _interceptionDecoratorData;

    internal IndexerSetImplementationBodyGenerator(
        SyncIndexerImplementation syncImplementation,
        InterceptionDecoratorData interceptionDecoratorData)
        : base(syncImplementation, interceptionDecoratorData)
    {
        _syncImplementation = syncImplementation;
        _interceptionDecoratorData = interceptionDecoratorData;
    }

    protected override void GenerateReturnVariableInitialization(StringBuilder code) { }

    protected override ImmutableArray<string> ArgumentNames => _syncImplementation.Indexer.Parameters.Select(p => p.Name).Append("value").ToImmutableArray();
    protected override ImmutableArray<ITypeParameterSymbol> TypeParameters => ImmutableArray<ITypeParameterSymbol>.Empty;
    protected override void GenerateProceedMethod(
        StringBuilder code,
        string invocationReference,
        string argumentsReference,
        IInvocationDescriptionNode invocationDescriptionNode)
    {
        if (invocationDescriptionNode.ProceedMethod)
        {
            var argumentsAssignment = string.Join(", ", _syncImplementation.Indexer.Parameters.Select((p, i) => $"({p.Type.FullName()}) {argumentsReference}[{i}]"));
            code.AppendLine($"{invocationReference}.ProceedAction = () => {_interceptionDecoratorData.InterfaceFieldReference}[{argumentsAssignment}] = ({_syncImplementation.Indexer.Type.FullName()}) {argumentsReference}[{ArgumentNames.Length - 1}];");
        }
    }

    protected override string GenerateMethodSeeking(StringBuilder code)
    {
        var methodRef = ReferenceGenerator.Generate("method");

        var membersWithTheSameName = _syncImplementation.DeclaringInterface.GetMembers("Item");
        if (membersWithTheSameName.Length == 1) // Simple case where there is only one method with the same name
            code.AppendLine($"var {methodRef} = typeof({_syncImplementation.DeclaringInterface.FullName()}).GetProperty(\"Item\")!.SetMethod!;");
        else
        {
            var paramsRef = ReferenceGenerator.Generate("params");
            code.AppendLine($"var {methodRef} = {WellKnownTypesCollections.Enumerable.FullName()}.First(typeof({_syncImplementation.DeclaringInterface.FullName()}).GetProperties(), p =>");
            code.AppendLine("{");
            code.AppendLine($"var {paramsRef} = p.GetIndexParameters();");
            code.AppendLine($"return {string.Join(" && ", MethodSignatureCheckingConditions())};");
            code.AppendLine("}).SetMethod!;");

            IEnumerable<string> MethodSignatureCheckingConditions()
            {
                yield return "p.Name == \"Item\"";
                yield return $"{paramsRef}.Length == {_syncImplementation.Indexer.Parameters.Length}";
                for (var i = 0; i < _syncImplementation.Indexer.Parameters.Length; i++)
                {
                    yield return _syncImplementation.Indexer.Parameters[i].Type is ITypeParameterSymbol
                        ? $"{paramsRef}[{i}].ParameterType.Name == \"{_syncImplementation.Indexer.Parameters[i].Type.Name}\""
                        : $"{paramsRef}[{i}].ParameterType == typeof({_syncImplementation.Indexer.Parameters[i].Type.FullName()})";
                }
            }
        }
        
        return methodRef;
    }

    protected override string ReturnValue => "default";

    protected override void GenerateReturnStatement(StringBuilder code) { }
}
