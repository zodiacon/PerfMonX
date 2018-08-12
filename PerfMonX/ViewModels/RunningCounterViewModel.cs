using OxyPlot;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerfMonX.ViewModels {
	sealed class RunningCounterViewModel : BindableBase {
		public PerformanceCounter Counter { get; }
		public List<DataPoint> Points { get; } = new List<DataPoint>(120);

		public RunningCounterViewModel(PerformanceCounter pc) {
			Counter = pc;
		}

		public float NextValue => Counter.NextValue();

		OxyColor _color = OxyColors.Blue;
		public OxyColor Color {
			get => _color;
			set => SetProperty(ref _color, value);
		}
	}
}
