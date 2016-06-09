
USE [ProfitWise]
GO

/****** Object:  Table [dbo].[OrderSkuHistory]    Script Date: 6/7/2016 12:53:47 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[OrderSkuHistory](
	[LineId] [int] IDENTITY(1,1) NOT NULL,
	[OrderNumber] [varchar](10) NULL,
	[ProductSku] [varchar](25) NULL,
	[Price] [money] NULL,
	[COGS] [money] NULL,
 CONSTRAINT [PK_OrderSkuHistory] PRIMARY KEY CLUSTERED 
(
	[LineId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO





DELETE FROM OrderSkuHistory

PRINT 'Starting... ' + CONVERT(varchar, SYSDATETIME(), 121)

EXEC Populate

PRINT 'Finishing... ' + CONVERT(varchar, SYSDATETIME(), 121)


SELECT COUNT(*) FROM OrderSkuHistory



SET QUOTED_IDENTIFIER ON
GO

USE ProfitWise
GO

DROP PROCEDURE [dbo].[Populate]
GO


CREATE PROCEDURE [dbo].[Populate]
AS 

BEGIN

	DECLARE @counter int = 1

	WHILE @counter < (10000)

	BEGIN 
		INSERT INTO OrderSkuHistory (OrderNumber, ProductSku, Price, COGS) 	
		VALUES ( 'ABCDEFDFK', 'TESTSKU', 20, 10 )
	
		SET @counter = @counter + 1
	END

END

GO


