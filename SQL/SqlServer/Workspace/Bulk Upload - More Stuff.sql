USE ProfitWise;
GO


SELECT * FROM uploads(100001);

DELETE FROM uploads(100001);

UPDATE uploads(100001) SET UploadStatus = 2 WHERE FileUploadId = 9;





