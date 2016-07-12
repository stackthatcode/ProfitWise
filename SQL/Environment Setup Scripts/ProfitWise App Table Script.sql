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


/** TODO - rename these tables to shopifyproductdata ***/

CREATE TABLE `shop` (
  `ShopId` int(6) unsigned NOT NULL AUTO_INCREMENT,
  `UserId` varchar(128) NOT NULL,
  PRIMARY KEY  (`ShopId`, `UserId`)
) ENGINE=InnoDB AUTO_INCREMENT=955973 DEFAULT CHARSET=utf8;


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
  `Sku` varchar(100) DEFAULT NULL,
  `Title` varchar(200) DEFAULT NULL,
  `Price` decimal(15,2) DEFAULT NULL,
  `Inventory` int(4) DEFAULT NULL,
  `PwProductId` BIGINT unsigned NULL, 
  PRIMARY KEY (`ShopId`,`ShopifyVariantId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE `shopifyorder` (
  `ShopId` int(6) unsigned NOT NULL,
  `ShopifyOrderId` BIGINT unsigned NOT NULL,
  `TotalPrice` decimal(15,2) DEFAULT NULL,
  `Email` varchar(128) DEFAULT NULL,
  `OrderNumber` varchar(128) DEFAULT NULL,  
  `CreatedAt` datetime NOT NULL,
  `UpdatedAt` datetime NOT NULL,
  PRIMARY KEY  (`ShopId`, `ShopifyOrderId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE `shopifyorderlineitem` (
  `ShopId` int(6) unsigned NOT NULL,
  `ShopifyOrderId` BIGINT unsigned NOT NULL,
  `ShopifyOrderLineId` BIGINT unsigned NOT NULL,
  `ShopifyProductId` BIGINT unsigned NULL,
  `ShopifyVariantId` BIGINT unsigned NULL,
  `Sku` varchar(128) DEFAULT NULL,
  `Quantity` int(6) unsigned NOT NULL,
  `UnitPrice` decimal(15,2) DEFAULT NULL,   
  `TotalDiscount` decimal(15,2) DEFAULT NULL,
  
  `ProductTitle` varchar(128) DEFAULT NULL,
  `VariantTitle` varchar(128) DEFAULT NULL,  
  `Name` varchar(256) DEFAULT NULL,  
  
  `PwProductId` BIGINT unsigned NULL, 
  PRIMARY KEY  (`ShopId`, `ShopifyOrderId`, `ShopifyOrderLineId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE `profitwiseproduct` (
  `PwProductId` BIGINT unsigned NOT NULL AUTO_INCREMENT,
  
  `ShopId` int(6) unsigned NOT NULL,
  
  `ProductTitle` varchar(128) DEFAULT NULL,
  `VariantTitle` varchar(128) DEFAULT NULL,
  `Name` varchar(256) DEFAULT NULL,  
  
  `Sku` varchar(128) DEFAULT NULL, 
  PRIMARY KEY  (`PwProductId` )  
) ENGINE=InnoDB DEFAULT CHARSET=utf8;




CREATE TABLE `profitwisebatchstate` (
  `ShopId` int(6) unsigned NOT NULL,
  `ProductsLastUpdated` datetime NULL,
  `OrderDatasetStart` datetime NULL,
  `OrderDatasetEnd` datetime NULL,
   PRIMARY KEY (`ShopId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;




CREATE TABLE `profitwisedatelookup` (
	`StartDate` DATE NOT NULL,
    `EndDate` DATE NOT NULL,
    PRIMARY KEY (`StartDate`, `EndDate`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;



CREATE TABLE `profitwisepreferences` (
  `ShopId` int(6) unsigned NOT NULL,
  `StartingDateForOrders` datetime NULL,
  PRIMARY KEY (`ShopId`)    
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


INSERT INTO profitwisepreferences VALUE (955973, '2016-07-01');


