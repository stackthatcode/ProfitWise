USE ProfitWise;
GO

DECLARE @PwShopId int = 100083;
DECLARE @ShopifyOrderId bigint = 448562004068;


SELECT * FROM ordertable(@PwShopId) WHERE ShopifyOrderId = @ShopifyOrderId;
SELECT * FROM orderlineitem(@PwShopId) WHERE ShopifyOrderId = @ShopifyOrderId;
SELECT * FROM profitreportentry(@PwShopId) WHERE ShopifyOrderId = @ShopifyOrderId;


DELETE FROM profitreportentry(@PwShopId) WHERE ShopifyOrderId = @ShopifyOrderId;
DELETE FROM orderlineitem(@PwShopId) WHERE ShopifyOrderId = @ShopifyOrderId;
DELETE FROM ordertable(@PwShopId) WHERE ShopifyOrderId = @ShopifyOrderId;


SELECT * FROM ordertable(@PwShopId) WHERE ShopifyOrderId = @ShopifyOrderId;
SELECT * FROM orderlineitem(@PwShopId) WHERE ShopifyOrderId = @ShopifyOrderId;
SELECT * FROM profitreportentry(@PwShopId) WHERE ShopifyOrderId = @ShopifyOrderId;

