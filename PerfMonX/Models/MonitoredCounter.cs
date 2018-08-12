using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerfMonX.Models {
	class MonitoredCounter {
		public string Category { get; set; }
		public string Counter { get; set; }
		public string Instance { get; set; }
	}
}
