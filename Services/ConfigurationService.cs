using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UniversalPressureController.Models;

namespace UniversalPressureController.Services
{
    /// <summary>
    /// Konfigurationsservice
    /// </summary>
    public class ConfigurationService
    {
        private readonly string _configPath;

        public ConfigurationService(string configPath = "config.json")
        {
            _configPath = configPath;
        }

        public AppConfiguration LoadConfiguration()
        {
            if (!File.Exists(_configPath))
            {
                var defaultConfig = CreateDefaultConfiguration();
                SaveConfiguration(defaultConfig);
                return defaultConfig;
            }

            try
            {
                string json = File.ReadAllText(_configPath);
                return JsonConvert.DeserializeObject<AppConfiguration>(json);
            }
            catch
            {
                return CreateDefaultConfiguration();
            }
        }

        public void SaveConfiguration(AppConfiguration config)
        {
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(_configPath, json);
        }

        private AppConfiguration CreateDefaultConfiguration()
        {
            return new AppConfiguration
            {
                GpibAddress = "GPIB0::7::INSTR",
                CommunicationTimeout = 5000,
                UpdateIntervalMs = 500,
                Channels = new List<ChannelConfig>
                {
                    new() { Id = 1, Name = "Kanal 1", MinValue = 0, MaxValue = 10, DefaultSetpoint = 1 },
                    new() { Id = 2, Name = "Kanal 2", MinValue = 0, MaxValue = 10, DefaultSetpoint = 1 }
                }
            };
        }
    }
}
