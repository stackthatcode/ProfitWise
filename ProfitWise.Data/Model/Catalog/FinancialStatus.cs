using System;
using System.Collections.Generic;

namespace ProfitWise.Data.Model.Catalog
{
    public class FinancialStatus
    {
        public const int Pending = 1;
        public const int Authorized = 2;
        public const int PartiallyPaid = 3;
        public const int Paid = 4;
        public const int PartiallyRefunded = 5;
        public const int Refunded = 6;
        public const int Voided = 7;
    }

    public static class FinancialStatusExtensions
    {
        private static readonly 
            Dictionary<string, int> Map =
                new Dictionary<string, int>
                {
                    { "pending", FinancialStatus.Pending },
                    { "authorized", FinancialStatus.Authorized },
                    { "partially_paid", FinancialStatus.PartiallyPaid },
                    { "paid", FinancialStatus.Paid },
                    { "partially_refunded", FinancialStatus.PartiallyRefunded },
                    { "refunded", FinancialStatus.Refunded },
                    { "voided", FinancialStatus.Voided },
                };

        public static int ToFinancialStatus(this string financial_status)
        {
            if (!Map.ContainsKey(financial_status))
            {
                throw new ArgumentException("financial_status");
            }
            return Map[financial_status];
        }
    }
}
