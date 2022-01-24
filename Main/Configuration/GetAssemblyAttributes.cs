namespace MrMeeseeks.DIE.Configuration;

internal interface IGetAssemblyAttributes
{
    IReadOnlyList<AttributeData> AllAssemblyAttributes { get; }
}

internal class GetAssemblyAttributes : IGetAssemblyAttributes
{
    private readonly GeneratorExecutionContext _context;

    internal GetAssemblyAttributes(GeneratorExecutionContext context)
    {
        _context = context;
    }

    public IReadOnlyList<AttributeData> AllAssemblyAttributes => new ReadOnlyCollection<AttributeData>(
        _context.Compilation.Assembly.GetAttributes());
}