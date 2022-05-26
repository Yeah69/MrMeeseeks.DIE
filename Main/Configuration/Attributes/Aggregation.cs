namespace MrMeeseeks.DIE.Configuration.Attributes;

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = false)]
public class AllImplementationsAggregationAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class FilterAllImplementationsAggregationAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class AssemblyImplementationsAggregationAttribute : Attribute
{
    public AssemblyImplementationsAggregationAttribute(params Type[] typesFromAssemblies) {}
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterAssemblyImplementationsAggregationAttribute : Attribute
{
    public FilterAssemblyImplementationsAggregationAttribute(params Type[] typesFromAssemblies) {}
}

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class ImplementationAggregationAttribute : Attribute
{
    public ImplementationAggregationAttribute(params Type[] types) {}
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterImplementationAggregationAttribute : Attribute
{
    public FilterImplementationAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class TransientAbstractionAggregationAttribute : Attribute
{
    public TransientAbstractionAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterTransientAbstractionAggregationAttribute : Attribute
{
    public FilterTransientAbstractionAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class TransientImplementationAggregationAttribute : Attribute
{
    public TransientImplementationAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterTransientImplementationAggregationAttribute : Attribute
{
    public FilterTransientImplementationAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class SyncTransientAbstractionAggregationAttribute : Attribute
{
    public SyncTransientAbstractionAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterSyncTransientAbstractionAggregationAttribute : Attribute
{
    public FilterSyncTransientAbstractionAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class SyncTransientImplementationAggregationAttribute : Attribute
{
    public SyncTransientImplementationAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterSyncTransientImplementationAggregationAttribute : Attribute
{
    public FilterSyncTransientImplementationAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class AsyncTransientAbstractionAggregationAttribute : Attribute
{
    public AsyncTransientAbstractionAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterAsyncTransientAbstractionAggregationAttribute : Attribute
{
    public FilterAsyncTransientAbstractionAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class AsyncTransientImplementationAggregationAttribute : Attribute
{
    public AsyncTransientImplementationAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterAsyncTransientImplementationAggregationAttribute : Attribute
{
    public FilterAsyncTransientImplementationAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class ContainerInstanceAbstractionAggregationAttribute : Attribute
{
    public ContainerInstanceAbstractionAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterContainerInstanceAbstractionAggregationAttribute : Attribute
{
    public FilterContainerInstanceAbstractionAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class ContainerInstanceImplementationAggregationAttribute : Attribute
{
    public ContainerInstanceImplementationAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterContainerInstanceImplementationAggregationAttribute : Attribute
{
    public FilterContainerInstanceImplementationAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class TransientScopeInstanceAbstractionAggregationAttribute : Attribute
{
    public TransientScopeInstanceAbstractionAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterTransientScopeInstanceAbstractionAggregationAttribute : Attribute
{
    public FilterTransientScopeInstanceAbstractionAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class TransientScopeInstanceImplementationAggregationAttribute : Attribute
{
    public TransientScopeInstanceImplementationAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterTransientScopeInstanceImplementationAggregationAttribute : Attribute
{
    public FilterTransientScopeInstanceImplementationAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class ScopeInstanceAbstractionAggregationAttribute : Attribute
{
    public ScopeInstanceAbstractionAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterScopeInstanceAbstractionAggregationAttribute : Attribute
{
    public FilterScopeInstanceAbstractionAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class ScopeInstanceAbstractionImplementationAttribute : Attribute
{
    public ScopeInstanceAbstractionImplementationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterScopeInstanceImplementationAggregationAttribute : Attribute
{
    public FilterScopeInstanceImplementationAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class TransientScopeRootAbstractionAggregationAttribute : Attribute
{
    public TransientScopeRootAbstractionAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterTransientScopeRootAbstractionAggregationAttribute : Attribute
{
    public FilterTransientScopeRootAbstractionAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class TransientScopeRootImplementationAggregationAttribute : Attribute
{
    public TransientScopeRootImplementationAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterTransientScopeRootImplementationAggregationAttribute : Attribute
{
    public FilterTransientScopeRootImplementationAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class ScopeRootAbstractionAggregationAttribute : Attribute
{
    public ScopeRootAbstractionAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterScopeRootAbstractionAggregationAttribute : Attribute
{
    public FilterScopeRootAbstractionAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class ScopeRootImplementationAggregationAttribute : Attribute
{
    public ScopeRootImplementationAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterScopeRootImplementationAggregationAttribute : Attribute
{
    public FilterScopeRootImplementationAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class DecoratorAbstractionAggregationAttribute : Attribute
{
    public DecoratorAbstractionAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterDecoratorAbstractionAggregationAttribute : Attribute
{
    public FilterDecoratorAbstractionAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class CompositeAbstractionAggregationAttribute : Attribute
{
    public CompositeAbstractionAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterCompositeAbstractionAggregationAttribute : Attribute
{
    public FilterCompositeAbstractionAggregationAttribute(params Type[] types) {}
}