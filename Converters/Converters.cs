using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using UniversalPressureController.Models;

namespace UniversalPressureController.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Brushes.Green : Brushes.Red;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ChannelStatus status)
            {
                return status switch
                {
                    ChannelStatus.Idle => Brushes.Gray,
                    ChannelStatus.Running => Brushes.Green,
                    ChannelStatus.Stabilizing => Brushes.Yellow,
                    ChannelStatus.Error => Brushes.Red,
                    ChannelStatus.Venting => Brushes.Blue,
                    _ => Brushes.Gray
                };
            }
            return Brushes.Gray;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
