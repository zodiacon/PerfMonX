using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerfMonX.ViewModels {
    public sealed class PerformanceCounterCategoryViewModel {
        public PerformanceCounterCategory Category { get; }
        public IList<PerformanceCounterViewModel> Counters { get; } = new ObservableCollection<PerformanceCounterViewModel>();
        public IList<string> Instances { get; } = new ObservableCollection<string>();

        public PerformanceCounterCategoryViewModel(PerformanceCounterCategory category) {
            Category = category;
        }

        public bool IsMultiInstance => Category.CategoryType == PerformanceCounterCategoryType.MultiInstance;
    }
}
