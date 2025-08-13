using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using UniversalPressureController.Models;
using UniversalPressureController.Services;

namespace UniversalPressureController.ViewModels
{
    /// <summary>
    /// Haupt-ViewModel
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IGpibCommunicationService _gpibService;
        private readonly ConfigurationService _configService;
        private readonly DispatcherTimer _updateTimer;
        private AppConfiguration _configuration;
        private bool _isConnected;
        private string _statusMessage;
        private string _gpibAddress;
        private ObservableCollection<Channel> _channels;
        private Channel _selectedChannel;
        private ObservableCollection<string> _logMessages;

        public bool IsConnected
        {
            get => _isConnected;
            set => SetField(ref _isConnected, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetField(ref _statusMessage, value);
        }

        public string GpibAddress
        {
            get => _gpibAddress;
            set => SetField(ref _gpibAddress, value);
        }

        public ObservableCollection<Channel> Channels
        {
            get => _channels;
            set => SetField(ref _channels, value);
        }

        public Channel SelectedChannel
        {
            get => _selectedChannel;
            set => SetField(ref _selectedChannel, value);
        }

        public ObservableCollection<string> LogMessages
        {
            get => _logMessages;
            set => SetField(ref _logMessages, value);
        }

        // Commands
        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand StartChannelCommand { get; }
        public ICommand StopChannelCommand { get; }
        public ICommand VentChannelCommand { get; }
        public ICommand SetSetpointCommand { get; }
        public ICommand LoadConfigCommand { get; }
        public ICommand SaveConfigCommand { get; }
        public ICommand ClearLogCommand { get; }

        public MainViewModel(IGpibCommunicationService gpibService, ConfigurationService configService)
        {
            _gpibService = gpibService;
            _configService = configService;

            Channels = new ObservableCollection<Channel>();
            LogMessages = new ObservableCollection<string>();

            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _updateTimer.Tick += async (s, e) => await UpdateMeasurements();

            ConnectCommand = new RelayCommand(async _ => await Connect(), _ => !IsConnected);
            DisconnectCommand = new RelayCommand(async _ => await Disconnect(), _ => IsConnected);
            StartChannelCommand = new RelayCommand(async p => await StartChannel(p as Channel), p => IsConnected && p is Channel);
            StopChannelCommand = new RelayCommand(async p => await StopChannel(p as Channel), p => IsConnected && p is Channel);
            VentChannelCommand = new RelayCommand(async p => await VentChannel(p as Channel), p => IsConnected && p is Channel);
            SetSetpointCommand = new RelayCommand(async p => await SetSetpoint(p as Channel), p => IsConnected && p is Channel);
            LoadConfigCommand = new RelayCommand(_ => LoadConfiguration());
            SaveConfigCommand = new RelayCommand(_ => SaveConfiguration());
            ClearLogCommand = new RelayCommand(_ => LogMessages.Clear());

            _gpibService.MessageReceived += (s, msg) => AddLog($"GPIB: {msg}");
            _gpibService.ErrorOccurred += (s, err) => AddLog($"ERROR: {err}");

            LoadConfiguration();
        }

        private async Task Connect()
        {
            try
            {
                StatusMessage = "Verbinde...";
                bool success = await _gpibService.ConnectAsync(GpibAddress);

                if (success)
                {
                    IsConnected = true;
                    StatusMessage = "Verbunden";
                    _updateTimer.Start();
                    AddLog($"Erfolgreich verbunden mit {GpibAddress}");
                }
                else
                {
                    StatusMessage = "Verbindung fehlgeschlagen";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Fehler: {ex.Message}";
                AddLog($"Verbindungsfehler: {ex.Message}");
            }
        }

        private async Task Disconnect()
        {
            try
            {
                _updateTimer.Stop();
                await _gpibService.DisconnectAsync();
                IsConnected = false;
                StatusMessage = "Getrennt";
                AddLog("Verbindung getrennt");
            }
            catch (Exception ex)
            {
                AddLog($"Fehler beim Trennen: {ex.Message}");
            }
        }

        private async Task StartChannel(Channel channel)
        {
            if (channel == null) return;

            try
            {
                channel.Status = ChannelStatus.Running;
                await _gpibService.SetSetpointAsync(channel.Id, channel.Setpoint);
                await _gpibService.StartControlAsync(channel.Id);
                channel.IsActive = true;
                AddLog($"Kanal {channel.Name} gestartet - Sollwert: {channel.Setpoint:F3} {channel.Unit}");
            }
            catch (Exception ex)
            {
                channel.Status = ChannelStatus.Error;
                AddLog($"Fehler beim Starten von Kanal {channel.Name}: {ex.Message}");
            }
        }

        private async Task StopChannel(Channel channel)
        {
            if (channel == null) return;

            try
            {
                await _gpibService.StopControlAsync(channel.Id);
                channel.IsActive = false;
                channel.Status = ChannelStatus.Idle;
                AddLog($"Kanal {channel.Name} gestoppt");
            }
            catch (Exception ex)
            {
                AddLog($"Fehler beim Stoppen von Kanal {channel.Name}: {ex.Message}");
            }
        }

        private async Task VentChannel(Channel channel)
        {
            if (channel == null) return;

            try
            {
                channel.Status = ChannelStatus.Venting;
                await _gpibService.VentAsync(channel.Id);
                channel.IsActive = false;
                AddLog($"Kanal {channel.Name} wird entlüftet");

                await Task.Delay(5000);
                channel.Status = ChannelStatus.Idle;
            }
            catch (Exception ex)
            {
                channel.Status = ChannelStatus.Error;
                AddLog($"Fehler beim Entlüften von Kanal {channel.Name}: {ex.Message}");
            }
        }

        private async Task SetSetpoint(Channel channel)
        {
            if (channel == null || !channel.IsActive) return;

            try
            {
                await _gpibService.SetSetpointAsync(channel.Id, channel.Setpoint);
                AddLog($"Neuer Sollwert für {channel.Name}: {channel.Setpoint:F3} {channel.Unit}");
            }
            catch (Exception ex)
            {
                AddLog($"Fehler beim Setzen des Sollwerts: {ex.Message}");
            }
        }

        private async Task UpdateMeasurements()
        {
            if (!IsConnected) return;

            foreach (var channel in Channels.Where(c => c.IsActive))
            {
                try
                {
                    double value = await _gpibService.ReadMeasurementAsync(channel.Id);
                    channel.ActualValue = value;
                    channel.LastUpdate = DateTime.Now;

                    double deviation = Math.Abs(channel.Deviation);
                    if (deviation < 0.01)
                        channel.Status = ChannelStatus.Running;
                    else if (deviation < 0.1)
                        channel.Status = ChannelStatus.Stabilizing;
                }
                catch (Exception ex)
                {
                    channel.Status = ChannelStatus.Error;
                    AddLog($"Messfehler Kanal {channel.Name}: {ex.Message}");
                }
            }
        }

        private void LoadConfiguration()
        {
            _configuration = _configService.LoadConfiguration();
            GpibAddress = _configuration.GpibAddress;

            Channels.Clear();
            foreach (var config in _configuration.Channels.Where(c => c.Enabled))
            {
                Channels.Add(new Channel
                {
                    Id = config.Id,
                    Name = config.Name,
                    MinValue = config.MinValue,
                    MaxValue = config.MaxValue,
                    Setpoint = config.DefaultSetpoint,
                    Unit = config.Unit,
                    Status = ChannelStatus.Idle
                });
            }

            _updateTimer.Interval = TimeSpan.FromMilliseconds(_configuration.UpdateIntervalMs);
            AddLog("Konfiguration geladen");
        }

        private void SaveConfiguration()
        {
            _configuration.GpibAddress = GpibAddress;
            _configuration.Channels = Channels.Select(c => new ChannelConfig
            {
                Id = c.Id,
                Name = c.Name,
                MinValue = c.MinValue,
                MaxValue = c.MaxValue,
                DefaultSetpoint = c.Setpoint,
                Unit = c.Unit,
                Enabled = true
            }).ToList();

            _configService.SaveConfiguration(_configuration);
            AddLog("Konfiguration gespeichert");
        }

        private void AddLog(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LogMessages.Insert(0, $"{DateTime.Now:HH:mm:ss.fff} - {message}");
                if (LogMessages.Count > 1000)
                    LogMessages.RemoveAt(LogMessages.Count - 1);
            });
        }
    }
}

