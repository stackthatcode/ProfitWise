USE ProfitWise
GO


use master
ALTER DATABASE ProfitWise SET SINGLE_USER WITH ROLLBACK IMMEDIATE 

--do you stuff here 

ALTER DATABASE ProfitWise SET MULTI_USER


SELECT * FROM profitwiseshop;

SELECT * FROM profitwiseproduct;
SELECT * FROM profitwiseproduct
WHERE PwShopId = 100001 AND PwMasterProductId = 220;

SELECT * FROM profitwisemastervariant;

SELECT * FROM profitwisevariant;

SELECT * FROM profitwisemastervariantcogsdetail;

SELECT t1.* 
FROM profitwisemastervariantcogsdetail t1
	INNER JOIN profitwisemastervariant t2 
		ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId
USE ProfitWise
GO


SELECT * FROM profitwiseproduct WHERE PwMasterProductId IN ( 222, 223 );

SELECT * FROM profitwisemastervariant WHERE PwMasterProductId = 222;

SELECT * FROM profitwisevariant WHERE PwMasterVariantId IN 
( SELECT PwMasterVariantId FROM profitwisemastervariant WHERE PwMasterProductId = 222 );


SELECT * FROM profitwisevariant WHERE SKU Like '3DUPLA285%';

SELECT * FROM profitwisereportquerystub;

SELECT * FROM profitwiseprofitreportentry;

SELECT * FROM profitwisemastervariant;

SELECT * FROM profitwisevariant;



-- Fixed Amount
SELECT t1.PwMasterVariantId, t2.Inventory, t1.CogsAmount
FROM profitwisemastervariant t1
	INNER JOIN profitwisevariant t2 ON t1.PwMasterVariantId = t2.PwMasterVariantId
WHERE t1.StockedDirectly = 1
-- AND t2.IsActive = 1 
AND t2.Inventory IS NOT NULL
AND t1.CogsTypeId = 1
AND t1.CogsDetail = 0;




-- UPDATE Percent profitwisevariant by PwMasterVariantId
UPDATE profitwisevariant
SET CurrentCogs = HighPrice * @UnitPricePercent
WHERE PwMasterVariantId = 872

-- UPDATE Fixed Price profitwisevariant by PwMasterVariantId
UPDATE profitwisevariant
SET CurrentCogs = @FixedAmount
WHERE PwMasterVariantId = 872

-- UPDATE Percent profitwisevariant by Pick List
UPDATE profitwisevariant
SET CurrentCogs = 1234
WHERE PwMasterVariantId IN ( SELECT PwMasterVariantId FROM profitwiserepor )




SELECT * FROM profitwisevariantunitcost;

INSERT INTO profitwisevariantunitcost
SELECT PwVariantId, PwShopId, @StartDate, @EndDate, HighPrice * @Margin
FROM profitwisevariant
WHERE IsActive = 1 
AND Inventory IS NOT NULL
AND PwShopId = @PwShopId
AND PwMasterVariantId = @PwMasterVariantId


DELETE FROM profitwisevariantunitcost
WHERE PwVariantId IN (
    SELECT t3.PwVariantId
	FROM profitwisemastervariant t2
		INNER JOIN profitwisevariant t3 ON t2.PwMasterVariantId = t3.PwMasterVariantId
	WHERE t2.PwShopId = @PwShopId 
	AND t3.PwShopId = @PwShopId
    AND t2.PwMasterVariantId = @PwMasterVariantId
) 
AND PwShopId = @PwShopId               


DELETE FROM profitwisevariantunitcost
WHERE PwVariantId IN
(
	SELECT t3.PwVariantId
	FROM profitwisepicklistmasterproduct t1 
		INNER JOIN profitwisemastervariant t2 ON t1.PwMasterProductId = t2.PwMasterProductId
		INNER JOIN profitwisevariant t3 ON t2.PwMasterVariantId = t3.PwMasterVariantId
	WHERE t1.PwShopId = 100001 
	AND t2.PwShopId = 100001
	AND t3.PwShopId = 100001
	AND t1.PwPickListId = 100233
)
AND PwShopId  = 100001;


SELECT * FROM profitwisepicklist;


