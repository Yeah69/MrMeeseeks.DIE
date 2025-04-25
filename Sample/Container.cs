using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;

internal sealed class Dependency : IValueTaskInitializer
{
    public ValueTask InitializeAsync() => default;
}

internal sealed class OuterDependency
{
    internal OuterDependency(
        // ReSharper disable once UnusedParameter.Local
        Dependency dependency)
    {
        
    }
}

[CreateFunction(typeof(Lazy<ValueTask<Task<ValueTask<Task<OuterDependency>>>>>), "Create")]
internal sealed partial class Container;

/*
internal class Class : ITaskInitializer, IContainerInstance
{
    public async Task InitializeAsync()
    {
        await Task.Delay(1000);
    }
}

internal class SyncClass : IContainerInstance;

internal class ParentClass
{
    internal ParentClass(Class classInstance, SyncClass syncClassInstance) {}
}

[CreateFunction(typeof(ParentClass), "Create")]
[CreateFunction(typeof(SyncClass), "CreateSync")]
internal sealed partial class Container;
//*/