USE ProfitWise




IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisetour]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].profitwisetour(
	[PwShopId] [bigint] NOT NULL,
	[ShowPreferences] [bit] NOT NULL,
	[ShowProducts] [bit] NOT NULL,
	[ShowProductDetails] [bit] NOT NULL,
	[ShowProductConsolidationOne] [bit] NOT NULL,
	[ShowProductConsolidationTwo] [bit] NOT NULL,
	[ShowProfitabilityDashboard] [bit] NOT NULL,
	[ShowEditFilters] [bit] NOT NULL,
	[ShowProfitabilityDetail] [bit] NOT NULL,
	[ShowGoodsOnHand] [bit] NOT NULL,

CONSTRAINT [PK_profitwisetour_PwShopId] PRIMARY KEY CLUSTERED 
(
	[PwShopId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO



DROP FUNCTION IF EXISTS dbo.tour
GO
CREATE FUNCTION dbo.tour(@PwShopId bigint)  
RETURNS TABLE  
AS  
RETURN SELECT * FROM profitwisetour WHERE PwShopId = @PwShopId;
GO



INSERT INTO profitwisetour SELECT PwShopId, 1, 1, 1, 1, 1, 1, 1, 1, 1 FROM profitwiseshop;

SELECT * FROM profitwisetour;



