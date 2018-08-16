using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using PerfMonX.Interfaces;
using PerfMonX.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace PerfMonX.ViewModels {
	sealed class GraphicTabViewModel : BindableBase, ITabViewModel, IDisposable {
		public string Header => "Graph";
		public string Icon => "/icons/graph.ico";
		public bool CanClose => true;

		Timer _timer;
		DispatcherTimer _refreshTimer;
		Dispatcher _dispatcher;
		DateTimeAxis _timeAxis;
		IMainViewModel _mainViewModel;
		static UpdateInterval[] _updateIntervals = new[]  {
			new UpdateInterval { Interval = 100, Text = "100 msec" },
			new UpdateInterval { Interval = 200, Text = "200 msec" },
			new UpdateInterval { Interval = 500, Text = "500 msec" },
			new UpdateInterval { Interval = 1000, Text = "1 sec" },
			new UpdateInterval { Interval = 2000, Text = "2 sec" },
			new UpdateInterval { Interval = 5000, Text = "5 sec" },
			new UpdateInterval { Interval = 10000, Text = "10 sec" },
		};

		public IList<RunningCounterViewModel> RunningCounters { get; }

		public PlotModel PlotModel { get; }

		public GraphicTabViewModel(IMainViewModel mainViewModel, IList<RunningCounterViewModel> counters) {
			_mainViewModel = mainViewModel;
			_dispatcher = Dispatcher.CurrentDispatcher;

			PlotModel = new PlotModel();
			RunningCounters = counters;

			var now = DateTimeAxis.ToDouble(DateTime.UtcNow);

			_timeAxis = new DateTimeAxis {
				Position = AxisPosition.Bottom,
				//Minimum = now,
				AbsoluteMinimum = now,
				//Maximum = DateTimeAxis.ToDouble(DateTime.UtcNow.AddMinutes(1)) + now,
				Title = "Time",
				TimeZone = TimeZoneInfo.Local,
				MaximumRange = DateTimeAxis.ToDouble(DateTime.UtcNow.AddMinutes(5)) - now,
				MinimumRange = DateTimeAxis.ToDouble(DateTime.UtcNow.AddSeconds(60)) - now,
			};
			_timeAxis.AxisChanged += OnTimeAxisChanged;

			PlotModel.Axes.Add(_timeAxis);
			PlotModel.Axes.Add(new LinearAxis {
				Position = AxisPosition.Left,
				//Minimum = 0,
				//Maximum = 100,
				IsPanEnabled = false,
				IsZoomEnabled = false,
				Title = "Value",
			});

			var colors = new OxyColor[] {
				OxyColors.Red, OxyColors.Blue, OxyColors.Green, OxyColors.Orange, OxyColors.Brown, OxyColors.Cyan, OxyColors.Purple,
				OxyColors.Gray, OxyColors.GreenYellow, OxyColors.Indigo, OxyColors.DarkBlue, OxyColors.Pink, OxyColors.Plum,
				OxyColors.SeaGreen, OxyColors.Fuchsia
			};

			int index = 0;
			foreach (var counter in counters) {
				var series = new LineSeries {
					StrokeThickness = 1,
					//MarkerStroke = OxyColors.Blue,
					Color = colors[index % colors.Length],
					ItemsSource = counter.Points,
				};
				index++;
				counter.LineColor = series.Color;
				counter.Series = series;
				PlotModel.Series.Add(series);
			}

			_refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
			_refreshTimer.Tick += OnRefresh;
			_refreshTimer.Start();

			var dispatcher = Dispatcher.CurrentDispatcher;
			_timer = new Timer(_ => Update(), null, 0, Interval);

		}

		private void OnTimeAxisChanged(object sender, AxisChangedEventArgs e) {
			
		}

		public UpdateInterval[] UpdateIntervals => _updateIntervals;

		UpdateInterval _updateInterval = _updateIntervals.First(i => i.Interval == 1000);
		public UpdateInterval UpdateInterval {
			get => _updateInterval;
			set {
				if (SetProperty(ref _updateInterval, value)) {
					if (_timer != null)
						_timer.Change(value.Interval, value.Interval);
				}
			}
		}
		private void Update() {
			foreach (var counter in RunningCounters) {
				if (counter.IsEnabled)
					counter.Points.Add(DateTimeAxis.CreateDataPoint(DateTime.UtcNow, counter.NextValue));
			}
			_dispatcher.InvokeAsync(() => {
				var now = DateTimeAxis.ToDouble(DateTime.UtcNow);
				var window = DateTimeAxis.ToDouble(DateTime.UtcNow.AddMinutes(1)) - DateTimeAxis.ToDouble(DateTime.UtcNow);
				_timeAxis.Minimum = now - window * .95;
				_timeAxis.Maximum = now + window * .05;
				PlotModel.InvalidatePlot(true);
			});
		}

		private void OnRefresh(object sender, EventArgs e) {
			foreach (var counter in RunningCounters)
				counter.Refresh();
		}

		bool _isPaused;
		public bool IsPaused {
			get => _isPaused;
			set {
				if (SetProperty(ref _isPaused, value)) {
					if (value) {
						_timer.Dispose();
						_timer = null;
					}
					else {
						_timer = new Timer(_ => Update(), null, 0, Interval);
					}
				}
			}
		}

		public DelegateCommandBase ExportCommand => new DelegateCommand(() => {
			var isPaused = IsPaused;
			try {
				IsPaused = true;
				var path = _mainViewModel.UI.FileDialogService.GetFileForSave("CSV Files|*.csv", "Export Data");
				if (path == null)
					return;
				ExportToFile(path);
			}
			finally {
				IsPaused = isPaused;
			}
		});

		public DelegateCommandBase ClearAllCommand => new DelegateCommand(() => {
			if (RunningCounters.Any() && RunningCounters[0].Points.Count > 0) {
				if (_mainViewModel.UI.MessageBoxService.ShowMessage("Clear all data?", Constants.Title, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
					return;
			}
			ClearAll();
		});

		private void ClearAll() {
			foreach (var counter in RunningCounters) {
				counter.Points.Clear();
			}
			_timeAxis.Minimum = _timeAxis.AbsoluteMinimum = DateTimeAxis.ToDouble(DateTime.UtcNow);
			_timeAxis.Reset();
		}

		void ExportToFile(string filename) {
			
		}

		public void Dispose() {
			_timer?.Dispose();
			foreach (var counter in RunningCounters)
				counter.Dispose();
		}

		int _interval = 1000;
		public int Interval {
			get => _interval;
			set {
				if (SetProperty(ref _interval, value)) {
					if (_timer != null)
						_timer.Change(value, value);
				}
			}
		}

		public DelegateCommandBase ResetCommand => new DelegateCommand(() => _timeAxis.Reset());
	}
}
