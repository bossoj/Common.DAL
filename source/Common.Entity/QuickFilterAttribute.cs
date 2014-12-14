using System;

namespace Common.Entity
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class QuickFilterAttribute : Attribute
    {
    }
}
