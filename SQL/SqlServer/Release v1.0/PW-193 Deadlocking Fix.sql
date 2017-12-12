USE ProfitWise

/** This script will clear all open connections to ProfitWise **/

ALTER DATABASE ProfitWise SET SINGLE_USER WITH ROLLBACK IMMEDIATE 

ALTER DATABASE ProfitWise
SET READ_COMMITTED_SNAPSHOT ON;
GO

ALTER DATABASE ProfitWise SET MULTI_USER

