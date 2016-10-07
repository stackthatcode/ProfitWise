USE profitwise;

SET SQL_SAFE_UPDATES = 0;


DROP TABLE IF EXISTS `currency`;
DROP TABLE IF EXISTS `exchangerate`;

CREATE TABLE `currency` (
	`CurrencyId` int NOT NULL,
    `Abbreviation` varchar(3) NOT NULL,
    `Symbol` varchar(3) NOT NULL,
    `Name` varchar(50) NOT NULL,
    Primary KEY (`CurrencyId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `exchangerate` (
	`SourceCurrencyId` int NOT NULL,
	`DestinationCurrencyId` int NOT NULL,
    `Date` date NOT NULL,
    `Rate` decimal(9,6) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

INSERT INTO `currency` VALUES ( 1, 'USD', '$', 'United States dollars' );
INSERT INTO `currency` VALUES ( 2, 'EUR', '€', 'Euros' );
INSERT INTO `currency` VALUES ( 3, 'JPY', '¥', 'Japanese yen' );
INSERT INTO `currency` VALUES ( 4, 'GBP', '£', 'Pounds sterling' );
INSERT INTO `currency` VALUES ( 5, 'AUD', '$', 'Australian dollars' );
INSERT INTO `currency` VALUES ( 6, 'CHF', 'Fr', 'Swiss francs' );
INSERT INTO `currency` VALUES ( 7, 'CAD', '$', 'Canadian dollars' );

/*
INSERT INTO `exchangerate` VALUES ( 1, 1, '2016-09-01', 1.0000);
INSERT INTO `exchangerate` VALUES ( 1, 2, '2016-09-01', 0.89718);
INSERT INTO `exchangerate` VALUES ( 1, 3, '2016-09-01', 103.71);
INSERT INTO `exchangerate` VALUES ( 1, 4, '2016-09-01', 0.75408);
INSERT INTO `exchangerate` VALUES ( 1, 5, '2016-09-01', 1.3279);
INSERT INTO `exchangerate` VALUES ( 1, 6, '2016-09-01', 0.98493);
INSERT INTO `exchangerate` VALUES ( 1, 7, '2016-09-01', 1.3129);
SELECT * FROM currency;
SELECT * FROM exchangerate;
*/

SELECT * FROM exchangerate;

DELETE FROM exchangerate WHERE Date >= '2016-08-01';

