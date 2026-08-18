using System.Threading;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.InjectionGraph.CodeGeneration.ConcreteNodeGenerators;
using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration;

internal sealed class ScopeNodeBaseCodeGenerator(
    FunctionUtility functionUtility,
    ContainerInfo containerInfo,
    ScopedInstanceInterfaceDescription scopedInstanceInterfaceDescription,
    InjectionNodeGenerator injectionNodeGenerator,
    ContextGenerator contextGenerator,
    ReferenceGenerator referenceGenerator,
    WellKnownTypes wellKnownTypes,
    ScopeNodeManager scopeNodeManager)
{
    internal ImmutableDictionary<NonContainerScopeNode, string> ScopeNodeToContainerPropertyReference =>
        field ??= scopeNodeManager.Scopes.OfType<NonContainerScopeNode>()
            .Concat(scopeNodeManager.TransientScopes)
            .ToImmutableDictionary(sn => sn, _ => referenceGenerator.Generate("Container"));

    internal ImmutableArray<string> GetInheritanceHeaderElements(ScopeNodeBase scopeNodeBase, bool isContainer)
    {
        var prefix = isContainer ? $"{containerInfo.Name}." : string.Empty;
        return [
            ..scopeNodeBase
                .ScopedInstances
                .Where(si => si.TypeNode.Outgoing.Any(o => o is ConcreteSyncEdge { Contexts: {} contexts } && contexts.Any(c => MatchingContext(scopeNodeBase, c))))
                .Select(si => $"{prefix}{scopedInstanceInterfaceDescription.SyncInterfaceName}<{si.TypeNode.Type.FullName()}>"),
            ..scopeNodeBase
                .ScopedInstances
                .Where(si => si.TypeNode.Outgoing.Any(o => o is ConcreteAsyncEdge { Contexts: {} contexts } && contexts.Any(c => MatchingContext(scopeNodeBase, c))))
                .Select(si => $"{prefix}{scopedInstanceInterfaceDescription.AsyncInterfaceName}<{si.TypeNode.Type.FullName()}>")];
    }

    private bool MatchingContext(ScopeNodeBase scopeNodeBase, EdgeContext context) =>
        (scopeNodeBase, context.ScopeNode) switch
        {
            (ContainerScopeNode, ScopeNodeContext.Container) => true,
            (TransientScopeNode { Name: var leftName }, ScopeNodeContext.TransientScope { TransientScopeName: var rightName }) => leftName == rightName,
            (ScopeNode { Name: var leftName }, ScopeNodeContext.Scope { ScopeName: var rightName }) => leftName == rightName,
            _ => false
        };
    
    internal void GenerateInterface(StringBuilder code)
    {
        var syncTypeParameterName = referenceGenerator.Generate("T");
        code.AppendLine($"private interface {scopedInstanceInterfaceDescription.SyncInterfaceName}<{syncTypeParameterName}>");
        code.AppendLine("{");
        code.AppendLine($"{syncTypeParameterName} {ScopedInstanceInterfaceDescription.SyncFunctionName}({contextGenerator.FullNameAndParameterName}, {wellKnownTypes.Boolean.FullName()} {functionUtility.DoScopedInstanceParameterName}, {wellKnownTypes.Boolean.FullName()} {functionUtility.DoScopeRootParameterName});");
        code.AppendLine("}");
        code.AppendLine();
        var asyncTypeParameterName = referenceGenerator.Generate("T");
        var valueTaskFullName = wellKnownTypes.ValueTask1 is not null && wellKnownTypes.ValueTask is not null
            ? wellKnownTypes.ValueTask.FullName()
            : wellKnownTypes.Task.FullName();
        code.AppendLine($"private interface {scopedInstanceInterfaceDescription.AsyncInterfaceName}<{asyncTypeParameterName}>");
        code.AppendLine("{");
        code.AppendLine($"{valueTaskFullName}<{asyncTypeParameterName}> {ScopedInstanceInterfaceDescription.AsyncFunctionName}({contextGenerator.FullNameAndParameterName}, {wellKnownTypes.Boolean.FullName()} {functionUtility.DoScopedInstanceParameterName}, {wellKnownTypes.Boolean.FullName()} {functionUtility.DoScopeRootParameterName});");
        code.AppendLine("}");
        code.AppendLine();
    }
    
    internal void GenerateScopeRootFunctions(StringBuilder code, NonContainerScopeNode scopeNode, string containerReference)
    {
        foreach ( var scopeRoot in scopeNode.ScopedRoots)
        {
            if (scopeRoot.TypeNode.SyncFunction is { } syncFunction)
                GenerateFunction(scopeRoot.SyncFunction, syncFunction);
            else if (scopeRoot.TypeNode.AsyncFunction is { } asyncFunction)
                GenerateFunction(scopeRoot.AsyncFunction, asyncFunction);
            continue;
            
            void GenerateFunction(IFunction outerFunction, IFunction innerFunction)
            {
                code.AppendLine(functionUtility.GenerateHeader(outerFunction));
        
                code.AppendLine("{");
        
                // ToDo Disposal Handling
                // ToDo Async Handling
            
                var newContextAssignment = scopeNode switch
                {
                    ScopeNode => contextGenerator.GenerateCopyAssignment(containerNode: containerReference, scopeNode: Constants.ThisKeyword, scopeNodeName: $"\"{scopeNode.Name}\""),
                    TransientScopeNode => contextGenerator.GenerateCopyAssignment(containerNode: containerReference, transientScopeNode: Constants.ThisKeyword, scopeNode: Constants.ThisKeyword, scopeNodeName: $"\"{scopeNode.Name}\"")
                };
                var await = outerFunction.Sync
                    ? ""
                    : "await ";
                code.AppendLine(newContextAssignment);
                code.AppendLine($"return {await}{containerReference}.{functionUtility.GenerateFunctionCall(innerFunction, doScopedInstance: true, doScopeRoot: false)};");
        
                code.AppendLine("}");
            }
        }
    }
    
    internal void GenerateScopedInstanceFunctions(StringBuilder code, ScopeNodeBase scopeNodeBase, string containerReference)
    {
        var semaphoreSlimFullName = wellKnownTypes.SemaphoreSlim.FullName();
        var objectFullName = wellKnownTypes.Object.FullName();

        foreach ( var scopedInstance in scopeNodeBase.ScopedInstances)
        {
            var typeSymbol = scopedInstance.TypeNode.Type;
            
            var scopedInstanceFieldReference = referenceGenerator.Generate("_scopedInstanceField", typeSymbol);
            code.AppendLine($"private {typeSymbol.FullName()}? {scopedInstanceFieldReference};");
            var scopedInstanceLockFieldReference = referenceGenerator.Generate("_scopedInstanceLock", typeSymbol);
            code.AppendLine($"private {semaphoreSlimFullName}? {scopedInstanceLockFieldReference} = new {semaphoreSlimFullName}(1);");
            

            if (scopedInstance.TypeNode.SyncFunction is { } syncFunction
                && scopedInstance.TypeNode.Outgoing.Any(e => e is ConcreteSyncEdge { Contexts: {} contexts } && contexts.Any(c => MatchingContext(scopeNodeBase, c))))
                GenerateFunction(scopedInstance.SyncFunction, syncFunction);
            if (scopedInstance.TypeNode.AsyncFunction is { } asyncFunction
                && scopedInstance.TypeNode.Outgoing.Any(e => e is ConcreteAsyncEdge { Contexts: {} contexts } && contexts.Any(c => MatchingContext(scopeNodeBase, c))))
                GenerateFunction(scopedInstance.AsyncFunction, asyncFunction);
            continue;

            void GenerateFunction(IFunction outerFunction, IFunction innerFunction)
            {
                var await = outerFunction.Sync
                    ? ""
                    : "await ";
                
                code.AppendLine(functionUtility.GenerateHeader(outerFunction));
                code.AppendLine("{");
                
                // ToDo Disposal Handling
                
                code.AppendLine($"if(!{objectFullName}.{nameof(ReferenceEquals)}({scopedInstanceFieldReference}, {Constants.NullKeyword}))");
                code.AppendLine($"return {scopedInstanceFieldReference};");
                
                if (outerFunction.Sync)
                    code.AppendLine($"{Constants.ThisKeyword}.{scopedInstanceLockFieldReference}?.{nameof(SemaphoreSlim.Wait)}();");
                else
                    code.AppendLine($"{await}({Constants.ThisKeyword}.{scopedInstanceLockFieldReference}?.{nameof(SemaphoreSlim.WaitAsync)}() ?? {wellKnownTypes.Task.FullName()}.{nameof(Task.CompletedTask)});");
                code.AppendLine("try");
                code.AppendLine("{");
                
                code.AppendLine($"if(!{objectFullName}.{nameof(ReferenceEquals)}({scopedInstanceFieldReference}, {Constants.NullKeyword}))");
                code.AppendLine($"return {scopedInstanceFieldReference};");
                    
                code.AppendLine($"{Constants.ThisKeyword}.{scopedInstanceFieldReference} = {await}{containerReference}.{functionUtility.GenerateFunctionCall(innerFunction, doScopedInstance: false, doScopeRoot: false)};");

                code.AppendLine("}");
                code.AppendLine("finally");
                code.AppendLine("{");
                code.AppendLine($"{Constants.ThisKeyword}.{scopedInstanceLockFieldReference}?.{nameof(SemaphoreSlim.Release)}();");
                code.AppendLine("}");
                
                code.AppendLine($"{Constants.ThisKeyword}.{scopedInstanceLockFieldReference} = {Constants.NullKeyword};");
                code.AppendLine($"return {Constants.ThisKeyword}.{scopedInstanceFieldReference};");
                
                code.AppendLine("}");
            }
        }
    }

    internal void GenerateScopeRootEntry(StringBuilder code, TypeNode rootNode, bool sync)
    {
        if (rootNode.ScopeRootConfiguration.Count == 0) 
            return;
        
        code.AppendLine($"if ({functionUtility.DoScopeRootParameterName})");
        code.AppendLine("{");

        var firstIteration = true;
        foreach (var keyValuePair in rootNode.ScopeRootConfiguration)
        {
            if (firstIteration)
                firstIteration = false;
            else
                code.Append("else ");
            var rootContext = keyValuePair.Key;
            var value =  keyValuePair.Value;
            var conditions = string.Join(" || ", value.PreviousScopeNodeContexts.Select(c =>
            {
                var scopeNodeName = c switch
                {
                    ScopeNodeContext.Container => containerInfo.Name,
                    ScopeNodeContext.Scope scope => scope.ScopeName,
                    ScopeNodeContext.TransientScope transientScope => transientScope.TransientScopeName,
                    _ => throw new ArgumentOutOfRangeException(nameof(c))
                };
                return $"{contextGenerator.ParameterName}.{contextGenerator.ScopeNodeNamePropertyName} == \"{scopeNodeName}\"";
            }));
            code.AppendLine($"if ({conditions})");
            code.AppendLine("{");

            var (scopeNode, calledFunction) = GetScopeNodeAndFunction();

            if (value.ScopeRootTypeTypeEdge is { Target: var scopeRootTypeNode, Source: var source })
            {
                var rootReference = injectionNodeGenerator.CallFunctionOrGenerateForInjectionNode(code, scopeRootTypeNode, source: source, sync: sync);
                var await = !sync ? "await " : "";
                code.AppendLine($"{rootReference}.{ScopeNodeToContainerPropertyReference[scopeNode]} = ({containerInfo.FullName}) {contextGenerator.ParameterName}.{contextGenerator.ContainerNodePropertyName};");
                code.AppendLine($"return {await}{rootReference}.{functionUtility.GenerateFunctionCall(calledFunction, doScopedInstance: true, doScopeRoot: true)};");
            }
            else
            {
                var await = !sync ? "await " : "";
                switch (rootContext)
                {
                    case ScopeNodeContext.TransientScope { TransientScopeName: var transientScopeName }:
                    {
                        var transientScopeReference = referenceGenerator.Generate("transientScope");
                        code.AppendLine(
                            $$"""
                              {{transientScopeName}} {{transientScopeReference}} = new {{transientScopeName}}() { {{ScopeNodeToContainerPropertyReference[scopeNode]}} = ({{containerInfo.FullName}}) {{contextGenerator.ParameterName}}.{{contextGenerator.ContainerNodePropertyName}} };
                              return {{await}}{{transientScopeReference}}.{{functionUtility.GenerateFunctionCall(calledFunction, doScopedInstance: true, doScopeRoot: true)}};
                              """);
                        break;
                    }
                    case ScopeNodeContext.Scope { ScopeName: var scopeName }:
                    {
                        var scopeReference = referenceGenerator.Generate("scope");
                        code.AppendLine(
                            $$"""
                              {{scopeName}} {{scopeReference}} = new {{scopeName}}() { {{ScopeNodeToContainerPropertyReference[scopeNode]}} = ({{containerInfo.FullName}}) {{contextGenerator.ParameterName}}.{{contextGenerator.ContainerNodePropertyName}} };
                              return {{await}}{{scopeReference}}.{{functionUtility.GenerateFunctionCall(calledFunction, doScopedInstance: true, doScopeRoot: true)}};
                              """);
                        break;
                    }
                    default:
                        throw new ArgumentException($"Parameter should be either {nameof(ScopeNodeContext.TransientScope)} or {nameof(ScopeNodeContext.Scope)} here, but is {rootContext.GetType().FullName}", nameof(rootContext));
                        
                }
            }
            
            code.AppendLine("}");
            continue;

            (NonContainerScopeNode, IFunction) GetScopeNodeAndFunction()
            {
                NonContainerScopeNode scopeNode = rootContext switch
                {
                    ScopeNodeContext.TransientScope { TransientScopeName: var transientScopeName } => 
                        scopeNodeManager.TransientScopes.First(s => s.Name == transientScopeName),
                    ScopeNodeContext.Scope { ScopeName: var scopeName } => 
                        scopeNodeManager.Scopes.First(s => s.Name == scopeName),
                    _ => throw new ArgumentException(
                        $"Parameter should be either {nameof(ScopeNodeContext.TransientScope)} or {nameof(ScopeNodeContext.Scope)} here, but is {rootContext.GetType().FullName}",
                        nameof(rootContext))
                };
                var (_, syncFunction, asyncFunction) = scopeNode.ScopedRoots.First(sr => CustomSymbolEqualityComparer.Default.Equals(sr.TypeNode.Type, rootNode.Type));
                return (
                    scopeNode, 
                    sync ? syncFunction : asyncFunction);
            }
        }
        
        code.AppendLine("}");
    }

    internal void GenerateScopedInstanceEntry(StringBuilder code, TypeNode rootNode, bool sync)
    {
        if (!rootNode.ScopeInstanceConfiguration.Any(kvp => kvp.Key is not ScopeLevel.None)) 
            return;
        
        // At this point it doesn't actually matter from which scope node (Container, Transient, Scope) the scoped instance is,
        // because we just need any function for the call (which will have the same name everytime).
        var (_, syncFunction, asyncFunction) = scopeNodeManager.ContainerScopeNode.ScopedInstances
            .Concat(scopeNodeManager.Scopes.SelectMany(s => s.ScopedInstances))
            .Concat(scopeNodeManager.TransientScopes.SelectMany(ts => ts.ScopedInstances))
            .First(sid => CustomSymbolEqualityComparer.Default.Equals(sid.TypeNode.Type, rootNode.Type));

        code.AppendLine($"if ({functionUtility.DoScopedInstanceParameterName})");
        code.AppendLine("{");

        if (rootNode.ScopeInstanceConfiguration.Count == 1)
            code.AppendLine(CreateReturn(rootNode.ScopeInstanceConfiguration.First().Key));
        else
            code.AppendLine(string.Join($"{Environment.NewLine}else ", rootNode.ScopeInstanceConfiguration
                .Where(kvp => kvp.Key is not ScopeLevel.None)
                .Select(CreateIf)));

        code.AppendLine("}");

        return;

        string CreateIf(KeyValuePair<ScopeLevel, HashSet<ScopeNodeContext>> levelConfiguration)
        {
            var level = levelConfiguration.Key;
            var contexts =  levelConfiguration.Value;
            var conditions = string.Join(" || ", contexts.Select(c =>
            {
                var scopeNodeName = c switch
                {
                    ScopeNodeContext.Container => containerInfo.Name,
                    ScopeNodeContext.Scope scope => scope.ScopeName,
                    ScopeNodeContext.TransientScope transientScope => transientScope.TransientScopeName,
                    _ => throw new ArgumentOutOfRangeException(nameof(c))
                };
                return $"{contextGenerator.ParameterName}.{contextGenerator.ScopeNodeNamePropertyName} == \"{scopeNodeName}\"";
            }));
            return $$"""
                     if ({{conditions}})
                     {
                     {{CreateReturn(level)}}
                     }
                     """;
        }

        string CreateReturn(ScopeLevel scopeLevel)
        {
            var contextProperty = scopeLevel switch
            {
                ScopeLevel.Container => contextGenerator.ContainerNodePropertyName,
                ScopeLevel.TransientScope => contextGenerator.TransientScopeNodePropertyName,
                ScopeLevel.Scope => contextGenerator.ScopeNodePropertyName
            };
            var calledFunction = sync
                ? syncFunction
                : asyncFunction;
            var castType = sync
                ? $"{scopedInstanceInterfaceDescription.SyncInterfaceName}<{rootNode.Type}>"
                : $"{scopedInstanceInterfaceDescription.AsyncInterfaceName}<{rootNode.Type}>";
            var await = sync
                ? ""
                : "await ";
            return $"return {await}(({castType}) {contextGenerator.ParameterName}.{contextProperty}).{functionUtility.GenerateFunctionCall(calledFunction, doScopedInstance: true, doScopeRoot: true)};";
        }
    }
}