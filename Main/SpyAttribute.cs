using System;

namespace MrMeeseeks.DIE
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class SpyAttribute : Attribute
    {
        // ReSharper disable once UnusedParameter.Local *** Is used in the generator
        public SpyAttribute(params Type[] type) {}
    }
}