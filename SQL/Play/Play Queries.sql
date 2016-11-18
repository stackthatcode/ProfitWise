USE profitwise;

SET SQL_SAFE_UPDATES = 0;

SELECT * FROM profitwisereportfilter;


SELECT ProductType AS Key, COUNT(*) AS Count
FROM profitwiseproduct
GROUP BY ProductType;


SELECT COUNT(DISTINCT(t1.PwProductId)) AS ProductCount, COUNT(t3.PwVariantId) AS VariantCount
FROM profitwiseproduct t1 
	INNER JOIN profitwisemastervariant t2 ON t1.PwMasterProductId = t2.PwMasterProductId
    INNER JOIN profitwisevariant t3 ON t2.PwMasterVariantId = t3.PwMasterVariantId    
WHERE t1.PwShopId = 100001 AND t2.PwShopId = 100001 AND t3.PwShopId = 100001
AND t1.IsPrimary = 1 AND t3.IsPrimary = 1

AND t1.ProductType IN ( SELECT StringKey FROM profitwisereportfilter WHERE PwReportId = 93333 AND FilterType = 'Product Type' )
AND t1.Vendor IN ( SELECT StringKey FROM profitwisereportfilter WHERE PwReportId = 93333 AND FilterType = 'Vendor' )
AND t1.PwMasterProductId IN ( SELECT NumberKey FROM profitwisereportfilter WHERE PwReportId = 93333 AND FilterType = 'Product' )
AND t2.PwMasterVariantId IN ( SELECT NumberKey FROM profitwisereportfilter WHERE PwReportId = 93333 AND FilterType = 'Product' )



SELECT * FROM profitwisereportfilter;



Master Product JOIN Product JOIN Master Variant JOIN Variant ( IsPrimary = true)
	WHERE 
	AND Product -> Product Type IN ( SELECT Product Type FROM Report Filters)
	AND Product -> Vendor IN ( SELECT Vendor FROM Report Filters)
	AND Master Product -> MPID IN ( SELECT MPID FROM Report Filters )
	AND Master Variant -> MVID IN ( SELECT MVID FROM Report Filters )

