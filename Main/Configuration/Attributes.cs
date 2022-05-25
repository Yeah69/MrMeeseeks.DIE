namespace MrMeeseeks.DIE.Configuration;

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
public class GenericParameterSubstitutesChoiceAttribute : Attribute
{
    public GenericParameterSubstitutesChoiceAttribute(Type unboundGenericType, string genericArgumentName, params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterGenericParameterSubstitutesChoiceAttribute : Attribute
{
    public FilterGenericParameterSubstitutesChoiceAttribute(Type unboundGenericType, string genericArgumentName) {}
}
    
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class GenericParameterChoiceAttribute : Attribute
{
    public GenericParameterChoiceAttribute(Type unboundGenericType, string genericArgumentName, Type chosenType) {}
}
    
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterGenericParameterChoiceAttribute : Attribute
{
    public FilterGenericParameterChoiceAttribute(Type unboundGenericType, string genericArgumentName) {}
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

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class PropertyChoiceAttribute : Attribute
{
    public PropertyChoiceAttribute(Type implementationType, params string[] propertyName)
    {
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterPropertyChoiceAttribute : Attribute
{
    public FilterPropertyChoiceAttribute(Type implementationType)
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
public class ImplementationChoiceAttribute : Attribute
{
    public ImplementationChoiceAttribute(Type type, Type implementationChoice) {}
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterImplementationChoiceAttribute : Attribute
{
    public FilterImplementationChoiceAttribute(Type type) {}
}

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class ImplementationCollectionChoiceAttribute : Attribute
{
    public ImplementationCollectionChoiceAttribute(Type type, params Type[] implementationChoice) {}
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterImplementationCollectionChoiceAttribute : Attribute
{
    public FilterImplementationCollectionChoiceAttribute(Type type) {}
}

[AttributeUsage(AttributeTargets.Assembly)]
public class ErrorDescriptionInsteadOfBuildFailureAttribute : Attribute
{
}
// ReSharper enable UnusedParameter.Local