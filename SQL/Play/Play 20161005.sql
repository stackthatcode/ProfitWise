USE ProfitWise;

SET SQL_SAFE_UPDATES = 0;


SELECT * FROM profitwiseshop;

SELECT * FROM profitwisebatchstate;


UPDATE profitwiseshop SET StartingDateForOrders = '2016-01-01';

SELECT * FROM profitwisebatchstate;


SELECT * FROM profitwisevariant;





/*** PLEASE SAVE THIS QUERY !!! ***/
SELECT t1.PwProductId, t1.PwVariantId, t2.PwMasterProductId, t3.PwMasterVariantId
FROM shopifyorderlineitem t1
	INNER JOIN profitwiseproduct t2 ON t1.PwProductId = t2.PwProductId
	INNER JOIN profitwisevariant t3 ON t1.PwVariantId = t3.PwVariantId	    
	INNER JOIN profitwisemastervariant t4 ON t3.PwMasterVariantId = t4.PwMasterVariantId AND t2.PwMasterProductId = t4.PwMasterProductId;
    
		


SELECT t1.PwMasterVariantId, t1.Exclude, t1.StockedDirectly, t1.CogsCurrencyId, t1.CogsAmount, t2.Title FROM profitwisemastervariant t1
	INNER JOIN profitwisevariant t2 ON t1.PwMasterVariantId = t2.PwMasterVariantId AND t2.IsPrimary = 1
WHERE t1.PwShopId = 100001;



SELECT * FROM profitwisemastervariant;

SELECT * FROM profitwisevariant;

