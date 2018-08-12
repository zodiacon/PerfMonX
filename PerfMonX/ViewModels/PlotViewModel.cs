using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using PerfMonX.Interfaces;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

namespace PerfMonX.ViewModels {
	class PlotViewModel : BindableBase {
		public PlotModel PlotModel { get; }

		public PlotViewModel(IMainViewModel mainViewModel, IEnumerable<DataPoint> data) {
			PlotModel = new PlotModel();
			var series = new LineSeries {
				StrokeThickness = 1,
				MarkerStroke = OxyColors.Blue,
				Color = OxyColors.Red,
				ItemsSource = data
			};
			PlotModel.Series.Add(series);
			PlotModel.Axes.Add(new DateTimeAxis {
				Position = AxisPosition.Bottom,
				Minimum = DateTimeAxis.ToDouble(DateTime.UtcNow),
				AbsoluteMinimum = DateTimeAxis.ToDouble(DateTime.UtcNow),
				Maximum = DateTimeAxis.ToDouble(DateTime.UtcNow.AddMinutes(1)),
				Title = "Time",
				TimeZone = TimeZoneInfo.Local,
				MaximumRange = TimeSpanAxis.ToDouble(TimeSpan.FromMinutes(5)),
				MinimumRange = TimeSpanAxis.ToDouble(TimeSpan.FromSeconds(10))
			});
			PlotModel.Axes.Add(new LinearAxis {
				Position = AxisPosition.Left,
				Minimum = 0,
				Maximum = 100,
				IsPanEnabled = false,
				IsZoomEnabled = false
			});
		}

		internal void Update() {
			PlotModel.InvalidatePlot(true);
		}
	}
}
