namespace MrMeeseeks.DIE.Configuration.Attributes;

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

[AttributeUsage(AttributeTargets.Assembly)]
public class ErrorDescriptionInsteadOfBuildFailureAttribute : Attribute
{
}
// ReSharper enable UnusedParameter.Local