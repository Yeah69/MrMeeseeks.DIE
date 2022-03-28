namespace MrMeeseeks.DIE.ResolutionBuilding;

public interface IResolutionBuilder<T>
{
    bool HasWorkToDo { get; }
    
    void DoWork();

    T Build();
}