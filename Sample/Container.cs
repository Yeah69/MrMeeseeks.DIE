using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

internal class I
{
    internal ValueTask Initialize() => 
        ValueTask.CompletedTask;
}

internal class Parent
{
    internal required I i { get; init; }
}

//[Initializer(typeof(I), nameof(I.Initialize))]
[CreateFunction(typeof(ValueTask<Task<ValueTask<Parent>>>), "Create")]
internal sealed partial class Container
{
    
}
