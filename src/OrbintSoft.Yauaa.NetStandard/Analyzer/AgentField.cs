//-----------------------------------------------------------------------
// <copyright file="AgentField.cs" company="OrbintSoft">
//   Yet Another User Agent Analyzer for .NET Standard
//   porting realized by Stefano Balzarotti, Copyright 2018-2020 (C) OrbintSoft
//
//   Original Author and License:
//
//   Yet Another UserAgent Analyzer
//   Copyright(C) 2013-2020 Niels Basjes
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//   https://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
// <author>Stefano Balzarotti, Niels Basjes</author>
// <date>2020, 05, 14, 19:47</date>
//-----------------------------------------------------------------------
namespace OrbintSoft.Yauaa.Analyzer
{
    using System;

    /// <summary>
    /// Defines a field of a parsed user agent, like LayoutEngineName or OperatingSystemVersion.
    /// </summary>
    [Serializable]
    public class AgentField : IAgentField, IEquatable<AgentField>
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
        /// Gets or sets the internal Confidence.
        /// </summary>
        internal long Confidence { get; set; }

        /// <summary>
        /// Gets or sets the Value.
        /// </summary>
        internal string Value { get; set; }

        /// <summary>
        /// Determines if the other agend field equals to this.
        /// </summary>
        /// <param name="other">The other AgentField.</param>
        /// <returns>True if equals.</returns>
        public bool Equals(AgentField other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other is null)
            {
                return false;
            }

            return this.Confidence == other.Confidence &&
                Equals(this.defaultValue, other.defaultValue) &&
                Equals(this.Value, other.Value);
        }

        /// <inheritdoc>/>.
        public override bool Equals(object other)
        {
            if (!(other is AgentField))
            {
                return false;
            }

            return this.Equals((AgentField)other);
        }

        /// <summary>
        /// Gets the Confidence, default -1.
        /// Higher is better, if less than 0 not realiable.
        /// </summary>
        /// <returns>The value.</returns>
        public long GetConfidence()
        {
            if (this.Value is null)
            {
                return -1; // Lie in case the value was wiped.
            }

            return this.Confidence;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return ValueTuple.Create(this.defaultValue, this.Value, this.Confidence).GetHashCode();
        }

        /// <summary>
        /// Get the value of the field.
        /// </summary>
        /// <returns>The value.</returns>
        public string GetValue()
        {
            return this.Value ?? this.defaultValue;
        }

        /// <summary>
        /// Resets the field with default values.
        /// </summary>
        public void Reset()
        {
            this.Value = this.defaultValue;
            this.Confidence = -1;
        }

        /// <summary>
        /// Sets the value using another field.
        /// This is done only if confidence is better.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>True if set..</returns>
        public bool SetValue(IAgentField field)
        {
            return this.SetValue(field.GetValue(), field.GetConfidence());
        }

        /// <summary>
        /// sets a new value with confidence, the new value is set only if the confidence is better.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        /// <param name="newConfidence">The new confidence.</param>
        /// <returns>True if value has been set.</returns>
        public bool SetValue(string newValue, long newConfidence)
        {
            if (newConfidence > this.Confidence)
            {
                this.Confidence = newConfidence;
                if (DefaultUserAgentFields.NULL_VALUE.Equals(newValue))
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
        /// Force set a new value with confidence, value is set without cheking confidence.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        /// <param name="newConfidence">The new confidence.</param>
        public void SetValueForced(string newValue, long newConfidence)
        {
            this.Confidence = newConfidence;

            if (DefaultUserAgentFields.NULL_VALUE.Equals(newValue))
            {
                this.Value = this.defaultValue;
            }
            else
            {
                this.Value = newValue;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (this.defaultValue is null)
            {
                return $"{{ value:'{this.Value}', confidence:'{this.Confidence}', default:null }}";
            }

            return $"{{ value:'{this.Value}', confidence:'{this.Confidence}', default:'{this.defaultValue}' }}";
        }
    }
}
