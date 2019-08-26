USE ProfitWise;
GO

/*
SELECT * FROM profitreportentry(100001);

SELECT * FROM ordertable(100001);

SELECT TOP 100 * FROM shopifyorder;

SELECT * FROM ordertag(100001);
*/


-- ### Table definition ###

DROP TABLE IF EXISTS [dbo].[shopifyordertag]
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[shopifyordertag]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[shopifyordertag](
	[PwShopId] [bigint] NOT NULL,
	[ShopifyOrderId] [bigint] NOT NULL,
	[Tag] [nvarchar](40) NOT NULL,
 CONSTRAINT [PK_shopifyordertag_PwShopId] PRIMARY KEY CLUSTERED 
(
	[PwShopId] ASC,
	[ShopifyOrderId] ASC,
	[Tag] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO


-- ### Multitenant Functions ###

DROP FUNCTION IF EXISTS dbo.ordertag
GO
CREATE FUNCTION dbo.ordertag(@PwShopId bigint)  
RETURNS TABLE  
AS  
RETURN SELECT * FROM shopifyordertag WHERE PwShopId = @PwShopId;
GO

INSERT INTO shopifyordertag



WITH TagQuery (PwShopId, ShopifyOrderId, Tag) AS
(
SELECT PwShopId, ShopifyOrderId, TRIM(value)  
FROM shopifyorder  
    CROSS APPLY STRING_SPLIT(Tags, ',')  
WHERE Tags <> ''
)
SELECT PwShopId, ShopifyOrderId, COUNT(Tag) 
FROM TagQuery 
GROUP BY PwShopId, ShopifyOrderId;


SELECT * FROM shopifyorder WHERE ShopifyOrderId = 111534080024;

SELECT * FROM shopifyordertag WHERE ShopifyOrderId = 111534080024;


