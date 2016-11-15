USE profitwise;

SET SQL_SAFE_UPDATES = 0;


/** Filter Structures ***/

DROP TABLE IF EXISTS `profitwisereport`;
DROP TABLE IF EXISTS `profitwisereportproducttype`;
DROP TABLE IF EXISTS `profitwisereportvendor`;
DROP TABLE IF EXISTS `profitwisereportmasterproduct`;
DROP TABLE IF EXISTS `profitwisereportsku`;

DROP VIEW IF EXISTS `vw_reportproducttypetoproduct`;
DROP VIEW IF EXISTS `vw_reportvendortoproduct`;
DROP VIEW IF EXISTS `vw_reportmasterproducttomastervariant`;



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
    `Ordering`  varchar(25) NULL,
	`CreatedDate` TIMESTAMP NOT NULL,
	`LastAccessedDate` TIMESTAMP NOT NULL,
    PRIMARY KEY (`PwReportId`, `PwShopId` )
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AUTO_INCREMENT=99739;

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
	`PwMasterVariantId` BIGINT NOT NULL,
    PRIMARY KEY (`PwReportId`, `PwShopId`, `PwMasterVariantId` )
) ENGINE=InnoDB DEFAULT CHARSET=utf8;




CREATE VIEW `vw_reportproducttypetoproduct` 
AS 
SELECT t1.PwShopId, t1.PwReportId, t1.ProductType, t2.PwProductId, t2.Vendor, t2.PwMasterProductId
FROM profitwisereportproducttype t1 
	INNER JOIN profitwiseproduct t2 
		ON t1.ProductType = t2.ProductType AND t1.PwShopId = t2.PwShopId AND t2.IsPrimary = 1;


CREATE VIEW `vw_reportvendortoproduct` 
AS 
SELECT t1.PwShopId, t1.PwReportId, t1.Vendor, t2.PwProductId
FROM profitwisereportvendor t1 
	INNER JOIN profitwiseproduct t2 
		ON t1.Vendor = t2.Vendor AND t1.PwShopId = t2.PwShopId;

CREATE VIEW `vw_reportmasterproducttomastervariant`
AS
SELECT t1.PwShopId, t1.PwReportId, t1.PwMasterProductId, t2.PwMasterVariantId
FROM profitwisereportmasterproduct t1
	INNER JOIN profitwisemastervariant t2
		ON t1.PwMasterProductId = t2.PwMasterProductId AND t1.PwShopId = t2.PwShopId;


/*  
3 Product Types
5 Vendors
12 Master Products
17 Master Variants
*/




