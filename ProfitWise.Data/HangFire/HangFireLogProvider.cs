using System;
using Hangfire.Logging;

namespace ProfitWise.Data.HangFire
{
    public class HangFireLogProvider : ILogProvider
    {        
        public ILog GetLogger(string name)
        {
            return new HangFireLogger();
        }
    }
}
