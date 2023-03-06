// ReSharper disable UnusedParameter.Local

namespace MrMeeseeks.DIE.Configuration.Attributes;

/// <summary>
/// Aggregates all implementations of the current assembly and all referenced assemblies. This includes .Net assemblies and assemblies from nuget packages.
/// See https://die.mrmeeseeks.dev/configuration/implementations/
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = false)]
public class AllImplementationsAggregationAttribute : Attribute
{
}

/// <summary>
/// Filters all implementations.
/// See https://die.mrmeeseeks.dev/configuration/implementations/
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class FilterAllImplementationsAggregationAttribute : Attribute
{
}

/// <summary>
/// Aggregates all implementations of a given assembly. Assemblies are passed by referencing any type from the assembly, because assemblies themselves cannot be referenced in code. You may pass multiple types to this attribute.
/// See https://die.mrmeeseeks.dev/configuration/implementations/
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class AssemblyImplementationsAggregationAttribute : Attribute
{
    public AssemblyImplementationsAggregationAttribute(params Type[] typesFromAssemblies) {}
}

/// <summary>
/// Filters all implementations of a given assembly. Assemblies are passed by any type from the assembly because assemblies themselves cannot be referenced in code. You may pass multiple types to this attribute.
/// See https://die.mrmeeseeks.dev/configuration/implementations/
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterAssemblyImplementationsAggregationAttribute : Attribute
{
    public FilterAssemblyImplementationsAggregationAttribute(params Type[] typesFromAssemblies) {}
}

/// <summary>
/// Aggregates all given implementations. Types passed must be implementations. You may pass multiple types to this attribute.
/// See https://die.mrmeeseeks.dev/configuration/implementations/
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class ImplementationAggregationAttribute : Attribute
{
    public ImplementationAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// Filters all given implementations. Types passed must be implementations. You may pass multiple types to this attribute.
/// See https://die.mrmeeseeks.dev/configuration/implementations/
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterImplementationAggregationAttribute : Attribute
{
    public FilterImplementationAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// For any given abstraction, all of its implementations are completely ignored for disposal management.
/// See https://die.mrmeeseeks.dev/configuration/disposal/
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class TransientAbstractionAggregationAttribute : Attribute
{
    public TransientAbstractionAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// For each given abstraction, all of its implementations are considered again for disposal management.
/// See https://die.mrmeeseeks.dev/configuration/disposal/
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterTransientAbstractionAggregationAttribute : Attribute
{
    public FilterTransientAbstractionAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// Any given implementation is completely ignored for disposal management.
/// See https://die.mrmeeseeks.dev/configuration/disposal/
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class TransientImplementationAggregationAttribute : Attribute
{
    public TransientImplementationAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// Each given implementation will be considered again for disposal management.
/// See https://die.mrmeeseeks.dev/configuration/disposal/
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterTransientImplementationAggregationAttribute : Attribute
{
    public FilterTransientImplementationAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// For any given abstraction, all of its implementations are ignored for sync disposal management.
/// See https://die.mrmeeseeks.dev/configuration/disposal/
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class SyncTransientAbstractionAggregationAttribute : Attribute
{
    public SyncTransientAbstractionAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// For each given abstraction, all of its implementations are considered again for sync disposal management.
/// See https://die.mrmeeseeks.dev/configuration/disposal/
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterSyncTransientAbstractionAggregationAttribute : Attribute
{
    public FilterSyncTransientAbstractionAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// Any given implementation will be ignored for sync disposal management.
/// See https://die.mrmeeseeks.dev/configuration/disposal/
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class SyncTransientImplementationAggregationAttribute : Attribute
{
    public SyncTransientImplementationAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// Any given implementation will be considered again for sync disposal management.
/// See https://die.mrmeeseeks.dev/configuration/disposal/
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterSyncTransientImplementationAggregationAttribute : Attribute
{
    public FilterSyncTransientImplementationAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// For any given abstraction, all of its implementations are ignored for async disposal management.
/// See https://die.mrmeeseeks.dev/configuration/disposal/
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class AsyncTransientAbstractionAggregationAttribute : Attribute
{
    public AsyncTransientAbstractionAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// For each given abstraction, all of its implementations are considered again for async disposal management.
/// See https://die.mrmeeseeks.dev/configuration/disposal/
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterAsyncTransientAbstractionAggregationAttribute : Attribute
{
    public FilterAsyncTransientAbstractionAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// Any given implementation is ignored for async disposal management.
/// See https://die.mrmeeseeks.dev/configuration/disposal/
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class AsyncTransientImplementationAggregationAttribute : Attribute
{
    public AsyncTransientImplementationAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// Any given implementation is considered for async disposal management.
/// See https://die.mrmeeseeks.dev/configuration/disposal/
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterAsyncTransientImplementationAggregationAttribute : Attribute
{
    public FilterAsyncTransientImplementationAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// For each given abstraction, all of its implementations are marked as scoped instances for the container.
/// See https://die.mrmeeseeks.dev/configuration/scoping/
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class ContainerInstanceAbstractionAggregationAttribute : Attribute
{
    public ContainerInstanceAbstractionAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// For each given abstraction, all of its implementations are discarded as scoped instances for the container.
/// See https://die.mrmeeseeks.dev/configuration/scoping/
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterContainerInstanceAbstractionAggregationAttribute : Attribute
{
    public FilterContainerInstanceAbstractionAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// Given implementations are marked as scoped instances for the container.
/// See https://die.mrmeeseeks.dev/configuration/scoping/
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class ContainerInstanceImplementationAggregationAttribute : Attribute
{
    public ContainerInstanceImplementationAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// Given implementations are discarded as scoped instances for the container.
/// See https://die.mrmeeseeks.dev/configuration/scoping/
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterContainerInstanceImplementationAggregationAttribute : Attribute
{
    public FilterContainerInstanceImplementationAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// For each given abstraction, all of its implementations are marked as scoped instances for transient scopes.
/// See https://die.mrmeeseeks.dev/configuration/scoping/
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class TransientScopeInstanceAbstractionAggregationAttribute : Attribute
{
    public TransientScopeInstanceAbstractionAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// For any given abstraction, all of its implementations are discarded as scoped instances for transient scopes.
/// See https://die.mrmeeseeks.dev/configuration/scoping/
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterTransientScopeInstanceAbstractionAggregationAttribute : Attribute
{
    public FilterTransientScopeInstanceAbstractionAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// Given implementations are marked as scoped instances for transient scopes.
/// See https://die.mrmeeseeks.dev/configuration/scoping/
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class TransientScopeInstanceImplementationAggregationAttribute : Attribute
{
    public TransientScopeInstanceImplementationAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// Given implementations are discarded as scoped instances for transient scopes.
/// See https://die.mrmeeseeks.dev/configuration/scoping/
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterTransientScopeInstanceImplementationAggregationAttribute : Attribute
{
    public FilterTransientScopeInstanceImplementationAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// For each given abstraction, all of its implementations are marked as scoped instances for scopes.
/// See https://die.mrmeeseeks.dev/configuration/scoping/
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class ScopeInstanceAbstractionAggregationAttribute : Attribute
{
    public ScopeInstanceAbstractionAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// Of any given abstraction, all of its implementations are discarded as scoped instances for scopes.
/// See https://die.mrmeeseeks.dev/configuration/scoping/
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterScopeInstanceAbstractionAggregationAttribute : Attribute
{
    public FilterScopeInstanceAbstractionAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// Given implementations are marked as scoped instances for scopes.
/// See https://die.mrmeeseeks.dev/configuration/scoping/
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class ScopeInstanceImplementationAggregationAttribute : Attribute
{
    public ScopeInstanceImplementationAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// Given implementations are discarded as scoped instances for scopes.
/// See https://die.mrmeeseeks.dev/configuration/scoping/
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterScopeInstanceImplementationAggregationAttribute : Attribute
{
    public FilterScopeInstanceImplementationAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// For each given abstraction, all of its implementations are marked as scope roots for transient scopes.
/// See https://die.mrmeeseeks.dev/configuration/scoping/
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class TransientScopeRootAbstractionAggregationAttribute : Attribute
{
    public TransientScopeRootAbstractionAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// For any given abstraction, all of its implementations are discarded as scope roots for transient scopes.
/// See https://die.mrmeeseeks.dev/configuration/scoping/
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterTransientScopeRootAbstractionAggregationAttribute : Attribute
{
    public FilterTransientScopeRootAbstractionAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// Given implementations are marked as scope roots for transient scopes.
/// See https://die.mrmeeseeks.dev/configuration/scoping/
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class TransientScopeRootImplementationAggregationAttribute : Attribute
{
    public TransientScopeRootImplementationAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// Given implementations are discarded as scope roots for transient scopes.
/// See https://die.mrmeeseeks.dev/configuration/scoping/
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterTransientScopeRootImplementationAggregationAttribute : Attribute
{
    public FilterTransientScopeRootImplementationAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// For each given abstraction, all of its implementations are marked as scope roots for scopes.
/// See https://die.mrmeeseeks.dev/configuration/scoping/
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class ScopeRootAbstractionAggregationAttribute : Attribute
{
    public ScopeRootAbstractionAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// For any given abstraction, all of its implementations are discarded as scope roots for scopes.
/// See https://die.mrmeeseeks.dev/configuration/scoping/
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterScopeRootAbstractionAggregationAttribute : Attribute
{
    public FilterScopeRootAbstractionAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// Given implementations are marked as scope roots for scopes.
/// See https://die.mrmeeseeks.dev/configuration/scoping/
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class ScopeRootImplementationAggregationAttribute : Attribute
{
    public ScopeRootImplementationAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// Given implementations are discarded as scope roots for scopes.
/// See https://die.mrmeeseeks.dev/configuration/scoping/
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterScopeRootImplementationAggregationAttribute : Attribute
{
    public FilterScopeRootImplementationAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// Of each given abstraction, all its implementations are interpreted as decorators. All abstractions are required to have a single generic type parameter.
/// See https://die.mrmeeseeks.dev/configuration/decorator-composite/
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class DecoratorAbstractionAggregationAttribute : Attribute
{
    public DecoratorAbstractionAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// Of each given abstraction, all its implementations are stopped being interpreted as decorators. All abstractions are required to have a single generic type parameter.
/// See https://die.mrmeeseeks.dev/configuration/decorator-composite/
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterDecoratorAbstractionAggregationAttribute : Attribute
{
    public FilterDecoratorAbstractionAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// Of each given abstraction, all its implementations are interpreted as composites. All abstractions are required to have a single generic type parameter.
/// See https://die.mrmeeseeks.dev/configuration/decorator-composite/
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class CompositeAbstractionAggregationAttribute : Attribute
{
    public CompositeAbstractionAggregationAttribute(params Type[] types) {}
}

/// <summary>
/// Of each given abstraction, all its implementations are interpreted as composites. All abstractions are required to have a single generic type parameter.
/// See https://die.mrmeeseeks.dev/configuration/decorator-composite/
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterCompositeAbstractionAggregationAttribute : Attribute
{
    public FilterCompositeAbstractionAggregationAttribute(params Type[] types) {}
}