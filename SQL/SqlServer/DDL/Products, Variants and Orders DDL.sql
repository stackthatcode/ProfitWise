
USE ProfitWise
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


DROP INDEX IF EXISTS [profitwisevariant].uq_profitwisevariant;

ALTER TABLE [profitwisevariant] 
	ALTER COLUMN [Sku] [nvarchar](100)

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
	[CogsTypeId] [smallint] NOT NULL,
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

DROP INDEX IF EXISTS [profitwiseproduct].uq_profitwiseproduct

ALTER TABLE [profitwiseproduct] 
	ALTER COLUMN [Title] [nvarchar](200)

ALTER TABLE [profitwiseproduct] 
	ALTER COLUMN [Vendor] [nvarchar](100)

ALTER TABLE [profitwiseproduct] 
	ALTER COLUMN [ProductType] [nvarchar](100)
	
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





