USE ProfitWise
GO

SELECT * FROM AspNetRoles;
SELECT * FROM AspNetUsers;

SELECT * FROM profitwiseshop;
SELECT * FROM profitwisebatchstate;


SELECT * FROM shopifyorder;

SELECT * FROM dbo.exchangerate;

SELECT * FROM HangFire.Job;

SELECT * FROM HangFire.JobQueue;

SELECT * FROM HangFire.JobParameter;

SELECT * FROM HangFire.Hash;

SELECT * FROM exchangerate;

SELECT * FROM shopifyorder;

SELECT * FROM profitwiseproduct;


DELETE FROM calendar_table;

INSERT INTO calendar_table SELECT * FROM Query2;


UPDATE calendar_table SET isHoliday = 0;

UPDATE calendar_table SET isWeekday = 1 WHERE isWeekday = 49;


SELECT * FROM calendar_table;
