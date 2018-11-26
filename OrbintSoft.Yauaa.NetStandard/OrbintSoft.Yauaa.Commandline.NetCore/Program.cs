using CommandLine;
using log4net;
using OrbintSoft.Yauaa.Analyzer;
using OrbintSoft.Yauaa.Debug;
using OrbintSoft.Yauaa.Parse;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace OrbintSoft.Yauaa.Commandline
{
    public class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        private enum OutputFormat
        {
            CSV, JSON, YAML
        }

        private static void PrintHeader(OutputFormat outputFormat, ICollection<string> fields)
        {
            switch (outputFormat)
            {
                case OutputFormat.CSV:
                    bool doSeparator = false;
                    foreach (string field in fields)
                    {
                        if (doSeparator)
                        {
                            Console.Write("\t");
                        }
                        else
                        {
                            doSeparator = true;
                        }
                        Console.Write(field);
                    }
                    Console.WriteLine();
                    break;
                default:
                    break;
            }
        }

        private static void PrintAgent(OutputFormat outputFormat, ICollection<string> fields, UserAgent agent)
        {
            switch (outputFormat)
            {
                case OutputFormat.CSV:
                    bool doSeparator = false;
                    foreach (string field in fields)
                    {
                        if (doSeparator)
                        {
                            Console.Write("\t");
                        }
                        else
                        {
                            doSeparator = true;
                        }
                        string value = agent.GetValue(field);
                        if (value != null)
                        {
                            Console.Write(value);
                        }
                    }
                    Console.WriteLine();
                    break;
                case OutputFormat.JSON:
                   // Console.WriteLine(agent.ToJson(fields));
                    break;
                case OutputFormat.YAML:
                    Console.WriteLine(agent.ToYamlTestCase());
                    break;
                default:
                    break;
            }
        }

        public static void Main(string[] args)
        {
            Options commandlineOptions = new Options();
            var parser = new Parser();
            try
            {                
                parser.ParseArguments<Options>(args);
                OutputFormat outputFormat = OutputFormat.YAML;
                if (commandlineOptions.CsvFormat)
                {
                    outputFormat = OutputFormat.CSV;
                }
                else if (commandlineOptions.JsonFormat)
                {
                    outputFormat = OutputFormat.JSON;                    
                }

                UserAgentAnalyzerTester.UserAgentAnalyzerTesterBuilder builder = UserAgentAnalyzerTester.NewBuilder();
                builder.HideMatcherLoadStats();
                builder.WithCache(commandlineOptions.CacheSize);
                if (commandlineOptions.Fields != null)
                {
                    foreach (string field in commandlineOptions.Fields)
                    {
                        builder.WithField(field);
                    }
                }
                UserAgentAnalyzerTester uaa = builder.Build() as UserAgentAnalyzerTester;

                ICollection<string> fields;
                if (commandlineOptions.Fields == null)
                {
                    fields = uaa.GetAllPossibleFieldNamesSorted();
                    fields.Add(UserAgent.USERAGENT_FIELDNAME);
                }
                else
                {
                    fields = commandlineOptions.Fields;
                }
                PrintHeader(outputFormat, fields);

                if (commandlineOptions.Useragent != null)
                {
                    UserAgent agent = uaa.Parse(commandlineOptions.Useragent);
                    PrintAgent(outputFormat, fields, agent);
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
                    byte[] bytes = Encoding.UTF8.GetBytes(input);
                    inputStream = new MemoryStream(bytes);
                }
                var sw = new StreamWriter(Console.OpenStandardOutput())
                {
                    AutoFlush = true
                };
                Console.SetOut(sw);
                UserAgentTreeFlattener flattenPrinter = new UserAgentTreeFlattener(new FlattenPrinter(sw));
                using (StreamReader br = new StreamReader(inputStream))
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

                        PrintAgent(outputFormat, fields, agent);
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
            //catch (CmdLineException e) {
            //    logVersion();
            //    LOG.error("Errors: {}", e.getMessage());
            //    LOG.error("");
            //    System.err.println("Usage: java jar <jar containing this class> <options>");
            //    parser.printUsage(System.err);
            //    returnValue = 1;
            //} catch (final Exception e) {
            //    LOG.error("IOException: {}", e);
            //    returnValue = 1;
            //}
            //System.exit(returnValue);
            //}
        }
    }
}
