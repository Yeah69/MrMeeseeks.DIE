using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;

internal interface IDependency
{
    bool IsInitialized { get; }
}
    
internal abstract class AsyncDependencyBase : IDependency, ITaskInitializer
{
    public bool IsInitialized { get; private set; }
    
    async Task ITaskInitializer.InitializeAsync()
    {
        await Task.Delay(500);
        IsInitialized = true;
    }
}

internal sealed class AsyncDependencyA : AsyncDependencyBase;

internal sealed class AsyncDependencyB : AsyncDependencyBase;

internal sealed class AsyncDependencyC : AsyncDependencyBase;

internal sealed class AsyncDependencyD : AsyncDependencyBase;

public sealed partial class MixedSynchronicityScopes
{

    internal sealed class Parent
    {
        internal required IAsyncEnumerable<IDependency> Dependencies { get; init; }
        internal required ValueTask<AsyncDependencyD> Dependency { get; init; }
    }

    [ImplementationCollectionChoice(typeof(IDependency), typeof(AsyncDependencyA), typeof(AsyncDependencyB), typeof(AsyncDependencyC))]
    [CreateFunction(typeof(Parent), "Create")]
    internal sealed partial class Container;
}//*/
