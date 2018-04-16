USE ProfitWise;

SELECT name, recovery_model_desc  
   FROM sys.databases  
      WHERE name = 'ProfitWise' ;  
GO  

ALTER DATABASE ProfitWise
SET RECOVERY SIMPLE
GO
DBCC SHRINKFILE (ProfitWise_log, 1)
GO
ALTER DATABASE ProfitWise
SET RECOVERY FULL

