using MrMeeseeks.DIE.Logging;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Nodes.Elements.Delegates;

internal interface ILazyNode : IDelegateBaseNode;

internal sealed partial class LazyNode : DelegateBaseNode, ILazyNode
{
    internal LazyNode(
        (INamedTypeSymbol Outer, INamedTypeSymbol Inner) delegateTypes,
        ILocalFunctionNode function,
        IReadOnlyList<ITypeSymbol> typeParameters,
        
        LocalDiagLogger localDiagLogger,
        IContainerNode parentContainer,
        ReferenceGenerator referenceGenerator) 
        : base(delegateTypes, function, typeParameters, localDiagLogger, parentContainer, referenceGenerator)
    {
    }
}