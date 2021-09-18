using CommandLine;
using System;
using System.IO;
using System.Threading;

namespace MGA
{
    public enum Mode
    {
        Acquisition,
        DumpParser,
        Unknown
    }

    public enum ExitCodes
    {
        CancellationRequested = -1,
        OK = 0,
        UnknownMode,
        CommandLineIncomplete,
        InvalidConfigurationFile,
        ConfigurationFileNotFound,
        UnknownError
    }

    public class Options
    {
        [Option('m', "mode", Required = true, Default = "a", HelpText = "Program mode: a = acquisition, d = daump parser")]
        public string ModeString { get; set; }

        [Option('p', "port", Required = false, HelpText = "COM port name")]
        public string PortName { get; set; }

        [Option('o', "output", Required = false, HelpText = "Output path override")]
        public string OutputPath { get; set; }

        [Option('f', "file", Required = false, HelpText = "Path to the dump file to be parsed")]
        public string DumpPath { get; set; }

        [Option('c', "conf", Required = false, HelpText = "Configuration file path")]
        public string ConfigurationPath { get; set; }

        [Option('e', "enforce", Required = false, Default = true, HelpText = "Error out when specified configuration file can not be found")]
        public bool EnforceConfiguration { get; set; }

        public Mode GetMode()
        {
            switch (ModeString[0])
            {
                case 'a':
                    return Mode.Acquisition;
                case 'd':
                    return Mode.DumpParser;
                default:
                    return Mode.Unknown;
            }
        }
    }

    public class Program
    {
        static CancellationTokenSource _Cancel;

        static int Main(string[] args)
        {
            _Cancel = new CancellationTokenSource();
            Console.CancelKeyPress += Console_CancelKeyPress;
            Options opt = null;
            Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                opt = o;
            });
            if (opt == null) return (int)ExitCodes.CommandLineIncomplete;
            if (opt.ConfigurationPath != null)
            {
                try
                {
                    Configuration.Load(opt.ConfigurationPath);
                }
                catch (FileNotFoundException)
                {
                    Logger.WriteInfo("Can't find specified configuration file.");
                    if (opt.EnforceConfiguration)
                    {
                        Logger.WriteInfo("Configuration file parameter is being enforced. Exiting.");
                        return (int)ExitCodes.ConfigurationFileNotFound;
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteError("Main", ex, opt.ConfigurationPath);
                    return (int)ExitCodes.InvalidConfigurationFile;
                }
            }
            else
            {
                Configuration.Load();
            }
            string outputPath = Configuration.Instance.GetSavePath(opt.OutputPath);
            Console.WriteLine("Processed output path template = " + outputPath ?? "none");
            try
            {
                switch (opt.GetMode())
                {
                    case Mode.Acquisition:
                        if (opt.PortName == null) return (int)ExitCodes.CommandLineIncomplete;
                        return (int)AcquisitionMain(opt.PortName, outputPath);
                    case Mode.DumpParser:
                        if (opt.DumpPath == null) return (int)ExitCodes.CommandLineIncomplete;
                        return (int)DumpParserMain(opt.DumpPath, outputPath);
                    default:
                        return (int)ExitCodes.UnknownMode;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(null, ex, "Error inside XxxMain initialization code.");
                return (int)ExitCodes.UnknownError;
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (!_Cancel.IsCancellationRequested) _Cancel.Cancel();
            e.Cancel = true;
        }

        static ExitCodes AcquisitionMain(string portName, string overridePath)
        {
            PipeServer.Initialize(Configuration.Instance.PipeName);
            PipeServer.Instance.ErrorOccured += Logger.WriteError;
            MGAResult.SaveLineFormat = Configuration.Instance.SaveLineFormat;
            using MGAResult res = new MGAResult(overridePath, PipeServer.Instance)
            {
                SelectSensors = Configuration.Instance.SelectSensors
            };
            using MGAServer serv = new MGAServer(portName);
            _Cancel.Token.Register(() => serv.Disconnect());
            serv.ErrorOccurred += Logger.WriteError;
            serv.PacketParsed += (s, p) =>
            {
                try
                {
                    Console.WriteLine(p.ToString());
                    res.Add(p);
                }
                catch (Exception ex)
                {
                    Logger.WriteError(res, ex, "Unable to add a packet into the results.");
                }
            };
            if (_Cancel.IsCancellationRequested) return ExitCodes.CancellationRequested;
            try
            {
                serv.Connect();
                serv.SendTargetHeaterResistances(Configuration.Instance.TargetHeaterResistances);
            }
            catch (Exception ex)
            {
                Logger.WriteError(serv, ex, "Can't connect or initialize the device.");
                return ExitCodes.UnknownError;
            }
            _Cancel.Token.WaitHandle.WaitOne();
            return ExitCodes.CancellationRequested;
        }

        static ExitCodes DumpParserMain(string filePath, string outputPath)
        {
            var ext = Path.GetExtension(filePath);
            MGAResult.SaveLineFormat = Configuration.Instance.SaveLineFormat;
            MGAResult res = null;
            try
            {
                res = (ext == ".txt" || ext == ".csv") ?
                    MGAParser.ParseLADump(filePath, outputPath) :
                    MGAParser.ParseRawDump(filePath, outputPath);
                res.Dispose();
            }
            catch (Exception ex)
            {
                Logger.WriteError(res, ex, "Unable to parse the dump.");
                return ExitCodes.UnknownError;
            }
            Console.WriteLine("Parser task complete.");
            return ExitCodes.OK;
        }
    }
}
