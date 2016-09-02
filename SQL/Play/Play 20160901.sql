USE ProfitWise;

SET SQL_SAFE_UPDATES = 0;



UPDATE profitwisepreferences SET StartingDateForOrders = '2016-04-01';

SELECT * FROM profitwisepreferences;



UPDATE profitwisebatchstate SET OrderDatasetEnd = '2016-08-31 00:00:00';

SELECT * FROM profitwisebatchstate;



SELECT t1.*, t2.* 
FROM profitwisemasterproduct t1
	INNER JOIN profitwiseproduct t2
		ON t1.PwMasterProductId = t2.PwMasterProductId
ORDER BY t1.PwMasterProductId, t2.PwProductId;
        
SELECT * FROM profitwiseproduct;



SELECT t1.*, t2.* 
FROM profitwisemastervariant t1
	INNER JOIN profitwisevariant t2
		ON t1.PwMasterVariantId = t2.PwMasterVariantId
ORDER BY t2.SKU;
        
        
        
SELECT * FROM profitwisevariant WHERE PwMasterVariantId IN ( 
	SELECT PwMasterVariantId FROM profitwisemastervariant WHERE PwMasterProductId = 51 );


SELECT * FROM profitwisemastervariant;

SELECT Title, COUNT(Title) FROM profitwiseproduct GROUP BY Title ORDER BY COUNT(Title) DESC;

SELECT SKU, COUNT(SKU) FROM profitwisevariant GROUP BY SKU ORDER BY COUNT(SKU) DESC;

SELECT * FROM profitwisevariant WHERE ShopifyVariantID = 24948131849;




DELETE FROM profitwisemasterproduct
WHERE PwShopId =  AND PwMasterProductId NOT IN ( SELECT PwMasterProductId FROM profitwiseproduct );

DELETE FROM profitwisemastervariant 
WHERE PwShopId =  AND PwMasterVariantId NOT IN ( SELECT PwMasterVariantId FROM profitwisevariant );




SELECT * FROM shopifyorderlineitem;

SELECT profitwisemastervariant.*, profitwisevariant.* 
                    FROM profitwisemastervariant INNER JOIN profitwisevariant 
                ON profitwisemastervariant.PwShopId = profitwisevariant.PwShopId 
                AND profitwisemastervariant.PwMasterVariantId = profitwisevariant.PwMasterVariantId;


SELECT t1.PwMasterVariantId, t1.PwShopId, t1.PwMasterProductId, t1.Exclude, t1.StockedDirectly, t2.PwVariantId, t2.PwShopId, t2.PwProductId, t2.ShopifyVariantId, t2.SKU, t2.Title, t2.IsActive, t2.IsPrimary
FROM profitwisemastervariant t1
	INNER JOIN profitwisevariant t2
		ON t1.PwShopId = t2.PwShopId 
		AND t1.PwMasterVariantId = t2.PwMasterVariantId;
                
                

