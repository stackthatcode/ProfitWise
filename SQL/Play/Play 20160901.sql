USE ProfitWise;

SET SQL_SAFE_UPDATES = 0;


SELECT * FROM aspnetuserclaims;


SELECT COUNT(*) FROM profitwiseshop;


UPDATE profitwisepreferences SET StartingDateForOrders = '2016-04-01';

UPDATE profitwisebatchstate SET OrderDatasetEnd = '2016-08-31 00:00:00';

SELECT * FROM profitwisebatchstate;

SELECT * FROM profitwiseshop;

SELECT * FROM shopifyorder;

SELECT * FROM shopifyorderlineitem;



SELECT * FROM profitwisemasterproduct;

SELECT * FROM profitwiseproduct;
        

SELECT * FROM profitwisemastervariant;

SELECT * FROM profitwisevariant;


        
SELECT * FROM profitwisevariant WHERE PwMasterVariantId IN ( 
	SELECT PwMasterVariantId FROM profitwisemastervariant WHERE PwMasterProductId = 51 );



SELECT Title, COUNT(Title) FROM profitwiseproduct GROUP BY Title ORDER BY COUNT(Title) DESC;

SELECT SKU, COUNT(SKU) FROM profitwisevariant GROUP BY SKU ORDER BY COUNT(SKU) DESC;



SELECT * FROM profitwisemastervariant;

SELECT * FROM profitwisevariant WHERE LowPrice = 0 AND HighPrice = 0;
SELECT * FROM profitwisevariant  WHERE IsActive = false;
SELECT * FROM profitwisevariant;
    


SELECT profitwisemastervariant.*, profitwisevariant.* 
FROM profitwisemastervariant 
	INNER JOIN profitwisevariant 
		ON profitwisemastervariant.PwShopId = profitwisevariant.PwShopId 
		AND profitwisemastervariant.PwMasterVariantId = profitwisevariant.PwMasterVariantId;

SELECT t1.PwMasterVariantId, t1.PwShopId, t1.PwMasterProductId, t1.Exclude, t1.StockedDirectly, t2.PwVariantId, t2.PwShopId, t2.PwProductId, t2.ShopifyVariantId, t2.SKU, t2.Title, t2.IsActive, t2.IsPrimary
FROM profitwisemastervariant t1
	INNER JOIN profitwisevariant t2
		ON t1.PwShopId = t2.PwShopId 
		AND t1.PwMasterVariantId = t2.PwMasterVariantId;
                
                
                


SELECT * FROM profitwisemasterproduct;

SELECT ShopifyProductId, COUNT(*) FROM profitwiseproduct GROUP BY ShopifyProductId;

SELECT * FROM profitwiseproduct WHERE ShopifyProductId = 348907633;

SELECT * FROM profitwiseproduct WHERE IsPrimary = 0;



SELECT * FROM profitwisemastervariant;

SELECT ShopifyVariantId, COUNT(*) FROM profitwisevariant GROUP BY ShopifyVariantId;

SELECT * FROM profitwisevariant WHERE IsPrimary = 0;

SELECT * FROM profitwisevariant WHERE ShopifyVariantId = 805343293;

SELECT * FROM profitwiseproduct WHERE PwProductId IN ( 58, 105 );




