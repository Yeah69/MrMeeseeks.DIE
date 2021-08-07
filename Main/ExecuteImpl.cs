using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using MrMeeseeks.StaticDelegateGenerator;
using System.Linq;
using System.Text;

namespace MrMeeseeks.DIE
{
    internal interface IExecute 
    {
        void Execute();
    }

    internal class ExecuteImpl : IExecute
    {
        private readonly GeneratorExecutionContext context;
        private readonly ITypeToImplementationsMapper typeToImplementationMapper;

        public ExecuteImpl(
            GeneratorExecutionContext context,
            ITypeToImplementationsMapper typeToImplementationMapper)
        {
            this.context = context;
            this.typeToImplementationMapper = typeToImplementationMapper;
        }

        public void Execute()
        {
            context.ReportDiagnostic(Diagnostic.Create(
                   new DiagnosticDescriptor("DIE00", "INFO", "Start", "INFO", DiagnosticSeverity.Warning, true),
                   Location.None));
            if (context
                .Compilation
                .GetTypeByMetadataName(typeof(ContainerAttribute).FullName ?? "") is not { } attributeType)
                return;


            foreach (var attributeData in context
                .Compilation
                .Assembly
                .GetAttributes()
                .Where(ad => ad.AttributeClass?.Equals(attributeType, SymbolEqualityComparer.Default) ?? false))
            {
                var countConstructorArguments = attributeData.ConstructorArguments.Length;
                if (countConstructorArguments is not 1)
                {
                    // Invalid code, ignore
                    continue;
                }

                var typeConstant = attributeData.ConstructorArguments[0];
                if (typeConstant.Kind != TypedConstantKind.Type)
                {
                    // Invalid code, ignore
                    continue;
                }
                if (!CheckValidType(typeConstant, out var type))
                {
                    continue;
                }

                var containerClassName = $"{type.Name}Container";

                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("DIE01", "INFO", type.FullName(), "INFO", DiagnosticSeverity.Warning, true),
                    Location.None));

                var typeToInject = typeToImplementationMapper.Map(type).First();

                var generatedContainer = new StringBuilder();

                generatedContainer = generatedContainer
                    .AppendLine($"namespace MrMeeseeks.DIE")
                    .AppendLine($"{{")
                    .AppendLine($"    internal class {containerClassName}")
                    .AppendLine($"    {{")
                    .AppendLine($"        public {type.FullName()} Resolve() => new {typeToInject.FullName()}();")
                    .AppendLine($"    }}")
                    .AppendLine($"}}")
                    ;

                var containerSource = CSharpSyntaxTree
                        .ParseText(SourceText.From(generatedContainer.ToString(), Encoding.UTF8))
                        .GetRoot()
                        .NormalizeWhitespace()
                        .SyntaxTree
                        .GetText();
                context.AddSource($"{type.Name}.g.cs", containerSource);
            }
        }

        private bool CheckValidType(TypedConstant typedConstant, out INamedTypeSymbol type)
        {
            type = (typedConstant.Value as INamedTypeSymbol)!;
            if (typedConstant.Value is null)
                return false;
            if (type.IsOrReferencesErrorType())
                // we will report an error for this case anyway.
                return false;
            if (type.IsUnboundGenericType)
                return false;
            if (!type.IsAccessibleInternally())
                return false;

            return true;
        }
    }
}
