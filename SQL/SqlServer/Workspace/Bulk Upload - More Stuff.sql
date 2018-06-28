USE ProfitWise;
GO

UPDATE uploads(100001) SET LastUpdated = DATEADD(day, -2, LastUpdated)



-- Old Uploads - System Repository Clean-up
SELECT * FROM profitwiseuploads
WHERE UploadStatus <> 1 
AND LastUpdated <= DATEADD(day, -7, GETUTCDATE())
ORDER BY [LastUpdated] DESC;



-- Descending Order
SELECT * FROM uploads(100001) 
WHERE UploadStatus <> 1 
ORDER BY [LastUpdated] DESC;



DELETE FROM uploads(100001);

UPDATE uploads(100001) SET UploadStatus = 2 WHERE FileUploadId = 9;


SELECT * FROM dbo.costofgoodsbydate(100001, '6/13/2018');
