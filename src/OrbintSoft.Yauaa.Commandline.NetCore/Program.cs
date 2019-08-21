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
//   
// </copyright>
// <author>Stefano Balzarotti, Niels Basjes</author>
// <date>2018, 11, 25, 18:21</date>
//-----------------------------------------------------------------------
using CommandLine;
using log4net;
using OrbintSoft.Yauaa.Analyzer;
using OrbintSoft.Yauaa.Debug;
using OrbintSoft.Yauaa.Parse;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

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
            CSV, JSON, YAML, XML, UNSUPPORTED
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
                    writer.WriteLine("Not supported yet.");
                    break;
                default:
                    break;
            }
        }

        public static void Main(string[] args)
        {
            var commandlineOptions = new Options();
            var parser = new Parser();
            try
            {                
                parser.ParseArguments<Options>(args);
                var outputFormat = OutputFormat.YAML;
                if (commandlineOptions.CsvFormat)
                {
                    outputFormat = OutputFormat.CSV;
                }
                else if (commandlineOptions.JsonFormat)
                {
                    outputFormat = OutputFormat.JSON;
                }
                else if(commandlineOptions.XmlFormat)
                {
                    outputFormat = OutputFormat.XML;
                }

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

                IList<string> fields;
                if (commandlineOptions.Fields == null)
                {
                    fields = uaa.GetAllPossibleFieldNamesSorted();
                    fields.Add(UserAgent.USERAGENT_FIELDNAME);
                }
                else
                {
                    fields = commandlineOptions.Fields;
                }
                //PrintHeader(outputFormat, fields);

                if (commandlineOptions.Useragent != null)
                {
                    var agent = uaa.Parse(commandlineOptions.Useragent);
                    //PrintAgent(outputFormat, fields, agent);
                    return;
                }

                // Open the file (or stdin)
                Stream inputStream;
                if (commandlineOptions.InFile != null)
                {
                    inputStream = new FileStream(commandlineOptions.InFile, FileMode.Open);
                }
                else
                {
                    var input = Console.ReadLine();
                    var bytes = Encoding.UTF8.GetBytes(input);
                    inputStream = new MemoryStream(bytes);
                }
                var sw = new StreamWriter(Console.OpenStandardOutput())
                {
                    AutoFlush = true
                };
                Console.SetOut(sw);
                var flattenPrinter = new UserAgentTreeFlattener(new FlattenPrinter(sw));
                using (var br = new StreamReader(inputStream))
                {
                    string strLine;

                    long ambiguities = 0;
                    long syntaxErrors = 0;

                    long linesTotal = 0;
                    long hitsTotal = 0;
                    long linesOk = 0;
                    long hitsOk = 0;
                    long linesMatched = 0;
                    long hitsMatched = 0;
                    Stopwatch stopwatch = Stopwatch.StartNew();

                    Stopwatch segmentStopwatch = Stopwatch.StartNew();
                    long segmentStartLines = linesTotal;

                    //Read File Line By Line
                    while ((strLine = br.ReadLine()) != null)
                    {
                        if (strLine.StartsWith(" ") || strLine.StartsWith("#") || strLine.Length == 0)
                        {
                            continue;
                        }

                        long hits = 1;
                        string agentStr = strLine;

                        if (strLine.Contains("\t"))
                        {
                            string[] parts = strLine.Split("\t", 2);
                            if (long.TryParse(parts[0], out hits))
                            {
                                agentStr = parts[1];
                            }
                            else
                            {
                                agentStr = strLine;
                            }
                        }

                        if (commandlineOptions.FullFlatten)
                        {
                            flattenPrinter.Parse(agentStr);
                            continue;
                        }

                        if (commandlineOptions.MatchedFlatten)
                        {
                            foreach (var match in uaa.GetUsedMatches(new UserAgent(agentStr)))
                            {
                                Console.WriteLine(match.Key + " " + match.Value);
                            }
                            continue;
                        }

                        UserAgent agent = uaa.Parse(agentStr);

                        bool hasBad = false;
                        foreach (string field in UserAgent.StandardFields)
                        {
                            if (agent.GetConfidence(field) < 0)
                            {
                                hasBad = true;
                                break;
                            }
                        }

                        linesTotal++;
                        hitsTotal += hits;

                        if (agent.HasSyntaxError)
                        {
                            if (outputFormat == OutputFormat.YAML)
                            {
                                Console.WriteLine("# Syntax error: " + agentStr);
                            }
                        }
                        else
                        {
                            linesOk++;
                            hitsOk += hits;
                        }

                        if (!hasBad)
                        {
                            linesMatched++;
                            hitsMatched += hits;
                        }

                        if (agent.HasAmbiguity)
                        {
                            ambiguities++;
                        }
                        if (agent.HasSyntaxError)
                        {
                            syntaxErrors++;
                        }

                        if (linesTotal % 1000 == 0)
                        {
                            long speed = (linesTotal - segmentStartLines) / (stopwatch.ElapsedMilliseconds);
                            Console.WriteLine(
                                string.Format("Lines = {0} (Ambiguities: {1} ; SyntaxErrors: {2}) Analyze speed = {3}/ms.",
                                    linesTotal, ambiguities, syntaxErrors, speed));
                            segmentStopwatch.Reset();
                            segmentStartLines = linesTotal;
                            ambiguities = 0;
                            syntaxErrors = 0;
                        }

                        if (commandlineOptions.OutputOnlyBadResults)
                        {
                            if (hasBad)
                            {
                                continue;
                            }
                        }

                        //PrintAgent(outputFormat, fields, agent);
                    }

                    Log.Info("-------------------------------------------------------------");
                    Log.Info(string.Format("Performance: {0} in {1} ms --> {2}/ms", linesTotal, stopwatch.ElapsedMilliseconds, linesTotal / stopwatch.ElapsedMilliseconds));
                    Log.Info("-------------------------------------------------------------");
                    Log.Info(string.Format("Parse results of {0} lines", linesTotal));
                    Log.Info(string.Format("Parsed without error: {0} ({1})", linesOk, 100.0 * linesOk / linesTotal));
                    Log.Info(string.Format("Parsed with    error: {0} ({1})", linesTotal - linesOk, 100.0 * (linesTotal - linesOk) / linesTotal));
                    Log.Info(string.Format("Fully matched       : {0} ({1})", linesMatched, 100.0 * linesMatched / linesTotal));

                    if (linesTotal != hitsTotal)
                    {
                        Log.Info("-------------------------------------------------------------");
                        Log.Info(string.Format("Parse results of {0} hits", hitsTotal));
                        Log.Info(string.Format("Parsed without error: %8d (=%6.2f%%)",
                            hitsOk, 100.0 * hitsOk / hitsTotal));
                        Log.Info(string.Format("Parsed with    error: %8d (=%6.2f%%)",
                            hitsTotal - hitsOk, 100.0 * (hitsTotal - hitsOk) / hitsTotal));
                        Log.Info(string.Format("Fully matched       : %8d (=%6.2f%%)",
                            hitsMatched, 100.0 * hitsMatched / hitsTotal));
                        Log.Info("-------------------------------------------------------------");
                    }
                }
            } catch (Exception e )
            {
                Log.Error(string.Format("IOException: {0}", e));               
            }
#if DEBUG
            Console.ReadKey();
#endif
        }
    }
}
