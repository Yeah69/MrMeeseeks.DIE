namespace MrMeeseeks.DIE;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class SpyAggregationAttribute : Attribute
{
    // ReSharper disable once UnusedParameter.Local *** Is used in the generator
    public SpyAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class TransientAggregationAttribute : Attribute
{
    // ReSharper disable once UnusedParameter.Local *** Is used in the generator
    public TransientAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class SingleInstanceAggregationAttribute : Attribute
{
    // ReSharper disable once UnusedParameter.Local *** Is used in the generator
    public SingleInstanceAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class ScopedInstanceAggregationAttribute : Attribute
{
    // ReSharper disable once UnusedParameter.Local *** Is used in the generator
    public ScopedInstanceAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class ScopeRootAggregationAttribute : Attribute
{
    // ReSharper disable once UnusedParameter.Local *** Is used in the generator
    public ScopeRootAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class DecoratorAggregationAttribute : Attribute
{
    // ReSharper disable once UnusedParameter.Local *** Is used in the generator
    public DecoratorAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class CompositeAggregationAttribute : Attribute
{
    // ReSharper disable once UnusedParameter.Local *** Is used in the generator
    public CompositeAggregationAttribute(params Type[] types) {}
}
    
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class DecoratorSequenceChoiceAttribute : Attribute
{
    // ReSharper disable once UnusedParameter.Local *** Is used in the generator
    public DecoratorSequenceChoiceAttribute(Type decoratedType, params Type[] types) {}
}