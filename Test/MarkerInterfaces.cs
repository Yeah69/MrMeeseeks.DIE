using System.Threading.Tasks;

namespace MrMeeseeks.DIE.Test;

public interface IContainerInstance { }
public interface ITransientScopeInstance { }
public interface IScopeInstance { }
public interface ITransientScopeRoot { }
public interface IScopeRoot { }
public interface ITransient { }
public interface ISyncTransient { }
public interface IAsyncTransient { }
// ReSharper disable once UnusedTypeParameter
public interface IDecorator<T> { }
// ReSharper disable once UnusedTypeParameter
public interface IComposite<T> { }
public interface IInitializer
{
    void Initialize();
}
public interface ITaskInitializer
{
    Task InitializeAsync();
}
public interface IValueTaskInitializer
{
    ValueTask InitializeAsync();
}