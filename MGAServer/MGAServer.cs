﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using RJCP.IO.Ports;

namespace MGAServer
{
    public class MGAServer : IDisposable
    {
        /*
         * Configuration == EEPROM image load??
         * Structure:
         * 
         * 0x4i - ith sensor page load start
         * 0x00 x 3
         * 0x01
         * 0x00 x 3
         * 0x01
         * 0x00 (or actually four bytes, first one just happens to be 0)
         * Three bytes, last nibbles represent target heater resistance with 2 decimal places, e.g. 0x0C 0x0F 0x00 = 33.12 Ohm
         * 0x00 x 3
         * 0x0A
         * 0x00 x 112
         * 0x8i - ith sensor page load end
         * .
         * .
         * .
         * 0xC0 - start measurement cycle?
         * 
         */

        public static int ConfigurationDataLoadDelay { get; set; } = 500;

        public event EventHandler<MGAPacket> PacketParsed;
        public event EventHandler<Exception> ErrorOccurred;

        public MGAServer(string portName)
        {
            Parser = new MGAParser();
            Port = new SerialPortStream(portName, 115200, 8, Parity.None, StopBits.One);
            Port.DataReceived += Port_DataReceived;
            Port.ErrorReceived += Port_ErrorReceived;
            _PacketQueue = new BlockingCollection<MGAPacket>();
        }

        public SerialPortStream Port { get; set; }
        public MGAParser Parser { get; }

        public void Connect()
        {
            _PacketDumpThread = new Thread(PacketDumper);
            _DumpTokenSource = new CancellationTokenSource();
            _PacketDumpThread.Start();
            try
            {
                Port.Open();
            }
            catch (Exception ex)
            {
                Disconnect();
                ErrorOccurred?.Invoke(this, ex);
            }
        }

        public void SendTargetHeaterResistances(float[] targetRes)
        {
            if (targetRes.Length != 4) throw new ArgumentOutOfRangeException();
            var fixedPoint = targetRes.Select(x => (uint)(x * 100));
            int n = 0;
            foreach (var item in fixedPoint)
            {
                ConfigurationPageSender(item, n++);
                Port.Flush();
                Thread.Sleep(ConfigurationDataLoadDelay);
            }
            Port.WriteByte(0x0C);
            Port.Flush();
        }

        public void Disconnect()
        {
            if (Port.IsOpen) Port.Close();
            try
            {
                _DumpTokenSource.Cancel();
                if (_PacketDumpThread.IsAlive) _PacketDumpThread.Join();
            }
            catch (ThreadStateException)
            { }
        }

        public void Dispose()
        {
            if (Port.IsOpen) Disconnect();
            _DumpTokenSource.Dispose();
            Port.Dispose();
            _PacketQueue.Dispose();
        }

        private BlockingCollection<MGAPacket> _PacketQueue;
        private Thread _PacketDumpThread;
        private CancellationTokenSource _DumpTokenSource;

        private void Port_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            if (e.EventType == SerialError.NoError) return;
            lock (this)
            {
                Port.DiscardInBuffer();
                Port.DiscardOutBuffer();
                Parser.ResetState();
            }
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int b;
            MGAPacket p;
            lock (this)
            {
                while ((b = Port.ReadByte()) != -1)
                {
                    p = Parser.ParseByte((byte)b);
                    if (p != null) _PacketQueue.Add(p);
                }
            }
        }

        private void PacketDumper()
        {
            while (true)
            {
                try
                {
                    PacketParsed?.Invoke(this, _PacketQueue.Take(_DumpTokenSource.Token));
                }
                catch (OperationCanceledException)
                {
                    while (_PacketQueue.TryTake(out MGAPacket p))
                    {
                        PacketParsed?.Invoke(this, p);
                    }
                    break;
                }
                catch (Exception ex)
                {
                    ErrorOccurred?.Invoke(this, ex);
                }
            }
        }

        private void ConfigurationPageSender(uint value, int n)
        {
            byte[] b = new byte[4];
            for (int i = 0; i < b.Length; i++)
            {
                b[b.Length - i - 1] = (byte)((value >> 4 * i) & 0x0F);
            }
            Port.WriteByte((byte)(0x40 + n));
            byte[] tripleZero = new byte[3] { 0, 0, 0 };
            for (int i = 0; i < 2; i++)
            {
                Port.Write(tripleZero);
                Port.WriteByte(0x01);
            }
            Port.Write(b);
            Port.Write(tripleZero);
            Port.WriteByte(0x0A);
            for (int i = 0; i < 112; i++)
            {
                Port.WriteByte(0x00);
            }
            Port.WriteByte((byte)(0x80 + n));
        }
    }
}