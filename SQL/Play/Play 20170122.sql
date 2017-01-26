USE profitwise;

SET SQL_SAFE_UPDATES = 0;


SELECT * FROM profitwisevariant WHERE PwMasterVariantId = 449;

SELECT * FROM profitwiseprofitreportentry;

SELECT * FROM shopifyorderlineitem WHERE PwVariantId = 449;


SELECT * FROM profitwisepicklistmasterproduct;


UPDATE shopifyorderlineitem SET UnitCogs = 0;

SELECT * FROM profitwiseprofitreportentry;


UPDATE profitwisemastervariant SET CogsDetail = 0, CogsTypeId = 1, CogsCurrencyId = 1, CogsAmount = 0;

DELETE FROM profitwisemastervariantcogsdetail;



SELECT * FROM profitwisemastervariantcogsdetail;

SELECT * FROM profitwiseshop;



UPDATE shopifyorderlineitem SET UnitCogs = 0;

SELECT * FROM shopifyorderlineitem WHERE UnitCogs IS NULL;


SELECT * FROM profitwisemastervariant WHERE PwMasterProductId = 31;

SELECT * FROM profitwisemastervariant;

 WHERE CogsCurrencyId IS NULL;


SELECT * FROM profitwiseshop;

SELECT * FROM profitwisebatchstate;


SELECT * FROM profitwiseprofitreportentry;

SELECT * FROM shopifyorderlineitem;



