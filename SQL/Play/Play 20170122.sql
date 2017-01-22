USE profitwise;

SET SQL_SAFE_UPDATES = 0;


SELECT * FROM profitwisevariant WHERE PwMasterVariantId = 449;

SELECT * FROM profitwiseprofitreportentry;

SELECT * FROM shopifyorderlineitem WHERE PwVariantId = 449;



UPDATE shopifyorderlineitem SET UnitCogs = 0;
