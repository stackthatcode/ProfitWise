

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



SELECT * FROM profitwisemastervariantcogscalc

SELECT * FROM profitwisemastervariantcogsdetail


SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + QUOTENAME(CONSTRAINT_NAME)), 'IsPrimaryKey') = 1
AND TABLE_NAME = 'profitwisemastervariant'

