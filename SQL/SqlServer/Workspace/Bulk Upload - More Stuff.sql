USE ProfitWise;
GO

SELECT * FROM uploads(100001);

WHERE DateCreated > DATEADD(hour, -1, GETUTCDATE());

DELETE FROM uploads(100001);

