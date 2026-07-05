using System;

namespace MergePatch
{
    /// <summary>
    /// Maps a patch DTO property to a differently named target property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PatchToAttribute : Attribute
    {
        /// <summary>
        /// Initializes a target-property mapping.
        /// </summary>
        /// <param name="targetPropertyName">The target property name to apply this patch property to.</param>
        public PatchToAttribute(string targetPropertyName)
        {
            TargetPropertyName = targetPropertyName;
        }

        /// <summary>
        /// Gets the target property name to apply this patch property to.
        /// </summary>
        public string TargetPropertyName { get; }
    }
}
