namespace OrbintSoft.Yauaa.Analyzer
{
    using System;

    /// <summary>
    /// Defines a field of a parsed user agent, like LayoutEngineName or OperatingSystemVersion.
    /// </summary>
    [Serializable]
    public class AgentField : IAgentField
    {
        /// <summary>
        /// The default value to be used for this field..
        /// </summary>
        private readonly string defaultValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentField"/> class.
        /// </summary>
        /// <param name="defaultValue">The default value to be used for this field...</param>
        internal AgentField(string defaultValue)
        {
            this.defaultValue = defaultValue;
            this.Reset();
        }

        /// <summary>
        /// Gets or sets the Confidence.
        /// </summary>
        internal long Confidence { get; set; }

        /// <summary>
        /// Gets or sets the Value.
        /// </summary>
        internal string Value { get; set; }

        /// <summary>
        /// The Equals.
        /// </summary>
        /// <param name="obj">The obj<see cref="object"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (!(obj is AgentField))
            {
                return false;
            }

            var that = (AgentField)obj;
            return this.Confidence == that.Confidence &&
                Equals(this.defaultValue, that.defaultValue) &&
                Equals(this.Value, that.Value);
        }

        /// <summary>
        /// The GetConfidence.
        /// </summary>
        /// <returns>The <see cref="long"/>.</returns>
        public long GetConfidence()
        {
            if (this.Value == null)
            {
                return -1; // Lie in case the value was wiped.
            }

            return this.Confidence;
        }

        /// <summary>
        /// The GetHashCode.
        /// </summary>
        /// <returns>The <see cref="int"/>.</returns>
        public override int GetHashCode()
        {
            return ValueTuple.Create(this.defaultValue, this.Value, this.Confidence).GetHashCode();
        }

        /// <summary>
        /// The GetValue.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        public string GetValue()
        {
            return this.Value ?? this.defaultValue;
        }

        /// <summary>
        /// The Reset.
        /// </summary>
        public void Reset()
        {
            this.Value = this.defaultValue;
            this.Confidence = -1;
        }

        /// <summary>
        /// The SetValue.
        /// </summary>
        /// <param name="field">The field<see cref="AgentField"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool SetValue(IAgentField field)
        {
            return this.SetValue(field.GetValue(), field.GetConfidence());
        }

        /// <summary>
        /// The SetValue.
        /// </summary>
        /// <param name="newValue">The newValue<see cref="string"/>.</param>
        /// <param name="newConfidence">The newConfidence<see cref="long"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool SetValue(string newValue, long newConfidence)
        {
            if (newConfidence > this.Confidence)
            {
                this.Confidence = newConfidence;
                if (UserAgent.NULL_VALUE.Equals(newValue))
                {
                    this.Value = this.defaultValue;
                }
                else
                {
                    this.Value = newValue;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// The SetValueForced.
        /// </summary>
        /// <param name="newValue">The newValue<see cref="string"/>.</param>
        /// <param name="newConfidence">The newConfidence<see cref="long"/>.</param>
        public void SetValueForced(string newValue, long newConfidence)
        {
            this.Confidence = newConfidence;

            if (UserAgent.NULL_VALUE.Equals(newValue))
            {
                this.Value = this.defaultValue;
            }
            else
            {
                this.Value = newValue;
            }
        }

        /// <summary>
        /// The ToString.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        public override string ToString()
        {
            if (this.defaultValue == null)
            {
                return "{ value:'" + this.Value + "', confidence:'" + this.Confidence + "', default:null }";
            }

            return "{ value:'" + this.Value + "', confidence:'" + this.Confidence + "', default:'" + this.defaultValue + "' }";
        }
    }
}
