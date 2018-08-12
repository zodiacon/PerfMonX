using OxyPlot;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerfMonX.ViewModels {
    public sealed class PerformanceCounterViewModel : BindableBase {
        public PerformanceCounter Counter { get; }

        public PerformanceCounterViewModel(PerformanceCounter counter) {
            Counter = counter;
        }

		public string InstanceName { get; set; }

    }
}
