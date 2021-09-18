using System;
using NamedPipeWrapper;

namespace PipeDebugHelper
{
    class Program
    {
        static NamedPipeClient<MGAPacket> Client;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Client = new NamedPipeClient<MGAPacket>("MGA_Broadcast_Pipe");
            Client.ServerMessage += Client_ServerMessage;
            Client.WaitForConnection();
            Client.WaitForDisconnection();
        }

        private static void Client_ServerMessage(NamedPipeConnection<MGAPacket, MGAPacket> connection, MGAPacket message)
        {
            Console.WriteLine(message.ToString());
        }
    }

    class MGAPacket
    {
        public MGAPacket()
        {

        }
        
        public int SensorIndex { get; set; }
        public float HeaterResistance { get; set; }
        public float Conductance { get; set; }
        public byte[] RawValue { get; set; }

        public override string ToString()
        {
            return $"{SensorIndex}: {Conductance:E3}, {HeaterResistance:F2}";
        }
    }
}
