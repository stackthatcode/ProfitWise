namespace Push.Foundation.Utilities.Helpers
{
    public static class HelperMethods
    {
        public static bool IsNullOrEmpty(this string input)
        {
            return string.IsNullOrEmpty(input);
        }

        public static string IsNullOrEmptyAlt(this string input, string alternative)
        {
            return string.IsNullOrEmpty(input) ? alternative : input;
        }
    }
}

