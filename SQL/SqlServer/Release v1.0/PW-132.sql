USE ProfitWise;
GO


ALTER TABLE shopifyorder
ALTER COLUMN [LastActivityDate] [date] NOT NULL;


ALTER TABLE profitwisereport
ALTER COLUMN [LastActivityDate] [date] NOT NULL;


ALTER TABLE shopifyorder
ALTER COLUMN [LastActivityDate] [date] NOT NULL;


UPDATE profitwiseshop SET TimeZone = 'America/Chicago';

