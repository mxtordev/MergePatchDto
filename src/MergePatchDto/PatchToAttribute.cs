using System;

namespace MergePatch
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PatchToAttribute : Attribute
    {
        public PatchToAttribute(string targetPropertyName)
        {
            TargetPropertyName = targetPropertyName;
        }

        public string TargetPropertyName { get; }
    }
}
