using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MGAServer
{
    public class MGAResult : List<MGAPacket>, IDisposable
    {
        public static string SaveLineFormat { get; set; }

        public MGAResult() : base()
        { }
        public MGAResult(string filePath) : this()
        {
            _SaveFile = new StreamWriter(new FileStream(filePath, FileMode.Append, FileAccess.Write));
        }

        public MGAPacket[] GetSensor(int index)
        {
            return this.Where(x => x.SensorIndex == index).ToArray();
        }

        public new void Add(MGAPacket item)
        {
            if (_SaveFile?.BaseStream?.CanWrite ?? false)
            {
                _SaveFile.WriteLine()
            }
            base.Add(item);
        }

        public void Dispose()
        {
            if (_Disposed) return;
            try
            {
                _SaveFile.Close();
                _SaveFile.Dispose();
            }
            catch (ObjectDisposedException)
            { }
            finally
            {
                _Disposed = true;
            }
        }


        private bool _Disposed = false;
        private StreamWriter _SaveFile;
    }
}
