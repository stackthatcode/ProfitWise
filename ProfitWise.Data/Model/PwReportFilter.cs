using System;
using Push.Foundation.Utilities.Helpers;

namespace ProfitWise.Data.Model
{
    public class PwReportFilter
    {
        public const string ProductType = "Product Type";
        public const string Vendor = "Vendor";
        public const string Product = "Product";
        public const string Sku = "Sku";

        public const string Empty = "(Empty)";

        public long PwReportId { get; set; }
        public long PwShopId { get; set; }
        public long PwFilterId { get; set; }
        public string FilterType { get; set; }
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

        public static bool FilterTypeUsesNumberKey(string filterType)
        {
            return filterType == Product || filterType == Sku;
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
            if (FilterType == Product)
            {
                return "Product is " + Title.IsNullOrEmptyAlt(Empty);
            }
            if (FilterType == Sku)
            {
                return "Variant is " + Title.IsNullOrEmptyAlt(Empty);
            }

            throw new Exception($"Can't recognize FilterType: {FilterType}");
        }
    }

}
