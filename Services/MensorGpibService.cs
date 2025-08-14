using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using NationalInstruments.Visa;

namespace UniversalPressureController.Services
{
    /// <summary>
    /// GPIB Kommunikationsservice für Mensor CPC (WIKA‑SCPI) mit asynchroner I/O‑Serialisierung via SemaphoreSlim.
    /// Kanalwahl: Mapping 1→A, 2→B. Befehle wirken auf den aktiven Kanal.
    /// </summary>
    public class MensorGpibService : IGpibCommunicationService, IDisposable, IAsyncDisposable
    {
        private MessageBasedSession _session;
        private ResourceManager _resourceManager;
        private readonly SemaphoreSlim _io = new(1, 1);
        private bool _disposed;

        public bool IsConnected => _session != null;

        public event EventHandler<string> MessageReceived;
        public event EventHandler<string> ErrorOccurred;

        public async Task<bool> ConnectAsync(string gpibAddress, CancellationToken ct = default)
        {
            await _io.WaitAsync(ct);
            try
            {
                _resourceManager = new ResourceManager();
                _session = (MessageBasedSession)_resourceManager.Open(gpibAddress);
                _session.TimeoutMilliseconds = 5000;
                _session.TerminationCharacterEnabled = true; // \n als Terminator

                _session.RawIO.Write("*IDN?\n");
                string id = _session.RawIO.ReadString();
                MessageReceived?.Invoke(this, $"Connected to: {id}");

                _session.RawIO.Write("*RST\n");
                _session.RawIO.Write("*CLS\n");
                return true;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Connection failed: {ex.Message}");
                return false;
            }
            finally
            {
                _io.Release();
            }
        }

        public async Task DisconnectAsync(CancellationToken ct = default)
        {
            await _io.WaitAsync(ct);
            try
            {
                _session?.Dispose();
                _resourceManager?.Dispose();
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Disconnect error: {ex.Message}");
            }
            finally
            {
                _session = null;
                _resourceManager = null;
                _io.Release();
            }
        }

        public async Task<string> QueryAsync(string command, CancellationToken ct = default)
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected");

            await _io.WaitAsync(ct);
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
            finally
            {
                _io.Release();
            }
        }

        public async Task SendCommandAsync(string command, CancellationToken ct = default)
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected");

            await _io.WaitAsync(ct);
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
            finally
            {
                _io.Release();
            }
        }

        // --------- Hilfsfunktionen (WIKA‑SCPI) ---------
        private static char MapChannel(int channel) => channel switch
        {
            1 => 'A',
            2 => 'B',
            _ => throw new ArgumentOutOfRangeException(nameof(channel), "Nur Kanal 1 oder 2 erlaubt.")
        };

        private Task SelectChannelAsync(int channel, CancellationToken ct = default)
        {
            char c = MapChannel(channel);
            return SendCommandAsync($":OUTP:CHAN {c}", ct);
        }

        // --------- Öffentliche API ---------

        public async Task<double> ReadMeasurementAsync(int channel, CancellationToken ct = default)
        {
            await SelectChannelAsync(channel, ct);
            string response = await QueryAsync(":MEAS:PRES?", ct);
            if (double.TryParse(response, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                return value;
            throw new FormatException($"Invalid measurement response: {response}");
        }

        public async Task SetSetpointAsync(int channel, double value, CancellationToken ct = default)
        {
            await SelectChannelAsync(channel, ct);
            await SendCommandAsync($":SOUR:PRES {value.ToString("F6", CultureInfo.InvariantCulture)}", ct);
        }

        public async Task StartControlAsync(int channel, CancellationToken ct = default)
        {
            await SelectChannelAsync(channel, ct);
            await SendCommandAsync(":OUTP:MODE CONT", ct); // oder CONTROL
        }

        public async Task StopControlAsync(int channel, CancellationToken ct = default)
        {
            await SelectChannelAsync(channel, ct);
            await SendCommandAsync(":OUTP:MODE MEAS", ct);
        }

        public async Task VentAsync(int channel, CancellationToken ct = default)
        {
            await SelectChannelAsync(channel, ct);
            await SendCommandAsync(":OUTP:MODE VENT", ct);
        }

        public Task<string> GetStatusAsync(CancellationToken ct = default)
            => QueryAsync(":SYST:ERR?", ct);

        // --------- Entsorgung ---------
        public void Dispose()
        {
            if (_disposed) return;
            try
            {
                _session?.Dispose();
                _resourceManager?.Dispose();
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Dispose error: {ex.Message}");
            }
            finally
            {
                _session = null;
                _resourceManager = null;
                _disposed = true;
                _io.Dispose();
            }
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
