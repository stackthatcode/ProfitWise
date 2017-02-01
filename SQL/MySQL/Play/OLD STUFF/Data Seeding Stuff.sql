USE ProfitWise;

drop procedure load_foo_test_data;


delimiter #
create procedure load_foo_test_data()
begin

declare v_max int unsigned default 1000;
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


CALL load_foo_test_data();


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



