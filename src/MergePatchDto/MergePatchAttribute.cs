using System;

namespace MergePatch
{
    /// <summary>
    /// Marks a DTO class for merge-patch presence tracking and source generation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class MergePatchAttribute : Attribute
    {
        /// <summary>
        /// Initializes a targetless merge-patch DTO configuration.
        /// </summary>
        public MergePatchAttribute()
        {
        }

        /// <summary>
        /// Initializes a merge-patch DTO configuration for the specified target type.
        /// </summary>
        /// <param name="targetType">The type that receives generated apply methods.</param>
        public MergePatchAttribute(Type targetType)
        {
            TargetType = targetType;
        }

        /// <summary>
        /// Gets the primary target type for generated apply methods, or <see langword="null" /> for targetless presence tracking.
        /// </summary>
        public Type? TargetType { get; }

        /// <summary>
        /// Gets or sets how generated JSON converters handle JSON properties that do not map to DTO members.
        /// </summary>
        public UnknownPropertyHandling UnknownPropertyHandling { get; set; } = UnknownPropertyHandling.Ignore;
    }
}
