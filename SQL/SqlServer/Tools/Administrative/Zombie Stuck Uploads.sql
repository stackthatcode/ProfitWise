USE ProfitWise;


-- Identifies uploads which may have Zombied!!!

DECLARE @TimeLimitMinutes int = 30;

SELECT * FROM profitwiseuploads 
WHERE UploadStatus = 1 AND DATEDIFF(minute, LastUpdated, GETUTCDATE()) > @TimeLimitMinutes;


-- *** DEADLY - will Zombie any uploads in *any shop* that are past time limit
/*
DECLARE @TimeLimitMinutes int = 0;
UPDATE profitwiseuploads SET UploadStatus = 5 
WHERE UploadStatus = 1 AND DATEDIFF(minute, LastUpdated, GETUTCDATE()) > @TimeLimitMinutes;
*/

