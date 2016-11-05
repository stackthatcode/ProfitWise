

SELECT * FROM profitwiseshop;


INSERT INTO profitwisereport 
( PwShopId, Name, Saved, AllProductTypes, AllVendors, AllProducts, AllSkus, Grouping, CreatedDate, LastAccessedDate ) 
VALUES 
( 1, 'Test TESt', 0, 0, 0, 0, 0, 'TEST TEST', '2016-01-01', '2016-01-01' );


SELECT * FROM profitwisereport;

UPDATE profitwisereport SET Saved = 1;


SELECT * FROM aspnetusers;

SELECT * FROM aspnetroles;



SELECT COUNT(DISTINCT ProductType) FROM profitwiseproduct;


SELECT ProductType, COUNT(*) FROM profitwiseproduct
WHERE IsPrimary = 1 GROUP BY ProductType;


SELECT * FROM profitwisereportproducttype;

SELECT * FROM profitwisereportvendor;


SELECT t1.PwMasterProductId, t1.Title, COUNT(*) AS Count
FROM profitwiseproduct t1 
	INNER JOIN profitwisemastervariant t2
		ON t1.PwMasterProductId = t2.PwMasterProductId AND t1.IsPrimary = 1 
	INNER JOIN profitwisevariant t3
		ON t2.PwMasterVariantId = t3.PwMasterVariantId AND t3.IsPrimary = 1
GROUP BY t1.PwMasterProductId, t1.Title;



SELECT * FROM profitwiseproduct;

SELECT * FROM profitwisemastervariant;

SELECT * FROM profitwisevariant;

SELECT * FROM profitwisereportmasterproduct;



