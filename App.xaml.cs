using System.Windows;
using UniversalPressureController.Services;
using UniversalPressureController.ViewModels;

namespace UniversalPressureController
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var gpibService = new MensorGpibService();
            var configService = new ConfigurationService();
            var mainViewModel = new MainViewModel(gpibService, configService);

            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            mainWindow.Show();
        }
    }
}
