using UniversalPressureController.Models;

namespace UniversalPressureController.Services
{
    public class ConfigurationService
    {
        public AppConfiguration Load() => new();
    }
}
