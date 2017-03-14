using System;

namespace ProfitWise.Data.Utility
{
    public class MethodLockResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
    }
}
