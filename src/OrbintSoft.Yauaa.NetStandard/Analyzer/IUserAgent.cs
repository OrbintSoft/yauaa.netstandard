//-----------------------------------------------------------------------
// <copyright file="IUserAgent.cs" company="OrbintSoft">
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
// <date>2020, 05, 15, 00:30</date>

namespace OrbintSoft.Yauaa.Analyzer
{
    using System.Collections.Generic;

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

        /// <summary>
        /// Extract the requested field by name.
        /// </summary>
        /// <param name="fieldName">The field name.</param>
        /// <returns>The extracted field.</returns>
        IAgentField Get(string fieldName);

        /// <summary>
        /// Gets the extracted value for for the specified field.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>The value.</returns>
        string GetValue(string fieldName);

        /// <summary>
        /// Gets the Confidence for for the specified field.
        /// A value less than 0 means that the extracted value for the field is not reliable.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>The confidence value.</returns>
        long GetConfidence(string fieldName);

        /// <summary>
        /// Retrieve the list of all available fields for the specified user agent.
        /// Some standard fields will be returned too with a default value even if they can't be extracted by the current user agent string.
        /// </summary>
        /// <returns>The list of field names.</returns>
        IList<string> GetAvailableFieldNames();

        /// <summary>
        /// Resets all fields to default value. (like if no parsing has occurred).
        /// </summary>
        void Reset();

        /// <summary>
        /// Set an user agent attribute (field) with a custom value and confidence.
        /// Value is set only if confidence is better than previous value.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="value">The value.</param>
        /// <param name="confidence">The confidence.</param>
        void Set(string attribute, string value, long confidence);

        /// <summary>
        /// Set an user agent attribute (field) with a custom value and confidence.
        /// Value is set without cheking the confidence.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="value">The value.</param>
        /// <param name="confidence">The confidence.</param>
        void SetForced(string attribute, string value, long confidence);
    }
}
