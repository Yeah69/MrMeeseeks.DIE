using MrMeeseeks.DIE;
using SampleChild;

[assembly:Spy(typeof(PublicTypes), typeof(InternalTypes))]

namespace MrMeeseeks.DIE.Sample
{
    public partial class Container : IContainer<IContext>
    {
    }
}
