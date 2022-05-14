namespace MrMeeseeks.DIE.TestNotInternalsVisibleToChild.Public;

public interface IClass {}

public class Class : IClass
{
    public Class()
    {
    }

    public Class(int i)
    {
        
    }
}

public interface IClassToo {}

public class ClassToo : IClassToo
{
    public ClassToo()
    {
        
    }

    public ClassToo(int i)
    {
        
    }
}

public interface IClass<T0, T1> {}

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