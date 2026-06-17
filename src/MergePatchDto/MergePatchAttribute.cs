using System;

namespace MergePatch
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class MergePatchAttribute : Attribute
    {
        public MergePatchAttribute()
        {
        }

        public MergePatchAttribute(Type targetType)
        {
            TargetType = targetType;
        }

        public Type? TargetType { get; }

        public UnknownPropertyHandling UnknownPropertyHandling { get; set; } = UnknownPropertyHandling.Ignore;
    }
}
