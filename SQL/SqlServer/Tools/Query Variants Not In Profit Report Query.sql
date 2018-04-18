USE ProfitWise;
GO

SELECT t1.* 
FROM profitreportentry(100001) t1
	INNER JOIN variant(100001) t2
		ON t1.PwProductId = t2.PwProductId AND t1.PwVariantId = t2.PwVariantId
WHERE PwMasterVariantId NOT IN (
	SELECT PwMasterVariantId FROM profitquerystub(100001) WHERE PwReportId = 1
) 
AND t1.EntryDate >= '04-01-2018';


SELECT * FROM variant(100001) WHERE PwVariantId = 57524;
SELECT * FROM variant(100001) WHERE PwVariantId = 230858;
