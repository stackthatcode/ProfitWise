

SELECT * FROM profitwisereportvendor;

SELECT * FROM profitwiseproduct;

/* Feed this query into UI for Vendor selection */

SELECT * FROM vw_reportproducttypetoproduct;


SELECT * FROM vw_reportvendortoproduct;

SELECT * FROM vw_reportmasterproducttomastervariant;




SELECT * FROM profitwisereportvendor;


DELETE FROM profitwisereportvendor 
WHERE PwShopId = 100001 
AND PwReportId = 99739
AND Vendor NOT IN (
	SELECT DISTINCT Vendor FROM vw_reportproducttypetoproduct 
	WHERE PwShopId = 100001 AND PwReportId = 99739
);


	SELECT DISTINCT Vendor FROM vw_reportproducttypetoproduct 
	WHERE PwShopId = 100001 AND PwReportId = 99739;


	SELECT DISTINCT Vendor FROM vw_reportproducttypetoproduct 
	WHERE PwShopId = 100001 AND ProductType IN ( 
		SELECT ProductType FROM profitwisereportproducttype
		WHERE PwShopId = 100001 AND PwReportId = 99739
	);


SELECT * FROM profitwisereportproducttype;


SELECT * FROM profitwisereportvendor;



SELECT Vendor, COUNT(*) AS Count FROM profitwiseproduct 
WHERE PwShopId = 100001 AND IsPrimary = 1 
AND PwProductId IN ( SELECT PwProductId FROM vw_reportproducttypetoproduct WHERE PwShopId = @PwShopId AND PwReportId = @reportId ) 
GROUP BY Vendor;



