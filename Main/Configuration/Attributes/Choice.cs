// ReSharper disable UnusedParameter.Local
namespace MrMeeseeks.DIE.Configuration.Attributes;
    
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