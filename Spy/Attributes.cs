using System;

namespace MrMeeseeks.DIE.Spy;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Assembly | AttributeTargets.Class)]
public class ConstructorChoiceAttribute : Attribute
{
    public ConstructorChoiceAttribute(Type implementationType, params Type[] parameterTypes)
    {
    }
}
