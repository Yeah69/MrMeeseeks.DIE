// ReSharper disable once CheckNamespace
// ReSharper disable UnusedParameter.Local
namespace MrMeeseeks.DIE.TestNotInternalsVisibleToChild.Internal;

internal interface IClass;

internal class Class : IClass
{
    internal Class()
    {
    }

    internal Class(int i)
    {
        
    }
}

internal interface IClassToo;

internal class ClassToo : IClassToo
{
    internal ClassToo()
    {
        
    }

    internal ClassToo(int i)
    {
        
    }
}

// ReSharper disable UnusedTypeParameter
internal interface IClass<T0, T1>;
// ReSharper restore UnusedTypeParameter

internal class Class<T0, T1> : IClass<T0, T1>
{
    internal Class()
    {
    }

    internal Class(int i)
    {
        
    }
}

internal static class StaticParent
{
    internal class Class : IClass
    {
        internal Class()
        {
        }

        internal Class(int i)
        {
        
        }
    }

    internal class ClassToo : IClassToo
    {
        internal ClassToo()
        {
        
        }

        internal ClassToo(int i)
        {
        
        }
    }

    internal class Class<T0, T1> : IClass<T0, T1>
    {
        internal Class()
        {
        }

        internal Class(int i)
        {
        
        }
    }
}

internal class Parent
{
    // ReSharper disable once EmptyConstructor
    internal Parent() {}
    internal class Class : IClass
    {
        internal Class()
        {
        }

        internal Class(int i)
        {
        
        }
    }

    internal class ClassToo : IClassToo
    {
        internal ClassToo()
        {
        
        }

        internal ClassToo(int i)
        {
        
        }
    }

    internal class Class<T0, T1> : IClass<T0, T1>
    {
        internal Class()
        {
        }

        internal Class(int i)
        {
        
        }
    }
}