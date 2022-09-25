using System.IO;
using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Test.UserDefinedElements.FactoryMethod.WithParameterInContainer;

[FilterAllImplementationsAggregation]
[ImplementationAggregation(typeof(FileInfo))]
[CreateFunction(typeof(FileInfo), "Create")]
internal sealed partial class Container
{
    private string DIE_Factory_Path => "C:\\Yeah.txt";
    private FileInfo DIE_Factory(string path) => new (path);
}