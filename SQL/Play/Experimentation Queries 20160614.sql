USE ProfitWise;


SET SQL_SAFE_UPDATES = 0;


DELETE FROM aspnetusers;
DELETE FROM aspnetroles;
DELETE FROM aspnetuserclaims;
DELETE FROM aspnetuserlogins;
DELETE FROM aspnetuserroles;



SELECT * FROM aspnetusers;

SELECT * FROM aspnetroles;

SELECT * FROM aspnetuserclaims;

SELECT * FROM aspnetuserlogins;

SELECT * FROM aspnetuserroles;



SELECT * FROM shop;

SELECT * FROM shopifyproduct;

SELECT * FROM shopifyvariant;

SELECT COUNT(*) FROM shopifyorderlineitem;

SELECT ShopifyOrderId, COUNT(*) FROM shopifyorderlineitem GROUP BY ShopifyOrderId;





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




