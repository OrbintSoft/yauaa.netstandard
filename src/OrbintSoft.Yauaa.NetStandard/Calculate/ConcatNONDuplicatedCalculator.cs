//-----------------------------------------------------------------------
// <copyright file="ConcatNONDuplicatedCalculator.cs" company="OrbintSoft">
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
// <date>2020, 04, 16, 08:50</date>
namespace OrbintSoft.Yauaa.Calculate
{
    using System;
    using OrbintSoft.Yauaa.Analyzer;

    /// <summary>
    /// ConcatNONDuplicatedCalculator.
    /// </summary>
    [Serializable]
    public class ConcatNONDuplicatedCalculator : IFieldCalculator
    {
        private readonly string targetName;
        private readonly string firstName;
        private readonly string secondName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcatNONDuplicatedCalculator"/> class.
        /// </summary>
        /// <param name="targetName">targetName.</param>
        /// <param name="firstName">firstName.</param>
        /// <param name="secondName">secondName.</param>
        public ConcatNONDuplicatedCalculator(string targetName, string firstName, string secondName)
        {
            this.targetName = targetName;
            this.firstName = firstName;
            this.secondName = secondName;
        }

        /// <inheritdoc/>
        public void Calculate(UserAgent userAgent)
        {
            var firstField = userAgent.Get(this.firstName);
            var secondField = userAgent.Get(this.secondName);

            string first = null;
            long firstConfidence = -1;
            string second = null;
            long secondConfidence = -1;

            if (firstField != null)
            {
                first = firstField.GetValue();
                firstConfidence = firstField.GetConfidence();
            }

            if (secondField != null)
            {
                second = secondField.GetValue();
                secondConfidence = secondField.GetConfidence();
            }

            if (first == null && second == null)
            {
                return; // Nothing to do
            }

            if (second == null)
            {
                if (firstConfidence >= 0)
                {
                    userAgent.Set(this.targetName, first, firstConfidence);
                }
                else
                {
                    userAgent.SetForced(this.targetName, "Unknown", firstConfidence);
                }

                return; // Nothing to do
            }
            else
            {
                if (first == null)
                {
                    if (secondConfidence >= 0)
                    {
                        userAgent.Set(this.targetName, second, secondConfidence);
                    }
                    else
                    {
                        userAgent.SetForced(this.targetName, "Unknown", secondConfidence);
                    }

                    return;
                }
            }

            if (first.Equals(second))
            {
                userAgent.Set(this.targetName, first, firstConfidence);
            }
            else
            {
                if (second.StartsWith(first))
                {
                    userAgent.Set(this.targetName, second, secondConfidence);
                }
                else
                {
                    var value = first + " " + second;
                    long confidence = Math.Max(firstConfidence, secondConfidence);
                    if (confidence < 0)
                    {
                        userAgent.SetForced(this.targetName, value, confidence);
                    }
                    else
                    {
                        userAgent.Set(this.targetName, value, confidence);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Calculate [{this.firstName} + {this.secondName}] --> {this.targetName}";
        }
    }
}
