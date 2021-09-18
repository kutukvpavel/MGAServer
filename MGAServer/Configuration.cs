﻿using System;
using System.IO;
using System.Text.Json;

namespace MGA
{
    public class Configuration
    {
        public static Configuration Instance { get; private set; }

        public float[] TargetHeaterResistances { get; set; } = new float[] { 17, 17, 17, 17 };
        public string SaveTarget { get; set; } = @".\results\sensor{{0}}_{0:yyyy-MM-dd_HH-mm-ss}.csv";
        public string SaveLineFormat { get; set; } = "{0:yyyy-MM-dd HH-mm-ss.ff};{1:E3};{2:F2}";
        public string PipeName { get; set; } = "MGA_Broadcast_Pipe";
        public int[] SelectSensors { get; set; } = new int[] { 0, 1, 2, 3 };

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
    }
}
