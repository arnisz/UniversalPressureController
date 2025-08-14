using System;
using System.Threading;
using System.Threading.Tasks;

namespace UniversalPressureController.Services
{
    /// <summary>
    /// GPIB Kommunikationsservice für Mensor CPC
    /// </summary>
    public interface IGpibCommunicationService
    {
        bool IsConnected { get; }
        event EventHandler<string> MessageReceived;
        event EventHandler<string> ErrorOccurred;

        Task<bool> ConnectAsync(string gpibAddress, CancellationToken cancellationToken = default);
        Task DisconnectAsync(CancellationToken cancellationToken = default);
        Task<string> QueryAsync(string command, CancellationToken cancellationToken = default);
        Task SendCommandAsync(string command, CancellationToken cancellationToken = default);
        Task<double> ReadMeasurementAsync(int channel, CancellationToken cancellationToken = default);
        Task SetSetpointAsync(int channel, double value, CancellationToken cancellationToken = default);
        Task StartControlAsync(int channel, CancellationToken cancellationToken = default);
        Task StopControlAsync(int channel, CancellationToken cancellationToken = default);
        Task VentAsync(int channel, CancellationToken cancellationToken = default);
        Task<string> GetStatusAsync(CancellationToken cancellationToken = default);
    }
}
