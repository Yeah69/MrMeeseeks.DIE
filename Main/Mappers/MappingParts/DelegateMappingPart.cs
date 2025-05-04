using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.Delegates;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Nodes.Roots;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Mappers.MappingParts;

internal interface IDelegateMappingPart : IMappingPart;

internal sealed class DelegateMappingPart : IDelegateMappingPart, IScopeInstance
{
    private readonly IContainerNode _parentContainer;
    private readonly IFunctionNode _parentFunction;
    private readonly Func<(INamedTypeSymbol Outer, INamedTypeSymbol Inner), ILocalFunctionNode, IReadOnlyList<ITypeSymbol>, ILazyNode> _lazyNodeFactory;
    private readonly Func<(INamedTypeSymbol Outer, INamedTypeSymbol Inner), ILocalFunctionNode, IReadOnlyList<ITypeSymbol>, IThreadLocalNode> _threadLocalNodeFactory;
    private readonly Func<(INamedTypeSymbol Outer, INamedTypeSymbol Inner), ILocalFunctionNode, IReadOnlyList<ITypeSymbol>, IFuncNode> _funcNodeFactory;
    private readonly Func<string, ITypeSymbol, IErrorNode> _errorNodeFactory;
    private readonly Func<ITypeSymbol, IReadOnlyList<ITypeSymbol>, ImmutableDictionary<ITypeSymbol, IParameterNode>, ILocalFunctionNodeRoot> _localFunctionNodeFactory;
    private readonly ITypeParameterUtility _typeParameterUtility;
    private readonly IUserDefinedElementsMappingPart _userDefinedElementsMappingPart;
    private readonly WellKnownTypes _wellKnownTypes;


    internal DelegateMappingPart(
        IContainerNode parentContainer, 
        IFunctionNode parentFunction, 
        ITypeParameterUtility typeParameterUtility,
        WellKnownTypes wellKnownTypes,
        IUserDefinedElementsMappingPart userDefinedElementsMappingPart,
        Func<(INamedTypeSymbol Outer, INamedTypeSymbol Inner), ILocalFunctionNode, IReadOnlyList<ITypeSymbol>, ILazyNode> lazyNodeFactory,
        Func<(INamedTypeSymbol Outer, INamedTypeSymbol Inner), ILocalFunctionNode, IReadOnlyList<ITypeSymbol>, IThreadLocalNode> threadLocalNodeFactory, 
        Func<(INamedTypeSymbol Outer, INamedTypeSymbol Inner), ILocalFunctionNode, IReadOnlyList<ITypeSymbol>, IFuncNode> funcNodeFactory, 
        Func<string, ITypeSymbol, IErrorNode> errorNodeFactory, 
        Func<ITypeSymbol, IReadOnlyList<ITypeSymbol>, ImmutableDictionary<ITypeSymbol, IParameterNode>, ILocalFunctionNodeRoot> localFunctionNodeFactory)
    {
        _parentContainer = parentContainer;
        _lazyNodeFactory = lazyNodeFactory;
        _threadLocalNodeFactory = threadLocalNodeFactory;
        _funcNodeFactory = funcNodeFactory;
        _errorNodeFactory = errorNodeFactory;
        _localFunctionNodeFactory = localFunctionNodeFactory;
        _typeParameterUtility = typeParameterUtility;
        _userDefinedElementsMappingPart = userDefinedElementsMappingPart;
        _parentFunction = parentFunction;
        _wellKnownTypes = wellKnownTypes;
    }

    public IElementNode? Map(MappingPartData data)
    {

        if (CustomSymbolEqualityComparer.Default.Equals(data.Type.OriginalDefinition, _wellKnownTypes.Lazy1)
            && data.Type is INamedTypeSymbol lazyType)
        {
            return _userDefinedElementsMappingPart.Map(data) 
                   ?? CreateDelegateNode(
                       lazyType, 
                       lazyType.TypeArguments.SingleOrDefault(), 
                       [], 
                       _lazyNodeFactory, 
                       "Lazy",
                       true);
        }

        if (CustomSymbolEqualityComparer.Default.Equals(data.Type.OriginalDefinition, _wellKnownTypes.ThreadLocal1)
            && data.Type is INamedTypeSymbol threadLocalType)
        {
            return _userDefinedElementsMappingPart.Map(data) 
                   ?? CreateDelegateNode(
                       threadLocalType, 
                       threadLocalType.TypeArguments.SingleOrDefault(), 
                       [], 
                       _threadLocalNodeFactory, 
                       "ThreadLocal",
                       true);
        }

        if (data.Type.TypeKind == TypeKind.Delegate 
            && data.Type.FullName().StartsWith("global::System.Func<", StringComparison.Ordinal)
            && data.Type is INamedTypeSymbol funcType)
        {
            return _userDefinedElementsMappingPart.Map(data) 
                   ?? CreateDelegateNode(
                       funcType, 
                       funcType.TypeArguments.LastOrDefault(), 
                       funcType.TypeArguments.Take(funcType.TypeArguments.Length - 1).ToArray(), 
                       _funcNodeFactory, 
                       "Func",
                       false);
        }

        return null;

        IElementNode CreateDelegateNode<TElementNode>(
            INamedTypeSymbol delegateType, 
            ITypeSymbol? returnType, 
            IReadOnlyList<ITypeSymbol> lambdaParameters,
            Func<(INamedTypeSymbol Outer, INamedTypeSymbol Inner), ILocalFunctionNode, IReadOnlyList<ITypeSymbol>, TElementNode> factory,
            string logLabel,
            bool passOverrides)
            where TElementNode : IElementNode
        {
            if (returnType is null)
            {
                return _errorNodeFactory(
                        $"{logLabel}: {delegateType.TypeArguments.Last().FullName()} is not a type symbol",
                        data.Type)
                    .EnqueueBuildJobTo(_parentContainer.BuildQueue, data.PassedContext);
            }

            var returnTypeForFunction = _typeParameterUtility.ReplaceTypeParametersByCustom(returnType);
            var function = _localFunctionNodeFactory(
                    returnTypeForFunction,
                    lambdaParameters,
                    passOverrides ? _parentFunction.Overrides : ImmutableDictionary<ITypeSymbol, IParameterNode>.Empty)
                .Function
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, data.PassedContext);
            _parentFunction.AddLocalFunction(function);

            var delegateTypeTypeArguments = delegateType.TypeArguments.ToArray();
            delegateTypeTypeArguments[delegateTypeTypeArguments.Length - 1] = returnTypeForFunction;
            var preparedDelegateType = delegateType.OriginalDefinition.Construct(delegateTypeTypeArguments);
            
            return factory((Outer: delegateType, Inner: preparedDelegateType), function, _typeParameterUtility.ExtractTypeParameters(returnType))
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, data.PassedContext);
        }
    }
}