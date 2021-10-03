using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;

namespace MrMeeseeks.DIE
{
    internal interface IContainerGenerator 
    {
        void Generate(IContainerInfo containerInfo, ResolutionBase resolutionBase);
    }

    internal class ContainerGenerator : IContainerGenerator
    {
        private readonly GeneratorExecutionContext _context;
        private readonly IDiagLogger _diagLogger;

        public ContainerGenerator(
            GeneratorExecutionContext context,
            IDiagLogger diagLogger)
        {
            _context = context;
            _diagLogger = diagLogger;
        }

        public void Generate(IContainerInfo containerInfo, ResolutionBase resolutionBase)
        {
            if (!containerInfo.IsValid || containerInfo.ResolutionRootType is null)
            {
                _diagLogger.Log($"return generation");
                return;
            }
            
            var generatedContainer = new StringBuilder()
                .AppendLine($"namespace {containerInfo.Namespace}")
                .AppendLine($"{{")
                .AppendLine($"partial class {containerInfo.Name}")
                .AppendLine($"{{")
                .AppendLine($"public {resolutionBase.TypeFullName} Resolve()")
                .AppendLine($"{{");

            generatedContainer = GenerateResolveFunctionFields(generatedContainer, resolutionBase);

            generatedContainer = GenerateResolveFunction(generatedContainer, resolutionBase);

            generatedContainer = generatedContainer
                .AppendLine($"return {resolutionBase.Reference};")
                .AppendLine($"}}");

            generatedContainer = generatedContainer
                .AppendLine($"}}")
                .AppendLine($"}}")
                ;

            var containerSource = CSharpSyntaxTree
                    .ParseText(SourceText.From(generatedContainer.ToString(), Encoding.UTF8))
                    .GetRoot()
                    .NormalizeWhitespace()
                    .SyntaxTree
                    .GetText();
            _context.AddSource($"{containerInfo.Namespace}.{containerInfo.Name}.g.cs", containerSource);

            static StringBuilder GenerateResolveFunctionFields(
                StringBuilder stringBuilder,
                ResolutionBase resolution)
            {
                switch (resolution)
                {
                    case InterfaceResolution(var reference, var typeFullName, var resolutionBase):
                        stringBuilder = GenerateResolveFunctionFields(stringBuilder, resolutionBase);
                        stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};");              
                        break;
                    case ConstructorResolution(var reference, var typeFullName, var parameters):
                        stringBuilder = parameters.Aggregate(stringBuilder,
                            (builder, tuple) => GenerateResolveFunctionFields(builder, tuple.Dependency));
                        stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};");
                        break;
                    default:
                        throw new Exception("Unexpected case or not implemented.");
                }

                return stringBuilder;
            }

            static StringBuilder GenerateResolveFunction(
                StringBuilder stringBuilder,
                ResolutionBase resolution)
            {
                switch (resolution)
                {
                    case InterfaceResolution(var reference, var typeFullName, var resolutionBase):
                        stringBuilder = GenerateResolveFunction(stringBuilder, resolutionBase);
                        stringBuilder = stringBuilder.AppendLine(
                            $"{reference} = ({typeFullName}) {resolutionBase.Reference};");              
                        break;
                    case ConstructorResolution(var reference, var typeFullName, var parameters):
                        stringBuilder = parameters.Aggregate(stringBuilder,
                            (builder, tuple) => GenerateResolveFunction(builder, tuple.Dependency));
                        stringBuilder = stringBuilder.AppendLine(
                            $"{reference} = new {typeFullName}({string.Join(", ", parameters.Select(d => $"{d.name}: {d.Dependency.Reference}"))});");
                        break;
                    default:
                        throw new Exception("Unexpected case or not implemented.");
                }

                return stringBuilder;
            }
        }
    }
}
