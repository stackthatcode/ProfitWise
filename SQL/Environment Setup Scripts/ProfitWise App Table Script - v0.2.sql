USE profitwise;


DROP TABLE IF EXISTS `shopifyproduct`; 
DROP TABLE IF EXISTS `shopifyvariant`; 
DROP TABLE IF EXISTS `shop`; 
DROP TABLE IF EXISTS `shopifyorder`; 
DROP TABLE IF EXISTS `shopifyorderlineitem`; 
DROP TABLE IF EXISTS `profitwisedatelookup`; 
DROP TABLE IF EXISTS `profitwiseproduct`;
DROP TABLE IF EXISTS `profitwisebatchstate`;
DROP TABLE IF EXISTS `profitwisepreferences`;


/** Start of new structures **/
DROP TABLE IF EXISTS `profitwiseshop`; 

DROP TABLE IF EXISTS `profitwisemasterproduct`; 
DROP TABLE IF EXISTS `profitwiseproduct`; 
DROP TABLE IF EXISTS `profitwisemastervariant`; 
DROP TABLE IF EXISTS `profitwisevariant`; 

DROP TABLE IF EXISTS `shopifyorder`; 
DROP TABLE IF EXISTS `shopifyorderlineitem`; 

DROP TABLE IF EXISTS `profitwisebatchstate`;
DROP TABLE IF EXISTS `profitwisepreferences`;



CREATE TABLE `profitwiseshop` (
  `PwShopId` BIGINT unsigned NOT NULL AUTO_INCREMENT,
  `UserId` varchar(128) NOT NULL,
  PRIMARY KEY  (`PwShopId`, `UserId`)
) ENGINE=InnoDB AUTO_INCREMENT=955973 DEFAULT CHARSET=utf8;



CREATE TABLE `profitwisemasterproduct` (
  `PwShopId` BIGINT unsigned NOT NULL,
  `PwMasterProductId` BIGINT unsigned NOT NULL AUTO_INCREMENT, 
  PRIMARY KEY (`PwShopId`,`PwMasterProductId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `profitwiseproduct` (
  `PwShopId` BIGINT unsigned NOT NULL,
  `PwProductId` BIGINT unsigned NOT NULL AUTO_INCREMENT,
  `PwMasterProductId` BIGINT unsigned NOT NULL,
  `ShopifyProductId` BIGINT unsigned NOT NULL,
  
  `Title` VARCHAR(200) DEFAULT NULL,
  `Vendor` VARCHAR(100) DEFAULT NULL,
  `ProductType` VARCHAR(100) DEFAULT NULL,
  
  `Active` TINYINT unsigned NOT NULL,
  `Primary` TINYINT unsigned NOT NULL,
  PRIMARY KEY (`PwShopId`,`PwProductId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE `profitwisemastervariant` (
  `PwShopId` BIGINT unsigned NOT NULL,
  `PwMasterVariantId` BIGINT unsigned NOT NULL AUTO_INCREMENT,
  `PwProductId` BIGINT unsigned NOT NULL,	/** Immutable **/  
  `Exclude` TINYINT NOT NULL,
  `StockedDirectly` TINYINT NOT NULL,
  PRIMARY KEY (`PwShopId`,`PwMasterVariantId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


/**  **/
CREATE TABLE `profitwisevariant` (
  `PwShopId` BIGINT unsigned NOT NULL,		/** PK **/
  `PwVariantId` BIGINT unsigned NOT NULL AUTO_INCREMENT,	/** PK **/  
  `PwMasterVariantId` BIGINT unsigned NOT NULL,		/** Can change i.e. can be assigned to another Master record **/
  
  `ShopifyVariantId` BIGINT unsigned NOT NULL,
  `Active` TINYINT NOT NULL,
  `Primary` TINYINT NOT NULL,

  PRIMARY KEY (`PwShopId`,`PwVariantId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;




CREATE TABLE `profitwiseproduct` (
  `PwProductId` BIGINT unsigned NOT NULL AUTO_INCREMENT,  
  `PwShopId` int(6) unsigned NOT NULL,  
  `ProductTitle` varchar(128) DEFAULT NULL,
  `VariantTitle` varchar(128) DEFAULT NULL,
  `Name` varchar(256) DEFAULT NULL,    
  `Sku` varchar(128) DEFAULT NULL, 
  `Price` decimal(15,2) DEFAULT NULL,
  `Inventory` int(4) DEFAULT NULL,  
  `Tags` varchar(500) DEFAULT NULL,

  PRIMARY KEY  (`PwProductId` )  
) ENGINE=InnoDB DEFAULT CHARSET=utf8;



CREATE TABLE `shopifyorder` (
  `PwShopId` int(6) unsigned NOT NULL,
  `ShopifyOrderId` BIGINT unsigned NOT NULL,
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
  `ShopifyOrderId` BIGINT unsigned NOT NULL,
  `ShopifyOrderLineId` BIGINT unsigned NOT NULL,
  `OrderDate` date NOT NULL,
  `ShopifyProductId` BIGINT unsigned NULL,
  `ShopifyVariantId` BIGINT unsigned NULL,
  `Sku` varchar(128) DEFAULT NULL,
  `Quantity` int(6) unsigned NOT NULL,
  `UnitPrice` decimal(15,2) DEFAULT NULL,   
  `TotalDiscount` decimal(15,2) DEFAULT NULL,
  `TotalRestockedQuantity` int(6) unsigned NOT NULL,
  `GrossRevenue` decimal(15,2) DEFAULT NULL,
  `ProductTitle` varchar(128) DEFAULT NULL,
  `VariantTitle` varchar(128) DEFAULT NULL,  
  `Name` varchar(256) DEFAULT NULL,  
  `Vendor` varchar(128) DEFAULT NULL,
  
  `PwProductId` BIGINT unsigned NOT NULL, 
  `PwVariantId` BIGINT unsigned NOT NULL, 
  
  `PwMasterProductId` BIGINT unsigned NOT NULL, /** Performance Optimization **/
  `PwMasterVariantId` BIGINT unsigned NOT NULL,  /** Performance Optimization **/
  
  
  PRIMARY KEY  (`PwShopId`, `ShopifyOrderId`, `ShopifyOrderLineId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;





CREATE TABLE `profitwisebatchstate` (
  `PwShopId` int(6) unsigned NOT NULL,
  `ProductsLastUpdated` datetime NULL,
  `OrderDatasetStart` datetime NULL,
  `OrderDatasetEnd` datetime NULL,
   PRIMARY KEY (`PwShopId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;





CREATE TABLE `profitwisedatelookup` (
	`StartDate` DATE NOT NULL,
    `EndDate` DATE NOT NULL,
    PRIMARY KEY (`StartDate`, `EndDate`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;



CREATE TABLE `profitwisepreferences` (
  `PwShopId` int(6) unsigned NOT NULL,
  `StartingDateForOrders` datetime NULL,
  PRIMARY KEY (`PwShopId`)    
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


INSERT INTO profitwisepreferences VALUE (955973, '2016-07-01');


