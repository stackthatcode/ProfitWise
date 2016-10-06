USE ProfitWise;

SET SQL_SAFE_UPDATES = 0;


SELECT * FROM profitwiseshop;

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




SELECT PwMasterProductId FROM profitwisepicklistmasterproduct WHERE PwShopId = 100001 AND PwPickListId = 100008;


