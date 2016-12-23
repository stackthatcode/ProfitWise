USE profitwise; # Set active database

SET SQL_SAFE_UPDATES = 0; # Turn off Safe Updates setting


DROP TABLE IF EXISTS `profitwiseshop`; 

DROP TABLE IF EXISTS `profitwisemasterproduct`; 
DROP TABLE IF EXISTS `profitwiseproduct`; 
DROP TABLE IF EXISTS `profitwisemastervariant`; 
DROP TABLE IF EXISTS `profitwisevariant`; 

DROP TABLE IF EXISTS `shopifyorder`; 
DROP TABLE IF EXISTS `shopifyorderlineitem`; 

DROP TABLE IF EXISTS `profitwisebatchstate`;

DROP TABLE IF EXISTS `profitwisequery`;
DROP TABLE IF EXISTS `profitwisequerymasterproduct`;

DROP TABLE IF EXISTS `profitwisepicklist`;
DROP TABLE IF EXISTS `profitwisepicklistmasterproduct`;




CREATE TABLE `profitwiseshop` (
  `PwShopId` BIGINT NOT NULL AUTO_INCREMENT, # ProfitWise's unique identifier for each shop
  `ShopOwnerUserId` varchar(128) NOT NULL, # Email address of shop owner
  `ShopifyShopId` BIGINT NULL, # Shopify's internal identifier for each shop
  `CurrencyId` INT NULL, # Numeric value representing shop currency
  `StartingDateForOrders` TIMESTAMP NULL, # Starting date for order data to import
  `TimeZone` varchar(50) NULL, # Shop time zone
  
  `IsAccessTokenValid` TINYINT NOT NULL, # Is the shop's access token currently valid?
  `IsShopEnabled` TINYINT NOT NULL, # Is the shop currently enabled in ProfitWise?
  `IsDataLoaded` TINYINT NOT NULL, # Has the shop's data been loaded into ProfitWise?
  
  PRIMARY KEY  (`PwShopId`, `ShopOwnerUserId`)
) ENGINE=InnoDB AUTO_INCREMENT=100001 DEFAULT CHARSET=utf8;


CREATE TABLE `profitwisemasterproduct` (
  `PwMasterProductId` BIGINT NOT NULL AUTO_INCREMENT, # ProfitWise's unique identifier for each product (can contain multiple "linked" Shopify products)
  `PwShopId` BIGINT NOT NULL, # ProfitWise's shop identifier
  PRIMARY KEY (`PwMasterProductId`, `PwShopId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `profitwiseproduct` (
  `PwProductId` BIGINT NOT NULL AUTO_INCREMENT, # ProfitWise's unique identifier for each product (maps to a PwMasterProductId)
  `PwShopId` BIGINT NOT NULL, # ProfitWise's shop identifier
  `PwMasterProductId` BIGINT NOT NULL, # ProfitWise's unique identifier for each product (can contain multiple "linked" Shopify products)
  `ShopifyProductId` BIGINT NULL, # Shopify's product identifier (maps to a PwProductId)
  
  `Title` VARCHAR(200) DEFAULT NULL, # Product title
  `Vendor` VARCHAR(100) DEFAULT NULL, # Product vendor
  `ProductType` VARCHAR(100) DEFAULT NULL, # Product type
  `Tags` TEXT DEFAULT NULL, # Product tags
  
  `IsActive` TINYINT unsigned NOT NULL, # Is product active in Shopify catalog?
  `IsPrimary` TINYINT unsigned NOT NULL, # Is this the primary product under the PwMasterProductId?
  `IsPrimaryManual` TINYINT unsigned NOT NULL, # Was this product manually set as the primary product under the PwMasterProductId?
  `LastUpdated` TIMESTAMP NOT NULL, # When was this product last updated in Shopify?
  PRIMARY KEY (`PwProductId`, `PwShopId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `profitwisemastervariant` (
  `PwMasterVariantId` BIGINT NOT NULL AUTO_INCREMENT, # ProfitWise's unique identifier for each variant (may contain multiple "linked" variants)
  `PwShopId` BIGINT NOT NULL, # ProfitWise's shop identifier
  `PwMasterProductId` BIGINT NOT NULL, # ProfitWise's unique identifier for each product (can contain multiple "linked" Shopify products)
  `Exclude` TINYINT NOT NULL, # Should this variant be excluded from ProfitWise reporting?
  `StockedDirectly` TINYINT NOT NULL, # Is this variant stocked directly (as opposed to drop-shipped)?
  
  `CogsCurrencyId` INT NULL, # Numeric value representing the currency for the CoGS data for this variant
  `CogsAmount` DECIMAL(15, 2) NULL, # CoGS value for this variant
  `CogsDetail` TINYINT NULL, # Is there detailed CoGS data (weighted average entries) for this variant?
  PRIMARY KEY (`PwMasterVariantId`,`PwShopId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `profitwisevariant` (
  `PwVariantId` BIGINT NOT NULL AUTO_INCREMENT,	# ProfitWise's unique identifier for each variant (maps to a PwMasterVariantId)
  `PwShopId` BIGINT NOT NULL, # ProfitWise's shop identifier
  `PwProductId` BIGINT NOT NULL, # ProfitWise's unique identifier for each product (maps to a PwMasterProductId)
  `PwMasterVariantId` BIGINT NOT NULL, # ProfitWise's unique identifier for each variant (may contain multiple "linked" variants)
  /** Can change i.e. can be assigned to another Master record **/
  
  `ShopifyProductId` BIGINT NULL, # Shopify's product identifier (maps to a PwProductId)
  `ShopifyVariantId` BIGINT NULL, # Shopify's variant identifier (maps to a PwVariantId)
  `Sku` VARCHAR(100) NULL, # Variant SKU
  `Title` VARCHAR(200) NULL, # Variant title
  `LowPrice` DECIMAL(15, 2) NOT NULL, # Lowest price for this variant from order history
  `HighPrice` DECIMAL(15, 2) NOT NULL, # Highest price for this variant from order history
  `Inventory` INT NULL, # Inventory count for this variant
  
  `IsActive` TINYINT NOT NULL, # Is this variant active in Shopify's catalog?
  `IsPrimary` TINYINT NOT NULL, # Is this the primary variant under the PwMasterVariantId?
  `IsPrimaryManual` TINYINT unsigned NOT NULL, # Was this variant manually selected as the parimary variant under the PwMasterVariantId?
  `LastUpdated` TIMESTAMP NOT NULL, # When was this variant last updated in Shopify?

  PRIMARY KEY (`PwVariantId`,`PwShopId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE `profitwisebatchstate` (
  `PwShopId` BIGINT unsigned NOT NULL, 	# ProfitWise's shop identifier
  `ProductsLastUpdated` TIMESTAMP NULL, # When did ProfitWise last update products from Shopify's catalog?
  `OrderDatasetStart` TIMESTAMP NULL, 	# Start date for the current order data set to be loaded from Shopify
  `OrderDatasetEnd` TIMESTAMP NULL, 	# End date for the current order data set to be loaded from Shopify
   PRIMARY KEY (`PwShopId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;





CREATE TABLE `shopifyorder` (
  `PwShopId` int(6) unsigned NOT NULL, # ProfitWise's shop identifier
  `ShopifyOrderId` BIGINT NOT NULL, # Shopify's unique order identifier
  `Email` varchar(128) DEFAULT NULL, # Email address associated with the order
  `OrderNumber` varchar(128) DEFAULT NULL, # Shopify order number
  `OrderLevelDiscount` decimal(15,2) DEFAULT NULL, # Discount applied at the order-level
  `SubTotal` decimal(15,2) DEFAULT NULL, # Order subtotal
  `TotalRefund` decimal(15,2) DEFAULT NULL,	# Total of refunds issued for this order
  `TaxRefundAmount` decimal(15,2) DEFAULT NULL, # Total of taxes refunded for this order
  `ShippingRefundAmount` decimal(15,2) DEFAULT NULL, # Total of shipping fees refunded for this order
  `FinancialStatus` varchar(25) DEFAULT NULL, # Financial settlement status of this order
  `Tags` varchar(500) DEFAULT NULL, # Tags for this order
  `CreatedAt` timestamp NOT NULL, # Date-time when this order was created
  `UpdatedAt` timestamp NOT NULL, # Date-time when this order was last updated in Shopify
  PRIMARY KEY  (`PwShopId`, `ShopifyOrderId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `shopifyorderlineitem` (
  `PwShopId` int(6) unsigned NOT NULL, # ProfitWise's shop identifier
  `ShopifyOrderId` BIGINT NOT NULL, # Shopify identifier for associated order
  `ShopifyOrderLineId` BIGINT NOT NULL, # Shopify identifier for order line item
  `OrderDateTimestamp` timestamp NOT NULL, # Date-time for associated order
  `OrderDate` DATE NOT NULL, # Date-time simplified to Date-only for easy joins
  `PwProductId` BIGINT NOT NULL, # Product (PwProductId) for this line item
  `PwVariantId` BIGINT NOT NULL, # Variant (PwVariantId) for this line item
  `Quantity` int(6) unsigned NOT NULL, # Quantity for this line item
  `UnitPrice` decimal(15,2) DEFAULT NULL, # Unit price for this line item (for qty 1)
  `TotalDiscount` decimal(15,2) DEFAULT NULL, # Total discount applied to this line item
  `TotalRestockedQuantity` int(6) unsigned NOT NULL, # Total quantity restocked from this line item
  `GrossRevenue` decimal(15,2) DEFAULT NULL, # Gross revenue for this line item (after discounts and refunds)
  
  `UnitCogs` decimal(15,2) DEFAULT NULL,	# Cost of Goods Sold per Unit
  
  PRIMARY KEY  (`PwShopId`, `ShopifyOrderId`, `ShopifyOrderLineId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;




CREATE TABLE `profitwisepicklist` (
  `PwPickListId` BIGINT NOT NULL AUTO_INCREMENT, # Identifier for the picklist containing a list of products to display
  `PwShopId` BIGINT NOT NULL, # ProfitWise's shop identifier
  `CreatedDate` TIMESTAMP NOT NULL, # Date-time when this picklist was created
  PRIMARY KEY (`PwPickListId`)
) ENGINE=InnoDB AUTO_INCREMENT=100001 DEFAULT CHARSET=utf8;

CREATE TABLE `profitwisepicklistmasterproduct` (
	`PwPickListId` BIGINT NOT NULL, # Identifier for the picklist containing a list of products to display
    `PwShopId` BIGINT NOT NULL, # ProfitWise's shop identifier
    `PwMasterProductId` BIGINT NOT NULL, # Product (PwMasterProductId) to include in this picklist
    PRIMARY KEY (`PwPickListId`, `PwShopId`, `PwMasterProductId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

