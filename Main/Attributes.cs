namespace MrMeeseeks.DIE;

// ReSharper disable UnusedParameter.Local *** The constructor parameters of the attributes will be used. Trust me, imma dev.
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class SpyAggregationAttribute : Attribute
{
    public SpyAggregationAttribute(params Type[] types) {}
}

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class SpyConstructorChoiceAggregationAttribute : Attribute
{
    public SpyConstructorChoiceAggregationAttribute(params Enum[] references) {}
}

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class ImplementationAggregationAttribute : Attribute
{
    public ImplementationAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class TransientAggregationAttribute : Attribute
{
    public TransientAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class ContainerInstanceAggregationAttribute : Attribute
{
    public ContainerInstanceAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class ScopeInstanceAggregationAttribute : Attribute
{
    public ScopeInstanceAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class ScopeRootAggregationAttribute : Attribute
{
    public ScopeRootAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class DecoratorAggregationAttribute : Attribute
{
    public DecoratorAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class CompositeAggregationAttribute : Attribute
{
    public CompositeAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class DecoratorSequenceChoiceAttribute : Attribute
{
    public DecoratorSequenceChoiceAttribute(Type decoratedType, params Type[] types) {}
}

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class ConstructorChoiceAttribute : Attribute
{
    public ConstructorChoiceAttribute(Type implementationType, params Type[] parameterTypes)
    {
    }
}
// ReSharper enable UnusedParameter.Local