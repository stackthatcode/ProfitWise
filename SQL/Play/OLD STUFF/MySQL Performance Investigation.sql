
USE profitwise;


/*
CREATE TABLE OrderSkuHistory (
	LineId INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
	OrderNumber VARCHAR(30) NOT NULL,
	ProductSku VARCHAR(30) NOT NULL,
	Price numeric(15,2),
	CoGS numeric(15,2)
);

SHOW TABLES;

*/
/* INSERT INTO OrderSkuHistory (OrderNumber, ProductSku, Price, CoGS) VALUES ('1312381930', 'ABCDEFG001', 9.00, 7.50);*/
    
CALL TestDataPopulation(1);

SELECT COUNT(*) FROM OrderSkuHistory;

DELETE FROM OrderSkuHistory WHERE LineId = LineId;

SELECT * FROM OrderSkuHistory;



DROP PROCEDURE TestDataPopulation; 


DELIMITER $$

CREATE PROCEDURE TestDataPopulation(p1 INT)
BEGIN
	SET autocommit = 0;
    SELECT 'Ok...';

	label1: LOOP
		INSERT INTO OrderSkuHistory (OrderNumber, ProductSku, Price, CoGS) VALUES ('1312381930', 'ABCDEFG001', 9.00, 7.50);			
		SET p1 = p1 + 1;
		
		IF p1 < 100000 THEN
		  ITERATE label1;	/* This is the same as C# 'goto' */
		END IF;
		
		LEAVE label1; /* This is the same is C# 'break' */
		
	END LOOP label1;
	
	COMMIT;
    SELECT 'Whuttt?';
	SET @x = p1;		
END $$

DELIMITER ;




