using System;
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

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
public class KeyAttribute : Attribute
{
    public KeyAttribute(object key) => Key = key;

    public object Key { get; }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class DecorationOrdinalAttribute : Attribute
{
    public DecorationOrdinalAttribute(int ordinal) => Ordinal = ordinal;

    public int Ordinal { get; }
}