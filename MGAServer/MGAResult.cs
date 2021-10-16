using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MGA
{
    public class MGAResult : List<MGAPacket>, IDisposable
    {
        public const int SensorCount = 4;

        public static string SaveLineFormat { get; set; }

        public MGAResult() : base()
        { }
        public MGAResult(string filePath, PipeServer pipe = null) : this()
        {
            if (filePath != null)
            {
                string dir = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir); 
                InitFiles(filePath);
            }
            _Pipe = pipe;
        }

        public int[] SelectSensors { get; set; }
        public bool KeepInRam { get; set; } = false;

        public MGAPacket[] GetSensor(int index)
        {
            return this.Where(x => x.SensorIndex == index).ToArray();
        }

        public new void Add(MGAPacket item)
        {
            if (!(SelectSensors?.Contains(item.SensorIndex) ?? true)) return;
            var sw = _SaveFile[item.SensorIndex];
            if (sw?.BaseStream?.CanWrite ?? false)
            {
                sw.WriteLine(SaveLineFormat, 
                    item.Timestamp, item.Conductance, item.HeaterResistance);
                if (_FlushCounter[item.SensorIndex] > 3)
                {
                    sw.FlushAsync();
                    _FlushCounter[item.SensorIndex] = 0;
                }
            }
            if (_Pipe != null)
            {
                _Pipe.Send(item);
            }
            if (KeepInRam) base.Add(item);
        }

        public void Dispose()
        {
            if (_Disposed) return;
            try
            {
                foreach (var item in _SaveFile)
                {
                    try
                    {
                        item?.Close();
                        item?.Dispose();
                    }
                    catch (ObjectDisposedException)
                    { }
                }
            }
            finally
            {
                _Disposed = true;
            }
        }

        #region Private

        private void InitFiles(string filePath)
        {
            for (int i = 0; i < _SaveFile.Length; i++)
            {
                _SaveFile[i] = new StreamWriter(new FileStream(
                    string.Format(filePath, i), FileMode.Append, FileAccess.Write, FileShare.Read));
            }
        }

        private bool _Disposed = false;
        private readonly StreamWriter[] _SaveFile = new StreamWriter[SensorCount];
        private readonly PipeServer _Pipe;
        private readonly int[] _FlushCounter = new int[SensorCount];

        #endregion
    }
}
