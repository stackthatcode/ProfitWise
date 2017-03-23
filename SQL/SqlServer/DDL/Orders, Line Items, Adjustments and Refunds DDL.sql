USE ProfitWise
GO

DROP TABLE IF EXISTS [dbo].[shopifyorderrefund]
GO

DROP TABLE IF EXISTS [dbo].[shopifyorderlineitem]
GO

DROP TABLE IF EXISTS [dbo].[shopifyorderadjustment]
GO

DROP TABLE IF EXISTS [dbo].[shopifyorder]
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
	[BalancingCorrection] [decimal](15, 2) NOT NULL,
	[LastActivityDate] [datetime] NOT NULL,
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

