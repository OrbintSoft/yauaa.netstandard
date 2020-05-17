//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="OrbintSoft">
//    Yet Another User Agent Analyzer for .NET Standard
//    porting realized by Stefano Balzarotti, Copyright 2018-2019 (C) OrbintSoft
//
//    Original Author and License:
//
//    Yet Another UserAgent Analyzer
//    Copyright(C) 2013-2019 Niels Basjes
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//    https://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// </copyright>
// <author>Stefano Balzarotti, Niels Basjes</author>
// <date>2018, 11, 25, 18:21</date>
//-----------------------------------------------------------------------
using CommandLine;
using CommandLine.Text;
using log4net;
using OrbintSoft.Yauaa.Analyzer;
using OrbintSoft.Yauaa.Debug;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace OrbintSoft.Yauaa.Commandline
{
    /// <summary>
    /// This is a console application program to use yauaa.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Defines the logger.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        /// <summary>
        /// Supported output format.
        /// </summary>
        private enum OutputFormat
        {
            TEXT,
            CSV,
            JSON,
            YAML,
            XML,
            UNSUPPORTED
        }

        /// <summary>
        /// Prints the headers in case the output is a CSV.
        /// </summary>
        /// <param name="writer">The writer to print the output (console, file..)</param>
        /// <param name="outputFormat">The output format.</param>
        /// <param name="fields">The list of fields to be printed</param>
        private static void PrintHeader(TextWriter writer, OutputFormat outputFormat, IEnumerable<string> fields)
        {
            switch (outputFormat)
            {
                case OutputFormat.CSV:
                    var doSeparator = false;
                    foreach (var field in fields)
                    {
                        if (doSeparator)
                        {
                            writer.Write("\t");
                        }
                        else
                        {
                            doSeparator = true;
                        }
                        writer.Write(field);
                    }
                    writer.WriteLine();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Prints the parsed user agent.
        /// </summary>
        /// <param name="writer">The writer to print the output (console, file..)</param>
        /// <param name="outputFormat">The output format(CSV, XML, JSON...)</param>
        /// <param name="fields">The list of fields to be printed.</param>
        /// <param name="agent">The parsed useragent.</param>
        private static void PrintAgent(TextWriter writer, OutputFormat outputFormat, IList<string> fields, UserAgent agent)
        {
            switch (outputFormat)
            {
                case OutputFormat.CSV:
                    var doSeparator = false;
                    foreach (var field in fields)
                    {
                        if (doSeparator)
                        {
                            writer.Write("\t");
                        }
                        else
                        {
                            doSeparator = true;
                        }
                        var value = agent.GetValue(field);
                        if (value != null)
                        {
                            writer.Write(value);
                        }
                    }
                    writer.WriteLine();
                    break;
                case OutputFormat.JSON:
                    writer.WriteLine(agent.ToJson(fields));
                    break;
                case OutputFormat.YAML:
                    writer.WriteLine(agent.ToYamlTestCase());
                    break;
                case OutputFormat.XML:
                    writer.WriteLine(agent.ToXML(fields));
                    break;
                case OutputFormat.TEXT:
                    writer.WriteLine(agent.ToString(fields));
                    break;
                case OutputFormat.UNSUPPORTED:                
                default:
                    writer.WriteLine("Not supported yet.");
                    break;
            }
        }

        private static IEnumerable<string> GetUserAgents(Options commandlineOptions)
        {
            if (!string.IsNullOrWhiteSpace(commandlineOptions.Useragent))
            {
                yield return commandlineOptions.Useragent;
            }
            else
            {
                var inputFile = commandlineOptions.InFile;
                if (!File.Exists(inputFile))
                {
                    throw new FileNotFoundException(inputFile);
                }
                else
                {
                    using (var reader = new StreamReader(inputFile))
                    {
                        while (!reader.EndOfStream)
                        {
                            yield return reader.ReadLine();
                        }
                    }
                }
            }
        }

        public static void Main(string[] args)
        {
            using (var parser = new Parser(w => { w.EnableDashDash = true; }))
            {
                var result = parser.ParseArguments<Options>(args);
                result.WithParsed(commandlineOptions =>
                {
                    Execute(commandlineOptions);
                });
                result.WithNotParsed(errors =>
               {
                   var helpText = HelpText.AutoBuild(result, h =>
                   {
                       // Configure HelpText here  or create your own and return it 
                       h.AdditionalNewLineAfterOption = false;
                       return HelpText.DefaultParsingErrorsHandler(result, h);
                   }, e =>
                   {

                       return e;
                   });
                   Console.WriteLine(helpText);
               });
            }
            if (Debugger.IsAttached)
            {
                Console.ReadKey();
            }
        }

        private static void Execute(Options commandlineOptions)
        {
            var outputFormat = SetOutputFormat(commandlineOptions);
            var uaa = CreateAndConfigureParser(commandlineOptions);
            var fields = GetFields(commandlineOptions, uaa);
            var userAgentStrings = GetUserAgents(commandlineOptions);
            var stream = GetStream(commandlineOptions);

            PrintHeader(stream, outputFormat, fields);
            foreach (var us in userAgentStrings)
            {
                var userAgent = uaa.Parse(us);
                PrintAgent(stream, outputFormat, fields, userAgent);
            }

            if (!string.IsNullOrWhiteSpace(commandlineOptions.OutFile))
            {
                stream.Dispose(); //I dispose file stream, console should not be disposed
            }            
        }

        private static StreamWriter GetStream(Options commandlineOptions)
        {
            if (!string.IsNullOrWhiteSpace(commandlineOptions.OutFile))
            {
                return new StreamWriter(commandlineOptions.OutFile);
            } else
            {
                var sw = new StreamWriter(Console.OpenStandardOutput())
                {
                    AutoFlush = true
                };
                Console.SetOut(sw);
                return sw;
            }
        }       

        private static IList<string> GetFields(Options commandlineOptions, UserAgentAnalyzerTester uaa)
        {
            IList<string> fields;
            if (commandlineOptions.Fields == null || !commandlineOptions.Fields.Any())
            {
                fields = uaa.GetAllPossibleFieldNamesSorted();
                fields.Add(DefaultUserAgentFields.USERAGENT_FIELDNAME);
            }
            else
            {
                fields = commandlineOptions.Fields;
            }

            return fields;
        }

        private static UserAgentAnalyzerTester CreateAndConfigureParser(Options commandlineOptions)
        {
            var builder = UserAgentAnalyzerTester.NewBuilder();
            builder.HideMatcherLoadStats();
            builder.WithCache(commandlineOptions.CacheSize);
            if (commandlineOptions.Fields != null)
            {
                foreach (var field in commandlineOptions.Fields)
                {
                    builder.WithField(field);
                }
            }
            var uaa = builder.Build() as UserAgentAnalyzerTester;
            return uaa;
        }

        private static OutputFormat SetOutputFormat(Options commandlineOptions)
        {
            var outputFormat = OutputFormat.TEXT;
            if (commandlineOptions.CsvFormat)
            {
                outputFormat = OutputFormat.CSV;
            }
            else if (commandlineOptions.JsonFormat)
            {
                outputFormat = OutputFormat.JSON;
            }
            else if (commandlineOptions.XmlFormat)
            {
                outputFormat = OutputFormat.XML;
            }
            else if (commandlineOptions.YamlFormat)
            {
                outputFormat = OutputFormat.YAML;
            }
            return outputFormat;
        }
    }
}
