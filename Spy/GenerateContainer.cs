using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace MrMeeseeks.DIE.Spy
{
    internal interface IContainerGenerator
    {
        void Generate(string namespaceName);
    }

    internal class ContainerGenerator : IContainerGenerator
    {
        private readonly GeneratorExecutionContext context;
        private readonly IGetAllImplementations getAllImplementations;

        public ContainerGenerator(
            GeneratorExecutionContext context,
            IGetAllImplementations getAllImplementations)
        {
            this.context = context;
            this.getAllImplementations = getAllImplementations;
        }

        public void Generate(string namespaceName)
        {
            var generatedContainer = new StringBuilder()
                .AppendLine($"namespace {namespaceName}")
                .AppendLine($"{{")
                .AppendLine($"    public class PublicTypes")
                .AppendLine($"    {{")
                ;

            var i = -1;
            foreach (var type in getAllImplementations.AllImplementations.Where(t => t.DeclaredAccessibility == Accessibility.Public))
            {
                generatedContainer = generatedContainer
                    .AppendLine($"        public {type.FullName()} Type{++i} {{ get; }}")
                    ;
            }

            generatedContainer = generatedContainer
                .AppendLine($"    }}")
                .AppendLine($"    internal class InternalTypes")
                .AppendLine($"    {{")
                ;
            
            i = -1;
            foreach (var type in getAllImplementations.AllImplementations.Where(t => t.DeclaredAccessibility == Accessibility.Internal))
            {
                generatedContainer = generatedContainer
                    .AppendLine($"        internal {type.FullName()} Type{++i} {{ get; }}")
                    ;
            }

            generatedContainer = generatedContainer
                .AppendLine($"    }}")
                .AppendLine($"}}")
                ;

            var containerSource = CSharpSyntaxTree
                .ParseText(SourceText.From(generatedContainer.ToString(), Encoding.UTF8))
                .GetRoot()
                .NormalizeWhitespace()
                .SyntaxTree
                .GetText();
            context.AddSource($"PublicTypes.g.cs", containerSource);
        }
    }
}
