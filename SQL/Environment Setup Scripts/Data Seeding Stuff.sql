USE ProfitWise;

drop procedure load_foo_test_data;


delimiter #
create procedure load_foo_test_data()
begin

declare v_max int unsigned default 500;
declare v_counter int unsigned default 1;

  while v_counter < v_max do
	START TRANSACTION;
    
    INSERT INTO shopifyorderlineitem ( ShopId, ShopifyOrderLineId, ShopifyOrderId, ShopifyProductId, ShopifyVariantId, ReportedSku, Quantity, UnitPrice, TotalDiscount )
	SELECT v_counter, t2.ShopifyOrderLineId, t2.ShopifyOrderId, t2.ShopifyProductId, t2.ShopifyVariantId, t2.ReportedSku, t2.Quantity, t2.UnitPrice, t2.TotalDiscount
	FROM shopifyorderlineitem t2 
    WHERE t2.ShopId = 955973;
    
    SET v_counter=v_counter+1;
	COMMIT;
  end while;
end #

delimiter ;



SET SQL_SAFE_UPDATES = 0;


drop procedure load_datelookup;


delimiter #
CREATE PROCEDURE load_datelookup()
BEGIN

  DECLARE date_counter datetime default '2014-01-01 00:00:00';  
  SET @date_counter = '2014-01-01 00:00:00';
  
  START TRANSACTION;
    
    DELETE FROM profitwisedatelookup;
    
	WHILE @date_counter <= '2020-12-31 00:00:00' DO
	
		INSERT INTO profitwisedatelookup ( StartDate, EndDate ) 
			VALUES (@date_counter, DATE_ADD( @date_counter, INTERVAL 1 DAY));
		
		SET @date_counter = DATE_ADD( @date_counter, INTERVAL 1 DAY);
		
	END WHILE;
  
  COMMIT;

  SELECT COUNT(*) FROM profitwisedatelookup;
END #

delimiter ;




SELECT CURRENT_DATE();

call load_datelookup;

SELECT * FROM profitwisedatelookup;

SELECT * FROM shopifyorder;






CALL load_foo_test_data;

SELECT * FROM shopifyorderlineitem;

SELECT ShopifyVariantId, ReportedSku, SUM(Quantity * UnitPrice) FROM shopifyorderlineitem WHERE ShopId = 955973 GROUP BY ShopifyVariantId;


SELECT * FROM profitwisedatelookup;




SELECT DATE(t2.CreatedAt), t3.ReportedSku, SUM(t3.UnitPrice * t3.Quantity)
FROM shopifyorder t2 
	INNER JOIN shopifyorderlineitem t3
		ON t2.ShopifyOrderId = t3.ShopifyOrderId
WHERE t3.ReportedSku = 'UM2PLUS'
GROUP BY DATE(t2.CreatedAt), t3.ReportedSku;






