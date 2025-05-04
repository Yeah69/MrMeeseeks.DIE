using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;

//*
internal class Class : IContainerInstance, ITaskInitializer
{
    public async Task InitializeAsync()
    {
        await Task.Delay(1000);
    }
}

[CreateFunction(typeof(Func<Class>), "Create")]
internal sealed partial class Container;
//*/