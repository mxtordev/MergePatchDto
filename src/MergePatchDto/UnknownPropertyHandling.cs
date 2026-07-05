namespace MergePatch
{
    /// <summary>
    /// Specifies how generated JSON converters handle JSON properties that do not map to DTO members.
    /// </summary>
    public enum UnknownPropertyHandling
    {
        /// <summary>
        /// Ignore unknown JSON properties during deserialization.
        /// </summary>
        Ignore,

        /// <summary>
        /// Reject unknown JSON properties during deserialization.
        /// </summary>
        Reject
    }
}
