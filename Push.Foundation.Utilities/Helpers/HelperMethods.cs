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

        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}

