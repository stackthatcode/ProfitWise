USE ProfitWise;

SET SQL_SAFE_UPDATES = 0;


SELECT * FROM profitwiseshop;

UPDATE profitwiseshop SET StartingDateForOrders = '2016-01-01';

SELECT * FROM profitwisebatchstate;

SELECT * FROM aspnetusers;
	
SELECT * FROM aspnetuserroles;

SELECT * FROM aspnetuserlogins;

SELECT * FROM profitwisepicklist;



UPDATE profitwisemastervariant 
SET StockedDirectly = 0
WHERE PwMasterProductId IN 
	( SELECT PwMasterProductId FROM profitwisepicklistmasterproduct WHERE PwShopId = 100001 AND PwPickListId = 100008 );

    
SELECT * FROM profitwisemastervariant WHERE PwMasterProductId = 3;


UPDATE profitwisemastervariant SET StockedDirectly = 0, Exclude = 0 WHERE PwMasterVariantId > 20 AND PwMasterVariantId <= 32;

SELECT * FROM profitwiseproduct WHERE PwMasterProductId = 3;

SELECT * FROM shopifyorderlineitem;

SELECT PwMasterProductId FROM profitwisepicklistmasterproduct WHERE PwShopId = 100001 AND PwPickListId = 100008;



/*** Building our Filter Queries ***/


SELECT DISTINCT(ProductType) FROM profitwiseproduct;

SELECT DISTINCT Vendor FROM profitwiseproduct WHERE ProductType = 'Filament'

SELECT DISTINCT Vendor FROM profitwiseproduct WHERE ProductType IN ( SELECT 'Accessories' );




SELECT PwMasterVariantId, COUNT(*) FROM profitwisevariant GROUP BY PwMasterVariantId;




/*** PLEASE SAVE THIS QUERY !!! ***/
SELECT t1.PwProductId, t1.PwVariantId, t2.PwMasterProductId, t3.PwMasterVariantId
FROM shopifyorderlineitem t1
	INNER JOIN profitwiseproduct t2 ON t1.PwProductId = t2.PwProductId
	INNER JOIN profitwisevariant t3 ON t1.PwVariantId = t3.PwVariantId	    
	INNER JOIN profitwisemastervariant t4 ON t3.PwMasterVariantId = t4.PwMasterVariantId AND t2.PwMasterProductId = t4.PwMasterProductId;
    
		


SELECT * FROM profitwisemastervariant;


profitwisemastervariant t1 INNER JOIN shopifyorderlineitem t2 t1.Pw;



