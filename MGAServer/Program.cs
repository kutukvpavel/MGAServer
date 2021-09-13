using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MGAServer
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0 && File.Exists(args[0]))
            {
                var res = MGAParser.ParseDump(args[0]);
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
            }
        }


    }
}
