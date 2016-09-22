
SET SQL_SAFE_UPDATES = 0;


SELECT * FROM profitwiseshop;




/** STEP #1 - CREATE NEW QUERY **/

INSERT INTO profitwisequery (PwShopId) VALUES ( 1 );

SELECT LAST_INSERT_ID();


/*** TEMPORARY ***/
DELETE FROM profitwisequerymasterproduct;

/** STEP #2 - INSERT MPId's from Variant Search **/

INSERT INTO profitwisequerymasterproduct (PwQueryId, PwShopId, PwMasterProductId)
SELECT DISTINCT 900000, 1, t2.PwMasterProductId
FROM profitwisevariant t1 
	INNER JOIN profitwisemastervariant t2 ON t1.PwMasterVariantId = t2.PwMasterVariantId
	INNER JOIN profitwiseproduct t3 ON t2.PwMasterProductId = t3.PwMasterProductId
        
WHERE (t1.PwShopId = 1 AND t2.PwShopId = 1 AND t3.PwShopId)
AND ( (t1.Title LIKE '%RED%') OR (t1.Sku LIKE '%RED%') OR ( t3.Title LIKE '%RED%' ) OR ( t3.Vendor LIKE '%RED%' ) )
AND ( (t1.Title LIKE '%Universe%') OR (t1.Sku LIKE '%Universe%') OR ( t3.Title LIKE '%Universe%' ) OR ( t3.Vendor LIKE '%Universe%' ) );


/*** TEMPORARY - what did we just select ??? ***/
SELECT * FROM profitwiseproduct WHERE PwMasterProductId IN ( SELECT PwMasterProductId FROM profitwisequerymasterproduct );


/** STEP #3 - Apply Filter **/
DELETE FROM profitwisequerymasterproduct
WHERE PwShopId = 1 AND PwQueryId = 900000
AND PwMasterProductId NOT IN (
	SELECT PwMasterProductId
    FROM profitwiseproduct
    WHERE PwShopId = 1
		AND PwMasterProductId IN 
			( SELECT PwMasterProductId FROM profitwisequerymasterproduct WHERE PwShopId = 1 AND PwQueryId = 900000 )
		AND Vendor = 'Ultimaker' 
		AND ProductType = 'Filament'
		AND Tags LIKE '%2.85%' 
		AND Tags LIKE '%PLA%' 
);


/** STEP #5 - Apply Filter **/
SELECT DISTINCT(PwMasterProductId)
FROM profitwisemastervariant 
WHERE PwShopId = 1
AND PwMasterProductId IN 
			( SELECT PwMasterProductId FROM profitwisequerymasterproduct WHERE PwShopId = 1 AND PwQueryId = 900000 )
AND CogsAmount IS NULL;

SELECT PwMasterProductId FROM profitwisequerymasterproduct WHERE PwShopId = 1 AND PwQueryId = 900000;


    

SELECT PwMasterProductId FROM profitwisequerymasterproduct WHERE PwShopId = 1 AND PwQueryId = 900000;

SELECT * FROM profitwisemasterproduct

SELECT * FROM profitwisevariant WHERE PwProductId = 8;


