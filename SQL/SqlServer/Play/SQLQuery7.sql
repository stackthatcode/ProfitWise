
-- Order Line UnitCoGS troubleshoot...

SELECT * FROM batchstate(100001);



SELECT * FROM product(100001) WHERE Title LIKE '%FlashForge Creator Pro (NEW)%'

SELECT * FROM orderlineitem(100001) WHERE PwProductId = 33 AND OrderDate >= '2016-01-01' AND FinancialStatus IN ( 3, 4, 5, 6 );


SELECT sum(netsales), SUM(cogs) FROM  profitreportentry(100001) WHERE EntryDate = '2016-01-10';

SELECT SUM(UnitCogs * NetQuantity) FROM orderlineitem(100001);

SELECT SUM(Amount) FROM orderrefund(100001);

SELECT SUM(Amount) FROM orderadjustment(100001);

SELECT * FROM AspNetUsers




UPDATE batchstate(100001) SET OrderDatasetStart = NULL, OrderDatasetEnd = NULL;

DELETE FROM profitreportentry(100001)

SELECT * FROM  profitreportentry(100001) WHERE EntryDate = '2016-01-10';

