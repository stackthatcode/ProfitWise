

SELECT * FROM systemstate;

SELECT * FROM profitwiseshop;



UPDATE profitwiseshop SET IsDataLoaded = 0 WHERE PwShopId = 100001;
UPDATE profitwiseshop SET IsDataLoaded = 1 WHERE PwShopId = 100001;



UPDATE profitwisetour 
	SET ShowPreferences = 1,
	ShowProducts = 1,
	ShowProductDetails = 1,
	ShowProductConsolidationOne = 1,
	ShowProductConsolidationTwo = 1,
	ShowProfitabilityDashboard = 1,
	ShowEditFilters = 1,
	ShowProfitabilityDetail = 1,
	ShowGoodsOnHand = 1
WHERE PwShopId = 100001;




SELECT t2.Cancelled, t2.FinancialStatus, t1.PaymentStatus, SUM(NetSales)
FROM profitreportentry(100001) t1
    INNER JOIN ordertable(100001) t2 ON t1.ShopifyOrderId = t2.ShopifyOrderId
WHERE EntryDate >= '2017-06-01' AND EntryDate <= '2017-06-01' AND EntryType <> 4
GROUP BY t2.Cancelled, t2.FinancialStatus, t1.PaymentStatus



SELECT * FROM profitreportentry(100001)
WHERE EntryDate = '2017-01-04' 



SELECT ShopifyOrderId, OrderCountOrderId, SUM (NetSales)
FROM profitreportentry(100001) 
WHERE EntryDate = '2017-01-04'
GROUP BY ShopifyOrderId, OrderCountOrderId
ORDER BY ShopifyOrderId


SELECT ShopifyOrderId, SUM (NetSales) 
FROM profitreportentry(100001) WHERE EntryDate = '2017-01-04' 
GROUP BY ShopifyOrderId;


SELECT SUM (NetSales)
FROM profitreportentryprocessed(100001, 1, 0.20, 1)
WHERE EntryDate = '2017-01-04' AND PwVariantId IS NOT NULL;





SELECT * FROM 
( SELECT t2.*
	FROM profitquerystub(100001) t1
		RIGHT OUTER JOIN variant(100001) t2 
			ON t1.PwMasterVariantId = t2.PwMasterVariantId
	WHERE t1.PwMasterVariantId IS NULL )



SELECT * FROM variant(100001) WHERE PwVariantId;

SELECT * FROM profitreportentryprocessed(100001, 1, 0.20, 1)
WHERE EntryDate = '2017-01-04' AND PwVariantId IN ( 355, 356 );


SELECT ShopifyOrderId, EntryType, SUM(NetSales)
FROM profitreportentry(100001) 
WHERE ShopifyOrderId  IN ( 4444155282, 4444204050, 4444594322  )
GROUP BY ShopifyOrderId, EntryType
ORDER BY ShopifyOrderId



SELECT * FROM profitreportentry(100001)




SELECT * FROM profitreportentryprocessed(100001, 1, 0.20, 1)
WHERE EntryDate = '2017-01-04' 

AND PwVariantId IS NULL


SELECT * FROM profitreportentry(100001) 
WHERE EntryDate = '2017-01-04' 
ORDER BY ShopifyOrderId




-- Shopify says => 14149.48
-- ProfitWise says => 14131.98

SELECT 14143.70 + 5.78
SELECT 14126.20




SELECT * FROM profitreportentry(100001) WHERE ShopifyOrderId = 4445943826;

SELECT * FROM profitwisemastervariantcogscalc

SELECT * FROM profitwisemastervariantcogsdetail

SELECT * FROM shopifyorder;

SELECT * FROM shopifyorderrefund;

SELECT * FROM shopifyorderadjustment;

SELECT * FROM shopifyorderlineitem;

SELECT * FROM profitwiseprofitquerystub;




