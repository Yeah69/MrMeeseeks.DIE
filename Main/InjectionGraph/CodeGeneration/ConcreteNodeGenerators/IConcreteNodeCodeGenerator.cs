using MrMeeseeks.DIE.InjectionGraph.Nodes;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration.ConcreteNodeGenerators;

internal interface IConcreteNodeCodeGenerator<in TNode> where TNode : IConcreteNode
{
    /// <summary>
    /// Generates the code for the concrete node type and returns the used reference.
    /// </summary>
    /// <param name="code">Here the generated code is added to.</param>
    /// <param name="typeNode"></param>
    /// <param name="concreteNode"></param>
    /// <param name="sync"></param>
    /// <param name="reference">If set, then this component assumes the reference is declared already. If not set, then this component will declare it.</param>
    /// <returns>The used reference.</returns>
    string Generate(StringBuilder code, TypeNode typeNode, TNode concreteNode, bool sync, string? reference = null);
}
