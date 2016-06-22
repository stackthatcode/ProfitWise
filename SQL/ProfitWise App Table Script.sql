USE profitwise;

DROP TABLE IF EXISTS `orderskuhistory`; 
DROP TABLE IF EXISTS `shopifyproduct`; 
DROP TABLE IF EXISTS `shopifyvariant`; 
DROP TABLE IF EXISTS `shop`; 


/** TODO - rename these tables to shopifyproductdata ***/

CREATE TABLE `shop` (
  `ShopId` int(6) unsigned NOT NULL AUTO_INCREMENT,
  `UserId` varchar(128) NOT NULL,
  PRIMARY KEY  (`ShopId`, `UserId`)
) ENGINE=InnoDB AUTO_INCREMENT=955973 DEFAULT CHARSET=utf8;


CREATE TABLE `shopifyproduct` (
  `UserId` varchar(128) NOT NULL,
  `ShopifyProductId` bigint(20) NOT NULL,
  `Title` varchar(200) DEFAULT NULL,
  PRIMARY KEY (`UserId`,`ShopifyProductId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE `shopifyvariant` (
  `UserId` varchar(128) NOT NULL,
  `ShopifyVariantId` bigint(20) NOT NULL,
  `ShopifyProductId` bigint(20) DEFAULT NULL,
  `Sku` varchar(100) DEFAULT NULL,
  `Title` varchar(200) DEFAULT NULL,
  `Price` decimal(15,2) DEFAULT NULL,
  PRIMARY KEY (`UserId`,`ShopifyVariantId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;




CREATE TABLE `shopifyorder` (
  `ShopId` int(6) unsigned NOT NULL,
  `ShopifyOrderId` int(6) unsigned NOT NULL,
  `TotalPrice` decimal(15,2) DEFAULT NULL,
  `Email` varchar(128) DEFAULT NULL,
  `OrderNumber` varchar(128) DEFAULT NULL,  
  PRIMARY KEY  (`ShopId`, `ShopifyOrderId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `shopifyorderlineitem` (
  `ShopId` int(6) unsigned NOT NULL,
  `ShopifyOrderLineId` int(6) unsigned NOT NULL,
  `ShopifyOrderId` int(6) unsigned NOT NULL,
  `ProductId` int(6) unsigned NOT NULL,
  `VariantId` int(6) unsigned NOT NULL,
  `ReportedSku` varchar(128) DEFAULT NULL,
  `Quantity` int(6) unsigned NOT NULL,
  `UnitPrice` decimal(15,2) DEFAULT NULL, 
  `TotalDiscount` decimal(15,2) DEFAULT NULL,   
  PRIMARY KEY  (`ShopId`, `ShopifyOrderLineId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;





/* This is an old table... will probably nuke it soon */
CREATE TABLE `orderskuhistory` (
  `LineId` int(6) unsigned NOT NULL AUTO_INCREMENT,
  `OrderNumber` varchar(30) NOT NULL,
  `ProductSku` varchar(30) NOT NULL,
  `Price` decimal(15,2) DEFAULT NULL,
  `CoGS` decimal(15,2) DEFAULT NULL,
  PRIMARY KEY (`LineId`)
) ENGINE=InnoDB AUTO_INCREMENT=955973 DEFAULT CHARSET=utf8;



