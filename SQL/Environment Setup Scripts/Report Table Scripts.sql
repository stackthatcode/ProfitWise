USE profitwise;

SET SQL_SAFE_UPDATES = 0;


/** Filter Structures ***/

DROP TABLE IF EXISTS `profitwisereport`;
DROP TABLE IF EXISTS `profitwisereportfilter`;



CREATE TABLE `profitwisereport` (
	`PwReportId` BIGINT NOT NULL AUTO_INCREMENT,
	`PwShopId` BIGINT NOT NULL,
	
    `Name` varchar(50) NULL,
    `CopyForEditing` TINYINT NOT NULL,
    `SystemReport` TINYINT NOT NULL,
    `StartDate` DATE NOT NULL,
    `EndDate` DATE NOT NULL,
    `Grouping`  VARCHAR(25) NULL,
    `Ordering`  VARCHAR(25) NULL,
    
	`CreatedDate` TIMESTAMP NOT NULL,
	`LastAccessedDate` TIMESTAMP NOT NULL,
    PRIMARY KEY (`PwReportId`, `PwShopId` )
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AUTO_INCREMENT=99739;

CREATE TABLE `profitwisereportfilter` (
	`PwReportId` BIGINT NOT NULL AUTO_INCREMENT,
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



