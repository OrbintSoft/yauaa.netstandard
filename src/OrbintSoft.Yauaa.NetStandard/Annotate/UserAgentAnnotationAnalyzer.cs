//-----------------------------------------------------------------------
// <copyright file="UserAgentAnnotationAnalyzer.cs" company="OrbintSoft">
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
    using System.Linq;
    using System.Reflection;
    using log4net;
    using OrbintSoft.Yauaa.Analyze;
    using OrbintSoft.Yauaa.Analyzer;

    /// <summary>
    /// This analyzer is used to parse an user agent and automap it's fields with a <see cref="YauaaFieldAttribute"/>.
    /// </summary>
    /// <typeparam name="T">The type to map.</typeparam>
    public class UserAgentAnnotationAnalyzer<T>
        where T : class
    {
        /// <summary>
        /// Defines the fields to be setted.
        /// </summary>
        private readonly IDictionary<string, IList<MethodInfo>> fieldSetters = new Dictionary<string, IList<MethodInfo>>();

        /// <summary>
        /// Defines the mapper.
        /// </summary>
        private IUserAgentAnnotationMapper<T> mapper = null;

        /// <summary>
        /// Defines the user agent analyzer.
        /// </summary>
        private UserAgentAnalyzer userAgentAnalyzer = null;

        /// <summary>
        /// Gets the cache size.
        /// </summary>
        public int CacheSize { get; private set; } = UserAgentAnalyzer.DEFAULT_PARSE_CACHE_SIZE;

        /// <summary>
        /// Diasble cache.
        /// </summary>
        public void DisableCaching()
        {
            this.SetCacheSize(0);
        }

        /// <summary>
        /// Initialize the analyzer.
        /// </summary>
        /// <param name="theMapper">The theMapper<see cref="IUserAgentAnnotationMapper{T}"/>.</param>
        public void Initialize(IUserAgentAnnotationMapper<T> theMapper)
        {
            this.mapper = theMapper;

            if (this.mapper is null)
            {
                throw new InvalidParserConfigurationException("[Initialize] The mapper instance is null.");
            }

            var classOfTArray = typeof(IUserAgentAnnotationMapper<T>).GenericTypeArguments;
            if (classOfTArray is null)
            {
                throw new InvalidParserConfigurationException("Couldn't find the used generic type of the UserAgentAnnotationMapper.");
            }

            var classOfT = classOfTArray[0];

            // Get all methods of the correct signature that have been annotated with YauaaField
            foreach (var method in theMapper.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (method.GetCustomAttribute(typeof(YauaaFieldAttribute)) is YauaaFieldAttribute field)
                {
                    var returnType = method.ReturnType;
                    var parameters = method.GetParameters().Select(p => p.ParameterType).ToArray();
                    if (returnType == typeof(void) && parameters.Length == 2 && parameters[0] == classOfT && parameters[1] == typeof(string))
                    {
                        if (!classOfT.IsVisible)
                        {
                            throw new InvalidParserConfigurationException($"The class {classOfT.Name} is not public.");
                        }

                        if (!method.IsPublic)
                        {
                            throw new InvalidParserConfigurationException($"Method annotated with YauaaField is not public: {method.Name}");
                        }

                        foreach (var fieldName in field.Value)
                        {
                            if (!this.fieldSetters.ContainsKey(fieldName))
                            {
                                this.fieldSetters[fieldName] = new List<MethodInfo>();
                            }

                            this.fieldSetters[fieldName].Add(method);
                        }
                    }
                    else
                    {
                        throw new InvalidParserConfigurationException(
                            $"In class [{this.mapper.GetType().Name}] the method [{method.Name}] has been annotated with YauaaField but it has the wrong method signature. It must look like [ public void {method.Name}({classOfT.Name} record, string value) ]");
                    }
                }
            }

            if (this.fieldSetters.Count == 0)
            {
                throw new InvalidParserConfigurationException("You MUST specify at least 1 field to extract.");
            }

            this.userAgentAnalyzer = UserAgentAnalyzer
            .NewBuilder()
            .HideMatcherLoadStats()
            .WithCache(this.CacheSize)
            .WithFields(this.fieldSetters.Keys)
            .DropTests()
            .ImmediateInitialization()
            .Build();
        }

        /// <summary>
        /// Maps the fields to class properties.
        /// </summary>
        /// <param name="record">The record to map.</param>
        /// <returns>The mapped record.</returns>
        public T Map(T record)
        {
            if (record == null)
            {
                return null;
            }

            if (this.mapper is null)
            {
                throw new InvalidParserConfigurationException("[Map] The mapper instance is null.");
            }

            var userAgent = this.userAgentAnalyzer.Parse(this.mapper.GetUserAgentString(record));

            foreach (var fieldSetter in this.fieldSetters)
            {
                var value = userAgent.GetValue(fieldSetter.Key);
                foreach (var method in fieldSetter.Value)
                {
                    try
                    {
                        method.Invoke(this.mapper, new object[] { record, value });
                    }
                    catch (Exception e)
                    {
                        throw new InvalidParserConfigurationException("A problem occurred while calling the requested setter", e);
                    }
                }
            }

            return record;
        }

        /// <summary>
        /// Sets the new size of the parsing cache.
        /// Note that this will also wipe the existing cache.
        /// </summary>
        /// <param name="newCacheSize">The size of the new LRU cache. As size of 0 will disable caching.</param>
        public void SetCacheSize(int newCacheSize)
        {
            this.CacheSize = newCacheSize > 0 ? newCacheSize : 0;
            if (this.userAgentAnalyzer != null)
            {
                this.userAgentAnalyzer.SetCacheSize(this.CacheSize);
            }
        }
    }
}
