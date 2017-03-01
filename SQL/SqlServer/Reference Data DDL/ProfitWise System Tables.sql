USE ProfitWise
GO


DROP TABLE IF EXISTS [dbo].[ints]
GO

DROP TABLE IF EXISTS [dbo].[calendar_table]
GO

DROP TABLE IF EXISTS [dbo].[exchangerate]
GO

DROP TABLE IF EXISTS [dbo].[currency]
GO




IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ints]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ints](
	[i] [smallint] NULL
) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[calendar_table]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[calendar_table](
	[dt] [date] NOT NULL,
	[y] [smallint] NULL,
	[q] [smallint] NULL,
	[m] [smallint] NULL,
	[d] [smallint] NULL,
	[dw] [smallint] NULL,
	[monthName] [nvarchar](9) NULL,
	[dayName] [nvarchar](9) NULL,
	[w] [smallint] NULL,
	[isWeekday] [smallint] NULL,
	[isHoliday] [smallint] NULL,
	[holidayDescr] [nvarchar](32) NULL,
	[isPayday] [smallint] NULL,
 CONSTRAINT [PK_calendar_table_dt] PRIMARY KEY CLUSTERED 
(
	[dt] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[currency]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[currency](
	[CurrencyId] [int] NOT NULL,
	[Abbreviation] [nvarchar](3) NOT NULL,
	[Symbol] [nvarchar](3) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_currency_CurrencyId] PRIMARY KEY CLUSTERED 
(
	[CurrencyId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[exchangerate]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[exchangerate](
	[SourceCurrencyId] [int] NOT NULL,
	[DestinationCurrencyId] [int] NOT NULL,
	[Date] [date] NOT NULL,
	[Rate] [decimal](9, 6) NOT NULL,
 CONSTRAINT [PK_exchangerate_SourceCurrencyId] PRIMARY KEY CLUSTERED 
(
	[SourceCurrencyId] ASC,
	[DestinationCurrencyId] ASC,
	[Date] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO


INSERT INTO currency VALUES ( 1, 'USD', '$', 'United States dollars' );
INSERT INTO currency VALUES ( 2, 'EUR', '€', 'Euros' );
INSERT INTO currency VALUES ( 3, 'JPY', '¥', 'Japanese yen' );
INSERT INTO currency VALUES ( 4, 'GBP', '£', 'Pounds sterling' );
INSERT INTO currency VALUES ( 5, 'AUD', '$', 'Australian dollars' );
INSERT INTO currency VALUES ( 6, 'CHF', 'Fr', 'Swiss francs' );
INSERT INTO currency VALUES ( 7, 'CAD', '$', 'Canadian dollars' );





-- Please enter the most recent [ExchangeRateLastDate] value into the INSERT statement below, before executing

SELECT * FROM [systemstate]

DROP TABLE IF EXISTS [dbo].[systemstate]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[systemstate]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[systemstate](
	[ExchangeRateLastDate] [date] NULL,
	[MaintenancePeriod] [bit] NOT NULL,
	[MaintenanceReason] varchar(200) NOT NULL,
) ON [PRIMARY]
END
GO

INSERT INTO systemstate VALUES ('2017-02-20', 0, '');





