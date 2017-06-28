

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


SELECT * FROM shopifyorder;

SELECT * FROM shopifyorderrefund;

SELECT * FROM shopifyorderadjustment;

SELECT * FROM shopifyorderlineitem;


SELECT * FROM profitwiseprofitquerystub



