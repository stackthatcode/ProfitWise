USE profitwise;

SET SQL_SAFE_UPDATES = 0;


DROP TABLE IF EXISTS `profitwiseshop`; 

DROP TABLE IF EXISTS `profitwisemasterproduct`; 
DROP TABLE IF EXISTS `profitwiseproduct`; 
DROP TABLE IF EXISTS `profitwisemastervariant`; 
DROP TABLE IF EXISTS `profitwisevariant`; 

DROP TABLE IF EXISTS `shopifyorder`; 
DROP TABLE IF EXISTS `shopifyorderlineitem`; 

DROP TABLE IF EXISTS `profitwisebatchstate`;




CREATE TABLE `profitwiseshop` (
  `PwShopId` BIGINT NOT NULL AUTO_INCREMENT,
  `UserId` varchar(128) NOT NULL,
  `ShopifyShopId` BIGINT NULL,
  `CurrencyId` INT NULL,
  `StartingDateForOrders` DATETIME NULL,
  PRIMARY KEY  (`PwShopId`, `UserId`)
) ENGINE=InnoDB AUTO_INCREMENT=100001 DEFAULT CHARSET=utf8;

CREATE TABLE `profitwisemasterproduct` (
  `PwMasterProductId` BIGINT NOT NULL AUTO_INCREMENT,
  `PwShopId` BIGINT NOT NULL,
  PRIMARY KEY (`PwMasterProductId`, `PwShopId`)
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
  PRIMARY KEY (`PwProductId`, `PwShopId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE `profitwisemastervariant` (
  `PwMasterVariantId` BIGINT NOT NULL AUTO_INCREMENT,
  `PwShopId` BIGINT NOT NULL,
  `PwMasterProductId` BIGINT NOT NULL,
  `Exclude` TINYINT NOT NULL,
  `StockedDirectly` TINYINT NOT NULL,
  `CogsCurrencyId` INT NULL,
  `CogsAmount` DECIMAL(15, 2) NULL,
  `CogsDetail` TINYINT NULL,
  PRIMARY KEY (`PwMasterVariantId`,`PwShopId`)
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
  `LowPrice` DECIMAL(15, 2) NOT NULL,
  `HighPrice` DECIMAL(15, 2) NOT NULL,
  `Inventory` INT NULL,
  `IsActive` TINYINT NOT NULL,
  `IsPrimary` TINYINT NOT NULL,

  PRIMARY KEY (`PwVariantId`,`PwShopId`)
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
  PRIMARY KEY  (`PwShopId`, `ShopifyOrderId`)
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
  PRIMARY KEY  (`PwShopId`, `ShopifyOrderId`, `ShopifyOrderLineId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;



CREATE TABLE `profitwisebatchstate` (
  `PwShopId` int(6) unsigned NOT NULL,
  `ProductsLastUpdated` datetime NULL,
  `OrderDatasetStart` datetime NULL,
  `OrderDatasetEnd` datetime NULL,
   PRIMARY KEY (`PwShopId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;




UPDATE profitwiseshop SET StartingDateForOrders = '2016-07-01'


