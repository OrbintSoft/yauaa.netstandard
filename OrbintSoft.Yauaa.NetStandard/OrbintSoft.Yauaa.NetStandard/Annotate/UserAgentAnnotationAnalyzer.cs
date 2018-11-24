//<copyright file="UserAgentAnnotationAnalyzer.cs" company="OrbintSoft">
//	Yet Another UserAgent Analyzer.NET Standard
//	Porting realized by Stefano Balzarotti, Copyright (C) OrbintSoft
//
//	Original Author and License:
//
//	Yet Another UserAgent Analyzer
//	Copyright(C) 2013-2018 Niels Basjes
//
//	Licensed under the Apache License, Version 2.0 (the "License");
//	you may not use this file except in compliance with the License.
//	You may obtain a copy of the License at
//
//	https://www.apache.org/licenses/LICENSE-2.0
//
//	Unless required by applicable law or agreed to in writing, software
//	distributed under the License is distributed on an "AS IS" BASIS,
//	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//	See the License for the specific language governing permissions and
//	limitations under the License.
//
//</copyright>
//<author>Stefano Balzarotti, Niels Basjes</author>
//<date>2018, 8, 16, 11:26</date>
//<summary></summary>

namespace OrbintSoft.Yauaa.Annotate
{
    using log4net;
    using OrbintSoft.Yauaa.Analyze;
    using OrbintSoft.Yauaa.Analyzer;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Defines the <see cref="UserAgentAnnotationAnalyzer{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UserAgentAnnotationAnalyzer<T> where T : class
    {
        /// <summary>
        /// Defines the Log
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(UserAgentAnnotationAnalyzer<T>));

        /// <summary>
        /// Defines the fieldSetters
        /// </summary>
        private readonly IDictionary<string, IList<MethodInfo>> fieldSetters = new Dictionary<string, IList<MethodInfo>>();

        /// <summary>
        /// Defines the mapper
        /// </summary>
        private IUserAgentAnnotationMapper<T> mapper = null;

        /// <summary>
        /// Defines the userAgentAnalyzer
        /// </summary>
        private UserAgentAnalyzer userAgentAnalyzer = null;

        /// <summary>
        /// The Initialize
        /// </summary>
        /// <param name="theMapper">The theMapper<see cref="IUserAgentAnnotationMapper{T}"/></param>
        public void Initialize(IUserAgentAnnotationMapper<T> theMapper)
        {
            mapper = theMapper;

            if (mapper == null)
            {
                throw new InvalidParserConfigurationException("[Initialize] The mapper instance is null.");
            }

            Type[] classOfTArray = typeof(IUserAgentAnnotationMapper<T>).GenericTypeArguments;
            if (classOfTArray == null)
            {
                throw new InvalidParserConfigurationException("Couldn't find the used generic type of the UserAgentAnnotationMapper.");
            }

            Type classOfT = classOfTArray[0];

            var anonymous = false;
            // Get all methods of the correct signature that have been annotated with YauaaField
            foreach (MethodInfo method in theMapper.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (method.GetCustomAttribute(typeof(YauaaFieldAttribute)) is YauaaFieldAttribute field)
                {
                    Type returnType = method.ReturnType;
                    Type[] parameters = method.GetParameters().Select(p => p.ParameterType).ToArray();
                    if (returnType == typeof(void) && parameters.Length == 2 && parameters[0] == classOfT && parameters[1] == typeof(string))
                    {
                        if (!classOfT.IsVisible)
                        {
                            throw new InvalidParserConfigurationException("The class " + classOfT.Name + " is not public.");
                        }

                        if (!method.IsPublic)
                        {
                            throw new InvalidParserConfigurationException("Method annotated with YauaaField is not public: " + method.Name);
                        }

                        if (anonymous)
                        {
                            string methodName =
                                method.ReturnType.Name + " " +
                                    method.Name + "(" + parameters[0].Name + " ," + parameters[1].Name + ");";
                            Log.Warn(string.Format("Trying to make anonymous {0} {1} accessible.", theMapper.GetType().Name, methodName));
                            //method.setAccessible(true); NOT POSSIBLE IN C#
                        }

                        foreach (string fieldName in field.Value)
                        {
                            if (!fieldSetters.ContainsKey(fieldName))
                            {
                                fieldSetters[fieldName] = new List<MethodInfo>();
                            }
                            fieldSetters[fieldName].Add(method);
                        }
                    }
                    else
                    {
                        throw new InvalidParserConfigurationException(
                            "In class [" + mapper.GetType().Name + "] the method [" + method.Name + "] " +
                            "has been annotated with YauaaField but it has the wrong method signature. " +
                            "It must look like [ public void " + method.Name + "(" + classOfT.Name + " record, String value) ]");
                    }
                }
            }

            if (fieldSetters.Count == 0)
            {
                throw new InvalidParserConfigurationException("You MUST specify at least 1 field to extract.");
            }

            var builder = UserAgentAnalyzer.NewBuilder();
            builder.HideMatcherLoadStats();
            builder.WithFields(fieldSetters.Keys.ToList());
            userAgentAnalyzer = builder.Build();
        }

        /// <summary>
        /// The Map
        /// </summary>
        /// <param name="record">The record<see cref="T"/></param>
        /// <returns>The <see cref="T"/></returns>
        public T Map(T record)
        {
            if (record == null)
            {
                return null;
            }
            if (mapper == null)
            {
                throw new InvalidParserConfigurationException("[Map] The mapper instance is null.");
            }

            UserAgent userAgent = userAgentAnalyzer.Parse(mapper.GetUserAgentString(record));

            foreach (var fieldSetter in fieldSetters)
            {
                string value = userAgent.GetValue(fieldSetter.Key);
                foreach (MethodInfo method in fieldSetter.Value)
                {
                    try
                    {
                        method.Invoke(mapper, new object[] { record, value });
                    }
                    catch (Exception e)
                    {
                        throw new InvalidParserConfigurationException("A problem occurred while calling the requested setter", e);
                    }
                }
            }
            return record;
        }
    }
}
