using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using PerfMonX.Interfaces;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PerfMonX.ViewModels {
	sealed class GraphicTabViewModel : BindableBase, ITabViewModel {
		public string Header => "Graph";
		public string Icon => null;

		DispatcherTimer _timer;
		IList<RunningCounterViewModel> _counters;

		public PlotModel PlotModel { get; }

		public GraphicTabViewModel(IList<RunningCounterViewModel> counters) {
			PlotModel = new PlotModel();
			_counters = counters;

			PlotModel.Axes.Add(new DateTimeAxis {
				Position = AxisPosition.Bottom,
				Minimum = DateTimeAxis.ToDouble(DateTime.UtcNow),
				AbsoluteMinimum = DateTimeAxis.ToDouble(DateTime.UtcNow),
				Maximum = DateTimeAxis.ToDouble(DateTime.UtcNow.AddMinutes(1)),
				Title = "Time",
				TimeZone = TimeZoneInfo.Local,
				MaximumRange = DateTimeAxis.ToDouble(DateTime.UtcNow.AddMinutes(10)) - DateTimeAxis.ToDouble(DateTime.UtcNow),
				MinimumRange = DateTimeAxis.ToDouble(DateTime.UtcNow.AddSeconds(10)) - DateTimeAxis.ToDouble(DateTime.UtcNow)
			});
			PlotModel.Axes.Add(new LinearAxis {
				Position = AxisPosition.Left,
				Minimum = 0,
				//Maximum = 100,
				IsPanEnabled = false,
				IsZoomEnabled = false
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
					Color = colors[(index++) % colors.Length],
					ItemsSource = counter.Points
				};
				PlotModel.Series.Add(series);
			}

			_timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
			_timer.Tick += _timer_Tick;
			_timer.Start();
		}

		private void _timer_Tick(object sender, EventArgs e) {
			foreach (var counter in _counters) {
				counter.Points.Add(DateTimeAxis.CreateDataPoint(DateTime.UtcNow, counter.NextValue));
			}
			PlotModel.InvalidatePlot(true);
		}
	}
}
