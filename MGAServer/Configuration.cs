using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MGA
{
    public class Configuration
    {
        public static Configuration Instance { get; private set; }

        public MGASensor[] Sensors { get; set; } = new MGASensor[]
        {
            new MGASensor() { ExplicitTargetResistance = 17 },
            new MGASensor() { ExplicitTargetResistance = 17 },
            new MGASensor() { ExplicitTargetResistance = 17 },
            new MGASensor() { ExplicitTargetResistance = 17 }
        };
        public string SaveTarget { get; set; } = @".\results\{0:yyyy-MM-dd_HH-mm-ss}_sensor{{0}}.csv";
        public string SaveLineFormat { get; set; } = "{0:yyyy-MM-dd HH-mm-ss.ff};{1:E3};{2:F2}";
        public string PipeName { get; set; } = "MGA_Broadcast_Pipe";
        public int[] SelectSensors { get; set; } = new int[] { 0, 1, 2, 3 };
        public float TargetTemperature { get; set; } = 300;
        public bool UpdateSensorConfiguration { get; set; } = true;

        public float[] GetTargetResistances()
        {
            return Sensors.Select(x => x.GetTargetResistance(TargetTemperature)).ToArray();
        }
        public string GetSavePath(string overridePath = null)
        {
            if ((overridePath?.Length ?? -1) == 0) return null;
            string buf = string.Format(overridePath ?? SaveTarget, DateTime.Now);
            if (Path.IsPathFullyQualified(buf)) return buf;
            return Path.GetFullPath(buf, Environment.CurrentDirectory);
        }

        public static void Load()
        {
            Instance = new Configuration();
        }
        public static void Load(string path)
        {
            Instance = JsonSerializer.Deserialize<Configuration>(File.ReadAllText(path));
        }
        public static void Save(string path)
        {
            File.WriteAllText(
                path,
                JsonSerializer.Serialize(Instance, new JsonSerializerOptions() { WriteIndented = true })
                );
        }
    }
}
