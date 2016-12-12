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
    
    `CopyOfSystemReport` TINYINT NOT NULL,
    `CopyForEditing` TINYINT NOT NULL,
    `OriginalReportId` BIGINT NULL,    
    
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
        
    PRIMARY KEY (`PwReportId`, `PwShopId`, `PwMasterVariantId` )
) ENGINE=InnoDB DEFAULT CHARSET=utf8;



