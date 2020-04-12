namespace OrbintSoft.Yauaa.Analyzer
{
    /// <summary>
    /// Defines a field of a parsed user agent, like LayoutEngineName or OperatingSystemVersion.
    /// </summary>
    public interface IAgentField
    {
        /// <summary>
        /// Gets the confidence.
        /// Higher is better.
        /// -1: This field should not be considered, the value is not reliable.
        /// </summary>
        /// <returns>A number >= -1.</returns>
        long GetConfidence();

        /// <summary>
        /// Gets the value of this field.
        /// </summary>
        /// <returns>The string value.</returns>
        string GetValue();

        /// <summary>
        /// Resets the value of this field.
        /// </summary>
        void Reset();

        /// <summary>
        /// Sets the value of this field from another field if confidence is better.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>
        /// True in case of success.
        /// False if the value cannot be set.
        /// </returns>
        bool SetValue(IAgentField field);

        /// <summary>
        /// Sets the value of this field if provided confidence is better.
        /// </summary>
        /// <param name="newValue">The value to be set.</param>
        /// <param name="newConfidence">The confidence of the new value.</param>
        /// <returns>
        /// True in case of success.
        /// False if the value cannot be set.
        /// </returns>
        bool SetValue(string newValue, long newConfidence);

        /// <summary>
        /// Sets a new value for the field with provided confidence.
        /// It doesn't check if confidence is better.
        /// </summary>
        /// <param name="newValue">The value to be set.</param>
        /// <param name="newConfidence">The confidence of the new value.</param>
        void SetValueForced(string newValue, long newConfidence);
    }
}
