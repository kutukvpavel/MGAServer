using System;
using System.IO;
using System.Linq;
using System.Threading;
using CommandLine;

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
        OK,
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
            switch (opt.GetMode())
            {
                case Mode.Acquisition:
                    if (opt.PortName == null) return (int)ExitCodes.CommandLineIncomplete;
                    return (int)AcquisitionMain(opt.PortName);
                case Mode.DumpParser:
                    if (opt.DumpPath == null) return (int)ExitCodes.CommandLineIncomplete;
                    return (int)DumpParserMain(opt.DumpPath);
                default:
                    return (int)ExitCodes.UnknownMode;
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            _Cancel.Cancel();
        }

        static ExitCodes AcquisitionMain(string portName)
        {
            try
            {
                PipeServer.Initialize(Configuration.Instance.PipeName);
                PipeServer.Instance.ErrorOccured += Logger.WriteError;
                using MGAResult res = new MGAResult(Configuration.Instance.GetSavePath(), PipeServer.Instance);
                using MGAServer serv = new MGAServer(portName);
                serv.ErrorOccurred += Logger.WriteError;
                serv.PacketParsed += (s, p) => res.Add(p);
                try
                {
                    serv.Connect();
                    serv.SendTargetHeaterResistances(Configuration.Instance.TargetHeaterResistances);
                }
                catch (Exception ex)
                {
                    Logger.WriteError(serv, ex, "Can't connect or initialize the device.");
                }
                _Cancel.Token.WaitHandle.WaitOne();
                try
                {
                    serv.Disconnect();
                }
                catch (Exception ex)
                {
                    Logger.WriteError(serv, ex, "Can't disconnect.");
                }
                return ExitCodes.OK;
            }
            catch (Exception ex)
            {
                Logger.WriteError(null, ex, "Unable to initialize acquistion API.");
                return ExitCodes.UnknownError;
            }
        }

        static ExitCodes DumpParserMain(string filePath)
        {
            var ext = Path.GetExtension(filePath);
            MGAResult res = (ext == ".txt" || ext == ".csv") ?
                    MGAParser.ParseLADump(filePath, Configuration.Instance.GetSavePath()) : 
                    MGAParser.ParseRawDump(filePath, Configuration.Instance.GetSavePath());
            return ExitCodes.OK;
        }
    }
}
