namespace MrMeeseeks.DIE.SampleChild.Internal;

internal interface IClass {}

internal class Class : IClass
{
    internal Class()
    {
    }

    internal Class(int i)
    {
        
    }
}

internal interface IClassToo {}

internal class ClassToo : IClassToo
{
    internal ClassToo()
    {
        
    }

    internal ClassToo(int i)
    {
        
    }
}

internal interface IClass<T0, T1> {}

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