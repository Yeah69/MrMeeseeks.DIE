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

            generatedContainer = GenerateResolutionFunction(generatedContainer, resolutionBase);

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



            static StringBuilder GenerateResolutionFunction(
                StringBuilder stringBuilder,
                ResolutionBase resolution)
            {
                stringBuilder = GenerateFields(stringBuilder, resolution);
                stringBuilder = GenerateResolutions(stringBuilder, resolution);
                
                return stringBuilder;
            }

            static StringBuilder GenerateFields(
                StringBuilder stringBuilder,
                ResolutionBase resolution)
            {
                switch (resolution)
                {
                    case InterfaceResolution(var reference, var typeFullName, var resolutionBase):
                        stringBuilder = GenerateFields(stringBuilder, resolutionBase);
                        stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};");              
                        break;
                    case ConstructorResolution(var reference, var typeFullName, var parameters):
                        stringBuilder = parameters.Aggregate(stringBuilder,
                            (builder, tuple) => GenerateFields(builder, tuple.Dependency));
                        stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};");
                        break;
                    case FuncResolution(var reference, var typeFullName, _):
                        stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};");
                        break;
                    default:
                        throw new Exception("Unexpected case or not implemented.");
                }

                return stringBuilder;
            }

            static StringBuilder GenerateResolutions(
                StringBuilder stringBuilder,
                ResolutionBase resolution)
            {
                switch (resolution)
                {
                    case InterfaceResolution(var reference, var typeFullName, var resolutionBase):
                        stringBuilder = GenerateResolutions(stringBuilder, resolutionBase);
                        stringBuilder = stringBuilder.AppendLine(
                            $"{reference} = ({typeFullName}) {resolutionBase.Reference};");              
                        break;
                    case ConstructorResolution(var reference, var typeFullName, var parameters):
                        stringBuilder = parameters.Aggregate(stringBuilder,
                            (builder, tuple) => GenerateResolutions(builder, tuple.Dependency));
                        stringBuilder = stringBuilder.AppendLine(
                            $"{reference} = new {typeFullName}({string.Join(", ", parameters.Select(d => $"{d.name}: {d.Dependency.Reference}"))});");
                        break;
                    case FuncResolution(var reference, var typeFullName, var resolutionBase):
                        stringBuilder = stringBuilder.AppendLine($"{reference} = () =>");
                        stringBuilder = stringBuilder.AppendLine($"{{");
                        GenerateResolutionFunction(stringBuilder, resolutionBase);
                        stringBuilder = stringBuilder.AppendLine($"return {resolutionBase.Reference};");
                        stringBuilder = stringBuilder.AppendLine($"}};");
                        break;
                    default:
                        throw new Exception("Unexpected case or not implemented.");
                }

                return stringBuilder;
            }
        }
    }
}
