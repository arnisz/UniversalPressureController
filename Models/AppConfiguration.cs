namespace UniversalPressureController.Models
{
    public class AppConfiguration
    {
        public string GpibAddress { get; set; } = "GPIB0::7::INSTR";
        public int CommunicationTimeout { get; set; } = 5000;
        public double UpdateIntervalMs { get; set; } = 500;
        public List<ChannelConfig> Channels { get; set; } = new();
        public bool AutoConnect { get; set; } = true;
        public bool LogCommunication { get; set; } = false;
        public string LogPath { get; set; } = @".\Logs";
    }
    
    public class ChannelConfig
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public double DefaultSetpoint { get; set; }
        public string Unit { get; set; } = "bar";
        public bool Enabled { get; set; } = true;
    }
 
}
