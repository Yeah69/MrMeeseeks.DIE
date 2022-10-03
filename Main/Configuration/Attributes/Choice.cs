// ReSharper disable UnusedParameter.Local
namespace MrMeeseeks.DIE.Configuration.Attributes;

/// <summary>
/// Aggregates generic type substitutes for the given unbound generic implementation and the selected generic parameter.
/// See https://die.mrmeeseeks.dev/configuration/generics/
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class GenericParameterSubstitutesChoiceAttribute : Attribute
{
    public GenericParameterSubstitutesChoiceAttribute(Type unboundGenericType, string genericArgumentName, params Type[] types) {}
}

/// <summary>
/// Discards generic type substitutions for the given unbound generic implementation and the selected generic parameter.
/// See https://die.mrmeeseeks.dev/configuration/generics/
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterGenericParameterSubstitutesChoiceAttribute : Attribute
{
    public FilterGenericParameterSubstitutesChoiceAttribute(Type unboundGenericType, string genericArgumentName) {}
}

/// <summary>
/// Specifies the generic type choice for the given unbound generic implementation and the selected generic parameter. For collections, this choice is automatically added.
/// See https://die.mrmeeseeks.dev/configuration/generics/
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class GenericParameterChoiceAttribute : Attribute
{
    public GenericParameterChoiceAttribute(Type unboundGenericType, string genericArgumentName, Type chosenType) {}
}

/// <summary>
/// Discards the generic type choice for the given unbound generic implementation and the selected generic parameter.
/// See https://die.mrmeeseeks.dev/configuration/generics/
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterGenericParameterChoiceAttribute : Attribute
{
    public FilterGenericParameterChoiceAttribute(Type unboundGenericType, string genericArgumentName) {}
}

/// <summary>
/// Selects a sequence of decorators to apply to the decorated implementation. This attribute is mandatory for all decorated implementations that have multiple decorators. A sequence can be configured either for the decorator interface type or for the concrete implementation. The configuration for the interface will be applied to all of its implementations, but if present, the configurations for the concrete implementation will take precedence. First, pass the decorated type interface, then the interface again (for fallback) or decorated implementation type (for specific), and then a list of decorator implementation types. The decorator implementations will be applied in order, that is, the decorated implementation instance will be injected into the first decorator implementation instance, which will be injected into the second, and so on. You can also disable decoration by leaving the list of decorator types empty.
/// See https://die.mrmeeseeks.dev/configuration/decorator-composite/
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class DecoratorSequenceChoiceAttribute : Attribute
{
    public DecoratorSequenceChoiceAttribute(Type interfaceType, Type decoratedType, params Type[] types) {}
}

/// <summary>
/// Cancels an active Decoration Sequence selection.
/// See https://die.mrmeeseeks.dev/configuration/decorator-composite/
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterDecoratorSequenceChoiceAttribute : Attribute
{
    public FilterDecoratorSequenceChoiceAttribute(Type interfaceType, Type decoratedType) {}
}

/// <summary>
/// Selects a constructor for the given implementation type to be used by DIE. If an implementation has multiple constructors that could potentially be used, choosing a constructor is mandatory for the implementation to be usable. Pass the implementation type first, then the types of the constructor parameters in the same order. To choose the parameterless constructor, just pass the only implementation type.
/// See https://die.mrmeeseeks.dev/configuration/implementations/
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class ConstructorChoiceAttribute : Attribute
{
    public ConstructorChoiceAttribute(Type implementationType, params Type[] parameterTypes)
    {
    }
}

/// <summary>
/// Discards the inherited constructor choice for the given implementation type. Just pass the implementation type.
/// See https://die.mrmeeseeks.dev/configuration/implementations/
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterConstructorChoiceAttribute : Attribute
{
    public FilterConstructorChoiceAttribute(Type implementationType)
    {
    }
}

/// <summary>
/// Selects the properties to be injected during instantiation. These properties must be mutable for the container (i.e. either public set/init or internal set/init within the same assembly or with appropriate InternalsVisibleTo usage). If no property choice is active, DIE will inject all accessible init properties by default. On the other hand, if a property choice is active, DIE will not inject any init properties that are not passed to the choice. Pass the implementation type first, followed by the names of the properties to be injected as strings.
/// See https://die.mrmeeseeks.dev/configuration/implementations/
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class PropertyChoiceAttribute : Attribute
{
    public PropertyChoiceAttribute(Type implementationType, params string[] propertyName)
    {
    }
}

/// <summary>
/// Discards the current property choice for the given implementation type. Just pass the implementation type.
/// See https://die.mrmeeseeks.dev/configuration/implementations/
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterPropertyChoiceAttribute : Attribute
{
    public FilterPropertyChoiceAttribute(Type implementationType)
    {
    }
}

/// <summary>
/// For the given abstraction type, it chooses the given implementation type, even if multiple implementations are registered. Pass the abstraction type first and the implementation type second.
/// See https://die.mrmeeseeks.dev/configuration/implementations/
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class ImplementationChoiceAttribute : Attribute
{
    public ImplementationChoiceAttribute(Type type, Type implementationChoice) {}
}

/// <summary>
/// Discards the current implementation choice for the given abstraction type. Pass only the abstraction type.
/// See https://die.mrmeeseeks.dev/configuration/implementations/
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterImplementationChoiceAttribute : Attribute
{
    public FilterImplementationChoiceAttribute(Type type) {}
}

/// <summary>
/// For the given abstraction type, it chooses the implementation types for collection injections. Pass the abstraction type first, then the implementation types.
/// See https://die.mrmeeseeks.dev/configuration/implementations/
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public class ImplementationCollectionChoiceAttribute : Attribute
{
    public ImplementationCollectionChoiceAttribute(Type type, params Type[] implementationChoice) {}
}

/// <summary>
/// Discards the inherited implementation collection choice for the given abstraction type. Pass only the abstraction type.
/// See https://die.mrmeeseeks.dev/configuration/implementations/
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterImplementationCollectionChoiceAttribute : Attribute
{
    public FilterImplementationCollectionChoiceAttribute(Type type) {}
}