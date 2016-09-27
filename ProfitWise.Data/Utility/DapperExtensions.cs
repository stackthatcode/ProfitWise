namespace ProfitWise.Data.Utility
{
    public static class DapperExtensions
    {
        public static string EncodeForLike(this string term)
        {
            return term.Replace("[", "[[]").Replace("%", "[%]");
        }

        public static string PrepForLike(this string term)
        {
            return "%" + term.EncodeForLike().Replace(" ", "%") + "%";
        } 
    }
}
