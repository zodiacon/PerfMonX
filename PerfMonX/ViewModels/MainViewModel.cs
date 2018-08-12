using PerfMonX.Interfaces;
using PerfMonX.Views;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Zodiacon.WPF;

namespace PerfMonX.ViewModels {
	sealed class MainViewModel : BindableBase, IMainViewModel {
		ObservableCollection<ITabViewModel> _tabs = new ObservableCollection<ITabViewModel>();
		ObservableCollection<RunningCounterViewModel> _runningCounters = new ObservableCollection<RunningCounterViewModel>();

		public IList<RunningCounterViewModel> RunningCounters => _runningCounters;

		public IList<ITabViewModel> Tabs => _tabs;
		public IUIServices UI { get; }

		public MainViewModel(IUIServices ui) {
			UI = ui;
		}

		public ICommand LoadedCommand => new DelegateCommand(async () => {
			var configTab = new ConfigureTabViewModel(this);
			Tabs.Add(configTab);
			SelectedTab = configTab;
			await configTab.InitAsync();
		});

		ITabViewModel _selectedTab;
		public ITabViewModel SelectedTab {
			get => _selectedTab;
			set => SetProperty(ref _selectedTab, value);
		}

		string _statusText;
		public string StatusText {
			get => _statusText;
			set => SetProperty(ref _statusText, value);
		}

		public void SetStatusText(string text) {
			StatusText = text;
		}
	}
}
