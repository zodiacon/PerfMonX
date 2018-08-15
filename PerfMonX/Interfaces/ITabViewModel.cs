using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerfMonX.Interfaces {
    interface ITabViewModel {
        string Header { get; }
        string Icon { get; }
		bool CanClose { get; }
    }
}
