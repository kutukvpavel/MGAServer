using System;
using NamedPipeWrapper;
using System.Text.Json;

namespace PipeDebugHelper
{
    class Program
    {
        static NamedPipeClient<string> Client;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Client = new NamedPipeClient<string>("MGA_Broadcast_Pipe");
            Client.ServerMessage += Client_ServerMessage;
            Client.Start();
            Client.WaitForConnection();
            Client.WaitForDisconnection();
            Client.Stop();
        }

        private static void Client_ServerMessage(NamedPipeConnection<string, string> connection, string message)
        {
            Console.WriteLine(JsonSerializer.Deserialize<MGAPacket>(message).ToString());
        }
    }

    class MGAPacket
    {
        public MGAPacket()
        {

        }
        
        public DateTime Timestamp { get; set; }
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
