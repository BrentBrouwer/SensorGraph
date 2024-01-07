using Microsoft.Research.DynamicDataDisplay.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorGraph.ViewModel
{
    public class VoltagePointCollection : RingArray<VoltagePoint>
    {
        const int TotalPoints = 2000;
        public VoltagePointCollection() : base(TotalPoints)
        {

        }
    }

    public class VoltagePoint
    {
        public DateTime Date { get; set; }
        public double Voltage { get; set; }

        public VoltagePoint(DateTime date, double voltage)
        {
            this.Date = date;
            this.Voltage = voltage;
        }
    }
}
