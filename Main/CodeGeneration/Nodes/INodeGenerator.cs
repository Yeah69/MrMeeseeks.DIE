namespace MrMeeseeks.DIE.CodeGeneration.Nodes;

internal interface INodeGenerator
{
    void Generate(StringBuilder code, ICodeGenerationVisitor visitor);
}