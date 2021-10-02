using MrMeeseeks.DIE;
using SampleChild;

[assembly:Spy(typeof(PublicTypes), typeof(InternalTypes))]

namespace MrMeeseeks.DIE.Sample
{
    internal partial class Container : IContainer<IContext>
    {
    }
}
