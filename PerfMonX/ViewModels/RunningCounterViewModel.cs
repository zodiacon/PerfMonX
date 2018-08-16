using OxyPlot;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;

namespace PerfMonX.ViewModels {
	sealed class RunningCounterViewModel : BindableBase, IDisposable {
		public PerformanceCounter Counter { get; }
		public List<DataPoint> Points { get; } = new List<DataPoint>(120);

		LineThickness[] _strokeThicknessValues;
		public LineThickness[] StrokeThicknessValues => _strokeThicknessValues;

		LineThickness _strokeThickness;
		public LineThickness StrokeThickness {
			get => _strokeThickness ?? (_strokeThickness = _strokeThicknessValues[0]);
			set {
				if (SetProperty(ref _strokeThickness, value)) {
					Series.StrokeThickness = value.Thickness;
				}
			}
		}

		float _scale = 1;
		public float Scale {
			get => _scale;
			set {
				if (SetProperty(ref _scale, value)) {
					for (int i = 0; i < Points.Count; i++)
						Points[i] = new DataPoint(Points[i].X, Points[i].Y / value);
				}
			}
		}

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
		public float MinValue { get; private set; } = float.MaxValue;
		public float MaxValue { get; private set; } = float.MinValue;

		OxyColor _lineColor;
		Brush _brush;

		public OxyColor LineColor {
			get => _lineColor;
			set {
				if (SetProperty(ref _lineColor, value)) {
					_brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(_lineColor.R, _lineColor.G, _lineColor.B));
					_brush.Freeze();

					RaisePropertyChanged(nameof(ColorAsBrush));

					_strokeThicknessValues = Enumerable.Range(1, 5).Select(i => new LineThickness { Thickness = i, Brush = _brush }).ToArray();
					RaisePropertyChanged(nameof(StrokeThicknessValues));
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

		public LineSeries Series { get; set; }

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

		public void Dispose() {
			Counter.Dispose();
		}

		public DelegateCommandBase ToggleCheckCommand => new DelegateCommand(() => IsVisible = !IsVisible);
	}
}
