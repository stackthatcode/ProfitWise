using System;
using ProfitWise.Data.Model.Reports;

namespace ProfitWise.Data.Model.Profit
{
    public class GroupingKey
    {
        public GroupingKey()
        {
            ReportGrouping = ReportGrouping.Overall;
        }

        public GroupingKey(ReportGrouping grouping, string key)

        {
            ReportGrouping = grouping;
            if (grouping == ReportGrouping.ProductType)
            {
                ProductType = key;
                return;
            }
            if (grouping == ReportGrouping.Vendor)
            {
                Vendor = key;
                return;
            }

            throw new ArgumentException("Inappropiate match with grouping");
        }

        public GroupingKey(ReportGrouping grouping, long key)

        {
            ReportGrouping = grouping;
            if (grouping == ReportGrouping.Product)
            {
                PwMasterProductId = key;
                return;
            }
            if (grouping == ReportGrouping.Variant)
            {
                PwMasterVariantId = key;
                return;
            }
            throw new ArgumentException("Inappropiate match with grouping");
        }

        public static GroupingKey Factory(ReportGrouping grouping, string groupKey)
        {
            if (grouping == ReportGrouping.ProductType || grouping == ReportGrouping.Vendor)
            {
                return new GroupingKey(grouping, groupKey);
            }
            else if (grouping == ReportGrouping.Product || grouping == ReportGrouping.Variant)
            {
                return new GroupingKey(grouping, Int64.Parse(groupKey));
            }
            else
            {
                return new GroupingKey();
            }
        }

        public ReportGrouping ReportGrouping { get; set; }

        public string ProductType { get; set; }
        public string Vendor { get; set; }
        public long PwMasterProductId { get; set; }
        public long PwMasterVariantId { get; set; }
        

        public string KeyValue
        {
            get
            {
                if (ReportGrouping == ReportGrouping.Product)
                {
                    return PwMasterProductId.ToString();
                }
                if (ReportGrouping == ReportGrouping.Variant)
                {
                    return PwMasterVariantId.ToString();
                }
                if (ReportGrouping == ReportGrouping.ProductType)
                {
                    return ProductType;
                }
                if (ReportGrouping == ReportGrouping.Vendor)
                {
                    return Vendor;
                }
                return "All";
            }
        }

        // Could easily move to extension method...
        public bool MatchWithOrderLine(OrderLineProfit orderLineProfit)
        {
            if (ReportGrouping == ReportGrouping.Product)
            {
                return orderLineProfit.SearchStub.PwMasterProductId == PwMasterProductId;
            }
            if (ReportGrouping == ReportGrouping.Variant)
            {
                return orderLineProfit.SearchStub.PwMasterVariantId == PwMasterVariantId;
            }
            if (ReportGrouping == ReportGrouping.ProductType)
            {
                return orderLineProfit.SearchStub.ProductType == ProductType;
            }
            if (ReportGrouping == ReportGrouping.Vendor)
            {
                return orderLineProfit.SearchStub.Vendor == Vendor;
            }
            return true;
        }
    }
}
