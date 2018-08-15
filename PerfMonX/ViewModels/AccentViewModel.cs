using MahApps.Metro;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace PerfMonX.ViewModels {
	sealed class AccentViewModel : BindableBase {
		public Accent Accent { get; }
		public AccentViewModel(Accent accent) {
			Accent = accent;
		}

		public Brush Brush => Accent.Resources["AccentColorBrush"] as Brush;
		public string Name => Accent.Name;

		bool _isCurrent;
		public bool IsCurrent {
			get => _isCurrent;
			set {
				if (SetProperty(ref _isCurrent, value) && value) {
					ChangeAccentColor();
				}
			}
		}

		public void ChangeAccentColor() {
			ThemeManager.ChangeAppStyle(Application.Current, Accent, ThemeManager.DetectAppStyle().Item1);
		}
	}
}
