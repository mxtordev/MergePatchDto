using System;

namespace MergePatch
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PatchUsingAttribute : Attribute
    {
        public PatchUsingAttribute(string methodName)
        {
            MethodName = methodName;
        }

        public string MethodName { get; }
    }
}
