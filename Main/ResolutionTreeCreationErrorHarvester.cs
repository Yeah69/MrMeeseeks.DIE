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
                case RootResolutionFunction:
                    break;
                case RangedInstanceReferenceResolution:
                    break;
                case ScopeRootFunction:
                    break;
                case ScopeRootResolution:
                    break;
                case FieldResolution:
                    break;
                case RangeResolution containerResolution:
                    foreach (var overload in containerResolution.RangedInstances.SelectMany(ri => ri.Overloads))
                        Inner(overload.Dependency, errorTreeItems);
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
                case CollectionResolution collectionResolution:
                    foreach (var resolutionTreeItem in collectionResolution.Parameter)
                        Inner(resolutionTreeItem, errorTreeItems);
                    break;
                case ParameterResolution:
                    break;
                case FuncResolution funcResolution:
                    foreach (var funcParameterResolution in funcResolution.Parameter)
                        Inner(funcParameterResolution, errorTreeItems);
                    Inner(funcResolution.Dependency, errorTreeItems);
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