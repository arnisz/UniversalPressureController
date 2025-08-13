using System;
using System.Globalization;
using System.Threading.Tasks;
using NationalInstruments.Visa;

namespace UniversalPressureController.Services
{
    /// <summary>
    /// GPIB Kommunikationsservice für Mensor CPC
    /// </summary>
    public class MensorGpibService : IGpibCommunicationService, IDisposable
    {
        private MessageBasedSession _session;
        private ResourceManager _resourceManager;
        private readonly object _lockObject = new();
        private bool _disposed;

        public bool IsConnected => _session != null;

        public event EventHandler<string> MessageReceived;
        public event EventHandler<string> ErrorOccurred;

        public async Task<bool> ConnectAsync(string gpibAddress)
        {
            return await Task.Run(() =>
            {
                try
                {
                    lock (_lockObject)
                    {
                        _resourceManager = new ResourceManager();
                        _session = (MessageBasedSession)_resourceManager.Open(gpibAddress);
                        _session.Timeout = 5000;
                        _session.TerminationCharacterEnabled = true;

                        _session.RawIO.Write("*IDN?\n");
                        string id = _session.RawIO.ReadString();
                        MessageReceived?.Invoke(this, $"Connected to: {id}");

                        _session.RawIO.Write("*RST\n");
                        _session.RawIO.Write("*CLS\n");

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    ErrorOccurred?.Invoke(this, $"Connection failed: {ex.Message}");
                    return false;
                }
            });
        }

        public async Task DisconnectAsync()
        {
            await Task.Run(() =>
            {
                lock (_lockObject)
                {
                    _session?.Dispose();
                    _resourceManager?.Dispose();
                    _session = null;
                    _resourceManager = null;
                }
            });
        }

        public async Task<string> QueryAsync(string command)
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected");

            return await Task.Run(() =>
            {
                lock (_lockObject)
                {
                    try
                    {
                        _session.RawIO.Write($"{command}\n");
                        string response = _session.RawIO.ReadString();
                        MessageReceived?.Invoke(this, $"Query: {command} -> {response}");
                        return response.Trim();
                    }
                    catch (Exception ex)
                    {
                        ErrorOccurred?.Invoke(this, $"Query error: {ex.Message}");
                        throw;
                    }
                }
            });
        }

        public async Task SendCommandAsync(string command)
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected");

            await Task.Run(() =>
            {
                lock (_lockObject)
                {
                    try
                    {
                        _session.RawIO.Write($"{command}\n");
                        MessageReceived?.Invoke(this, $"Command sent: {command}");
                    }
                    catch (Exception ex)
                    {
                        ErrorOccurred?.Invoke(this, $"Command error: {ex.Message}");
                        throw;
                    }
                }
            });
        }

        public async Task<double> ReadMeasurementAsync(int channel)
        {
            string response = await QueryAsync($":SENS{channel}:PRES?");
            if (double.TryParse(response, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                return value;
            throw new FormatException($"Invalid measurement response: {response}");
        }

        public async Task SetSetpointAsync(int channel, double value)
        {
            await SendCommandAsync($":SOUR{channel}:PRES {value:F6}");
        }

        public async Task StartControlAsync(int channel)
        {
            await SendCommandAsync($":OUTP{channel}:STAT ON");
        }

        public async Task StopControlAsync(int channel)
        {
            await SendCommandAsync($":OUTP{channel}:STAT OFF");
        }

        public async Task VentAsync(int channel)
        {
            await SendCommandAsync($":SYST:VENT{channel}");
        }

        public async Task<string> GetStatusAsync()
        {
            return await QueryAsync(":SYST:ERR?");
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                DisconnectAsync().Wait();
                _disposed = true;
            }
        }
    }
}

