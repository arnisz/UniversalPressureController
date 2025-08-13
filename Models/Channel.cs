using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UniversalPressureController.Models
{
    /// <summary>
    /// Kanal-Datenmodell für Druckregler
    /// </summary>
    public class Channel : INotifyPropertyChanged
    {
        private int _id;
        private string _name;
        private double _setpoint;
        private double _actualValue;
        private double _minValue;
        private double _maxValue;
        private string _unit = "bar";
        private bool _isActive;
        private ChannelStatus _status;
        private DateTime _lastUpdate;
        
        public int Id 
        { 
            get => _id; 
            set { _id = value; OnPropertyChanged(); } 
        }
        
        public string Name 
        { 
            get => _name; 
            set { _name = value; OnPropertyChanged(); } 
        }
        
        public double Setpoint 
        { 
            get => _setpoint; 
            set 
            { 
                _setpoint = Math.Max(_minValue, Math.Min(_maxValue, value)); 
                OnPropertyChanged(); 
            } 
        }
        
        public double ActualValue 
        { 
            get => _actualValue; 
            set { _actualValue = value; OnPropertyChanged(); OnPropertyChanged(nameof(Deviation)); } 
        }
        
        public double Deviation => ActualValue - Setpoint;
        
        public double MinValue 
        { 
            get => _minValue; 
            set { _minValue = value; OnPropertyChanged(); } 
        }
        
        public double MaxValue 
        { 
            get => _maxValue; 
            set { _maxValue = value; OnPropertyChanged(); } 
        }
        
        public string Unit 
        { 
            get => _unit; 
            set { _unit = value; OnPropertyChanged(); } 
        }
        
        public bool IsActive 
        { 
            get => _isActive; 
            set { _isActive = value; OnPropertyChanged(); } 
        }
        
        public ChannelStatus Status 
        { 
            get => _status; 
            set { _status = value; OnPropertyChanged(); } 
        }
        
        public DateTime LastUpdate 
        { 
            get => _lastUpdate; 
            set { _lastUpdate = value; OnPropertyChanged(); } 
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum ChannelStatus
    {
        Idle,
        Running,
        Stabilizing,
        Error,
        Venting
    }
}
