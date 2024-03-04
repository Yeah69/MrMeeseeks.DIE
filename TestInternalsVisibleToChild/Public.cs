// ReSharper disable once CheckNamespace
// ReSharper disable UnusedParameter.Local
namespace MrMeeseeks.DIE.TestInternalsVisibleToChild.Public;

public interface IClass;

public class Class : IClass
{
    public Class()
    {
    }

    public Class(int i)
    {
        
    }
}

public interface IClassToo;

public class ClassToo : IClassToo
{
    public ClassToo()
    {
        
    }

    public ClassToo(int i)
    {
        
    }
}

// ReSharper disable UnusedTypeParameter
public interface IClass<T0, T1>;
// ReSharper restore UnusedTypeParameter

public class Class<T0, T1> : IClass<T0, T1>
{
    public Class()
    {
    }

    public Class(int i)
    {
        
    }
}

public static class StaticParent
{
    public class Class : IClass
    {
        public Class()
        {
        }

        public Class(int i)
        {
        
        }
    }

    public class ClassToo : IClassToo
    {
        public ClassToo()
        {
        
        }

        public ClassToo(int i)
        {
        
        }
    }

    public class Class<T0, T1> : IClass<T0, T1>
    {
        public Class()
        {
        }

        public Class(int i)
        {
        
        }
    }
}

public class Parent
{
    public class Class : IClass
    {
        public Class()
        {
        }

        public Class(int i)
        {
        
        }
    }

    public class ClassToo : IClassToo
    {
        public ClassToo()
        {
        
        }

        public ClassToo(int i)
        {
        
        }
    }

    public class Class<T0, T1> : IClass<T0, T1>
    {
        public Class()
        {
        }

        public Class(int i)
        {
        
        }
    }
}