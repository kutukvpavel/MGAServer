using System;
using RJCP.IO.Ports;
using System.Threading;

namespace PortDebugHelper
{
    class Program
    {
        static SerialPortStream p;
        static CancellationTokenSource c;

        static void Main(string[] args)
        {
            c = new CancellationTokenSource();
            c.Token.Register(() => p.Close());
            Console.WriteLine("Hello World!");
            Console.CancelKeyPress += Console_CancelKeyPress;
            p = new SerialPortStream("COM32", 115200);
            p.DataReceived += P_DataReceived;
            p.Open();
            c.Token.WaitHandle.WaitOne();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            c.Cancel();
            e.Cancel = true;
        }

        private static void P_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int i;
            while ((i = p.ReadByte()) != -1)
            {
                byte b = (byte)i;
                Console.WriteLine("0x{0:X2}", b);
            }
        }
    }
}
