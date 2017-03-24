

SELECT * FROM profitquerystub(100001) WHERE PwReportId = 1;

SELECT * FROM report(100001);

SELECT * FROM reportfilter(100001) WHERE PwReportId = 99887;

SELECT * FROM product(100001) WHERE Vendor = '';


SELECT t1.* FROM variant(100001) t1 INNER JOIN 
	mastervariant(100001) t2 ON t1.PwMasterVariantId = t2.PwMasterVariantId
WHERE t1.PwMasterVariantId IN ( 363, 364, 362 );

SELECT * FROM Product(100001) WHERE PwProductId IN (73, 74, 75);

SELECT * FROM mastervariant(100001) WHERE PwMasterVariantId IN ( 363, 364, 362 );


SELECT * FROM profitreportentry(100001);


SELECT * FROM ordertable(100001) WHERE OrderNumber = '#5238072';

SELECT * FROM ordertable(100001) WHERE OrderDate = '2017-01-01';

SELECT * FROM profitwisereport;

SELECT * FROM systemstate;


SELECT * FROM shop(100001);


SELECT * FROM product(100001) WHERE Title LIKE '%Ultimaker 2%'

SELECT * FROM variant(100001) WHERE SKU LIKE '%UM2%'

SELECT * FROM product(100001) WHERE Title LIKE '%Ultimaker 3%'

SELECT * FROM variant(100001) WHERE SKU LIKE '%UM3%'


UPDATE shop(100001) SET TimeZone = '(GMT-05:00) Eastern Time'


