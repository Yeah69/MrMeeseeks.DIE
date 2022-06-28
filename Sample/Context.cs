using System;
using System.IO;
using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Test.ConstructorChoice.WithParameter;

[ImplementationAggregation(typeof(FileInfo))]
[ConstructorChoice(typeof(FileInfo), typeof(string))]
[CreateFunction(typeof(Func<string, FileInfo>), "Create")]
internal sealed partial class Container
{
    
}