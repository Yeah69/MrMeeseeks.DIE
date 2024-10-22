using MrMeeseeks.DIE.Configuration.Interception;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.CodeGeneration;

internal interface IInterceptorDecoratorGenerator
{
    string? Generate();
}

internal class InterceptorDecoratorGenerator : IInterceptorDecoratorGenerator
{
    private readonly IInvocationTypeManager _invocationTypeManager;
    private readonly IReferenceGenerator _referenceGenerator;
    private readonly IOrdinaryTypeParameterConstraintsDisplayer _ordinaryTypeParameterConstraintsDisplayer;
    private readonly Func<SyncMethodImplementation, InterceptionDecoratorData, MethodImplementationBodyGenerator> _methodImplementationBodyGeneratorFactory;
    private readonly Func<SyncPropertyImplementation, InterceptionDecoratorData, PropertyGetImplementationBodyGenerator> _propertyGetImplementationBodyGeneratorFactory;
    private readonly Func<SyncPropertyImplementation, InterceptionDecoratorData, PropertySetImplementationBodyGenerator> _propertySetImplementationBodyGeneratorFactory;
    private readonly Func<SyncEventImplementation, InterceptionDecoratorData, EventAddImplementationBodyGenerator> _eventAddImplementationBodyGeneratorFactory;
    private readonly Func<SyncEventImplementation, InterceptionDecoratorData, EventRemoveImplementationBodyGenerator> _eventRemoveImplementationBodyGeneratorFactory;
    private readonly Func<SyncIndexerImplementation, InterceptionDecoratorData, IndexerGetImplementationBodyGenerator> _indexerGetImplementationBodyGeneratorFactory;
    private readonly Func<SyncIndexerImplementation, InterceptionDecoratorData, IndexerSetImplementationBodyGenerator> _indexerSetImplementationBodyGeneratorFactory;

    internal InterceptorDecoratorGenerator(
        // parameters
        
        // dependencies
        IInvocationTypeManager invocationTypeManager,
        IReferenceGenerator referenceGenerator,
        IOrdinaryTypeParameterConstraintsDisplayer ordinaryTypeParameterConstraintsDisplayer,
        Func<SyncMethodImplementation, InterceptionDecoratorData, MethodImplementationBodyGenerator> methodImplementationBodyGeneratorFactory,
        Func<SyncPropertyImplementation, InterceptionDecoratorData, PropertyGetImplementationBodyGenerator> propertyGetImplementationBodyGeneratorFactory,
        Func<SyncPropertyImplementation, InterceptionDecoratorData, PropertySetImplementationBodyGenerator> propertySetImplementationBodyGeneratorFactory, 
        Func<SyncEventImplementation, InterceptionDecoratorData, EventAddImplementationBodyGenerator> eventAddImplementationBodyGeneratorFactory,
        Func<SyncEventImplementation, InterceptionDecoratorData, EventRemoveImplementationBodyGenerator> eventRemoveImplementationBodyGeneratorFactory,
        Func<SyncIndexerImplementation, InterceptionDecoratorData, IndexerGetImplementationBodyGenerator> indexerGetImplementationBodyGeneratorFactory, 
        Func<SyncIndexerImplementation, InterceptionDecoratorData, IndexerSetImplementationBodyGenerator> indexerSetImplementationBodyGeneratorFactory)
    {
        _invocationTypeManager = invocationTypeManager;
        _referenceGenerator = referenceGenerator;
        _ordinaryTypeParameterConstraintsDisplayer = ordinaryTypeParameterConstraintsDisplayer;
        _methodImplementationBodyGeneratorFactory = methodImplementationBodyGeneratorFactory;
        _propertyGetImplementationBodyGeneratorFactory = propertyGetImplementationBodyGeneratorFactory;
        _propertySetImplementationBodyGeneratorFactory = propertySetImplementationBodyGeneratorFactory;
        _eventAddImplementationBodyGeneratorFactory = eventAddImplementationBodyGeneratorFactory;
        _eventRemoveImplementationBodyGeneratorFactory = eventRemoveImplementationBodyGeneratorFactory;
        _indexerGetImplementationBodyGeneratorFactory = indexerGetImplementationBodyGeneratorFactory;
        _indexerSetImplementationBodyGeneratorFactory = indexerSetImplementationBodyGeneratorFactory;
    }
    
    public string? Generate()
    {
        var interceptionDecoratorDatas = _invocationTypeManager.InterceptorBasedDecoratorTypes.ToList();
        if (interceptionDecoratorDatas.Count == 0)
            return null;
        
        var code = new StringBuilder();
        code.AppendLine("#nullable enable");
        code.AppendLine($"namespace {Constants.NamespaceForGeneratedUtilities}");
        code.AppendLine("{");
        
        foreach (var interceptionDecoratorData in interceptionDecoratorDatas)
        {
            var interfaceConstructorParameterReference = _referenceGenerator.Generate("interface");
            var interceptorConstructorParameterReference = _referenceGenerator.Generate("interceptor");
            code.AppendLine($"internal class {interceptionDecoratorData.Name} : {interceptionDecoratorData.InterfaceFullName}");
            code.AppendLine("{");
            code.AppendLine($"private readonly {interceptionDecoratorData.InterfaceFullName} {interceptionDecoratorData.InterfaceFieldReference};");
            code.AppendLine($"private readonly {interceptionDecoratorData.InterceptorFullName} {interceptionDecoratorData.InterceptorFieldReference};");
            code.AppendLine($"internal {interceptionDecoratorData.Name}({interceptionDecoratorData.InterceptorFullName} {interceptorConstructorParameterReference}, {interceptionDecoratorData.InterfaceFullName} {interfaceConstructorParameterReference})");
            code.AppendLine("{");
            code.AppendLine($"{interceptionDecoratorData.InterfaceFieldReference} = {interfaceConstructorParameterReference};");
            code.AppendLine($"{interceptionDecoratorData.InterceptorFieldReference} = {interceptorConstructorParameterReference};");
            code.AppendLine("}");
            
            foreach (var implementation in interceptionDecoratorData.Implementations)
            {
                switch (implementation)
                {
                    case DelegationPropertyImplementation delegationPropertyImplementation:
                        GenerateImplementationForDelegationProperty(delegationPropertyImplementation, interceptionDecoratorData);
                        break;
                    case DelegationIndexerImplementation delegationIndexerImplementation:
                        GenerateImplementationForDelegationIndexer(delegationIndexerImplementation, interceptionDecoratorData);
                        break;
                    case DelegationMethodImplementation delegationMethodImplementation:
                        GenerateImplementationForDelegationMethod(delegationMethodImplementation, interceptionDecoratorData);
                        break;
                    case DelegationEventImplementation delegationEventImplementation:
                        GenerateImplementationForDelegationEvent(delegationEventImplementation, interceptionDecoratorData);
                        break;
                    case SyncPropertyImplementation syncPropertyImplementation:
                        GenerateImplementationForSyncProperty(syncPropertyImplementation, interceptionDecoratorData);
                        break;
                    case SyncIndexerImplementation syncIndexerImplementation:
                        GenerateImplementationForSyncIndexer(syncIndexerImplementation, interceptionDecoratorData);
                        break;
                    case SyncMethodImplementation syncMethodImplementation:
                        GenerateImplementationForSyncMethod(syncMethodImplementation, interceptionDecoratorData);
                        break;
                    case SyncEventImplementation syncEventImplementation:
                        GenerateImplementationForSyncEvent(syncEventImplementation, interceptionDecoratorData);
                        break;
                }
            }
            
            code.AppendLine("}");
        }
        code.AppendLine("}");
        code.AppendLine("#nullable disable");
        return code.ToString();

        void GenerateImplementationForDelegationProperty(
            DelegationPropertyImplementation delegationPropertyImplementation,
            InterceptionDecoratorData interceptionDecoratorData)
        {
            code.AppendLine($"{delegationPropertyImplementation.TypeFullName} {delegationPropertyImplementation.DeclaringInterfaceFullName}.{delegationPropertyImplementation.Name}");
            code.AppendLine("{");
            if (delegationPropertyImplementation.HasGetter)
                code.AppendLine($"get {{ return (({delegationPropertyImplementation.DeclaringInterfaceFullName}){interceptionDecoratorData.InterfaceFieldReference}).{delegationPropertyImplementation.Name}; }}");
            if (delegationPropertyImplementation.HasSetter)
                code.AppendLine($"set {{ (({delegationPropertyImplementation.DeclaringInterfaceFullName}){interceptionDecoratorData.InterfaceFieldReference}).{delegationPropertyImplementation.Name} = value; }}");
            code.AppendLine("}");
        }

        void GenerateImplementationForDelegationIndexer(
            DelegationIndexerImplementation delegationIndexerImplementation,
            InterceptionDecoratorData interceptionDecoratorData)
        {
            code.AppendLine($"{delegationIndexerImplementation.TypeFullName} {delegationIndexerImplementation.DeclaringInterfaceFullName}.this[{string.Join(", ", delegationIndexerImplementation.Parameters.Select(p => $"{p.TypeFullName} {p.Name}"))}]");
            code.AppendLine("{");
            if (delegationIndexerImplementation.HasGetter)
                code.AppendLine($"get {{ return (({delegationIndexerImplementation.DeclaringInterfaceFullName}){interceptionDecoratorData.InterfaceFieldReference})[{string.Join(", ", delegationIndexerImplementation.Parameters.Select(p => p.Name))}]; }}");
            if (delegationIndexerImplementation.HasSetter)
                code.AppendLine($"set {{ (({delegationIndexerImplementation.DeclaringInterfaceFullName}){interceptionDecoratorData.InterfaceFieldReference})[{string.Join(", ", delegationIndexerImplementation.Parameters.Select(p => p.Name))}] = value; }}");
            code.AppendLine("}");
        }

        void GenerateImplementationForDelegationMethod(
            DelegationMethodImplementation delegationMethodImplementation,
            InterceptionDecoratorData interceptionDecoratorData)
        {
            var genericsPart = delegationMethodImplementation.GenericTypeParameters.Count == 0
                ? string.Empty
                : $"<{string.Join(", ", delegationMethodImplementation.GenericTypeParameters)}>";
            var typeConstraints = string.Join(" ", delegationMethodImplementation.TypeParameters.Select(tp => _ordinaryTypeParameterConstraintsDisplayer.Display(tp)).Where(s => s is not ""));
            code.AppendLine($"{delegationMethodImplementation.TypeFullName} {delegationMethodImplementation.DeclaringInterfaceFullName}.{delegationMethodImplementation.Name}{genericsPart}({string.Join(", ", delegationMethodImplementation.Parameters.Select(p => $"{p.TypeFullName} {p.Name}"))}) {typeConstraints}");
            code.AppendLine("{");
            var returnPart = delegationMethodImplementation.ReturnsVoid
                ? string.Empty
                : "return ";
            code.AppendLine($"{returnPart}(({delegationMethodImplementation.DeclaringInterfaceFullName}){interceptionDecoratorData.InterfaceFieldReference}).{delegationMethodImplementation.Name}{genericsPart}({string.Join(", ", delegationMethodImplementation.Parameters.Select(p => p.Name))});");
            code.AppendLine("}");
        }

        void GenerateImplementationForDelegationEvent(
            DelegationEventImplementation delegationEventImplementation,
            InterceptionDecoratorData interceptionDecoratorData)
        {
            code.AppendLine($"event {delegationEventImplementation.TypeFullName} {delegationEventImplementation.DeclaringInterfaceFullName}.{delegationEventImplementation.Name}");
            code.AppendLine("{");
            code.AppendLine($"add {{ (({delegationEventImplementation.DeclaringInterfaceFullName}){interceptionDecoratorData.InterfaceFieldReference}).{delegationEventImplementation.Name} += value; }}");
            code.AppendLine($"remove {{ (({delegationEventImplementation.DeclaringInterfaceFullName}){interceptionDecoratorData.InterfaceFieldReference}).{delegationEventImplementation.Name} += value; }}");
            code.AppendLine("}");
        }

        void GenerateImplementationForSyncProperty(
            SyncPropertyImplementation syncPropertyImplementation,
            InterceptionDecoratorData interceptionDecoratorData)
        {
            var declaringInterface = syncPropertyImplementation.DeclaringInterface;
            var property = syncPropertyImplementation.Property;
            var returnType = property.Type.FullName();
            code.AppendLine($"{returnType} {declaringInterface.FullName()}.{property.Name}");
            code.AppendLine("{");
            if (property.GetMethod is not null)
            {
                code.AppendLine("get");
                code.AppendLine("{");
                _propertyGetImplementationBodyGeneratorFactory(syncPropertyImplementation, interceptionDecoratorData).Generate(code);
                code.AppendLine("}");
            }
            if (property.SetMethod is not null)
            {
                code.AppendLine("set");
                code.AppendLine("{");
                _propertySetImplementationBodyGeneratorFactory(syncPropertyImplementation, interceptionDecoratorData).Generate(code);
                code.AppendLine("}");
            }
            code.AppendLine("}");
        }

        void GenerateImplementationForSyncIndexer(
            SyncIndexerImplementation syncIndexerImplementation,
            InterceptionDecoratorData interceptionDecoratorData)
        {
            var declaringInterface = syncIndexerImplementation.DeclaringInterface;
            var indexer = syncIndexerImplementation.Indexer;
            var returnType = indexer.Type.FullName();
            var parameters = string.Join(", ", indexer.Parameters.Select(p => $"{p.Type.FullName()} {p.Name}"));
            code.AppendLine($"{returnType} {declaringInterface.FullName()}.this[{parameters}]");
            code.AppendLine("{");
            if (indexer.GetMethod is not null)
            {
                code.AppendLine("get");
                code.AppendLine("{");
                _indexerGetImplementationBodyGeneratorFactory(syncIndexerImplementation, interceptionDecoratorData).Generate(code);
                code.AppendLine("}");
            }
            if (indexer.SetMethod is not null)
            {
                code.AppendLine("set");
                code.AppendLine("{");
                _indexerSetImplementationBodyGeneratorFactory(syncIndexerImplementation, interceptionDecoratorData).Generate(code);
                code.AppendLine("}");
            }
            code.AppendLine("}");
        }

        void GenerateImplementationForSyncMethod(
            SyncMethodImplementation syncMethodImplementation,
            InterceptionDecoratorData interceptionDecoratorData)
        {
            var declaringInterface = syncMethodImplementation.DeclaringInterface;
            var method = syncMethodImplementation.Method;
            var genericsPart = method.TypeParameters.Length == 0
                ? string.Empty
                : $"<{string.Join(", ", method.TypeParameters.Select(tp => tp.FullName()))}>";
            var returnType = method.ReturnsVoid ? "void" : method.ReturnType.FullName();
            var typeConstraints = string.Join(" ", method.TypeParameters.Select(tp => _ordinaryTypeParameterConstraintsDisplayer.Display(tp)).Where(s => s is not ""));
            code.AppendLine($"{returnType} {declaringInterface.FullName()}.{method.Name}{genericsPart}({string.Join(", ", method.Parameters.Select(p => $"{p.Type.FullName()} {p.Name}"))}) {typeConstraints}");
            code.AppendLine("{");
            
            _methodImplementationBodyGeneratorFactory(syncMethodImplementation, interceptionDecoratorData).Generate(code);
            
            code.AppendLine("}");
        }

        void GenerateImplementationForSyncEvent(
            SyncEventImplementation syncEventImplementation,
            InterceptionDecoratorData interceptionDecoratorData)
        {
            var declaringInterface = syncEventImplementation.DeclaringInterface;
            var @event = syncEventImplementation.Event;
            var eventType = @event.Type.FullName();
            code.AppendLine($"event {eventType}? {declaringInterface.FullName()}.{@event.Name}");
            code.AppendLine("{");
            if (@event.AddMethod is not null)
            {
                code.AppendLine("add");
                code.AppendLine("{");
                _eventAddImplementationBodyGeneratorFactory(syncEventImplementation, interceptionDecoratorData).Generate(code);
                code.AppendLine("}");
            }
            if (@event.RemoveMethod is not null)
            {
                code.AppendLine("remove");
                code.AppendLine("{");
                _eventRemoveImplementationBodyGeneratorFactory(syncEventImplementation, interceptionDecoratorData).Generate(code);
                code.AppendLine("}");
            }
            code.AppendLine("}");
        }
    }
}