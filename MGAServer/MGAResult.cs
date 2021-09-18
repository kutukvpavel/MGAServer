using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MGA
{
    public class MGAResult : List<MGAPacket>, IDisposable
    {
        public static string SaveLineFormat { get; set; }

        public MGAResult() : base()
        { }
        public MGAResult(string filePath, PipeServer pipe = null) : this()
        {
            InitFiles(filePath);
            _Pipe = pipe;
        }

        public int[] SelectSensors { get; set; }

        public MGAPacket[] GetSensor(int index)
        {
            return this.Where(x => x.SensorIndex == index).ToArray();
        }

        public new void Add(MGAPacket item)
        {
            if (!(SelectSensors?.Contains(item.SensorIndex) ?? true)) return;
            if (_SaveFile[item.SensorIndex]?.BaseStream?.CanWrite ?? false)
            {

                _SaveFile[item.SensorIndex].WriteLine(SaveLineFormat, DateTime.Now, item.Conductance, item.HeaterResistance);
            }
            if (_Pipe != null)
            {
                _Pipe.Send(item);
            }
            base.Add(item);
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

        private void InitFiles(string filePath)
        {
            for (int i = 0; i < _SaveFile.Length; i++)
            {
                _SaveFile[i] = new StreamWriter(new FileStream(
                    string.Format(filePath, i), FileMode.Append, FileAccess.Write, FileShare.Read));
            }
        }

        private bool _Disposed = false;
        private readonly StreamWriter[] _SaveFile = new StreamWriter[4];
        private readonly PipeServer _Pipe;
    }
}
