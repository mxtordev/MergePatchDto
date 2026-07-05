using System;

namespace MergePatch
{
    /// <summary>
    /// Adds an additional target type for a merge-patch DTO.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class MergePatchTargetAttribute : Attribute
    {
        /// <summary>
        /// Initializes a merge-patch target declaration for the specified type.
        /// </summary>
        /// <param name="targetType">The additional type that receives generated apply methods.</param>
        public MergePatchTargetAttribute(Type targetType)
        {
            TargetType = targetType;
        }

        /// <summary>
        /// Gets the additional target type for generated apply methods.
        /// </summary>
        public Type TargetType { get; }
    }
}
