using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;
using System.Linq;

namespace MGAServer
{
    public class MGAParser
    {
        /*
         * Packet structure:
         * Preamble = (+ extra 0x00 before bulk transmission starts) 0x00 0xFF 0xFF 0xAA
         * Sensor Index (1 byte)
         * 8 bytes of data, sign inverted (2's complement 24-bit ints?)
         * 
         * first byte == ??? (every 5 seconds goes 0x00 -> 0x81 -> 0x01 -> 0x00, i.e. 0 -> 1 -> -1 -> 0 ?)
         * 3 bytes = heater resistance with 3 decimal places
         * 3 bytes = raw conductance (proportional to 1/16, probably means one nibble actually is shifted somewhere)
         * 
         * last byte = ??? (for now included into conductance)
         * 
         * Next packet(s) for this bulk transmisson (4 packets/transmission)
         */

        public static byte[] PacketPreamble { get; set; } = new byte[] { 0x00, 0xFF, 0xFF, 0xAA };
        public static char Delimeter { get; set; } = ',';
        public static int ValueColumnIndex { get; set; } = 1;
        public static CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;
        public static int HeaderLines { get; set; } = 1;
        public static bool ReverseEndianess { get; set; } = true;
        public static int SkipToTemperature { get; set; } = 1;
        public static int SkipToConductance { get; set; } = 0;
        public static int TemperatureLength { get; set; } = 3;
        public static int ConductanceLength { get; set; } = 4;

        public static MGAResult ParseDump(string path)
        {
            var data = File.ReadLines(path);
            MGAResult res = new MGAResult();
            var p = new MGAParser();
            int headerSkip = 0;
            foreach (var line in data)
            {
                string l = line.Replace("0x", "");
                if (headerSkip++ < HeaderLines) continue;
                string s = l.Split(Delimeter)[ValueColumnIndex];
                byte val = byte.Parse(s, NumberStyles.HexNumber, Culture);
                MGAPacket packet = p.ParseByte(val);
                if (packet != null) res.Add(packet);
            }
            return res;
        }

        private enum State
        {
            SearchingForPreamble,
            ConfirmingPreamble,
            ReadingSensorIndex,
            SkippingToTemperature,
            ReadingHeater,
            SkippingToConductance,
            ReadingConductance
        }

        State s = State.SearchingForPreamble;
        int indexer = 1;
        int si = -1;
        float temp = 0;
        float cond;
        List<byte> raw = new List<byte>(9);

        public MGAPacket ParseByte(byte val)
        {
            if (s != State.SearchingForPreamble && s != State.ConfirmingPreamble)
            {
                raw.Add(val);
            }
            else
            {
                raw.Clear();
            }
            switch (s)
            {
                case State.SearchingForPreamble:
                    if (val == PacketPreamble[0])
                    {
                        s = State.ConfirmingPreamble;
                        indexer = 1;
                    }
                    break;
                case State.ConfirmingPreamble:
                    if (val != PacketPreamble[indexer++])
                    {
                        if (val != PacketPreamble[0]) s = State.SearchingForPreamble;
                        indexer = 1;
                    }
                    else
                    {
                        if (indexer == PacketPreamble.Length) s = State.ReadingSensorIndex;
                    }
                    break;
                case State.ReadingSensorIndex:
                    si = val;
                    s = SkipToTemperature > 0 ? State.SkippingToTemperature : State.ReadingHeater;
                    indexer = 0;
                    break;
                case State.SkippingToTemperature:
                    if (++indexer == SkipToTemperature)
                    {
                        indexer = 0;
                        s = State.ReadingHeater;
                    }
                    break;
                case State.ReadingHeater:
                    if (++indexer == TemperatureLength)
                    {
                        var b = raw.TakeLast(TemperatureLength).ToList();
                        b[0] ^= 0x80;
                        for (int i = 0; i < (sizeof(int) - TemperatureLength); i++)
                        {
                            b = b.Prepend<byte>(0).ToList();
                        }
                        if (ReverseEndianess) b.Reverse();
                        temp = BitConverter.ToInt32(b.ToArray(), 0) / 1000.0f;
                        s = SkipToConductance > 0 ? State.SkippingToConductance : State.ReadingConductance;
                        indexer = 0;
                    }
                    break;
                case State.SkippingToConductance:
                    if (++indexer == SkipToConductance)
                    {
                        indexer = 0;
                        s = State.ReadingConductance;
                    }
                    break;
                case State.ReadingConductance:
                    if (++indexer == ConductanceLength)
                    {
                        var b = raw.TakeLast(ConductanceLength).ToList();
                        b[0] ^= 0x80;
                        for (int i = 0; i < (sizeof(int) - ConductanceLength); i++)
                        {
                            b = b.Prepend<byte>(0).ToList();
                        }
                        if (ReverseEndianess) b.Reverse();
                        cond = BitConverter.ToInt32(b.ToArray(), 0) / 1.6E9f;
                        s = State.SearchingForPreamble;
                        indexer = 0;
                        return new MGAPacket(si, temp, cond, raw.ToArray());
                    }
                    break;
                default:
                    throw new InvalidOperationException();
            }
            return null;
        }
    }

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
