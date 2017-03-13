
-- Order Line UnitCoGS troubleshoot...

SELECT * FROM batchstate(100001);


UPDATE batchstate(100001) SET OrderDatasetStart = NULL, OrderDatasetEnd = NULL;


SELECT * FROM orderlineitem(100001) WHERE UnitCoGS = 0;


