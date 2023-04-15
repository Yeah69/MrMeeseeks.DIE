using System.IO;

namespace MrMeeseeks.DIE.Utility;

internal interface IPaths
{
    string Analytics { get; }
    string AnalyticsResolutionGraph(string containerName);
}

internal class Paths : IPaths
{
    internal Paths(GeneratorExecutionContext context)
    {
        Analytics = Path.Combine(
            Path.GetDirectoryName(context.Compilation.SyntaxTrees.First(x => x.HasCompilationUnitRoot).FilePath) ?? "",
            $"_{Constants.DieAbbreviation}_Analytics");
    }

    public string Analytics { get; }
    
    public string AnalyticsResolutionGraph(string containerName) => 
        Path.Combine(Analytics, $"ResolutionGraph_{containerName}.puml");
}