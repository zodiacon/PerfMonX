using PerfMonX.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zodiacon.WPF;

namespace PerfMonX.Interfaces {
	interface IMainViewModel {
		void SetStatusText(string text);
		IUIServices UI { get; }
		IList<RunningCounterViewModel> RunningCounters { get; }
		IList<ITabViewModel> Tabs { get; }
		ITabViewModel SelectedTab { get; set; }
	}
}
