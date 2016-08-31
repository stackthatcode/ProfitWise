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

DROP TABLE IF EXISTS `shopifyproduct`; 
DROP TABLE IF EXISTS `shopifyvariant`; 

DROP TABLE IF EXISTS `shopifyorder`; 
DROP TABLE IF EXISTS `shopifyorderlineitem`; 

DROP TABLE IF EXISTS `profitwisedatelookup`; 
DROP TABLE IF EXISTS `profitwiseproduct`;

DROP TABLE IF EXISTS `profitwisebatchstate`;
DROP TABLE IF EXISTS `profitwisepreferences`;



CREATE TABLE `shopifyproduct` (
  `ShopId` varchar(128) NOT NULL,
  `ShopifyProductId` BIGINT NOT NULL,
  `Title` varchar(200) DEFAULT NULL,
  PRIMARY KEY (`ShopId`,`ShopifyProductId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE `shopifyvariant` (
  `ShopId` varchar(128) NOT NULL,
  `ShopifyVariantId` BIGINT NOT NULL,
  `ShopifyProductId` BIGINT NOT NULL,
  `PwProductId` BIGINT unsigned NULL, 
  PRIMARY KEY (`ShopId`,`ShopifyVariantId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE `profitwiseproduct` (
  `PwProductId` BIGINT unsigned NOT NULL AUTO_INCREMENT,  
  `ShopId` int(6) unsigned NOT NULL,  
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
  
  `PwProductId` BIGINT unsigned NULL, 
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


