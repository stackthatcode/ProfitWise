
SET SQL_SAFE_UPDATES = 0;


SELECT * FROM shop;

/****/

SELECT * FROM shopifyproduct;

SELECT *, PwProductId FROM shopifyvariant WHERE ShopId = 955973;

SELECT *, PwProductId FROM shopifyorderlineitem WHERE ShopId = 955973 AND ReportedSku LIKE '%SIMPLE%';

/****/


SELECT ShopId, PwProductId, ProductTitle, VariantTitle, `Name` FROM profitwiseproduct;






++ Some entrie for SKU's from Order Line Items



/*SELECT ShopId, PwProductId, */




SELECT COUNT(*) FROM shopifyorderlineitem;

SELECT * FROM shopifyorderlineitem WHERE ShopId = 955973 AND ShopifyProductId IS NULL;






CREATE TEMPORARY TABLE IF NOT EXISTS ttanalysis AS (SELECT DISTINCT ShopifyVariantId, ReportedSku FROM shopifyorderlineitem WHERE ShopId = 955973);

SELECT * FROM ttanalysis;

SELECT ReportedSku, COUNT(*) FROM ttanalysis GROUP BY ReportedSku ORDER BY COUNT(*);

SELECT ShopifyVariantId FROM shopifyorderlineitem WHERE ShopId = 955973 AND ReportedSku = 'UM2EXTPLUS' GROUP BY ShopifyVariantId;

SELECT * FROM shopifyorderlineitem WHERE ShopId = 955973 AND ReportedSku = 'UM2EXTPLUS' AND ShopifyVariantId IS NULL;





OPTIMIZE TABLE shopifyorderlineitem;




/* This will create the Shops  */
INSERT INTO Shop
SELECT t1.ShopifyOrderId, 'XYZXFDFKL'
FROM shopifyorder t1
WHERE t1.ShopifyOrderId > ( SELECT MAX(ShopId) FROM Shop )
ORDER BY t1.ShopifyOrderId LIMIT 100;

INSERT INTO shopifyorderlineitem ( ShopId, ShopifyOrderLineId, ShopifyOrderId, ShopifyProductId, ShopifyVariantId, ReportedSku, Quantity, UnitPrice, TotalDiscount )
SELECT t1.ShopId, t2.ShopifyOrderLineId, t2.ShopifyOrderId, t2.ShopifyProductId, t2.ShopifyVariantId, t2.ReportedSku, t2.Quantity, t2.UnitPrice, t2.TotalDiscount
FROM Shop t1 INNER JOIN shopifyorderlineitem t2 
WHERE t1.ShopId NOT IN ( SELECT DISTINCT ShopId FROM shopifyorderlineitem )
AND t2.ShopId = 955973;




