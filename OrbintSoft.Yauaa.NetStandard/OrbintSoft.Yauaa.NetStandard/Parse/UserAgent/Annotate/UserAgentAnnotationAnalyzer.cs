/*
 * Yet Another UserAgent Analyzer .NET Standard
 * Porting realized by Balzarotti Stefano, Copyright (C) OrbintSoft
 * 
 * Original Author and License:
 * 
 * Yet Another UserAgent Analyzer
 * Copyright (C) 2013-2018 Niels Basjes
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * All rights should be reserved to the original author Niels Basjes
 */

using log4net;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgent.Analyze;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgent.Annotate
{
    public class UserAgentAnnotationAnalyzer<T> where T: class
    {
        private IUserAgentAnnotationMapper<T> mapper = null;
        private UserAgentAnalyzer userAgentAnalyzer = null;
        private static readonly ILog LOG = LogManager.GetLogger(typeof(UserAgentAnnotationAnalyzer<T>));

        private readonly Dictionary<string, List<MethodInfo>> fieldSetters = new Dictionary<string, List<MethodInfo>>();


        public void Initialize(IUserAgentAnnotationMapper<T> theMapper)
        {
            mapper = theMapper;

            if (mapper == null)
            {
                throw new ArgumentNullException("[Initialize] The mapper instance is null.");
            }

            Type[] classOfTArray = mapper.GetType().GenericTypeArguments;
            if (classOfTArray == null || classOfTArray[0] == null)
            {
                throw new NullReferenceException("Couldn't find the used generic type of the UserAgentAnnotationMapper.");
            }

            Type classOfT = classOfTArray[0];

            var anonymous = false;
            // Get all methods of the correct signature that have been annotated with YauaaField
            foreach (MethodInfo method in theMapper.GetType().GetMethods())
            {
                if (method.GetCustomAttribute(typeof(YauaaFieldAttribute)) is YauaaFieldAttribute field)
                {
                    Type returnType = method.ReturnType;
                    Type[] parameters = method.GetParameters().Select(p => p.GetType()).ToArray();
                    if (returnType == typeof(void) && parameters.Length == 2 && parameters[0] == classOfT && parameters[1] == typeof(string))
                    {
                        if (!method.IsPublic || !classOfT.IsPublic)
                        {
                            throw new InvalidParserConfigurationException("Method annotated with YauaaField is not public: " + method.Name);
                        }

                        if (anonymous)
                        {
                            string methodName =
                                method.ReturnType.Name + " " +
                                    method.Name + "(" + parameters[0].Name + " ," + parameters[1].Name + ");";
                            LOG.Warn(string.Format("Trying to make anonymous {0} {1} accessible.", theMapper.GetType().Name, methodName));
                            //method.setAccessible(true); NOT POSSIBLE IN C#
                        }

                        foreach (string fieldName in field.Value)
                        {
                            if (!fieldSetters.ContainsKey(fieldName))
                            {
                                fieldSetters[fieldName] = new List<MethodInfo>();
                            }
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
            if (fieldSetters.Count != 0)
            {
                builder.WithFields(fieldSetters.Keys.ToList());
            }
            userAgentAnalyzer = builder.Build();
        }

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
                        throw new InvalidParserConfigurationException("Couldn't call the requested setter", e);
                    }
                }
            }
            return record;
        }
        
    }
    
}
