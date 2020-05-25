//-----------------------------------------------------------------------
// <copyright file="YauaaFieldAttribute.cs" company="OrbintSoft">
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
// <date>2018, 11, 24, 12:49</date>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Annotate
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the <see cref="YauaaFieldAttribute" />, it useful to map a custom user agent field to your class property.
    /// </summary>
    public sealed class YauaaFieldAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="YauaaFieldAttribute"/> class.
        /// </summary>
        /// <param name="value">The user agent field.</param>
        public YauaaFieldAttribute(params string[] value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YauaaFieldAttribute"/> class.
        /// </summary>
        /// <param name="value">The user agent field.</param>
        public YauaaFieldAttribute(string value)
        {
            this.Value = new string[1] { value };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YauaaFieldAttribute"/> class.
        /// </summary>
        /// <param name="value1">The user agent field1.</param>
        /// <param name="value2">The user agent field2.</param>
        public YauaaFieldAttribute(string value1, string value2)
        {
            this.Value = new string[2] { value1, value2 };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YauaaFieldAttribute"/> class.
        /// </summary>
        /// <param name="value1">The user agent field1.</param>
        /// <param name="value2">The user agent field2.</param>
        /// <param name="value3">The user agent field3.</param>
        public YauaaFieldAttribute(string value1, string value2, string value3)
        {
            this.Value = new string[3] { value1, value2, value3 };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YauaaFieldAttribute"/> class.
        /// </summary>
        /// <param name="values">A list of user agent fields. </param>
        public YauaaFieldAttribute(List<string> values)
        {
            this.Value = values.ToArray();
        }

        /// <summary>
        /// Gets the Value.
        /// </summary>
        public string[] Value { get; }
    }
}
