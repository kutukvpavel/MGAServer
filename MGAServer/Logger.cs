using System;
using System.Collections.Generic;
using System.Text;
using LLibrary;

namespace MGA
{
    public static class Logger
    {
        private static L _Instance = new L();

        public static void WriteInfo(string data)
        {
            _Instance.Info(data);
        }

        public static void WriteError(object sender, Exception e)
        {
            WriteError(sender, e, null);
        }

        public static void WriteError(object sender, Exception e, object data)
        {
            string buf = $"Exception from object '{sender ?? "static"}': {e}";
            if (data != null) buf += $"{Environment.NewLine}Additional data: {data}";
            _Instance.Error(buf);
        }
    }
}
