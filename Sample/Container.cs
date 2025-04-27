using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;

//*
internal class Class : IContainerInstance
{
    public async Task InitializeAsync()
    {
        await Task.Delay(1000);
    }
}

internal class SyncClass : IContainerInstance;

internal class ParentClass
{
    internal ParentClass(Func<Task<Class>> classInstance, SyncClass syncClassInstance, object obj) {}
}

[CreateFunction(typeof(ParentClass), "Create")]
[CreateFunction(typeof(SyncClass), "CreateSync")]
internal sealed partial class Container
{
    private Task<object> DIE_Factory_Object = new(() => new object());
}
//*/