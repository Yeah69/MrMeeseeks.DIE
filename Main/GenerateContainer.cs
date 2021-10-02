using System;
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

            generatedContainer = GenerateResolveFunctionAlternative(generatedContainer, resolutionBase);

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

            static StringBuilder GenerateResolveFunctionAlternative(
                StringBuilder stringBuilder,
                ResolutionBase resolution)
            {
                switch (resolution)
                {
                    case InterfaceResolution interfaceResolution:
                        stringBuilder = GenerateResolveFunctionAlternative(stringBuilder, interfaceResolution.Dependency);
                        stringBuilder = stringBuilder.AppendLine(
                            $"var {interfaceResolution.Reference} = ({interfaceResolution.TypeFullName}) {interfaceResolution.Dependency.Reference};");              
                        break;
                    case ConstructorResolution constructorResolution:
                        foreach (var parameterResolution in constructorResolution.Dependencies)
                        {
                            stringBuilder = GenerateResolveFunctionAlternative(stringBuilder, parameterResolution);
                        }
                        stringBuilder = stringBuilder.AppendLine(
                            $"var {constructorResolution.Reference} = new {constructorResolution.TypeFullName}({string.Join(", ", constructorResolution.Dependencies.Select(d => d.Reference))});");
                        break;
                    default:
                        throw new Exception("Unexpected case or not implemented.");
                }

                return stringBuilder;
            }
        }
    }
}
