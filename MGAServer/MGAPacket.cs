using System;

namespace MGA
{
    public class MGAPacket
    {
        public MGAPacket()
        {

        }
        public MGAPacket(int index, float resistance, float conductance, byte[] raw, DateTime timestamp) : this()
        {
            SensorIndex = index;
            HeaterResistance = resistance;
            Conductance = conductance;
            RawValue = raw;
            Timestamp = timestamp;
        }

        public DateTime Timestamp { get; }
        public int SensorIndex { get; }
        public float HeaterResistance { get; }
        public float Conductance { get; }
        public byte[] RawValue { get; }

        public override string ToString()
        {
            return $"{SensorIndex}: {Conductance:E3}, {HeaterResistance:F2}";
        }
    }
}
