//-----------------------------------------------------------------------
// <copyright file="AbstractUserAgentAnalyzerDirectBuilder.cs" company="OrbintSoft">
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
// <date>2020, 06, 08, 18:23</date>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Analyzer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using OrbintSoft.Yauaa.Calculate;
    using OrbintSoft.Yauaa.Utils;

    /// <summary>
    /// This is used to build a <see cref="AbstractUserAgentAnalyzerDirect"/>.
    /// </summary>
    /// <typeparam name="TUAA">the UserAgent Analyzer.</typeparam>
    /// <typeparam name="TB">the Builder.</typeparam>
    public abstract class AbstractUserAgentAnalyzerDirectBuilder<TUAA, TB>
            where TUAA : AbstractUserAgentAnalyzerDirect
            where TB : AbstractUserAgentAnalyzerDirectBuilder<TUAA, TB>
    {
        /// <summary>
        /// Defines the loaded yaml resources.
        /// </summary>
        private readonly IList<ResourcesPath> resources = new List<ResourcesPath>();

        /// <summary>
        /// Defines if analyzer has been built.
        /// </summary>
        private volatile bool didBuildStep = false;

        /// <summary>
        /// Defines number of iterations to be done to preheat the CLR.
        /// </summary>
        private int preheatIterations = 0;

        /// <summary>
        /// Defines the user agent analyzr to use.
        /// </summary>
        private TUAA uaa;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractUserAgentAnalyzerDirectBuilder{UAA, B}"/> class.
        /// </summary>
        /// <param name="newUaa">The newUaa.</param>
        protected AbstractUserAgentAnalyzerDirectBuilder(TUAA newUaa)
        {
            this.uaa = newUaa;
            this.uaa.SetShowMatcherStats(false);
            this.resources.Add(AbstractUserAgentAnalyzerDirect.DefaultResources);
        }

        /// <summary>
        /// Add a set of additional rules. Useful in handling specific cases.
        /// </summary>
        /// <param name="resourceString">resourceString The dirctory that contains the resources list that needs to be added.</param>
        /// <param name="filter">The filte (default *.yaml).</param>
        /// <returns>the current Builder instance.</returns>
        public TB AddResources(string resourceString, string filter = "*.yaml")
        {
            this.FailIfAlreadyBuilt();
            this.resources.Add(new ResourcesPath(resourceString, filter));
            return (TB)this;
        }

        /// <summary>
        /// Construct the analyzer and run the preheat (if requested).
        /// </summary>
        /// <returns>The User Agent Analyzer.</returns>
        public virtual TUAA Build()
        {
            this.FailIfAlreadyBuilt();

            // In case we only want specific fields we must all these special cases too
            if (this.uaa.WantedFieldNames != null)
            {
                // Special field that affects ALL fields.
                this.uaa.WantedFieldNames.Add(DefaultUserAgentFields.SET_ALL_FIELDS);

                // This is always needed to determine the Hacker fallback
                this.uaa.WantedFieldNames.Add(DefaultUserAgentFields.DEVICE_CLASS);
            }

            this.AddCalculatedConcatNONDuplicated(DefaultUserAgentFields.AGENT_NAME_VERSION_MAJOR, DefaultUserAgentFields.AGENT_NAME, DefaultUserAgentFields.AGENT_VERSION_MAJOR);
            this.AddCalculatedConcatNONDuplicated(DefaultUserAgentFields.AGENT_NAME_VERSION, DefaultUserAgentFields.AGENT_NAME, DefaultUserAgentFields.AGENT_VERSION);
            this.AddCalculatedMajorVersionField(DefaultUserAgentFields.AGENT_VERSION_MAJOR, DefaultUserAgentFields.AGENT_VERSION);

            this.AddCalculatedConcatNONDuplicated(DefaultUserAgentFields.WEBVIEW_APP_NAME_VERSION_MAJOR, DefaultUserAgentFields.WEBVIEW_APP_NAME, DefaultUserAgentFields.WEBVIEW_APP_VERSION_MAJOR);
            this.AddCalculatedMajorVersionField(DefaultUserAgentFields.WEBVIEW_APP_VERSION_MAJOR, DefaultUserAgentFields.WEBVIEW_APP_VERSION);

            this.AddCalculatedConcatNONDuplicated(DefaultUserAgentFields.LAYOUT_ENGINE_NAME_VERSION_MAJOR, DefaultUserAgentFields.LAYOUT_ENGINE_NAME, DefaultUserAgentFields.LAYOUT_ENGINE_VERSION_MAJOR);
            this.AddCalculatedConcatNONDuplicated(DefaultUserAgentFields.LAYOUT_ENGINE_NAME_VERSION, DefaultUserAgentFields.LAYOUT_ENGINE_NAME, DefaultUserAgentFields.LAYOUT_ENGINE_VERSION);
            this.AddCalculatedMajorVersionField(DefaultUserAgentFields.LAYOUT_ENGINE_VERSION_MAJOR, DefaultUserAgentFields.LAYOUT_ENGINE_VERSION);

            this.AddCalculatedMajorVersionField(DefaultUserAgentFields.OPERATING_SYSTEM_NAME_VERSION_MAJOR, DefaultUserAgentFields.OPERATING_SYSTEM_NAME_VERSION);
            this.AddCalculatedConcatNONDuplicated(DefaultUserAgentFields.OPERATING_SYSTEM_NAME_VERSION, DefaultUserAgentFields.OPERATING_SYSTEM_NAME, DefaultUserAgentFields.OPERATING_SYSTEM_VERSION);
            this.AddCalculatedMajorVersionField(DefaultUserAgentFields.OPERATING_SYSTEM_VERSION_MAJOR, DefaultUserAgentFields.OPERATING_SYSTEM_VERSION);

            if (this.uaa.IsWantedField(DefaultUserAgentFields.NETWORK_TYPE))
            {
                this.uaa.FieldCalculators.Add(new CalculateNetworkType());
            }

            if (this.uaa.IsWantedField(DefaultUserAgentFields.DEVICE_NAME))
            {
                this.uaa.FieldCalculators.Add(new CalculateDeviceName());
                this.AddSpecialDependencies(DefaultUserAgentFields.DEVICE_NAME, DefaultUserAgentFields.DEVICE_BRAND);
            }

            if (this.uaa.IsWantedField(DefaultUserAgentFields.DEVICE_BRAND))
            {
                this.uaa.FieldCalculators.Add(new CalculateDeviceBrand());

                // If we do not have a Brand we try to extract it from URL/Email iff present.
                this.AddSpecialDependencies(DefaultUserAgentFields.DEVICE_BRAND, DefaultUserAgentFields.AGENT_INFORMATION_URL, DefaultUserAgentFields.AGENT_INFORMATION_EMAIL);
            }

            if (this.uaa.IsWantedField(DefaultUserAgentFields.AGENT_INFORMATION_EMAIL))
            {
                this.uaa.FieldCalculators.Add(new CalculateAgentEmail());
            }

            this.uaa.FieldCalculators = this.uaa.FieldCalculators.Reverse().ToList();

            var mustDropTestsLater = !this.uaa.WillKeepTests;
            if (this.preheatIterations != 0)
            {
                this.uaa.KeepTests();
            }

            this.uaa.Initialize(this.resources);
            if (this.preheatIterations < 0)
            {
                this.uaa.PreHeat();
            }
            else
            {
                if (this.preheatIterations > 0)
                {
                    this.uaa.PreHeat(this.preheatIterations);
                }
            }

            if (mustDropTestsLater)
            {
                this.uaa.DropTests();
            }

            this.didBuildStep = true;
            return this.uaa;
        }

        /// <summary>
        /// Load all patterns and rules but do not yet build the lookup hashMaps yet.
        /// For the engine to run these lookup hashMaps are needed so they will be constructed once "just in time".
        /// </summary>
        /// <returns>The builder.</returns>
        public TB DelayInitialization()
        {
            this.FailIfAlreadyBuilt();
            this.uaa.DelayInitialization();
            return (TB)this;
        }

        /// <summary>
        /// Drop the default set of rules. Useful in parsing ONLY company specific useragents.
        /// </summary>
        /// <returns>the current Builder instance.</returns>
        public TB DropDefaultResources()
        {
            this.FailIfAlreadyBuilt();
            this.resources.Remove(AbstractUserAgentAnalyzerDirect.DefaultResources);
            return (TB)this;
        }

        /// <summary>
        /// Remove all testcases in memory after initialization.
        /// </summary>
        /// <returns>The builder.</returns>
        public TB DropTests()
        {
            this.FailIfAlreadyBuilt();
            this.uaa.DropTests();
            return (TB)this;
        }

        /// <summary>
        /// Set the stats logging during the startup of the analyzer back to the default of "minimal".
        /// </summary>
        /// <returns>The builder.</returns>
        public TB HideMatcherLoadStats()
        {
            this.FailIfAlreadyBuilt();
            this.uaa.SetShowMatcherStats(false);
            return (TB)this;
        }

        /// <summary>
        /// Load all patterns and rules and immediately build the lookup hashMaps.
        /// </summary>
        /// <returns>the current Builder instance.</returns>
        public TB ImmediateInitialization()
        {
            this.FailIfAlreadyBuilt();
            this.uaa.ImmediateInitialization();
            return (TB)this;
        }

        /// <summary>
        /// Retain all testcases in memory after initialization.
        /// </summary>
        /// <returns>The builder.</returns>
        public TB KeepTests()
        {
            this.FailIfAlreadyBuilt();
            this.uaa.KeepTests();
            return (TB)this;
        }

        /// <summary>
        /// Use the available testcases to preheat the jvm on this analyzer.
        /// All available testcases will be run exactly once.
        /// </summary>
        /// <returns>the current Builder instance.</returns>
        public TB Preheat()
        {
            this.FailIfAlreadyBuilt();
            this.preheatIterations = -1;
            return (TB)this;
        }

        /// <summary>
        /// Use the available testcases to preheat the jvm on this analyzer.
        /// </summary>
        /// <param name="iterations">iterations How many testcases must be run.</param>
        /// <returns>the current Builder instance.</returns>
        public TB Preheat(int iterations)
        {
            this.FailIfAlreadyBuilt();
            this.preheatIterations = iterations;
            return (TB)this;
        }

        /// <summary>
        /// Log additional information during the startup of the analyzer.
        /// </summary>
        /// <returns>The builder.</returns>
        public TB ShowMatcherLoadStats()
        {
            this.FailIfAlreadyBuilt();
            this.uaa.SetShowMatcherStats(true);
            return (TB)this;
        }

        /// <summary>
        /// Specify that we simply want to retrieve all possible fields.
        /// </summary>
        /// <returns>The builder.</returns>
        public TB WithAllFields()
        {
            this.FailIfAlreadyBuilt();
            this.uaa.WantedFieldNames = null;
            return (TB)this;
        }

        /// <summary>
        /// Specify an additional field that we want to retrieve.
        /// </summary>
        /// <param name="fieldName">The name of the additional field.</param>
        /// <returns>the current Builder instance.</returns>
        public TB WithField(string fieldName)
        {
            this.FailIfAlreadyBuilt();
            if (this.uaa.WantedFieldNames == null)
            {
                this.uaa.WantedFieldNames = new HashSet<string>();
            }

            this.uaa.WantedFieldNames.Add(fieldName);

            return (TB)this;
        }

        /// <summary>
        /// Specify a set of additional fields that we want to retrieve.
        /// </summary>
        /// <param name="fieldNames">The collection of names of the additional fields.</param>
        /// <returns>the current Builder instance.</returns>
        public TB WithFields(ICollection<string> fieldNames)
        {
            foreach (var fieldName in fieldNames)
            {
                this.WithField(fieldName);
            }

            return (TB)this;
        }

        /// <summary>
        /// Specify a set of additional fields that we want to retrieve.
        /// </summary>
        /// <param name="fieldNames">The fieldNames.</param>
        /// <returns>the current Builder instance.</returns>
        public TB WithFields(params string[] fieldNames)
        {
            foreach (var fieldName in fieldNames)
            {
                this.WithField(fieldName);
            }

            return (TB)this;
        }

        /// <summary>
        /// Set maximum length of a useragent for it to be classified as Hacker without any analysis.
        /// </summary>
        /// <param name="newUserAgentMaxLength">The newUserAgentMaxLength<see cref="int"/>.</param>
        /// <returns>The builder.</returns>
        public TB WithUserAgentMaxLength(int newUserAgentMaxLength)
        {
            this.FailIfAlreadyBuilt();
            this.uaa.SetUserAgentMaxLength(newUserAgentMaxLength);
            return (TB)this;
        }

        /// <summary>
        /// Set the User agent Analyzer to be used.
        /// </summary>
        /// <param name="a">The User Agent Analyzer.</param>
        internal void SetUAA(TUAA a)
        {
            this.uaa = a;
        }

        /// <summary>
        /// Throws an exception if the analyzer has already been built.
        /// </summary>
        protected void FailIfAlreadyBuilt()
        {
            if (this.didBuildStep)
            {
                throw new Exception("A builder can provide only a single instance. It is not allowed to set values after doing build()");
            }
        }

        /// <summary>
        /// Add custom dependencies.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="dependencies">The dependencies.</param>
        private void AddSpecialDependencies(string result, params string[] dependencies)
        {
            if (this.uaa.IsWantedField(result))
            {
                if (this.uaa.WantedFieldNames != null)
                {
                    foreach (var item in dependencies)
                    {
                        this.uaa.WantedFieldNames.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// Add a calculator to calculate major version.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="dependency">The dependency.</param>
        private void AddCalculatedMajorVersionField(string result, string dependency)
        {
            if (this.uaa.IsWantedField(result))
            {
                this.uaa.FieldCalculators.Add(new MajorVersionCalculator(result, dependency));
                if (this.uaa.WantedFieldNames != null)
                {
                    this.uaa.WantedFieldNames.Add(dependency);
                }
            }
        }

        /// <summary>
        /// dd a calculator to calculate concatenated fields.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="first">The first field to concatenate.</param>
        /// <param name="second">The second field to concatenate.</param>
        private void AddCalculatedConcatNONDuplicated(string result, string first, string second)
        {
            if (this.uaa.IsWantedField(result))
            {
                this.uaa.FieldCalculators.Add(new ConcatNONDuplicatedCalculator(result, first, second));
                if (this.uaa.WantedFieldNames != null)
                {
                    this.uaa.WantedFieldNames.Add(first);
                    this.uaa.WantedFieldNames.Add(second);
                }
            }
        }
    }
}
