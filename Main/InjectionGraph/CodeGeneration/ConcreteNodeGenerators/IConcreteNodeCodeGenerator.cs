using MrMeeseeks.DIE.InjectionGraph.Nodes;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration.ConcreteNodeGenerators;

internal interface IConcreteNodeCodeGenerator<in TNode> where TNode : IConcreteNode
{
    string Generate(StringBuilder code, TypeNode typeNode, TNode concreteNode);
}
