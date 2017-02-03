USE ProfitWise
GO


DELETE FROM profitwisereportquerystub
DELETE FROM profitwisereportfilter
DELETE FROM profitwisereport



DELETE FROM profitwisereportquerystub WHERE PwReportId IN 
( SELECT PwReportId FROM profitwisereport 
WHERE LastAccessedDate < dateadd(minute, -15, getdate()) AND CopyForEditing = 1)

DELETE FROM profitwisereportfilter WHERE PwReportId IN 
( SELECT PwReportId FROM profitwisereport 
WHERE LastAccessedDate < dateadd(minute, -15, getdate()) AND CopyForEditing = 1)

DELETE FROM profitwisereport WHERE PwReportId IN 
( SELECT PwReportId FROM profitwisereport 
WHERE LastAccessedDate < dateadd(minute, -15, getdate()) AND CopyForEditing = 1)



