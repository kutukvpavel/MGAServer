using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace MGA
{
    public class Configuration
    {
        public static Configuration Instance { get; private set; }

        public float[] TargetHeaterResistances { get; set; } = new float[] { 33, 33, 33, 33 };
        public string SaveTarget { get; set; } = @".\results\sensor{{0}}_{0:yyyy-MM-dd_HH-mm-ss}.csv";
        public string SaveLineFormat { get; set; } = "{0},{1},{2}";
        public string PipeName { get; set; } = "MGA_Broadcast_Pipe";

        public string GetSavePath()
        {
            string buf = string.Format(SaveTarget, DateTime.Now);
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
    }
}
