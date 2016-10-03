namespace Push.Foundation.Utilities.Helpers
{
    public static class HelperMethods
    {
        public static bool IsNullOrEmpty(this string input)
        {
            return input == null || input == "";
        }
    }
}

