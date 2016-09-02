USE profitwise;

DROP TABLE IF EXISTS `profitwiseshop`; 

DROP TABLE IF EXISTS `profitwisemasterproduct`; 
DROP TABLE IF EXISTS `profitwiseproduct`; 
DROP TABLE IF EXISTS `profitwisemastervariant`; 
DROP TABLE IF EXISTS `profitwisevariant`; 

DROP TABLE IF EXISTS `shopifyorder`; 
DROP TABLE IF EXISTS `shopifyorderlineitem`; 

DROP TABLE IF EXISTS `profitwisebatchstate`;
DROP TABLE IF EXISTS `profitwisepreferences`;

DROP TABLE IF EXISTS `currency`;
DROP TABLE IF EXISTS `exchangerate`;




CREATE TABLE `profitwiseshop` (
  `PwShopId` BIGINT NOT NULL AUTO_INCREMENT,
  `UserId` varchar(128) NOT NULL,
  Primary KEY  (`PwShopId`, `UserId`)
) ENGINE=InnoDB AUTO_INCREMENT=955973 DEFAULT CHARSET=utf8;

CREATE TABLE `profitwisemasterproduct` (
  `PwMasterProductId` BIGINT NOT NULL AUTO_INCREMENT,
  `PwShopId` BIGINT NOT NULL,
  Primary KEY (`PwMasterProductId`, `PwShopId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `profitwiseproduct` (
  `PwProductId` BIGINT NOT NULL AUTO_INCREMENT,
  `PwShopId` BIGINT NOT NULL,
  `PwMasterProductId` BIGINT NOT NULL,
  `ShopifyProductId` BIGINT NULL,
  
  `Title` VARCHAR(200) DEFAULT NULL,
  `Vendor` VARCHAR(100) DEFAULT NULL,
  `ProductType` VARCHAR(100) DEFAULT NULL,
  
  `IsActive` TINYINT unsigned NOT NULL,
  `IsPrimary` TINYINT unsigned NOT NULL,
  `Tags` TEXT DEFAULT NULL,
  Primary KEY (`PwProductId`, `PwShopId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE `profitwisemastervariant` (
  `PwMasterVariantId` BIGINT NOT NULL AUTO_INCREMENT,
  `PwShopId` BIGINT NOT NULL,
  `PwMasterProductId` BIGINT NOT NULL,
  `Exclude` TINYINT NOT NULL,
  `StockedDirectly` TINYINT NOT NULL,
  Primary KEY (`PwMasterVariantId`,`PwShopId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `profitwisevariant` (
  `PwVariantId` BIGINT NOT NULL AUTO_INCREMENT,	/** PK **/  
  `PwShopId` BIGINT NOT NULL,		/** PK **/
  `PwProductId` BIGINT NOT NULL,	/** Immutable **/  
  `PwMasterVariantId` BIGINT NOT NULL,		/** Can change i.e. can be assigned to another Master record **/
  
  `ShopifyProductId` BIGINT NULL,
  `ShopifyVariantId` BIGINT NULL,
  `Sku` VARCHAR(100) NULL,
  `Title` VARCHAR(200) NULL,
  `IsActive` TINYINT NOT NULL,
  `IsPrimary` TINYINT NOT NULL,

  Primary KEY (`PwVariantId`,`PwShopId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;




CREATE TABLE `shopifyorder` (
  `PwShopId` int(6) unsigned NOT NULL,
  `ShopifyOrderId` BIGINT NOT NULL,
  `Email` varchar(128) DEFAULT NULL,
  `OrderNumber` varchar(128) DEFAULT NULL,  
  `OrderLevelDiscount` decimal(15,2) DEFAULT NULL,
  `SubTotal` decimal(15,2) DEFAULT NULL,
  `TotalRefund` decimal(15,2) DEFAULT NULL,
  `TaxRefundAmount` decimal(15,2) DEFAULT NULL,
  `ShippingRefundAmount` decimal(15,2) DEFAULT NULL,
  `FinancialStatus` varchar(25) DEFAULT NULL,
  `Tags` varchar(500) DEFAULT NULL,  
  `CreatedAt` datetime NOT NULL,
  `UpdatedAt` datetime NOT NULL,
  Primary KEY  (`PwShopId`, `ShopifyOrderId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `shopifyorderlineitem` (
  `PwShopId` int(6) unsigned NOT NULL,
  `ShopifyOrderId` BIGINT NOT NULL,
  `ShopifyOrderLineId` BIGINT NOT NULL,
  `OrderDate` date NOT NULL,  
  `PwProductId` BIGINT NOT NULL, 
  `PwVariantId` BIGINT NOT NULL,      
  `Quantity` int(6) unsigned NOT NULL,
  `UnitPrice` decimal(15,2) DEFAULT NULL,   
  `TotalDiscount` decimal(15,2) DEFAULT NULL,
  `TotalRestockedQuantity` int(6) unsigned NOT NULL,
  `GrossRevenue` decimal(15,2) DEFAULT NULL,
  Primary KEY  (`PwShopId`, `ShopifyOrderId`, `ShopifyOrderLineId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;




CREATE TABLE `profitwisebatchstate` (
  `PwShopId` int(6) unsigned NOT NULL,
  `ProductsLastUpdated` datetime NULL,
  `OrderDatasetStart` datetime NULL,
  `OrderDatasetEnd` datetime NULL,
   Primary KEY (`PwShopId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `profitwisepreferences` (
  `PwShopId` int(6) unsigned NOT NULL,
  `StartingDateForOrders` datetime NULL,
  Primary KEY (`PwShopId`)    
) ENGINE=InnoDB DEFAULT CHARSET=utf8;




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



/*
CREATE TABLE `profitwisedatelookup` (
	`StartDate` DATE NOT NULL,
    `EndDate` DATE NOT NULL,
    Primary KEY (`StartDate`, `EndDate`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
*/

INSERT INTO profitwisepreferences VALUE (955973, '2016-07-01');


