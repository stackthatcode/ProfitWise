USE ProfitWise;


SET SQL_SAFE_UPDATES = 0;


SELECT * FROM shop;

/****/

SELECT * FROM shopifyproduct;

SELECT * FROM shopifyvariant WHERE ShopId = 955973;

SELECT * FROM shopifyorderlineitem WHERE ShopId = 955973;

/****/


SELECT ShopId, PwProductId /*, ProductTitle, VariantTitle, `Name` */ FROM profitwiseproduct;








/*SELECT ShopId, PwProductId, */


SELECT COUNT(*) FROM shopifyorderlineitem;

SELECT * FROM shopifyorderlineitem WHERE ShopId = 955973 AND ShopifyProductId IS NULL;

OPTIMIZE TABLE shopifyorderlineitem;



/* This will create the Shops  
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
*/



