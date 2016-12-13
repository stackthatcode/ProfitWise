USE ProfitWise;

SET SQL_SAFE_UPDATES = 0;




SELECT * FROM vw_ReportOrderset WHERE PwReportId = 99739;

SELECT t1.PwReportId, t1.PwShopId, t2.PwMasterVariantId, t2.PwProductId, t2.PwVariantId, 


SELECT * FROM profitwisereportquerystub;


SELECT t1.PwReportId, t1.PwShopId, t2.PwMasterVariantId, t2.PwProductId, t2.PwVariantId, 
		t3.OrderDate, t3.ShopifyOrderId, t3.ShopifyOrderLineId, t3.Quantity, t3.TotalRestockedQuantity, t3.UnitPrice, t3.GrossRevenue,
        t4.OrderNumber
FROM profitwisereportquerystub t1
	INNER JOIN profitwisevariant t2
		ON t1.PwMasterVariantId = t2.PwMasterVariantId 
			AND t1.PwShopId = t2.PwShopId
	INNER JOIN shopifyorderlineitem t3
		ON t2.PwProductId = t3.PwProductId 
			AND t2.PwVariantId = t3.PwVariantId
            AND t2.PwShopID = t3.PwShopId
	INNER JOIN shopifyorder t4
		ON t3.ShopifyOrderId = t4.ShopifyOrderId
            AND t3.PwShopID = t4.PwShopId
WHERE t1.PwReportID = 99740
AND t1.PwShopId = 1
AND t3.OrderDate >= '2016-01-01'
AND t3.OrderDate <= '2016-12-31';



SELECT * FROM vw_ReportOrderset;


SELECT * FROM shopifyorderlineitem;



# OLD STUFF vvvvvvv

SELECT * FROM profitwisereport;

SELECT * FROM profitwisereportfilter;

# PwReportId, PwShopId, PwFilterId, FilterType, NumberKey, StringKey, Title, Description, DisplayOrder


# Now is the time for the sacred Report Queries!!!
SELECT * FROM vw_MasterProductAndVariantSearch;
SELECT * FROM profitwisereportquerystub;
SELECT * FROM profitwisereportfilter;
SELECT * FROM profitwiseshop;



SELECT * FROM vw_MasterProductAndVariantSearch;

# STAGE 1
DELETE FROM profitwisereportquerystub;

INSERT INTO profitwisereportquerystub
SELECT 99739, 1, PwMasterVariantId
FROM vw_MasterProductAndVariantSearch

GROUP BY PwMasterVariantId;


# We can now use this stub to join back up with our Product and Variant data for Product Title, Variant Title or Sku
SELECT * FROM vw_MasterProductAndVariantSearch t1
	INNER JOIN profitwisereportquerystub t2
		ON t1.PwMasterVariantId	= t2.PwMasterVariantId;



# STAGE 2 - pull the order data
SELECT t2.PwMasterVariantId, t2.PwProductId, t2.PwVariantId, t4.OrderNumber, t3.ShopifyOrderId, t3.ShopifyOrderLineId, t3.OrderDate, 
		t3.Quantity, t3.TotalRestockedQuantity, t3.UnitPrice, t3.GrossRevenue
FROM profitwisereportquerystub t1
	INNER JOIN profitwisevariant t2
    	ON t1.PwMasterVariantId = t2.PwMasterVariantId
	INNER JOIN shopifyorderlineitem t3
		ON t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId
	INNER JOIN shopifyorder t4
		ON t3.ShopifyOrderId = t4.ShopifyOrderId

        
WHERE t2.PwMasterVariantId = 427;

SELECT * FROM profitwisevariant WHERE PwMasterVariantId = 427;



# STAGE 3 - pull the CoGS data (version 1)
SELECT t1.PwMasterVariantId, t1.CogsCurrencyId, t1.CogsAmount
FROM profitwisemastervariant t1 
	INNER JOIN profitwisereportquerystub t2 ON t1.PwMasterVariantId = t2.PwMasterVariantId;



SELECT * FROM profitwisemastervariant;


# STAGE 4 - pre-grouped output

SELECT PwMasterProductId, ProductType, Vendor, PwMasterVariantId, OrderDate, NormalizedGrossRevenue, NormalizedCostOfGoodsSold

# TODO - how to join up to get Product Title and Variant Title...?


SELECT * FROM vw_MasterProductAndVariantSearch;




# JSON-ready output
SELECT GroupingKey, GroupingName, OrderDate, TotalNormalizedGrossRevenue, TotalNormalizedCostOfGoods


SELECT * FROM vw_ReportOrderset;

SELECT * FROM vw_ReportOrderset WHERE OrderDate > '2016-03-01';
WHERE PwShopId = @PwShopId AND PWReportId = @reportId

SELECT * FROM shopifyorderlineitem;
SELECT MAX(OrderDate) FROM shopifyorderlineitem;

SELECT * FROM profitwisevariant;

SELECT * FROM profitwiseproduct WHERE ProductType IS NULL;


SELECT * FROM vw_MasterProductAndVariantSearch;

SELECT t2.*
FROM profitwisereportquerystub t1 
	INNER JOIN vw_MasterProductAndVariantSearch t2
		ON t1.PwMasterVariantId = t2.PwMasterVariantId;




# Step #2 - isolate the Master Variant Id

SELECT PwMasterVariantId
FROM vw_MasterProductAndVariantSearch
WHERE 1 = 1 AND PwMasterProductId IN ( SELECT NumberKey FROM profitwisereportfilter WHERE FilterType = 3 );

#AND t3.PwMasterVariantId IN ( SELECT NumberKey FROM profitwisereportfilter WHERE FilterType = 4 ) 
#AND t1.ProductType IN ( SELECT StringKey FROM profitwisereportfilter WHERE FilterType = 1 )
#AND t1.Vendor IN ( SELECT StringKey FROM profitwisereportfilter WHERE FilterType = 2 )


CREATE VIEW vw_MasterVariantToVariant AS






### Milestone - this is the basic CoGS query (!!!)
### OLD STUFF
SELECT t4.*, t3.*, t2.*, t1.*
FROM shopifyorderlineitem t1
	INNER JOIN profitwisevariant t2 ON t1.PwVariantId = t2.PwVariantId
    INNER JOIN profitwisemastervariant t3 ON t2.PwMasterVariantId = t3.PwMasterVariantId
	INNER JOIN shopifyorder t4 ON t1.ShopifyOrderId = t4.ShopifyOrderId
    
WHERE t2.PwVariantId IN 
(
	# Possibly cache this result?
	SELECT PwVariantId FROM profitwisevariant WHERE PwMasterVariantId IN 
	( 	
		SELECT PwMasterVariantId
		FROM vw_MasterProductAndVariantSearch
        
        # This is the part that will be customized...
		WHERE 1 = 1 AND PwMasterProductId IN ( SELECT NumberKey FROM profitwisereportfilter WHERE FilterType = 3  ) 
	) 
);








SELECT * FROM profitwisevariant;

SELECT PwMasterProductId FROM vw_MasterProductAndVariant;

SELECT * FROM shopifyorderlineitem;


SELECT * FROM vw_MasterProductAndVariant  WHERE PwMasterProductId = 88;


SELECT * FROM profitwiseproduct WHERE 


