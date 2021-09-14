using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace MGAServer
{
    public class Configuration
    {
        public static Configuration Instance { get; private set; }

        public float[] TargetHeaterResistances { get; set; } = new float[] { 33, 33, 33, 33 };

        public static void Load(string path)
        {
            Instance = JsonSerializer.Deserialize<Configuration>(File.ReadAllText(path));
        }
    }
}
