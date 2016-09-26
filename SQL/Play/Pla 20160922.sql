
SET SQL_SAFE_UPDATES = 0;


SELECT * FROM profitwiseshop;




/** STEP #1 - CREATE NEW QUERY **/

INSERT INTO profitwisequery (PwShopId) VALUES ( 100001 );

SELECT LAST_INSERT_ID();


/*** TEMPORARY ***/
DELETE FROM profitwisequerymasterproduct;

DELETE FROM profitwiseproduct WHERE Vendor = 'Zortrax';


UPDATE profitwisemastervariant 
SET CogsAmount = 20
WHERE PwMasterProductId IN ( 6, 8, 9 );


SELECT * FROM profitwisequerymasterproduct;


SELECT COUNT(t2.PwMasterProductId)
FROM profitwisevariant t1 
	INNER JOIN profitwisemastervariant t2 ON t1.PwMasterVariantId = t2.PwMasterVariantId
	INNER JOIN profitwiseproduct t3 ON t2.PwMasterProductId = t3.PwMasterProductId
        
WHERE (t1.PwShopId = 100001 AND t2.PwShopId = 100001 AND t3.PwShopId = 100001)
AND ( (t1.Title LIKE '%RED%') OR (t1.Sku LIKE '%RED%') OR ( t3.Title LIKE '%RED%' ) OR ( t3.Vendor LIKE '%RED%' ) )
AND ( (t1.Title LIKE '%Universe%') OR (t1.Sku LIKE '%Universe%') OR ( t3.Title LIKE '%Universe%' ) OR ( t3.Vendor LIKE '%Universe%' ) ); 




/** STEP #2 - INSERT MPId's from Variant Search **/

INSERT INTO profitwisequerymasterproduct (PwQueryId, PwShopId, PwMasterProductId)
SELECT DISTINCT 900000, 100001, t2.PwMasterProductId
FROM profitwisevariant t1 
	INNER JOIN profitwisemastervariant t2 ON t1.PwMasterVariantId = t2.PwMasterVariantId
	INNER JOIN profitwiseproduct t3 ON t2.PwMasterProductId = t3.PwMasterProductId
        
WHERE (t1.PwShopId = 100001 AND t2.PwShopId = 100001 AND t3.PwShopId = 100001)
AND ( (t1.Title LIKE '%RED%') OR (t1.Sku LIKE '%RED%') OR ( t3.Title LIKE '%RED%' ) OR ( t3.Vendor LIKE '%RED%' ) )
AND ( (t1.Title LIKE '%Universe%') OR (t1.Sku LIKE '%Universe%') OR ( t3.Title LIKE '%Universe%' ) OR ( t3.Vendor LIKE '%Universe%' ) ); 



/*** TEMPORARY - what did we just select ??? ***/
SELECT * FROM profitwiseproduct WHERE PwMasterProductId IN ( SELECT PwMasterProductId FROM profitwisequerymasterproduct );


/** STEP #3 - Apply the non-CoGS Filter **/
DELETE FROM profitwisequerymasterproduct
WHERE PwShopId = 100001
AND PwQueryId = 900000
AND PwMasterProductId NOT IN (
	SELECT PwMasterProductId
    FROM profitwiseproduct
    WHERE PwShopId = 100001
		AND Vendor = 'Ultimaker' 
        
		AND ProductType = 'Filament'
        
		AND Tags LIKE '%2.85%' 
		AND Tags LIKE '%PLA%' 
);


/** STEP #4 - Apply the CoGS Filter **/
DELETE FROM profitwisequerymasterproduct
WHERE PwShopId = 100001
AND PwQueryId = 900000
AND PwMasterProductId NOT IN (
	SELECT DISTINCT(PwMasterProductId)
	FROM profitwisemastervariant 
	WHERE PwShopId = 100001
	AND CogsAmount IS NULL
);


/*** TEMPORARY - what did we just select ??? ***/
SELECT * FROM profitwiseproduct WHERE PwMasterProductId IN ( SELECT PwMasterProductId FROM profitwisequerymasterproduct );



USE ProfitWise;

SELECT PwMasterProductId FROM profitwisequerymasterproduct WHERE PwShopId = 100001 AND PwQueryId = 900004;



/** STEP #4 - Get Paginated sequence of Master Products **/

SELECT 	t1.PwMasterProductId, t1.PwProductId, t1.Title, t1.Vendor
FROM profitwiseproduct t1
WHERE t1.PwShopId = 100001 AND t1.IsPrimary = true
AND t1.PwMasterProductId IN ( 
	SELECT PwMasterProductId FROM profitwisequerymasterproduct WHERE PwShopId = 100001 AND PwQueryId = 900000 )
ORDER BY t1.Title ASC
LIMIT 0, 200;


/** STEP #5 - Get  **/

SELECT 	t2.PwMasterVariantId, t2.Exclude, t2.StockedDirectly, t2.CogsCurrencyId, t2.CogsAmount, t2.CogsDetail,
        t3.PwVariantId, t3.LowPrice, t3.HighPrice, t3.Inventory
FROM profitwisemastervariant t2 
	INNER JOIN profitwisevariant t3 ON t2.PwMasterVariantId = t3.PwMasterVariantId
WHERE t2.PwShopId = 100001
AND t3.PwShopId = 100001 AND t3.IsPrimary = true
AND t2.PwMasterProductId IN ( 
	SELECT PwMasterProductId FROM profitwisequerymasterproduct WHERE PwShopId = 100001 AND PwQueryId = 900004 )




SELECT PwMasterProductId FROM profitwisequerymasterproduct WHERE PwShopId = 100001 AND PwQueryId = 900000;

SELECT * FROM profitwiseproduct;

PwMasterVariantId, PwShopId, PwMasterProductId, Exclude, StockedDirectly, CogsCurrencyId, CogsAmount, CogsDetail



