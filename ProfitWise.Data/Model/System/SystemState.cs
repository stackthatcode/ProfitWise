using System;

namespace ProfitWise.Data.Model.System
{
    public class SystemState
    {
        public DateTime? ExchangeRateLastDate { get; set; }
        public bool MaintenanceActive { get; set; }
        public string MaintenanceReason { get; set; }
    }
}
