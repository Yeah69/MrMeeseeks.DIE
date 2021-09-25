using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.CodeAnalysis;

namespace MrMeeseeks.DIE
{
    public interface IGetAssemblyAttributes
    {
        IReadOnlyList<AttributeData> AllAssemblyAttributes { get; }
    }

    internal class GetAssemblyAttributes : IGetAssemblyAttributes
    {
        private readonly GeneratorExecutionContext _context;

        public GetAssemblyAttributes(GeneratorExecutionContext context)
        {
            _context = context;
        }

        public IReadOnlyList<AttributeData> AllAssemblyAttributes => new ReadOnlyCollection<AttributeData>(
            _context.Compilation.Assembly.GetAttributes());
    }
}