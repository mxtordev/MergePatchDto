using System;

namespace MergePatch
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class MergePatchTargetAttribute : Attribute
    {
        public MergePatchTargetAttribute(Type targetType)
        {
            TargetType = targetType;
        }

        public Type TargetType { get; }
    }
}
