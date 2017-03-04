
USE [ProfitWise]
GO


DROP TABLE IF EXISTS [dbo].[AspNetUserRoles]
GO

DROP TABLE IF EXISTS [dbo].[AspNetUserLogins]
GO

DROP TABLE IF EXISTS [dbo].[AspNetUserClaims]
GO

DROP TABLE IF EXISTS [dbo].[AspNetRoles]
GO

DROP TABLE IF EXISTS [dbo].[AspNetUsers]
GO

DROP TABLE IF EXISTS [dbo].[profitwiseshop]
GO

DROP TABLE IF EXISTS [dbo].[profitwisebatchstate]
GO

DROP TABLE IF EXISTS [dbo].[profitwiserecurringcharge]
GO





SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetRoles]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[AspNetRoles](
	[Id] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetRoles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[AspNetRoles] WHERE [Id] IN ( 'ADMIN' ))
BEGIN
	INSERT INTO [dbo].[AspNetRoles]  VALUES ('2fe92131-c529-4819-8ecd-f6fbb3011ddb', 'ADMIN');
END

IF NOT EXISTS (SELECT * FROM [dbo].[AspNetRoles] WHERE [Id] IN ( 'USER' ))
BEGIN
	INSERT INTO [dbo].[AspNetRoles]  VALUES ('d884c7fd-86b8-4acf-8fdf-c0e2c7efd009', 'USER');
END




IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserClaims]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.AspNetUserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserLogins]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[AspNetUserLogins](
	[LoginProvider] [nvarchar](128) NOT NULL,
	[ProviderKey] [nvarchar](128) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetUserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserRoles]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[AspNetUserRoles](
	[UserId] [nvarchar](128) NOT NULL,
	[RoleId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetUserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO



IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[AspNetUsers](
	[Id] [nvarchar](128) NOT NULL,
	[Email] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEndDateUtc] [datetime] NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
	[UserName] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO



IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwiseshop]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwiseshop](
	[PwShopId] [bigint] IDENTITY(100001,1) NOT NULL,
	[ShopOwnerUserId] [nvarchar](128) NOT NULL,
	[ShopifyShopId] [bigint] NULL,
	[Domain] [nvarchar](100) NULL,
	[CurrencyId] [int] NULL,
	[TimeZone] [nvarchar](50) NULL,
	[IsAccessTokenValid] [smallint] NOT NULL,
	[IsShopEnabled] [smallint] NOT NULL,
	[IsBillingValid] [smallint] NOT NULL,
	[IsDataLoaded] [smallint] NOT NULL,
	[StartingDateForOrders] [datetime] NULL,
	[UseDefaultMargin] [smallint] NOT NULL,
	[DefaultMargin] [decimal](15, 2) NOT NULL,
	[ProfitRealization] [smallint] NOT NULL,
	[DateRangeDefault] [smallint] NOT NULL
 CONSTRAINT [PK_profitwiseshop_PwShopId] PRIMARY KEY CLUSTERED 
(
	[PwShopId] ASC,
	[ShopOwnerUserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwisebatchstate]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwisebatchstate](
	[PwShopId] [bigint] NOT NULL,
	[ProductsLastUpdated] [datetime] NULL,
	[OrderDatasetStart] [datetime] NULL,
	[OrderDatasetEnd] [datetime] NULL,
	[InitialRefreshJobId] [nvarchar](128),
	[RoutineRefreshJobId] [nvarchar](128),
 CONSTRAINT [PK_profitwisebatchstate_PwShopId] PRIMARY KEY CLUSTERED 
(
	[PwShopId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO


IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[profitwiserecurringcharge]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[profitwiserecurringcharge](
	[PwShopId] [bigint] NOT NULL,
	[PwChargeId] [bigint] NOT NULL,
	[ShopifyRecurringChargeId] [nvarchar](50) NOT NULL,

	[ConfirmationUrl] [nvarchar](1024) NULL,	
	[LastStatus] [int] NOT NULL,
	[IsPrimary] [bit] NOT NULL,

	[DateCreated] [datetime] NULL,
	[LastUpdated] [datetime] NULL,
	[LastJson] [nvarchar](2048) NOT NULL,

 CONSTRAINT [PK_profitwiserecurringcharge_PwShopId] PRIMARY KEY CLUSTERED 
(
	[PwShopId] ASC,
	[PwChargeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO

