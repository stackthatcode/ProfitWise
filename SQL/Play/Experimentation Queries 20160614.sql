USE ProfitWise;


SET SQL_SAFE_UPDATES = 0;


SELECT * FROM aspnetusers;

SELECT * FROM aspnetuserclaims;

SELECT * FROM profitwisedatelookup;


SELECT * FROM shop;

SELECT * FROM shopifyproduct;

SELECT * FROM shopifyvariant;

SELECT * FROM profitwisebatchstate;	/** What we currently have for each Shop **/

SELECT * FROM profitwisepreferences; /** What each Shop wants **/


SELECT * FROM shopifyorderlineitem;

SELECT * FROM shopifyorderlineitem WHERE ShopifyVariantId IS NULL;

SELECT * FROM profitwiseproduct WHERE PwProductId NOT IN ( SELECT PwProductId FROM shopifyVariant ) ORDER BY Name;

SELECT * FROM profitwiseproduct;

SELECT * FROM profitwise




/** Do something with this...!
OPTIMIZE TABLE shopifyorderlineitem;
**/


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

/** Does preference overlap Batch State...? **/

/* Preferences for 
SELECT * FROM profitwisepreferences;

UPDATE profitwisepreferences SET StartingDateForOrders = '2014-05-01';

SELECT * FROM profitwisebatchstate;

UPDATE profitwisebatchstate SET OrderDatasetStart = NULL, OrderDatasetEnd = NULL;
*/




