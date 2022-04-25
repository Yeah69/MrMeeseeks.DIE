namespace MrMeeseeks.DIE.Configuration;

// ReSharper disable UnusedParameter.Local *** The constructor parameters of the attributes will be used. Trust me, imma dev.
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class SpyAggregationAttribute : Attribute
{
    public SpyAggregationAttribute(params Type[] types) {}
}

// ReSharper disable UnusedParameter.Local *** The constructor parameters of the attributes will be used. Trust me, imma dev.
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterSpyAggregationAttribute : Attribute
{
    public FilterSpyAggregationAttribute(params Type[] types) {}
}

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class SpyConstructorChoiceAggregationAttribute : Attribute
{
    public SpyConstructorChoiceAggregationAttribute(params Type[] references) {}
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterSpyConstructorChoiceAggregationAttribute : Attribute
{
    public FilterSpyConstructorChoiceAggregationAttribute(params Type[] references) {}
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
public class TransientAggregationAttribute : Attribute
{
    public TransientAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterTransientAggregationAttribute : Attribute
{
    public FilterTransientAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class SyncTransientAggregationAttribute : Attribute
{
    public SyncTransientAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterSyncTransientAggregationAttribute : Attribute
{
    public FilterSyncTransientAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class AsyncTransientAggregationAttribute : Attribute
{
    public AsyncTransientAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterAsyncTransientAggregationAttribute : Attribute
{
    public FilterAsyncTransientAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class ContainerInstanceAggregationAttribute : Attribute
{
    public ContainerInstanceAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterContainerInstanceAggregationAttribute : Attribute
{
    public FilterContainerInstanceAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class TransientScopeInstanceAggregationAttribute : Attribute
{
    public TransientScopeInstanceAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterTransientScopeInstanceAggregationAttribute : Attribute
{
    public FilterTransientScopeInstanceAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class ScopeInstanceAggregationAttribute : Attribute
{
    public ScopeInstanceAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterScopeInstanceAggregationAttribute : Attribute
{
    public FilterScopeInstanceAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class TransientScopeRootAggregationAttribute : Attribute
{
    public TransientScopeRootAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterTransientScopeRootAggregationAttribute : Attribute
{
    public FilterTransientScopeRootAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class ScopeRootAggregationAttribute : Attribute
{
    public ScopeRootAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterScopeRootAggregationAttribute : Attribute
{
    public FilterScopeRootAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class DecoratorAggregationAttribute : Attribute
{
    public DecoratorAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterDecoratorAggregationAttribute : Attribute
{
    public FilterDecoratorAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class CompositeAggregationAttribute : Attribute
{
    public CompositeAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterCompositeAggregationAttribute : Attribute
{
    public FilterCompositeAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class GenericParameterSubstituteAggregationAttribute : Attribute
{
    public GenericParameterSubstituteAggregationAttribute(Type unboundGenericType, string genericArgumentName, params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterGenericParameterSubstituteAggregationAttribute : Attribute
{
    public FilterGenericParameterSubstituteAggregationAttribute(Type unboundGenericType, string genericArgumentName, params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class GenericParameterSubstituteChoiceAttribute : Attribute
{
    public GenericParameterSubstituteChoiceAttribute(Type unboundGenericType, string genericArgumentName, Type chosenType) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterGenericParameterSubstituteChoiceAttribute : Attribute
{
    public FilterGenericParameterSubstituteChoiceAttribute(Type unboundGenericType, string genericArgumentName) {}
}
    
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class DecoratorSequenceChoiceAttribute : Attribute
{
    public DecoratorSequenceChoiceAttribute(Type decoratedType, params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterDecoratorSequenceChoiceAttribute : Attribute
{
    public FilterDecoratorSequenceChoiceAttribute(Type decoratedType) {}
}

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class ConstructorChoiceAttribute : Attribute
{
    public ConstructorChoiceAttribute(Type implementationType, params Type[] parameterTypes)
    {
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterConstructorChoiceAttribute : Attribute
{
    public FilterConstructorChoiceAttribute(Type implementationType)
    {
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class CustomScopeForRootTypesAttribute : Attribute
{
    public CustomScopeForRootTypesAttribute(params Type[] types)
    {
    }
}

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class TypeInitializerAttribute : Attribute
{
    public TypeInitializerAttribute(Type type, string methodName)
    {
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterTypeInitializerAttribute : Attribute
{
    public FilterTypeInitializerAttribute(Type type)
    {
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class CreateFunctionAttribute : Attribute
{
    public CreateFunctionAttribute(Type type, string methodNamePrefix)
    {
    }
}
// ReSharper enable UnusedParameter.Local