using System;
using System.Collections.Generic;
using System.Text;

namespace MGAServer
{
    public class MGAPacket
    {
        public MGAPacket(int index, float resistance, float conductance, byte[] raw)
        {
            SensorIndex = index;
            HeaterResistance = resistance;
            Conductance = conductance;
            RawValue = raw;
        }

        public int SensorIndex { get; }
        public float HeaterResistance { get; }
        public float Conductance { get; }
        public byte[] RawValue { get; }
    }
}
