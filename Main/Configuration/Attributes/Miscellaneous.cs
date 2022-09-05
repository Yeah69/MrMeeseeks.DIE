// ReSharper disable UnusedParameter.Local
namespace MrMeeseeks.DIE.Configuration.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class CustomScopeForRootTypesAttribute : Attribute
{
    public CustomScopeForRootTypesAttribute(params Type[] types)
    {
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class UserDefinedConstructorParametersInjectionAttribute : Attribute
{
    public UserDefinedConstructorParametersInjectionAttribute(Type type)
    {
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class UserDefinedPropertiesInjectionAttribute : Attribute
{
    public UserDefinedPropertiesInjectionAttribute(Type type)
    {
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class UserDefinedInitializerParametersInjectionAttribute : Attribute
{
    public UserDefinedInitializerParametersInjectionAttribute(Type type)
    {
    }
}

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class InitializerAttribute : Attribute
{
    public InitializerAttribute(Type type, string methodName)
    {
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterInitializerAttribute : Attribute
{
    public FilterInitializerAttribute(Type type)
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