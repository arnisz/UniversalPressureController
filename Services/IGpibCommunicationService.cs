using System;
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

        Task<bool> ConnectAsync(string gpibAddress);
        Task DisconnectAsync();
        Task<string> QueryAsync(string command);
        Task SendCommandAsync(string command);
        Task<double> ReadMeasurementAsync(int channel);
        Task SetSetpointAsync(int channel, double value);
        Task StartControlAsync(int channel);
        Task StopControlAsync(int channel);
        Task VentAsync(int channel);
        Task<string> GetStatusAsync();
    }
}
