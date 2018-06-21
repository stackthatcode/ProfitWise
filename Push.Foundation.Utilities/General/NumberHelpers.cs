namespace Push.Foundation.Utilities.General
{
    public static class NumberHelpers
    {
        public static bool IsInteger(this string input)
        {
            int value;
            return int.TryParse(input, out value);
        }

        public static bool IsDecimal(this string input)
        {
            decimal value;
            return decimal.TryParse(input, out value);
        }

        public static bool IsWithinRange(this string input, decimal low, decimal high)
        {
            var value = decimal.Parse(input);
            return value >= low && value <= high;
        }
    }
}

