using PerfMonX.Interfaces;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerfMonX.ViewModels {
	sealed class PerformanceCounterTabViewModel : BindableBase, ITabViewModel {
		public string Header => "Counters";
		public string Icon => "/icons/counters.ico";
	}
}
