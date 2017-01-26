USE profitwise; # Set active database

SET SQL_SAFE_UPDATES = 0; # Turn off Safe Updates setting


DROP TABLE IF EXISTS `systemstate`;
DROP TABLE IF EXISTS `currency`;
DROP TABLE IF EXISTS `exchangerate`;

CREATE TABLE `systemstate` (
	`ExchangeRateLastDate` DATE NULL		# Marks the end of the Exchange Rate data
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

INSERT INTO `systemstate` VALUES ( NULL );


CREATE TABLE `currency` (
	`CurrencyId` int NOT NULL, # Numeric value of currency
    `Abbreviation` varchar(3) NOT NULL, # Currency abbreviation
    `Symbol` varchar(3) NOT NULL, # Currency symbol
    `Name` varchar(50) NOT NULL, # Currency name
    Primary KEY (`CurrencyId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `exchangerate` (
	`SourceCurrencyId` int NOT NULL, 		# Currency to convert from
	`DestinationCurrencyId` int NOT NULL, 	# Currency to convert to
    `Date` date NOT NULL, 					# Date for currency conversion data
    `Rate` decimal(9,6) NOT NULL,  			# Multiplier for currency conversion
	Primary KEY (`SourceCurrencyId`, `DestinationCurrencyId`, `Date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

ALTER TABLE `exchangerate` ADD INDEX `Date` (`Date`);


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

SELECT * FROM systemstate;

SELECT * FROM exchangerate;
# WHERE Date = '2014-01-01';

SELECT * FROM currency;


