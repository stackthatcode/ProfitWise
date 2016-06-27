using System;

namespace Push.Foundation.Utilities.Logging
{
    public class InsulatedExecution
    {
        public static void Act(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                LoggerSingleton.Get().Error(ex);
            }
        }
    }
}
