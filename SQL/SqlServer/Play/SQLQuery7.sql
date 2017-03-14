



-- ### Batch State and Entry Ledger resets ###
/*
UPDATE batchstate(100001) SET OrderDatasetStart = NULL, OrderDatasetEnd = NULL;

DELETE FROM  profitreportentry(100001);
*/

SELECT * FROM profitreportentry(100001) WHERE EntryType = 4;



DECLARE @ShopifyOrderId bigint = 1511488453;
SELECT * FROM orderlineitem(100001) WHERE ShopifyOrderId = @ShopifyOrderId;
SELECT * FROM orderrefund(100001) WHERE ShopifyOrderId = @ShopifyOrderId;
SELECT * FROM orderadjustment(100001) WHERE ShopifyOrderId = @ShopifyOrderId;
SELECT * FROM ordertable(100001) WHERE ShopifyOrderId = @ShopifyOrderId;





DECLARE @CogsAmount decimal(15,2) = 0.66;
DECLARE @PwShopId int = 100001;
DECLARE @CogsCurrencyId int = 1;
DECLARE @DestinationCurrencyId int = 1;
DECLARE @PwMasterProductId int = 161;
DECLARE @StartDate datetime = '2015-12-01';
DECLARE @EndDate datetime = '2099-12-31';

UPDATE t3 SET t3.UnitCogs = (@CogsAmount * ISNULL(t4.Rate, 0))                                 
FROM mastervariant(@PwShopId) t1                 
	INNER JOIN variant(@PwShopId) t2                
		ON t1.PwMasterVariantId = t2.PwMasterVariantId                
	INNER JOIN orderlineitem(@PwShopId) t3                
		ON t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId                               
	LEFT JOIN exchangerate t4                
		ON t3.OrderDate = t4.[Date]                 
		AND t4.SourceCurrencyId = @CogsCurrencyId                
		AND t4.DestinationCurrencyId = @DestinationCurrencyId                
WHERE t1.PwShopId = @PwShopId 
AND t1.PwMasterProductId = @PwMasterProductId 
AND t3.OrderDate >= @StartDate AND t3.OrderDate <= @EndDate


