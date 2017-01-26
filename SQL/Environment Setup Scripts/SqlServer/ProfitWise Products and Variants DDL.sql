
USE ProfitWise
GO


DROP VIEW IF EXISTS [dbo].[vw_masterproductandvariantsearch]
GO


DROP TABLE IF EXISTS [dbo].[profitwisevariant]
GO

DROP TABLE IF EXISTS [dbo].[profitwisemastervariantcogsdetail]
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
 CONSTRAINT [PK_profitwisepicklist_PwPickListId] PRIMARY KEY CLUSTERED 
(
	[PwPickListId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO


CREATE VIEW [dbo].[vw_masterproductandvariantsearch] (
   [PwShopId], 
   [PwMasterProductId], 
   [ProductTitle], 
   [Vendor], 
   [ProductType], 
   [PwMasterVariantId], 
   [VariantTitle], 
   [Sku])
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
   FROM ((profitwiseproduct  AS t1 
      INNER JOIN profitwisemastervariant  AS t2 
      ON ((t1.PwMasterProductId = t2.PwMasterProductId))) 
      INNER JOIN profitwisevariant  AS t3 
      ON ((t2.PwMasterVariantId = t3.PwMasterVariantId)))
   WHERE ((t1.IsPrimary = 1) AND (t3.IsPrimary = 1))
GO


