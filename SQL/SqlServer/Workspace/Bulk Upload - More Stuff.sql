USE ProfitWise;
GO

SELECT * FROM uploads(100001);


SELECT * FROM uploads(@PwShopId)

--WHERE DateCreated > DATEADD(hour, -1, GETUTCDATE())


DELETE FROM uploads(100001);

UPDATE uploads(100001) SET UploadStatus = 2 WHERE FileUploadId = 9;





