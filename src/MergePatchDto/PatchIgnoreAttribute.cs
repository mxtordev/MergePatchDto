using System;

namespace MergePatch
{
    /// <summary>
    /// Excludes a patch DTO property from generated target application.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PatchIgnoreAttribute : Attribute
    {
        /// <summary>
        /// Initializes a patch ignore declaration.
        /// </summary>
        public PatchIgnoreAttribute()
        {
        }
    }
}
