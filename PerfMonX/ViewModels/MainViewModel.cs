using MahApps.Metro;
using PerfMonX.Interfaces;
using PerfMonX.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;
using Zodiacon.WPF;

namespace PerfMonX.ViewModels {
	sealed class MainViewModel : BindableBase, IMainViewModel {
		readonly ObservableCollection<ITabViewModel> _tabs = new ObservableCollection<ITabViewModel>();
		readonly ObservableCollection<RunningCounterViewModel> _runningCounters = new ObservableCollection<RunningCounterViewModel>();

		public IList<RunningCounterViewModel> RunningCounters => _runningCounters;
		public AccentViewModel[] Accents => ThemeManager.Accents.Select(a => new AccentViewModel(a)).ToArray();
		public AppTheme[] Themes => ThemeManager.AppThemes.ToArray();

		AccentViewModel _currentAccent;

		public AccentViewModel CurrentAccent => _currentAccent;

		public ICommand ChangeAccentCommand => new DelegateCommand<AccentViewModel>(accent => {
			if (_currentAccent != null)
				_currentAccent.IsCurrent = false;
			_currentAccent = accent;
			accent.IsCurrent = true;
			RaisePropertyChanged(nameof(CurrentAccent));
		}, accent => accent != _currentAccent).ObservesProperty(() => CurrentAccent);

		public ICommand ChangeThemeCommand => new DelegateCommand<AppTheme>(theme => {
			var style = ThemeManager.DetectAppStyle();
			if (theme != style.Item1) {
				ThemeManager.ChangeAppStyle(Application.Current, style.Item2, theme);
			}
		});

		public IList<ITabViewModel> Tabs => _tabs;
		public IUIServices UI { get; }

		public MainViewModel(IUIServices ui) {
			UI = ui;
		}

		public ICommand LoadedCommand => new DelegateCommand(async () => {
			var configTab = new ConfigureTabViewModel(this);
			Tabs.Add(configTab);
			var graph = new GraphicTabViewModel(this, new List<RunningCounterViewModel> {
				new RunningCounterViewModel(new PerformanceCounter("Processor", "% Processor Time", "_Total", true))
			});
			Tabs.Add(graph);
			SelectedTab = graph;
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

		bool _alwaysOnTop;
		public DelegateCommandBase AlwaysOnTopCommand => new DelegateCommand<Window>(win => win.Topmost = _alwaysOnTop = !win.Topmost);
		public DelegateCommandBase CloseTabCommand => new DelegateCommand<ITabViewModel>(tab => {
			if (tab is IDisposable disposable)
				disposable.Dispose();
			Tabs.Remove(tab);
		});

		public void SaveSettings() {
			var style = ThemeManager.DetectAppStyle();
			var settings = new Settings {
				AlwaysOnTop = _alwaysOnTop,
				AccentColor = style.Item2.Name,
				Theme = style.Item1.Name
			};
			try {
				using (var fs = new FileStream(GetSettingsPath(), FileMode.Create)) {
					var serializer = new DataContractSerializer(typeof(Settings));
					serializer.WriteObject(fs, settings);
				}
			}
			catch {
			}
		}

		private string GetSettingsPath() {
			var directory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\PerfMonX";
			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);
			return directory + @"\PerfMonX.Settings.Xml";
		}

		public void LoadSettings(Window window) {
			try {
				using (var fs = File.OpenRead(GetSettingsPath())) {
					var serializer = new DataContractSerializer(typeof(Settings));
					var settings = serializer.ReadObject(fs) as Settings;
					if (settings != null) {
						if (settings.AlwaysOnTop)
							window.Topmost = true;
						var accent = Accents.FirstOrDefault(acc => acc.Name == settings.AccentColor);
						if(accent != null)
							ChangeAccentCommand.Execute(accent);
						var theme = Themes.FirstOrDefault(t => t.Name == settings.Theme);
						if (theme != null)
							ChangeThemeCommand.Execute(theme);
					}
				}
			}
			catch {
			}
		}
	}
}
