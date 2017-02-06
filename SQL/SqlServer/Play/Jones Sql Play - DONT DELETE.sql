USE ProfitWise
GO

SELECT * FROM profitwisereportquerystub;


DELETE FROM profitwisereportquerystub WHERE PwReportId IN 
( SELECT PwReportId FROM profitwisereport 
WHERE LastAccessedDate < dateadd(minute, -15, getdate()) AND CopyForEditing = 1)

DELETE FROM profitwisereportfilter WHERE PwReportId IN 
( SELECT PwReportId FROM profitwisereport 
WHERE LastAccessedDate < dateadd(minute, -15, getdate()) AND CopyForEditing = 1)

DELETE FROM profitwisereport WHERE PwReportId IN 
( SELECT PwReportId FROM profitwisereport 
WHERE LastAccessedDate < dateadd(minute, -15, getdate()) AND CopyForEditing = 1)



