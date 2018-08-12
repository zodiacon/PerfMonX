using OxyPlot;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace PerfMonX.ViewModels {
	sealed class RunningCounterViewModel : BindableBase {
		public PerformanceCounter Counter { get; }
		public List<DataPoint> Points { get; } = new List<DataPoint>(120);

		public RunningCounterViewModel(PerformanceCounter pc) {
			Counter = pc;
		}

		bool _minValueChanged, _maxValueChanged;

		public float NextValue {
			get {
				try {
					var value = Counter.NextValue();
					LastValue = value;
					if (value < MinValue) {
						MinValue = value;
						_minValueChanged = true;
					}
					else if (value > MaxValue) {
						MaxValue = value;
						_maxValueChanged = true;
					}
					return value;
				}
				catch (Exception) {
					// counter went away
					IsEnabled = false;
					return 0;
				}
			}
		}

		public bool IsEnabled { get; private set; } = true;

		public float LastValue { get; private set; }
		public float MinValue { get; private set; }
		public float MaxValue { get; private set; }

		OxyColor _color;
		Brush _brush;

		public OxyColor Color {
			get => _color;
			set {
				if (SetProperty(ref _color, value)) {
					_brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(_color.R, _color.G, _color.B));
					RaisePropertyChanged(nameof(ColorAsBrush));
				}
			}
		}

		public Brush ColorAsBrush => _brush;

		bool _isVisible = true;
		public bool IsVisible {
			get => _isVisible;
			set {
				if (SetProperty(ref _isVisible, value)) {
					Series.IsVisible = value;
				}
			}
		}

		public Series Series { get; set; }

		public string CategoryName => Counter.CategoryName;
		public string CounterName => Counter.CounterName;
		public string InstanceName => Counter.InstanceName ?? "-";

		public void Refresh() {
			RaisePropertyChanged(nameof(LastValue));
			if (_minValueChanged) {
				RaisePropertyChanged(nameof(MinValue));
				_minValueChanged = false;
			}
			if (_maxValueChanged) {
				RaisePropertyChanged(nameof(MaxValue));
				_maxValueChanged = false;
			}
		}

		public DelegateCommandBase ToggleCheckCommand => new DelegateCommand(() => IsVisible = !IsVisible);
	}
}
