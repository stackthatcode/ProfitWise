USE ProfitWise
GO



DROP TABLE IF EXISTS [dbo].[profitwiseprofitreportentry]
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwiseprofitreportentry]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwiseprofitreportentry](
	[PwShopId] [bigint] NOT NULL,
	[EntryDate] [date] NOT NULL,
	[EntryType] [smallint] NOT NULL,
	[ShopifyOrderId] [bigint] NOT NULL,
	[SourceId] [bigint] NOT NULL,
	[PwProductId] [bigint] NULL,
	[PwVariantId] [bigint] NULL,
	[NetSales] [decimal](15, 2) NULL,
	[CoGS] [decimal](15, 2) NULL,
	[Quantity] [int] NULL,
 CONSTRAINT [PK_profitwiseprofitreportentry_PwShopId] PRIMARY KEY CLUSTERED 
(
	[PwShopId] ASC,
	[EntryDate] ASC,
	[EntryType] ASC,
	[ShopifyOrderId] ASC,
	[SourceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO



DROP TABLE IF EXISTS [dbo].[profitwiseprofitquerystub]
GO

DROP TABLE IF EXISTS [dbo].[profitwisegoodsonhandquerystub]
GO

DROP TABLE IF EXISTS [dbo].[profitwisereportfilter]
GO

DROP TABLE IF EXISTS [dbo].[profitwisereport]
GO




IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwiseprofitquerystub]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwiseprofitquerystub](
	[PwReportId] [bigint] NOT NULL,
	[PwShopId] [bigint] NOT NULL,
	[PwMasterVariantId] [bigint] NOT NULL,
	[PwMasterProductId] [bigint] NOT NULL,
	[Vendor] [nvarchar](100) NULL,
	[ProductType] [nvarchar](100) NULL,
	[ProductTitle] [nvarchar](100) NULL,
	[Sku] [nvarchar](100) NULL,
	[VariantTitle] [nvarchar](100) NULL,
 CONSTRAINT [PK_profitwiseprofitquerystub_PwReportId] PRIMARY KEY CLUSTERED 
(
	[PwReportId] ASC,
	[PwShopId] ASC,
	[PwMasterVariantId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO




IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisegoodsonhandquerystub]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwisegoodsonhandquerystub](
	[PwReportId] [bigint] NOT NULL,
	[PwShopId] [bigint] NOT NULL,
	[PwVariantId] [bigint] NOT NULL,
	[PwProductId] [bigint] NOT NULL,
	[Vendor] [nvarchar](100) NULL,
	[ProductType] [nvarchar](100) NULL,
	[ProductTitle] [nvarchar](100) NULL,
	[Sku] [nvarchar](100) NULL,
	[VariantTitle] [nvarchar](100) NULL,
 CONSTRAINT [PK_profitwisegoodsonhandquerystub_PwReportId] PRIMARY KEY CLUSTERED 
(
	[PwReportId] ASC,
	[PwShopId] ASC,
	[PwVariantId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO





IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisereport]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwisereport](
	[PwReportId] [bigint] IDENTITY(99790,1) NOT NULL,
	[PwShopId] [bigint] NOT NULL,
	[ReportTypeId] [smallint] NOT NULL,	
	[Name] [nvarchar](50) NULL,
	[IsSystemReport] [smallint] NOT NULL,
	[CopyForEditing] [smallint] NOT NULL,
	[OriginalReportId] [bigint] NOT NULL,
	[StartDate] [date] NOT NULL,
	[EndDate] [date] NOT NULL,
	[GroupingId] [smallint] NULL,
	[OrderingId] [smallint] NULL,
	[CreatedDate] [datetime] NOT NULL,
	[LastAccessedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_profitwisereport_PwReportId] PRIMARY KEY CLUSTERED 
(
	[PwReportId] ASC,
	[PwShopId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO


IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisereportfilter]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwisereportfilter](
	[PwReportId] [bigint] NOT NULL,
	[PwShopId] [bigint] NOT NULL,
	[PwFilterId] [bigint] NOT NULL,
	[FilterType] [smallint] NOT NULL,
	[NumberKey] [bigint] NULL,
	[StringKey] [nvarchar](100) NULL,
	[Title] [nvarchar](100) NULL,
	[Description] [nvarchar](150) NULL,
	[DisplayOrder] [int] NOT NULL,
 CONSTRAINT [PK_profitwisereportfilter_PwReportId] PRIMARY KEY CLUSTERED 
(
	[PwReportId] ASC,
	[PwShopId] ASC,
	[PwFilterId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO

