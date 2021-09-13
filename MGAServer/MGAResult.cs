using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MGAServer
{
    public class MGAResult : List<MGAPacket>
    {
        public MGAResult() : base()
        { }

        public MGAPacket[] GetSensor(int index)
        {
            return this.Where(x => x.SensorIndex == index).ToArray();
        }
    }
}
