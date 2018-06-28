namespace Push.Foundation.Utilities.General
{
    public static class NumberHelpers
    {
        public static bool IsInteger(this string input)
        {
            int value;
            return int.TryParse(input, out value);
        }

        public static bool IsNonZeroDecimal(this string input)
        {
            return input.IsDecimal() && decimal.Parse(input) != 0m;
        }

        public static bool IsZero(this string input)
        {
            return !input.IsNonZeroDecimal();
        }

        public static bool IsDecimal(this string input)
        {
            decimal value;
            return decimal.TryParse(input, out value);
        }

        public static decimal ToDecimal(this string input)
        {
            return decimal.Parse(input);
        }

        public static int ToInteger(this string input)
        {
            return int.Parse(input);
        }

        public static long ToLong(this string input)
        {
            return long.Parse(input);
        }

        public static bool IsWithinRange(this string input, decimal low, decimal high)
        {
            var value = decimal.Parse(input);
            return value >= low && value <= high;
        }
    }
}

