using System.Threading;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration;

internal sealed class NodeCodeGenerator(
    FunctionUtility functionUtility,
    ContainerInfo containerInfo,
    ScopedInstanceInterfaceDescription scopedInstanceInterfaceDescription,
    ContextGenerator contextGenerator,
    ReferenceGenerator referenceGenerator, 
    WellKnownTypes wellKnownTypes)
{
    internal ImmutableArray<string> GetInheritanceHeaderElements(ScopeNode scopeNode, bool isContainer)
    {
        var prefix = isContainer ? $"{containerInfo.Name}." : string.Empty;
        return [..scopeNode.ScopedInstances.Select(si => 
            $"{prefix}{scopedInstanceInterfaceDescription.InterfaceName}<{si.TypeNode.Type.FullName()}>")];
    }
    
    internal void GenerateInterface(StringBuilder code)
    {
        var typeParameterName = referenceGenerator.Generate("T");
        code.AppendLine($"private interface {scopedInstanceInterfaceDescription.InterfaceName}<{typeParameterName}>");
        code.AppendLine("{");
        code.AppendLine($"{typeParameterName} {ScopedInstanceInterfaceDescription.FunctionName}({contextGenerator.FullNameAndParameterName}, {wellKnownTypes.Boolean.FullName()} {functionUtility.DoScopedInstanceParameterName});");
        code.AppendLine("}");
        code.AppendLine();
    }
    
    internal void GenerateFunctions(StringBuilder code, ScopeNode scopeNode, string containerReference)
    {
        var semaphoreSlimFullName = wellKnownTypes.SemaphoreSlim.FullName();
        var objectFullName = wellKnownTypes.Object.FullName();

        foreach ( var scopedInstance in scopeNode.ScopedInstances)
        {
            var typeSymbol = scopedInstance.TypeNode.Type;
            var function = scopedInstance.Function;
            var scopedInstanceFieldReference = referenceGenerator.Generate("_scopedInstanceField", typeSymbol);
            var scopedInstanceLockFieldReference = referenceGenerator.Generate("_scopedInstanceLock", typeSymbol);
            code.AppendLine($"private {typeSymbol.FullName()}? {scopedInstanceFieldReference};");
            code.AppendLine($"private {semaphoreSlimFullName}? {scopedInstanceLockFieldReference} = new {semaphoreSlimFullName}(1);");
            
            code.AppendLine(functionUtility.GenerateHeader(function));
            
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
            
            var calledCreateFunction = scopedInstance.TypeNode.Incoming.Where(te => te.Type is FunctionEdgeType)
                .Select(te => ((FunctionEdgeType) te.Type).Function)
                .First();
            code.AppendLine($"{Constants.ThisKeyword}.{scopedInstanceFieldReference} = {containerReference}.{functionUtility.GenerateFunctionCall(calledCreateFunction, doScopedInstance: false)};");

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