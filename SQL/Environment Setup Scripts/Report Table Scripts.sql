USE profitwise;

SET SQL_SAFE_UPDATES = 0;


/** Filter Structures ***/

DROP TABLE IF EXISTS `profitwisereport`;
DROP TABLE IF EXISTS `profitwisereportproducttype`;
DROP TABLE IF EXISTS `profitwisereportvendor`;
DROP TABLE IF EXISTS `profitwisereportmasterproduct`;
DROP TABLE IF EXISTS `profitwisereportsku`;



CREATE TABLE `profitwisereport` (
	`PwReportId` BIGINT NOT NULL AUTO_INCREMENT,
	`PwShopId` BIGINT NOT NULL,
	`Name` varchar(50) NULL,
    `Saved` TINYINT NOT NULL,
    `AllProductTypes` TINYINT NOT NULL,
    `AllVendors` TINYINT NOT NULL,
    `AllProducts` TINYINT NOT NULL,
    `AllSkus` TINYINT NOT NULL,
    `Grouping`  varchar(25) NULL,
	`CreatedDate` TIMESTAMP NOT NULL,
	`LastAccessedDate` TIMESTAMP NOT NULL,
    PRIMARY KEY (`PwReportId`, `PwShopId` )
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `profitwisereportproducttype` (
	`PwReportId` BIGINT NOT NULL AUTO_INCREMENT,
	`PwShopId` BIGINT NOT NULL,
	`ProductType` varchar(100) NOT NULL,
    PRIMARY KEY (`PwReportId`, `PwShopId`, `ProductType` )
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `profitwisereportvendor` (
	`PwReportId` BIGINT NOT NULL AUTO_INCREMENT,
	`PwShopId` BIGINT NOT NULL,
	`Vendor` varchar(100) NOT NULL,
    PRIMARY KEY (`PwReportId`, `PwShopId`, `Vendor` )
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `profitwisereportmasterproduct` (
	`PwReportId` BIGINT NOT NULL AUTO_INCREMENT,
	`PwShopId` BIGINT NOT NULL,
	`PwMasterProductId` BIGINT NOT NULL,
    PRIMARY KEY (`PwReportId`, `PwShopId`, `PwMasterProductId` )
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `profitwisereportsku` (
	`PwReportId` BIGINT NOT NULL AUTO_INCREMENT,
	`PwShopId` BIGINT NOT NULL,
	`Sku` varchar(100) NOT NULL,
    PRIMARY KEY (`PwReportId`, `PwShopId`, `Sku` )
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


