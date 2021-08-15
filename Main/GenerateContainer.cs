using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MrMeeseeks.DIE
{
    internal interface IContainerGenerator 
    {
        void Generate(AttributeData attributeData);
    }

    internal class ContainerGenrator : IContainerGenerator
    {
        private readonly GeneratorExecutionContext context;
        private readonly IDiagLogger diagLogger;
        private readonly ITypeToImplementationsMapper typeToImplementationMapper;

        public ContainerGenrator(
            GeneratorExecutionContext context,
            IDiagLogger diagLogger,
            ITypeToImplementationsMapper typeToImplementationMapper)
        {
            this.context = context;
            this.diagLogger = diagLogger;
            this.typeToImplementationMapper = typeToImplementationMapper;
        }

        public void Generate(AttributeData attributeData)
        {
            var countConstructorArguments = attributeData.ConstructorArguments.Length;
            if (countConstructorArguments is not 1)
            {
                // Invalid code, ignore
                return;
            }

            var typeConstant = attributeData.ConstructorArguments[0];
            if (typeConstant.Kind != TypedConstantKind.Type)
            {
                // Invalid code, ignore
                return;
            }
            if (!CheckValidType(typeConstant, out var type))
            {
                return;
            }
            var id = -1;
            var stack = new Stack<DependencyWrapper>();

            var containerClassName = $"{type.Name}Container";

            var typeToInject = typeToImplementationMapper.Map(type).First();

            stack.Push(new DependencyWrapper(ResolutionStage.Prefix, ++id, type, typeToInject, new List<int>()));

            var generatedContainer = new StringBuilder()
                .AppendLine($"namespace MrMeeseeks.DIE")
                .AppendLine($"{{")
                .AppendLine($"    internal class {containerClassName}")
                .AppendLine($"    {{")
                .AppendLine($"        public {type.FullName()} Resolve()")
                .AppendLine($"        {{");

            while (stack.Any())
            {
                var subject = stack.Pop();
                if (subject is { ResolutionStage: ResolutionStage.Prefix })
                {
                    var parameterIds = new List<int>();
                    stack.Push(subject with { ResolutionStage = ResolutionStage.Postfix, ParameterIds = parameterIds });
                    var ctor = subject.ImplementationType.Constructors.First();
                    foreach (var parameter in ctor.Parameters.Select(p => p.Type))
                    {
                        var namedParameter = (INamedTypeSymbol)parameter;
                        var typeToInjectParameter = typeToImplementationMapper.Map(namedParameter).First();
                        var parameterWrapper = new DependencyWrapper(ResolutionStage.Prefix, ++id, namedParameter, typeToInjectParameter, new List<int>());
                        stack.Push(parameterWrapper);
                        parameterIds.Add(parameterWrapper.Id);
                    }
                }
                else if (subject is { ResolutionStage: ResolutionStage.Postfix })
                {
                    generatedContainer = generatedContainer
                        .AppendLine($"            var _{subject.Id} = new {subject.ImplementationType.FullName()}({string.Join(", ", subject.ParameterIds.Select(id => $"_{id}"))});")
                        ;
                }//*/
            }

            generatedContainer = generatedContainer
                .AppendLine($"            return _0;")
                .AppendLine($"        }}")
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
