USE profitwise;

SET SQL_SAFE_UPDATES = 0;


/** Filter Structures ***/
DROP VIEW IF EXISTS `vw_MasterProductAndVariantSearch`;

DROP TABLE IF EXISTS `profitwisereport`;
DROP TABLE IF EXISTS `profitwisereportfilter`;
DROP TABLE IF EXISTS `profitwisereportquerystub`;


CREATE VIEW vw_MasterProductAndVariantSearch 
AS
SELECT 
	t1.PwShopId,
	t1.PwMasterProductId,
	t1.Title AS ProductTitle,
	t1.Vendor,
	t1.ProductType,
	t3.PwMasterVariantId,
	t3.Title AS VariantTitle,
	t3.Sku
FROM
	profitwiseproduct t1
		INNER JOIN
	profitwisemastervariant t2 ON t1.PwMasterProductId = t2.PwMasterProductId
		INNER JOIN
	profitwisevariant t3 ON t2.PwMasterVariantId = t3.PwMasterVariantId
WHERE
	t1.IsPrimary = 1 AND t3.IsPrimary = 1;


CREATE TABLE `profitwisereport` (
	`PwReportId` BIGINT NOT NULL AUTO_INCREMENT,
	`PwShopId` BIGINT NOT NULL,
	
    `Name` varchar(50) NULL,
    
    `IsSystemReport` TINYINT NOT NULL,
    `CopyForEditing` TINYINT NOT NULL,
    `OriginalReportId` BIGINT NOT NULL,    
    
    `StartDate` DATE NOT NULL,
    `EndDate` DATE NOT NULL,
    `GroupingId`  SMALLINT NULL,
    `OrderingId`  SMALLINT NULL,
    
	`CreatedDate` TIMESTAMP NOT NULL,
	`LastAccessedDate` TIMESTAMP NOT NULL,
    PRIMARY KEY (`PwReportId`, `PwShopId` )
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AUTO_INCREMENT=99739;

CREATE TABLE `profitwisereportfilter` (
	`PwReportId` BIGINT NOT NULL,
	`PwShopId` BIGINT NOT NULL,
    `PwFilterId` BIGINT NOT NULL,
	`FilterType` SMALLINT NOT NULL,
	`NumberKey` BIGINT NULL,
	`StringKey` VARCHAR(100) NULL,
    `Title` VARCHAR(100) NULL,
    `Description` VARCHAR(150) NULL,
    `DisplayOrder` INT NOT NULL,
    PRIMARY KEY (`PwReportId`, `PwShopId`, `PwFilterId` )
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `profitwisereportquerystub` (
	`PwReportId` BIGINT NOT NULL,
	`PwShopId` BIGINT NOT NULL,    
    `PwMasterVariantId` BIGINT NOT NULL,
    `PwMasterProductId` BIGINT NOT NULL,
    `Vendor` VARCHAR(100) NULL,
    `ProductType` VARCHAR(100) NULL,   
    `ProductTitle` VARCHAR(100) NULL,
    `Sku` VARCHAR(100) NULL,
    `VariantTitle` VARCHAR(100) NULL,
    PRIMARY KEY (`PwReportId`, `PwShopId`, `PwMasterVariantId` )
) ENGINE=InnoDB DEFAULT CHARSET=utf8;



ALTER TABLE `profitwisereportquerystub` ADD INDEX `PwMasterProductId` (`PwMasterProductId`);
ALTER TABLE `profitwisereportquerystub` ADD INDEX `Vendor` (`Vendor`);
ALTER TABLE `profitwisereportquerystub` ADD INDEX `ProductType` (`ProductType`);





DROP TABLE IF EXISTS `profitwiseprofitreportentry`;

CREATE TABLE `profitwiseprofitreportentry` (
  `PwShopId` int(6) unsigned NOT NULL, 	# ProfitWise's shop identifier
  `EntryDate` DATE NOT NULL,
  `EntryType` TINYINT NOT NULL, 		# { 1 == Sale, 2 == Refund, 3 == Adjustment }
  `ShopifyOrderId` BIGINT NOT NULL, 	# Shopify identifier for associated order
  `SourceId` BIGINT NOT NULL, 
  `PwProductId` BIGINT NULL, 	
  `PwVariantId` BIGINT NULL, 	
  `NetSales` decimal(15,2) DEFAULT NULL,
  `CoGS` decimal(15,2) DEFAULT NULL,
  `Quantity` INT NULL,
  
  PRIMARY KEY  (`PwShopId`, `EntryDate`, `EntryType`, `ShopifyOrderId`, `SourceId` )
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

ALTER TABLE `profitwiseprofitreportentry` ADD INDEX `ShopifyOrderId` (`ShopifyOrderId`);


