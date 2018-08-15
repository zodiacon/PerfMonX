using PerfMonX.Interfaces;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PerfMonX.ViewModels {
	sealed class ConfigureTabViewModel : BindableBase, ITabViewModel {
		public string Header => "Configuration";
		public string Icon => "/icons/config.ico";
		public bool CanClose => false;

		private string _machineName = ".";

		public string MachineName {
			get => _machineName;
			set => SetProperty(ref _machineName, value);
		}

		public PerformanceCounterCategoryViewModel[] Categories { get; private set; }
		public PerformanceCounterViewModel[] Counters { get; private set; }
		public string[] Instances { get; private set; }
		public ObservableCollection<RunningCounterViewModel> ActualCounters { get; } = new ObservableCollection<RunningCounterViewModel>();

		public DelegateCommandBase AddCountersCommand { get; }
		public DelegateCommandBase RunCountersCommand { get; }

		private PerformanceCounterCategoryViewModel _selectedCategory;

		public PerformanceCounterCategoryViewModel SelectedCategory {
			get => _selectedCategory;
			set {
				if (SetProperty(ref _selectedCategory, value)) {
					UpdateCounters();
					RaisePropertyChanged(nameof(Counters));
					RaisePropertyChanged(nameof(Instances));
				}
			}
		}

		private IList _selectedCounters, _selectedInstances;

		public IList SelectedCounters {
			get => _selectedCounters;
			set {
				_selectedCounters = value;
				RaisePropertyChanged(nameof(SelectedCounters));
			}
		}

		public IList SelectedInstances {
			get => _selectedInstances;
			set {
				_selectedInstances = value;
				RaisePropertyChanged(nameof(SelectedInstances));
			}
		}

		public IList<PerformanceCounterViewModel> GetSelectedCounters() {
			return SelectedCounters.Cast<PerformanceCounterViewModel>().ToList();
		}

		public IList<string> GetSelectedInstances() {
			return SelectedInstances.Cast<string>().ToList();
		}

		private void UpdateCounters() {
			Counters = null;
			Instances = null;
			SelectedCounters = null;
			SelectedInstances = null;
			if (SelectedCategory == null)
				return;

			if (SelectedCategory.IsMultiInstance) {
				var names = SelectedCategory.Category.GetInstanceNames();
				if (names.Length > 0) {
					try {
						Counters = SelectedCategory.Category.GetCounters(names[0]).OrderBy(c => c.CounterName).Select(c => new PerformanceCounterViewModel(c)).ToArray();
						Instances = names.OrderBy(name => name).ToArray();
					}
					catch {
					}
				}
			}
			else {
				Counters = SelectedCategory.Category.GetCounters().Select(c => new PerformanceCounterViewModel(c)).ToArray();
			}
		}

		readonly IMainViewModel MainViewModel;
		public ConfigureTabViewModel(IMainViewModel mainView) {
			MainViewModel = mainView;

			AddCountersCommand = new DelegateCommand(() => {
				if (SelectedCategory.IsMultiInstance) {
					foreach (var counter in GetSelectedCounters()) {
						foreach (var instance in GetSelectedInstances()) {
							var pc = new PerformanceCounter(SelectedCategory.Category.CategoryName, counter.Counter.CounterName, instance, true);
							ActualCounters.Add(new RunningCounterViewModel(pc));
						}
					}
				}
				else {
					foreach (var counter in GetSelectedCounters()) {
						var pc = new PerformanceCounter(SelectedCategory.Category.CategoryName, counter.Counter.CounterName, true);
						ActualCounters.Add(new RunningCounterViewModel(pc));
					}
				}
			}, () => SelectedCategory != null && SelectedCounters?.Count > 0 && (SelectedInstances?.Count > 0 && SelectedCategory.IsMultiInstance || !SelectedCategory.IsMultiInstance))
			.ObservesProperty(() => SelectedCategory).ObservesProperty(() => SelectedInstances).ObservesProperty(() => SelectedCounters);

			RunCountersCommand = new DelegateCommand(() => {
				if (ActualCounters.Count == 0) {
					MainViewModel.UI.MessageBoxService.ShowMessage("Please select at least one counter", Constants.Title);
					return;
				}

				var tab = new GraphicTabViewModel(MainViewModel, ActualCounters.ToList());
				MainViewModel.Tabs.Add(tab);
				MainViewModel.SelectedTab = tab;
				ActualCounters.Clear();
			});
		}

		public async Task InitAsync() {
			await InitCategoriesAsync();
		}

		async Task InitCategoriesAsync() {
			MainViewModel.SetStatusText("Loading peformance counter categories...");
			Categories = await Task.Run(() => {
				Thread.CurrentThread.Priority = ThreadPriority.Lowest;
				return PerformanceCounterCategory.GetCategories(MachineName).OrderBy(c => c.CategoryName).Select(c => new PerformanceCounterCategoryViewModel(c)).ToArray();
			});
			RaisePropertyChanged(nameof(Categories));
			MainViewModel.SetStatusText(string.Empty);
		}

		private string _searchCategoryText;

		public string SearchCategoryText {
			get => _searchCategoryText;
			set {
				if (SetProperty(ref _searchCategoryText, value)) {
					if (Categories == null)
						return;
					var view = CollectionViewSource.GetDefaultView(Categories);
					if (value == null)
						view.Filter = null;
					else {
						var text = value.ToLower();
						view.Filter = obj => {
							var cat = (PerformanceCounterCategoryViewModel)obj;
							return cat.Category.CategoryName.ToLower().Contains(text);
						};
					}
				}
			}
		}

		string _searchCounterText;
		public string SearchCounterText {
			get => _searchCounterText;
			set {
				if (SetProperty(ref _searchCounterText, value)) {
					var view = CollectionViewSource.GetDefaultView(Counters);
					if (value == null)
						view.Filter = null;
					else {
						var text = value.ToLower();
						view.Filter = obj => {
							var cat = (PerformanceCounterViewModel)obj;
							return cat.Counter.CounterName.ToLower().Contains(text);
						};
					}
				}
			}
		}

		string _searchInstanceText;
		public string SearchInstanceText {
			get => _searchInstanceText;
			set {
				if (SetProperty(ref _searchInstanceText, value)) {
					var view = CollectionViewSource.GetDefaultView(Instances);
					if (value == null)
						view.Filter = null;
					else {
						var text = value.ToLower();
						view.Filter = obj => {
							var instance = (string)obj;
							return instance.ToLower().Contains(text);
						};
					}
				}
			}
		}
	}
}
