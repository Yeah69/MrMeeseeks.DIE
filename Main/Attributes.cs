namespace MrMeeseeks.DIE;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class SpyAttribute : Attribute
{
    // ReSharper disable once UnusedParameter.Local *** Is used in the generator
    public SpyAttribute(params Type[] type) {}
}
    
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class TransientAttribute : Attribute
{
    // ReSharper disable once UnusedParameter.Local *** Is used in the generator
    public TransientAttribute(params Type[] type) {}
}
    
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class SingleInstanceAttribute : Attribute
{
    // ReSharper disable once UnusedParameter.Local *** Is used in the generator
    public SingleInstanceAttribute(params Type[] type) {}
}