using Microsoft.CodeAnalysis.CSharp;
using MrMeeseeks.DIE;

namespace DebugWorkaround;

public class Workaround
{
    [Fact(Skip = "")]
    public void Debug()
    {
        var generator = new SourceGenerator();

        var driver = CSharpGeneratorDriver.Create(generator);
        var cancellationToken =  CancellationToken.None;

        var compilation = CSharpCompilation.Create("Workaround",
            [
                CSharpSyntaxTree.ParseText(File.ReadAllText("/home/yeah69/Documents/GitRepositories/MrMeeseeks.DIE_Refactoring/Sample/Container.cs"), cancellationToken: cancellationToken),
                CSharpSyntaxTree.ParseText(File.ReadAllText("/home/yeah69/Documents/GitRepositories/MrMeeseeks.DIE_Refactoring/Sample/AssemblyInfo.cs"), cancellationToken: cancellationToken),
                CSharpSyntaxTree.ParseText(File.ReadAllText("/home/yeah69/Documents/GitRepositories/MrMeeseeks.DIE_Refactoring/Sample/Program.cs"), cancellationToken: cancellationToken),
            ],
            [
                Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(MrMeeseeks.DIE.Configuration.Attributes.InitializerAttribute).Assembly.Location),
                Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(DieExceptionKind).Assembly.Location),
                Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(System.Collections.Concurrent.ConcurrentBag<>).Assembly.Location),
                Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(LinkedList<>).Assembly.Location),
                Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
            ]);

        var runResult = driver.RunGenerators(compilation, cancellationToken).GetRunResult();
    }
}