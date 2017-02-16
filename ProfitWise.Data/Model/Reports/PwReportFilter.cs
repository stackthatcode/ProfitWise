using System;
using Push.Foundation.Utilities.Helpers;

namespace ProfitWise.Data.Model.Reports
{
    public class PwReportFilter
    {
        public const int ProductType = 1;
        public const int Vendor = 2;
        public const int MasterProduct = 3;
        public const int MasterVariant = 4;
        public const int Product = 5;
        public const int Variant = 6;


        public const string Empty = "(Empty)";

        public long PwReportId { get; set; }
        public long PwShopId { get; set; }
        public long PwFilterId { get; set; }
        public int FilterType { get; set; }
        public long? NumberKey { get; set; }
        public string StringKey { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }


        public void SetKeyFromExternal(string input)
        {
            if (FilterTypeUsesNumberKey(FilterType))
            {
                NumberKey = long.Parse(input);
            }
            else
            {
                StringKey = input;
            }
        }

        public bool UsesNumberKey => FilterTypeUsesNumberKey(FilterType);

        public static bool FilterTypeUsesNumberKey(int filterType)
        {
            return filterType == MasterProduct || filterType == MasterVariant ||
                   filterType == Product || filterType == Variant;
        }

        public string DescriptionBuilder()
        {
            if (FilterType == ProductType)
            {
                return "Product type is " + Title.IsNullOrEmptyAlt(Empty);
            }
            if (FilterType == Vendor)
            {
                return "Vendor is " + Title.IsNullOrEmptyAlt(Empty);
            }
            if (FilterType == MasterProduct || FilterType == Product)
            {
                return "Product is " + Title.IsNullOrEmptyAlt(Empty);
            }
            if (FilterType == MasterVariant || FilterType == Variant)
            {
                return "Variant is " + Title.IsNullOrEmptyAlt(Empty);
            }

            throw new Exception($"Can't recognize FilterType: {FilterType}");
        }
    }
}

