
USE ProfitWise
GO



DROP VIEW IF EXISTS [dbo].[vw_masterproductandvariantsearch]
GO

DROP VIEW IF EXISTS [dbo].[vw_standaloneproductandvariantsearch]
GO

DROP TABLE IF EXISTS [dbo].[profitwisevariant]
GO

DROP TABLE IF EXISTS [dbo].[profitwisemastervariantcogsdetail]
GO

DROP TABLE IF EXISTS [dbo].[profitwisemastervariantcogscalc]
GO

DROP TABLE IF EXISTS [dbo].[profitwisemastervariant]
GO

DROP TABLE IF EXISTS [dbo].[profitwiseproduct]
GO

DROP TABLE IF EXISTS [dbo].[profitwisemasterproduct]
GO

DROP TABLE IF EXISTS [dbo].[profitwisepicklistmasterproduct]
GO

DROP TABLE IF EXISTS [dbo].[profitwisepicklist]
GO


DROP TABLE IF EXISTS [dbo].[shopifyorderrefund]
GO

DROP TABLE IF EXISTS [dbo].[shopifyorderlineitem]
GO

DROP TABLE IF EXISTS [dbo].[shopifyorderadjustment]
GO

DROP TABLE IF EXISTS [dbo].[shopifyorder]
GO




IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisevariant]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwisevariant](
	[PwVariantId] [bigint] IDENTITY(873,1) NOT NULL,
	[PwShopId] [bigint] NOT NULL,
	[PwProductId] [bigint] NOT NULL,
	[PwMasterVariantId] [bigint] NOT NULL,
	[ShopifyProductId] [bigint] NULL,
	[ShopifyVariantId] [bigint] NULL,
	[Sku] [nvarchar](100) NULL,
	[Title] [nvarchar](200) NULL,
	[LowPrice] [decimal](15, 2) NOT NULL,
	[HighPrice] [decimal](15, 2) NOT NULL,
	[Inventory] [int] NULL,
	[IsActive] [smallint] NOT NULL,
	[IsPrimary] [smallint] NOT NULL,
	[IsPrimaryManual] [tinyint] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_profitwisevariant_PwVariantId] PRIMARY KEY CLUSTERED 
(
	[PwVariantId] ASC,
	[PwShopId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO

ALTER TABLE [profitwisevariant] 
	ALTER COLUMN [Sku] [nvarchar](100)
	COLLATE SQL_Latin1_General_CP1_CS_AS

-- This logic alone is worthy of discussion...
CREATE UNIQUE INDEX uq_profitwisevariant
  ON dbo.[profitwisevariant]([PwShopId], [PwProductId], [ShopifyVariantId], [Sku], [Title])
  WHERE [ShopifyVariantId] IS NOT NULL;




IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisemastervariant]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwisemastervariant](
	[PwMasterVariantId] [bigint] IDENTITY(872,1) NOT NULL,
	[PwShopId] [bigint] NOT NULL,
	[PwMasterProductId] [bigint] NOT NULL,
	[Exclude] [smallint] NOT NULL,
	[StockedDirectly] [smallint] NOT NULL,
	[CogsTypeId] [smallint] NULL,
	[CogsCurrencyId] [int] NULL,
	[CogsAmount] [decimal](15, 2) NULL,
	[CogsMarginPercent] [decimal](4, 2) NULL,
	[CogsDetail] [smallint] NOT NULL,
 CONSTRAINT [PK_profitwisemastervariant_PwMasterVariantId] PRIMARY KEY CLUSTERED 
(
	[PwMasterVariantId] ASC,
	[PwShopId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisemastervariantcogsdetail]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwisemastervariantcogsdetail](
	[PwMasterVariantId] [bigint] NOT NULL,
	[PwShopId] [bigint] NOT NULL,
	[CogsDate] [date] NOT NULL,
	[CogsTypeId] [smallint] NOT NULL,
	[CogsCurrencyId] [int] NULL,
	[CogsAmount] [decimal](15, 2) NULL,
	[CogsMarginPercent] [decimal](4, 2) NULL,
 CONSTRAINT [PK_profitwisemastervariantcogsdetail_PwMasterVariantId] PRIMARY KEY CLUSTERED 
(
	[PwMasterVariantId] ASC,
	[PwShopId] ASC,
	[CogsDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO





IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisemastervariantcogscalc]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwisemastervariantcogscalc] (
	[PwMasterVariantId] [bigint] NOT NULL,
	[PwShopId] [bigint] NOT NULL,
	[StartDate] [date] NOT NULL,
	[EndDate] [date] NOT NULL,
	[PercentMultiplier] [decimal](15, 2) NOT NULL,
	[SourceCurrencyId] [int] NOT NULL,
	[FixedAmount] [decimal](15, 2) NOT NULL
CONSTRAINT [PK_profitwisemastervariantcogscalc_PwVariantId] PRIMARY KEY CLUSTERED 
(
	[PwMasterVariantId] ASC,
	[PwShopId] ASC,
	[StartDate] ASC,
	[EndDate] ASC	
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO



IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwiseproduct]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwiseproduct](
	[PwProductId] [bigint] IDENTITY(203,1) NOT NULL,
	[PwShopId] [bigint] NOT NULL,
	[PwMasterProductId] [bigint] NOT NULL,
	[ShopifyProductId] [bigint] NULL,
	[Title] [nvarchar](200) NULL,
	[Vendor] [nvarchar](100) NULL,
	[ProductType] [nvarchar](100) NULL,
	[Tags] [nvarchar](max) NULL,
	[IsActive] [tinyint] NOT NULL,
	[IsPrimary] [tinyint] NOT NULL,
	[IsPrimaryManual] [tinyint] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_profitwiseproduct_PwProductId] PRIMARY KEY CLUSTERED 
(
	[PwProductId] ASC,
	[PwShopId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO

ALTER TABLE [profitwiseproduct] 
	ALTER COLUMN [Title] [nvarchar](200)
	COLLATE SQL_Latin1_General_CP1_CS_AS

ALTER TABLE [profitwiseproduct] 
	ALTER COLUMN [Vendor] [nvarchar](100)
	COLLATE SQL_Latin1_General_CP1_CS_AS

ALTER TABLE [profitwiseproduct] 
	ALTER COLUMN [ProductType] [nvarchar](100)
	COLLATE SQL_Latin1_General_CP1_CS_AS



CREATE UNIQUE INDEX uq_profitwiseproduct
	ON dbo.[profitwiseproduct]([PwShopId], [ShopifyProductId], [Title], [Vendor])
	WHERE [ShopifyProductId] IS NOT NULL;


IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisemasterproduct]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwisemasterproduct](
	[PwMasterProductId] [bigint] IDENTITY(201,1) NOT NULL,
	[PwShopId] [bigint] NOT NULL,
 CONSTRAINT [PK_profitwisemasterproduct_PwMasterProductId] PRIMARY KEY CLUSTERED 
(
	[PwMasterProductId] ASC,
	[PwShopId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisepicklistmasterproduct]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwisepicklistmasterproduct](
	[PwPickListId] [bigint] NOT NULL,
	[PwShopId] [bigint] NOT NULL,
	[PwMasterProductId] [bigint] NOT NULL,
 CONSTRAINT [PK_profitwisepicklistmasterproduct_PwPickListId] PRIMARY KEY CLUSTERED 
(
	[PwPickListId] ASC,
	[PwShopId] ASC,
	[PwMasterProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO


IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisepicklist]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwisepicklist](
	[PwPickListId] [bigint] IDENTITY(100001,1) NOT NULL,
	[PwShopId] [bigint] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[LastAccessed] [datetime] NOT NULL,
 CONSTRAINT [PK_profitwisepicklist_PwPickListId] PRIMARY KEY CLUSTERED 
(
	[PwPickListId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO




IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[shopifyorder]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[shopifyorder](
	[PwShopId] [bigint] NOT NULL,
	[ShopifyOrderId] [bigint] NOT NULL,
	[Email] [nvarchar](128) NULL,
	[OrderNumber] [nvarchar](128) NULL,
	[OrderDate] [date] NOT NULL,
	[OrderLevelDiscount] [decimal](15, 2) NULL,
	[FinancialStatus] [smallint] NOT NULL,
	[Tags] [nvarchar](500) NULL,
	[CreatedAt] [datetime] NOT NULL,
	[UpdatedAt] [datetime] NOT NULL,
	[Cancelled] [smallint] NOT NULL,
 CONSTRAINT [PK_shopifyorder_PwShopId] PRIMARY KEY CLUSTERED 
(
	[PwShopId] ASC,
	[ShopifyOrderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO


IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[shopifyorderadjustment]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[shopifyorderadjustment](
	[PwShopId] [bigint] NOT NULL,
	[ShopifyAdjustmentId] [bigint] NOT NULL,
	[AdjustmentDate] [date] NOT NULL,
	[ShopifyOrderId] [bigint] NOT NULL,
	[Amount] [decimal](15, 2) NULL,
	[TaxAmount] [decimal](15, 2) NULL,
	[Kind] [nvarchar](100) NULL,
	[Reason] [nvarchar](100) NULL,
 CONSTRAINT [PK_shopifyorderadjustment_PwShopId] PRIMARY KEY CLUSTERED 
(
	[PwShopId] ASC,
	[ShopifyAdjustmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO


IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[shopifyorderlineitem]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[shopifyorderlineitem](
	[PwShopId] [bigint] NOT NULL,
	[ShopifyOrderId] [bigint] NOT NULL,
	[ShopifyOrderLineId] [bigint] NOT NULL,
	[OrderDateTimestamp] [datetime] NULL,
	[OrderDate] [date] NOT NULL,
	[FinancialStatus] [smallint] NOT NULL,
	[PwProductId] [bigint] NOT NULL,
	[PwVariantId] [bigint] NOT NULL,
	[Quantity] [bigint] NOT NULL,
	[UnitPrice] [decimal](15, 2) NULL,
	[TotalDiscount] [decimal](15, 2) NULL,
	[TotalAfterAllDiscounts] [decimal](15, 2) NULL,
	[NetQuantity] [int] NOT NULL,
	[UnitCogs] [decimal](15, 2) NOT NULL,
 CONSTRAINT [PK_shopifyorderlineitem_PwShopId] PRIMARY KEY CLUSTERED 
(
	[PwShopId] ASC,
	[ShopifyOrderId] ASC,
	[ShopifyOrderLineId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[shopifyorderrefund]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[shopifyorderrefund](
	[PwShopId] [bigint] NOT NULL,
	[ShopifyRefundId] [bigint] NOT NULL,
	[ShopifyOrderId] [bigint] NOT NULL,
	[ShopifyOrderLineId] [bigint] NOT NULL,
	[RefundDate] [date] NOT NULL,
	[PwProductId] [bigint] NOT NULL,
	[PwVariantId] [bigint] NOT NULL,
	[Amount] [decimal](15, 2) NULL,
	[RestockQuantity] [bigint] NOT NULL,
 CONSTRAINT [PK_shopifyorderrefund_PwShopId] PRIMARY KEY CLUSTERED 
(
	[PwShopId] ASC,
	[ShopifyRefundId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO






-- TODO - this badly needs multi-tenant filtering (!!!)
CREATE VIEW [dbo].[vw_masterproductandvariantsearch] (
	[PwShopId], [PwMasterProductId], [ProductTitle], [Vendor], [ProductType], [PwMasterVariantId], [VariantTitle], [Sku])
AS 
   SELECT 
      t1.PwShopId AS PwShopId, 
      t1.PwMasterProductId AS PwMasterProductId, 
      t1.Title AS ProductTitle, 
      t1.Vendor AS Vendor, 
      t1.ProductType AS ProductType, 
      t3.PwMasterVariantId AS PwMasterVariantId, 
      t3.Title AS VariantTitle, 
      t3.Sku AS Sku
   FROM profitwiseproduct  AS t1 
      INNER JOIN profitwisemastervariant  AS t2 
		ON t1.PwShopId = t2.PwShopId AND t1.PwMasterProductId = t2.PwMasterProductId
      INNER JOIN profitwisevariant  AS t3 
		ON t2.PwShopId = t3.PwShopId AND t2.PwMasterVariantId = t3.PwMasterVariantId
   WHERE t1.IsPrimary = 1 AND t3.IsPrimary = 1
GO

-- TODO - this badly needs multi-tenant filtering (!!!)
CREATE VIEW [dbo].[vw_standaloneproductandvariantsearch] (
	[PwShopId], [PwProductId], [ProductTitle], [Vendor], [ProductType], [PwVariantId], [VariantTitle], [Sku],
	[IsProductActive], [IsVariantActive], [Inventory], [PwMasterVariantId], [StockedDirectly])
AS 
   SELECT 
      t1.PwShopId AS PwShopId, 
      t1.PwProductId,
	  t1.Title AS ProductTitle, 
      t1.Vendor AS Vendor, 
      t1.ProductType AS ProductType, 
      t3.PwVariantId,
	  t3.Title AS VariantTitle, 
      t3.Sku AS Sku,
	  t1.IsActive AS IsProductActive,
	  t3.IsActive AS IsVariantActive,
	  t3.Inventory,
	  t3.[PwMasterVariantId],
	  t4.StockedDirectly
   FROM profitwiseproduct AS t1 
		INNER JOIN profitwisevariant AS t3
			ON t1.PwShopId = t3.PwShopId AND t1.PwProductId = t3.PwProductId
		INNER JOIN profitwisemastervariant AS t4
			ON t3.[PwMasterVariantId] = t4.[PwMasterVariantId]
GO


CREATE VIEW [dbo].[vw_goodsonhand]
AS 
	SELECT * FROM [vw_standaloneproductandvariantsearch]
	WHERE StockedDirectly = 1
	AND Inventory IS NOT NULL
	AND IsProductActive = 1
	AND IsVariantActive = 1
GO


