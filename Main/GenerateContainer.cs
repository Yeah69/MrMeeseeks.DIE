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
        void Generate(INamedTypeSymbol containerClass);
    }

    internal class ContainerGenerator : IContainerGenerator
    {
        private readonly GeneratorExecutionContext _context;
        private readonly IDiagLogger _diagLogger;
        private readonly WellKnownTypes _wellKnownTypes;
        private readonly ITypeToImplementationsMapper _typeToImplementationsMapper;

        public ContainerGenerator(
            GeneratorExecutionContext context,
            IDiagLogger diagLogger,
            WellKnownTypes wellKnownTypes,
            ITypeToImplementationsMapper typeToImplementationsMapper)
        {
            _context = context;
            _diagLogger = diagLogger;
            _wellKnownTypes = wellKnownTypes;
            _typeToImplementationsMapper = typeToImplementationsMapper;
        }

        public void Generate(INamedTypeSymbol containerClass)
        {
            var namedTypeSymbol = containerClass.AllInterfaces.Single(x => x.OriginalDefinition.Equals(_wellKnownTypes.Container, SymbolEqualityComparer.Default));
            _diagLogger.Log($"Interface type {namedTypeSymbol.FullName()}");
            var typeParameterSymbol = namedTypeSymbol.TypeArguments.Single();
            _diagLogger.Log($"Generic type {typeParameterSymbol.FullName()}");
            if (typeParameterSymbol is not INamedTypeSymbol { } type 
                || type.IsUnboundGenericType
                || !type.IsAccessibleInternally())
            {
                _diagLogger.Log($"return generation");
                return;
            }

            var typeToInject = _typeToImplementationsMapper.Map(type).First();

            var generatedContainer = new StringBuilder()
                .AppendLine($"namespace MrMeeseeks.DIE")
                .AppendLine($"{{")
                .AppendLine($"    internal partial class {containerClass.Name}")
                .AppendLine($"    {{");

            generatedContainer = GenerateResolveFunction(generatedContainer, type, typeToInject, _typeToImplementationsMapper);

            generatedContainer = generatedContainer
                .AppendLine($"            return _0;")
                .AppendLine($"        }}");

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
            _context.AddSource($"{type.Name}.g.cs", containerSource);

            static StringBuilder GenerateResolveFunction(
                StringBuilder stringBuilder, 
                INamedTypeSymbol namedTypeSymbol,
                INamedTypeSymbol typeToInject,
                ITypeToImplementationsMapper typeToImplementationsMapper)
            {
                stringBuilder = stringBuilder
                    .AppendLine($"        public {namedTypeSymbol.FullName()} Resolve()")
                    .AppendLine($"        {{");

                var id = -1;
                var stack = new Stack<DependencyWrapper>();
                stack.Push(new DependencyWrapper(ResolutionStage.Prefix, ++id, namedTypeSymbol, typeToInject, new List<int>()));
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
                            var typeToInjectParameter = typeToImplementationsMapper.Map(namedParameter).First();
                            var parameterWrapper = new DependencyWrapper(ResolutionStage.Prefix, ++id, namedParameter,
                                typeToInjectParameter, new List<int>());
                            stack.Push(parameterWrapper);
                            parameterIds.Add(parameterWrapper.Id);
                        }
                    }
                    else if (subject is { ResolutionStage: ResolutionStage.Postfix })
                    {
                        stringBuilder = stringBuilder
                                .AppendLine(
                                    $"            var _{subject.Id} = new {subject.ImplementationType.FullName()}({string.Join(", ", subject.ParameterIds.Select(id => $"_{id}"))});")
                            ;
                    }
                }

                return stringBuilder;
            }
        }
    }
}
