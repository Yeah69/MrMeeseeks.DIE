namespace MrMeeseeks.DIE;

public interface IContainer<T>
{
    T Resolve();
}