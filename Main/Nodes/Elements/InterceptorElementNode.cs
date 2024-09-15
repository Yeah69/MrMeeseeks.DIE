namespace MrMeeseeks.DIE.Nodes.Elements;

internal interface IInterceptionElementNode : IElementNode
{
    IElementNode InterceptorInstance { get; }
    IElementNode DecoratedInstance { get; }
}

internal sealed partial class InterceptionElementNode : IInterceptionElementNode
{
    internal InterceptionElementNode(
        // parameters
        string typeFullName,
        (IElementNode Interceptor, IElementNode Decorated) innerInstances,
        
        // dependencies
        IReferenceGenerator referenceGenerator)
    {
        Reference = referenceGenerator.Generate("interception");
        TypeFullName = typeFullName;
        InterceptorInstance = innerInstances.Interceptor;
        DecoratedInstance = innerInstances.Decorated;
    }
    
    public void Build(PassedContext passedContext)
    {
    }

    public string TypeFullName { get; }
    public string Reference { get; }
    public IElementNode InterceptorInstance { get; }
    public IElementNode DecoratedInstance { get; }
}