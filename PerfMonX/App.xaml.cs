using PerfMonX.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Zodiacon.WPF;

namespace PerfMonX {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
		MainViewModel _mainViewModel;

        protected override void OnStartup(StartupEventArgs e) {
			Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

            var ui = new UIServicesDefaults();
            var vm = new MainViewModel(ui);
            var win = new MainWindow { DataContext = vm };
            ui.MessageBoxService.SetOwner(win);
			vm.LoadSettings(win);
			_mainViewModel = vm;

            win.Show();
        }

		protected override void OnExit(ExitEventArgs e) {
			_mainViewModel.SaveSettings();

			base.OnExit(e);
		}
	}
}
