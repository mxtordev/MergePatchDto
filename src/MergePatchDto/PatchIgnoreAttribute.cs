using System;

namespace MergePatch
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PatchIgnoreAttribute : Attribute
    {
    }
}
