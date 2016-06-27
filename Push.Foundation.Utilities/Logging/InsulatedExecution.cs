using System;

namespace Push.Utilities.Logging
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
