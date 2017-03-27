USE ProfitWise;
GO


ALTER TABLE shopifyorder
ALTER COLUMN [LastActivityDate] [date] NOT NULL;

ALTER TABLE profitwiseshop
ALTER COLUMN [StartingDateForOrders] [date] NOT NULL;

UPDATE profitwiseshop SET TimeZone = 'America/Chicago';

