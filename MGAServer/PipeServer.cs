using System;
using System.Collections.Generic;
using System.Text;
using NamedPipeWrapper;

namespace MGA
{
    public class PipeServer : IDisposable
    {
        private static PipeServer _Instance;
        public static PipeServer Instance { 
            get
            {
                if (_Instance == null) throw new InvalidOperationException("The pipe hasn't been initialized yet.");
                return _Instance;
            }
        }

        public static void Initialize(string pipeName)
        {
            if (Instance != null) throw new InvalidOperationException("The pipe has already been initialized.");
            _Instance = new PipeServer(pipeName);
        }

        public event EventHandler<Exception> ErrorOccured;

        public void Send(MGAPacket data)
        {
            _Pipe.PushMessage(data);
        }

        public void Dispose()
        {
            try
            {
                _Pipe.Stop();
            }
            catch (ObjectDisposedException)
            { }
        }

        private PipeServer(string pipeName)
        {
            _Pipe = new NamedPipeServer<MGAPacket>(pipeName);
            _Pipe.Error += Pipe_Error;
            _Pipe.Start();
        }

        private void Pipe_Error(Exception exception)
        {
            ErrorOccured?.Invoke(this, exception);
        }

        private readonly NamedPipeServer<MGAPacket> _Pipe;
    }
}
