using System;
using System.IO;
using System.Linq;
using CommandLine;

namespace MGAServer
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
        static int Main(string[] args)
        {
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

        static ExitCodes AcquisitionMain(string portName)
        {
            using MGAResult res = new MGAResult(Configuration.Instance.GetSavePath());
            using MGAServer serv = new MGAServer(portName);

            return ExitCodes.OK;
        }

        static ExitCodes DumpParserMain(string filePath)
        {
            var ext = Path.GetExtension(filePath);
            MGAResult res = (ext == ".txt" || ext == ".csv") ?
                    MGAParser.ParseLADump(filePath) : MGAParser.ParseRawDump(filePath);
            var r1 = res.GetSensor(0).Select(x => string.Format("{0} {1:F3} {2}",
                string.Join(' ', x.RawValue.Select(x => x.ToString("X2")).ToArray())
                , x.HeaterResistance, x.Conductance)).ToArray();
            var r2 = res.GetSensor(1).Select(x => string.Format("{0} {1:F3} {2}",
                string.Join(' ', x.RawValue.Select(x => x.ToString("X2")).ToArray())
                , x.HeaterResistance, x.Conductance)).ToArray();
            var r3 = res.GetSensor(2).Select(x => string.Format("{0} {1:F3} {2}",
                string.Join(' ', x.RawValue.Select(x => x.ToString("X2")).ToArray())
                , x.HeaterResistance, x.Conductance)).ToArray();
            var r4 = res.GetSensor(3).Select(x => string.Format("{0} {1:F3} {2}",
                string.Join(' ', x.RawValue.Select(x => x.ToString("X2")).ToArray())
                , x.HeaterResistance, x.Conductance)).ToArray();
            r4.GetEnumerator();
            return ExitCodes.OK;
        }
    }
}
