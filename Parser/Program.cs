using System;
using System.Diagnostics;
using System.Threading.Tasks;

using MiKoSolutions.SemanticParsers.CSharp.Yaml;

using SystemFile = System.IO.File;

namespace MiKoSolutions.SemanticParsers.CSharp
{
    public static class Program
    {
        private static readonly Guid InstanceId = Guid.NewGuid();

        /// <summary>
        /// Console input:
        /// 1. "input file"
        /// 2. "encoding"
        /// 3. "output file"
        /// 4. "end" -> ends session
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task<int> Main(string[] args)
        {
            // check for GMaster or PlasticSCM or SemanticMerge arguments (to allow debugging without the tools)
            if (args.Length == 2)
            {
                var shell = args[0]; // reserved for future usage
                var flagFile = args[1];

                SystemFile.WriteAllBytes(flagFile, new byte[] { 0x42 });
            }

            var watch = Stopwatch.StartNew();

            while (true)
            {
                var inputFile = await Console.In.ReadLineAsync();

                if (inputFile is null || "end".Equals(inputFile, StringComparison.OrdinalIgnoreCase))
                {
                    // session is done
                    Tracer.Trace($"Terminating as session was ended (instance {InstanceId:B})");

                    return 0;
                }

                var encodingToUse = await Console.In.ReadLineAsync();

                var outputFile = await Console.In.ReadLineAsync();

                try
                {
                    var parseErrors = false;
                    try
                    {
                        watch.Restart();

                        // SystemFile.Copy(inputFile, $@"z:\{Path.GetFileName(inputFile)}", true);

                        var file = Parser.Parse(inputFile, encodingToUse);

                        using (var writer = SystemFile.CreateText(outputFile))
                        {
                            YamlWriter.Write(writer, file);
                        }

                        parseErrors = file.ParsingErrorsDetected is true;
                        if (parseErrors)
                        {
                            var parsingError = file.ParsingErrors[0];
                            Tracer.Trace(parsingError.ErrorMessage);
                            Tracer.Trace(parsingError.Location);
                        }

                        Console.WriteLine(parseErrors ? "KO" : "OK");
                    }
                    finally
                    {
                        Tracer.Trace($"Parsing took {watch.Elapsed:s\\.fff} secs  (instance {InstanceId:B}), errors found: {parseErrors}");
                    }
                }
                catch (Exception ex)
                {
                    Tracer.Trace($"Exception: {ex}", ex);

                    Console.WriteLine("KO");

                    return 0;
                }
            }
        }
    }
}
