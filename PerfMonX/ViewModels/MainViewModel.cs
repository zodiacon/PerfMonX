using MahApps.Metro;
using PerfMonX.Interfaces;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Zodiacon.WPF;

namespace PerfMonX.ViewModels {
	sealed class MainViewModel : BindableBase, IMainViewModel {
		readonly ObservableCollection<ITabViewModel> _tabs = new ObservableCollection<ITabViewModel>();
		readonly ObservableCollection<RunningCounterViewModel> _runningCounters = new ObservableCollection<RunningCounterViewModel>();

		public IList<RunningCounterViewModel> RunningCounters => _runningCounters;
		public AccentViewModel[] Accents => ThemeManager.Accents.Select(a => new AccentViewModel(a)).ToArray();

		AccentViewModel _currentAccent;

		public AccentViewModel CurrentAccent => _currentAccent;

		public ICommand ChangeAccentCommand => new DelegateCommand<AccentViewModel>(accent => {
			if (_currentAccent != null)
				_currentAccent.IsCurrent = false;
			_currentAccent = accent;
			accent.IsCurrent = true;
			RaisePropertyChanged(nameof(CurrentAccent));
		}, accent => accent != _currentAccent).ObservesProperty(() => CurrentAccent);

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

		public DelegateCommandBase AlwaysOnTopCommand => new DelegateCommand<Window>(win => win.Topmost = !win.Topmost);
		public DelegateCommandBase CloseTabCommand => new DelegateCommand<ITabViewModel>(tab => {
			if (tab is IDisposable disposable)
				disposable.Dispose();
			Tabs.Remove(tab);
		});

	}
}
