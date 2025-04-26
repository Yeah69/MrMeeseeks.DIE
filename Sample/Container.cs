using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;
//*

internal interface IInterface
{
    bool IsInitialized { get; }
}

internal sealed class DependencyA : ITaskInitializer, IInterface
{
    public bool IsInitialized { get; private set; }
    
    async Task ITaskInitializer.InitializeAsync()
    {
        await Task.Delay(500);
        IsInitialized = true;
    }
}

internal sealed class DependencyB : IValueTaskInitializer, IInterface
{
    public bool IsInitialized { get; private set; }
    
    async ValueTask IValueTaskInitializer.InitializeAsync()
    {
        await Task.Delay(500);
        IsInitialized = true;
    }
}

internal sealed class DependencyC : IInitializer, IInterface
{
    public bool IsInitialized { get; private set; }
    
    void IInitializer.Initialize()
    {
        IsInitialized = true;
    }
}

internal sealed class DependencyD : IInterface
{
    public bool IsInitialized => true;
}

[CreateFunction(typeof(IReadOnlyList<ValueTask<IInterface>>), "Create")]
internal sealed partial class Container; //*/

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