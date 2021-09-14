using System;
using System.Collections.Generic;
using System.Text;
using LLibrary;

namespace MGAServer
{
    public static class Logger
    {
        private static L _Instance = new L();

        public static void WriteInfo(string data)
        {
            _Instance.Info(data);
        }

        public static void WriteError(object sender, Exception e, object data = null)
        {
            string buf = $"Exception from object '{sender}': {e}";
            if (data != null) buf += $"{Environment.NewLine}Additional data: {data}";
            _Instance.Error(buf);
        }
    }
}
