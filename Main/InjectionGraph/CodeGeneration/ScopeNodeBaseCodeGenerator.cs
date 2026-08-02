using System.Threading;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration;

internal sealed class ScopeNodeBaseCodeGenerator(
    FunctionUtility functionUtility,
    ContainerInfo containerInfo,
    ScopedInstanceInterfaceDescription scopedInstanceInterfaceDescription,
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
        return [..scopeNodeBase.ScopedInstances.Select(si => 
            $"{prefix}{scopedInstanceInterfaceDescription.InterfaceName}<{si.TypeNode.Type.FullName()}>")];
    }
    
    internal void GenerateInterface(StringBuilder code)
    {
        var typeParameterName = referenceGenerator.Generate("T");
        code.AppendLine($"private interface {scopedInstanceInterfaceDescription.InterfaceName}<{typeParameterName}>");
        code.AppendLine("{");
        code.AppendLine($"{typeParameterName} {ScopedInstanceInterfaceDescription.FunctionName}({contextGenerator.FullNameAndParameterName}, {wellKnownTypes.Boolean.FullName()} {functionUtility.DoScopedInstanceParameterName}, {wellKnownTypes.Boolean.FullName()} {functionUtility.DoScopeRootParameterName});");
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
                code.AppendLine(newContextAssignment);
                code.AppendLine($"return {containerReference}.{functionUtility.GenerateFunctionCall(innerFunction, doScopedInstance: true, doScopeRoot: false)};");
        
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

            if (scopedInstance.TypeNode.SyncFunction is { } syncFunction)
                GenerateFunction(scopedInstance.SyncFunction, syncFunction);
            else if (scopedInstance.TypeNode.AsyncFunction is { } asyncFunction)
                GenerateFunction(scopedInstance.AsyncFunction, asyncFunction);
            continue;

            void GenerateFunction(IFunction outerFunction, IFunction innerFunction)
            {
                var scopedInstanceFieldReference = referenceGenerator.Generate("_scopedInstanceField", typeSymbol);
                var scopedInstanceLockFieldReference = referenceGenerator.Generate("_scopedInstanceLock", typeSymbol);
                code.AppendLine($"private {typeSymbol.FullName()}? {scopedInstanceFieldReference};");
                code.AppendLine($"private {semaphoreSlimFullName}? {scopedInstanceLockFieldReference} = new {semaphoreSlimFullName}(1);");
                
                code.AppendLine(functionUtility.GenerateHeader(outerFunction));
                    
                code.AppendLine("{");
                
                // ToDo Disposal Handling
                
                code.AppendLine($"if(!{objectFullName}.{nameof(ReferenceEquals)}({scopedInstanceFieldReference}, {Constants.NullKeyword}))");
                code.AppendLine($"return {scopedInstanceFieldReference};");
                // ToDo Async Handling
                code.AppendLine($"{Constants.ThisKeyword}.{scopedInstanceLockFieldReference}?.{nameof(SemaphoreSlim.Wait)}();");
                code.AppendLine("try");
                code.AppendLine("{");
                
                code.AppendLine($"if(!{objectFullName}.{nameof(ReferenceEquals)}({scopedInstanceFieldReference}, {Constants.NullKeyword}))");
                code.AppendLine($"return {scopedInstanceFieldReference};");
                    
                code.AppendLine($"{Constants.ThisKeyword}.{scopedInstanceFieldReference} = {containerReference}.{functionUtility.GenerateFunctionCall(innerFunction, doScopedInstance: false, doScopeRoot: false)};");

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
        
        if (rootNode.ScopeRootConfiguration.Count == 1)
            code.AppendLine(CreateReturn(rootNode.ScopeRootConfiguration.First().Key));
        else
            code.AppendLine(string.Join($"{Environment.NewLine}else ", rootNode.ScopeRootConfiguration
                .Select(CreateIf)));
        
        code.AppendLine("}");

        return;
        
        string CreateIf(KeyValuePair<ScopeNodeContext, HashSet<ScopeNodeContext>> rootConfiguration)
        {
            var rootContext = rootConfiguration.Key;
            var contexts =  rootConfiguration.Value;
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
                     {{CreateReturn(rootContext)}}
                     }
                     """;
        }

        string CreateReturn(ScopeNodeContext scopeRootContext)
        {
            switch (scopeRootContext)
            {
                case ScopeNodeContext.TransientScope { TransientScopeName: var transientScopeName }:
                {
                    var transientScopeNode = scopeNodeManager.TransientScopes.First(s => s.Name == transientScopeName);
                    var (_, syncFunction, asyncFunction) = transientScopeNode.ScopedRoots.First(sr => CustomSymbolEqualityComparer.Default.Equals(sr.TypeNode.Type, rootNode.Type));
                    var transientScopeReference = referenceGenerator.Generate("transientScope");
                    var calledFunction = sync
                        ? syncFunction
                        : asyncFunction;
                    var await = !sync ? "await " : "";
                    return $$"""
                             {{transientScopeName}} {{transientScopeReference}} = new {{transientScopeName}}() { {{ScopeNodeToContainerPropertyReference[transientScopeNode]}} = ({{containerInfo.FullName}}) {{contextGenerator.ParameterName}}.{{contextGenerator.ContainerNodePropertyName}} };
                             return {{await}}{{transientScopeReference}}.{{functionUtility.GenerateFunctionCall(calledFunction, doScopedInstance: true, doScopeRoot: true)}};
                             """;
                }
                case ScopeNodeContext.Scope { ScopeName: var scopeName }:
                {
                    var scopeNode = scopeNodeManager.Scopes.First(s => s.Name == scopeName);
                    var (_, syncFunction, asyncFunction) = scopeNode.ScopedRoots.First(sr => CustomSymbolEqualityComparer.Default.Equals(sr.TypeNode.Type, rootNode.Type));
                    var scopeReference = referenceGenerator.Generate("scope");
                    var calledFunction = sync
                        ? syncFunction
                        : asyncFunction;
                    var await = !sync ? "await " : "";
                    return $$"""
                             {{scopeName}} {{scopeReference}} = new {{scopeName}}() { {{ScopeNodeToContainerPropertyReference[scopeNode]}} = ({{containerInfo.FullName}}) {{contextGenerator.ParameterName}}.{{contextGenerator.ContainerNodePropertyName}} };
                             return {{await}}{{scopeReference}}.{{functionUtility.GenerateFunctionCall(calledFunction, doScopedInstance: true, doScopeRoot: true)}};
                             """;
                }
                default:
                    throw new ArgumentException($"Parameter should be either {nameof(ScopeNodeContext.TransientScope)} or {nameof(ScopeNodeContext.Scope)} here, but is {scopeRootContext.GetType().FullName}", nameof(scopeRootContext));
                    
            }

        }
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
            return $"return (({scopedInstanceInterfaceDescription.InterfaceName}<{rootNode.Type}>) {contextGenerator.ParameterName}.{contextProperty}).{functionUtility.GenerateFunctionCall(calledFunction, doScopedInstance: true, doScopeRoot: true)};";
        }
    }
}