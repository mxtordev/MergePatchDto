using System;

namespace MergePatch
{
    /// <summary>
    /// Uses a named mapping method when applying a patch DTO property to a target.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PatchUsingAttribute : Attribute
    {
        /// <summary>
        /// Initializes a custom apply-method mapping.
        /// </summary>
        /// <param name="methodName">The method name used to apply this patch property.</param>
        public PatchUsingAttribute(string methodName)
        {
            MethodName = methodName;
        }

        /// <summary>
        /// Gets the method name used to apply this patch property.
        /// </summary>
        public string MethodName { get; }
    }
}
