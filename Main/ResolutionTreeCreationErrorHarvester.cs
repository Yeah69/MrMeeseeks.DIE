namespace MrMeeseeks.DIE;

internal interface IResolutionTreeCreationErrorHarvester
{
    IReadOnlyList<ErrorTreeItem> Harvest(ResolutionTreeItem root);
}

internal class ResolutionTreeCreationErrorHarvester : IResolutionTreeCreationErrorHarvester
{
    public IReadOnlyList<ErrorTreeItem> Harvest(ResolutionTreeItem root)
    {
        var errorTreeItems = new List<ErrorTreeItem>();
        Inner(root, errorTreeItems);
        return errorTreeItems;

        static void Inner(ResolutionTreeItem item, ICollection<ErrorTreeItem> errorTreeItems)
        {
            switch (item)
            {
                case DeferringResolvable:
                    break;
                case FunctionCallResolution:
                    break;
                case RootResolutionFunction:
                    break;
                case TransientScopeAsDisposableResolution:
                    break;
                case ScopeRootFunction:
                    break;
                case ScopeRootResolution:
                    break;
                case FieldResolution:
                    break;
                case RangeResolution containerResolution:
                    foreach (var overload in containerResolution.RangedInstanceFunctionGroups.SelectMany(ri => ri.Overloads))
                        Inner(overload.Resolvable, errorTreeItems);
                    foreach (var rootResolution in containerResolution.RootResolutions)
                        Inner(rootResolution, errorTreeItems);
                    break;
                case ErrorTreeItem errorTreeItem:
                    errorTreeItems.Add(errorTreeItem);
                    break;
                case ConstructorResolution constructorResolution:
                    foreach (var valueTuple in constructorResolution.Parameter)
                        Inner(valueTuple.Dependency, errorTreeItems);
                    break;
                case SyntaxValueTupleResolution syntaxValueTupleResolution:
                    foreach (var resolvable in syntaxValueTupleResolution.Items)
                        Inner(resolvable, errorTreeItems);
                    break;
                case CollectionResolution collectionResolution:
                    foreach (var resolutionTreeItem in collectionResolution.Parameter)
                        Inner(resolutionTreeItem, errorTreeItems);
                    break;
                case ParameterResolution:
                    break;
                case FuncResolution:
                    break;
                case InterfaceResolution interfaceResolution:
                    Inner(interfaceResolution.Dependency, errorTreeItems);
                    break;
                case Resolvable:
                    throw new ArgumentOutOfRangeException(nameof(item));
                default:
                    throw new ArgumentOutOfRangeException(nameof(item));
            }
        }
    }
}