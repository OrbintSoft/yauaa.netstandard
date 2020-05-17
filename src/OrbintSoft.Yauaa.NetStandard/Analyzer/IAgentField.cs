//-----------------------------------------------------------------------
// <copyright file="IAgentField.cs" company="OrbintSoft">
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
// <date>2020, 05, 15, 12:33</date>
namespace OrbintSoft.Yauaa.Analyzer
{
    /// <summary>
    /// Defines a field of a parsed user agent, like LayoutEngineName or OperatingSystemVersion.
    /// </summary>
    public interface IAgentField
    {
        /// <summary>
        /// Gets a value indicating whether the field is valoriozid with a default value.
        /// </summary>
        bool IsDefaultValue { get; }

        /// <summary>
        /// Gets default value to be used for this field.
        /// </summary>
        string DefaultValue { get; }

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
