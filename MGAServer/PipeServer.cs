using NamedPipeWrapper;
using System;
using System.Text.Json;

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

        public static void Initialize(string pipeName, string labPidPipeName, float initialSetpoint)
        {
            if (_Instance != null) throw new InvalidOperationException("The pipe has already been initialized.");
            _Instance = new PipeServer(pipeName, labPidPipeName, initialSetpoint);
        }

        public event EventHandler<Exception> ErrorOccured;
        public event EventHandler<float> SetpointChanged;

        public void Send(MGAPacket data)
        {
            _Pipe.PushMessage(JsonSerializer.Serialize(data));
        }

        public void Dispose()
        {
            try
            {
                _Pipe.Stop();
                if (_LabPidPipe != null) _LabPidPipe.Stop();
            }
            catch (ObjectDisposedException)
            { }
        }

        private PipeServer(string pipeName, string labPidPipeName, float initialSetpoint)
        {
            _LastSetpoint = initialSetpoint;
            _Pipe = new NamedPipeServer<string>(pipeName);
            _Pipe.Error += Pipe_Error;
            if ((labPidPipeName?.Length ?? 0) > 0)
            {
                _LabPidPipe = new NamedPipeClient<string>(labPidPipeName);
                _LabPidPipe.Error += Pipe_Error;
                _LabPidPipe.ServerMessage += LabPidPipe_ServerMessage;
                _LabPidPipe.Start();
            }
            _Pipe.Start();
        }

        private void LabPidPipe_ServerMessage(NamedPipeConnection<string, string> connection, string message)
        {
            try
            {
                var p = JsonSerializer.Deserialize<LabPid.Packet>(message);
                if (p.Setpoint != _LastSetpoint)
                {
                    SetpointChanged.Invoke(this, p.Setpoint);
                    _LastSetpoint = p.Setpoint;
                }
            }
            catch (Exception ex)
            {
                ErrorOccured.Invoke(this, ex);
            }
        }

        private void Pipe_Error(Exception exception)
        {
            ErrorOccured?.Invoke(this, exception);
        }

        private float _LastSetpoint;
        private readonly NamedPipeServer<string> _Pipe;
        private readonly NamedPipeClient<string> _LabPidPipe;
    }
}
