USE ProfitWise;

SET SQL_SAFE_UPDATES = 0;


SELECT * FROM profitwisereport;

SELECT * FROM profitwisereportfilter;

# PwReportId, PwShopId, PwFilterId, FilterType, NumberKey, StringKey, Title, Description, DisplayOrder


# Now is the time for the sacred Report Queries!!!
SELECT * FROM vw_MasterProductAndVariantSearch;
SELECT * FROM profitwisereportquerystub;
SELECT * FROM profitwisereportfilter;

SELECT * FROM profitwiseshop;


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
		t3.Quantity, t3.UnitPrice
FROM profitwisereportquerystub t1
	INNER JOIN profitwisevariant t2
    	ON t1.PwMasterVariantId = t2.PwMasterVariantId
	INNER JOIN shopifyorderlineitem t3
		ON t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId
	INNER JOIN shopifyorder t4
		ON t3.ShopifyOrderId = t4.ShopifyOrderId;


        
SELECT * FROM shopifyorderlineitem;
SELECT * FROM shopifyorder;
SELECT * FROM profitwisevariant;



SELECT * FROM profitwiseproduct WHERE ProductType IS NULL;



# Step #2 - isolate the Master Variant Id

SELECT PwMasterVariantId
FROM vw_MasterProductAndVariantSearch
WHERE 1 = 1 AND PwMasterProductId IN ( SELECT NumberKey FROM profitwisereportfilter WHERE FilterType = 3 );

#AND t3.PwMasterVariantId IN ( SELECT NumberKey FROM profitwisereportfilter WHERE FilterType = 4 ) 
#AND t1.ProductType IN ( SELECT StringKey FROM profitwisereportfilter WHERE FilterType = 1 )
#AND t1.Vendor IN ( SELECT StringKey FROM profitwisereportfilter WHERE FilterType = 2 )


CREATE VIEW vw_MasterVariantToVariant AS



### Milestone - this is the basic CoGS query (!!!)

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


