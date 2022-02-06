using System.Threading.Tasks;

namespace MrMeeseeks.DIE.Sample;

public interface IContainerInstance { }
public interface ITransientScopeInstance { }
public interface IScopeInstance { }
public interface ITransientScopeRoot { }
public interface IScopeRoot { }
public interface ITransient { }
public interface IDecorator<T> { }
public interface IComposite<T> { }
public interface ITypeInitializer
{
    void Initialize();
}
public interface ITaskTypeInitializer
{
    Task InitializeAsync();
}
public interface IValueTaskTypeInitializer
{
    ValueTask InitializeAsync();
}