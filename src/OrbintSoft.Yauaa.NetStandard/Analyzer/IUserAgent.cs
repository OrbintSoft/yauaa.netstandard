namespace OrbintSoft.Yauaa.Analyzer
{
    /// <summary>
    /// A user agenent with its fields.
    /// </summary>
    public interface IUserAgent
    {
        /// <summary>
        /// Gets or sets the user agent strings.
        /// </summary>
        string UserAgentString { get; set; }

        /// <summary>
        /// Gets the numer of ambiguities found.
        /// </summary>
        int AmbiguityCount { get; }

        /// <summary>
        /// Gets a value indicating whether some fields are ambiguos.
        /// </summary>
        bool HasAmbiguity { get; }

        /// <summary>
        /// Gets a value indicating whether the user agent contains syntax errors.
        /// </summary>
        bool HasSyntaxError { get; }
    }
}
