using System;

namespace MrMeeseeks.DIE.Spy
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class ContainerAttribute : Attribute
    {
        // ReSharper disable once UnusedParameter.Local *** Is used in the generator
        public ContainerAttribute(Type type)
        {
        }
    }
}
