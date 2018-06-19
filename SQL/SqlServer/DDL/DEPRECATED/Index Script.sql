
-- **** profitwisevariant Index Scripting

IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_Variant_PwMasterVariantId')   
    DROP INDEX IX_Variant_PwMasterVariantId ON profitwisevariant;   
GO  
CREATE NONCLUSTERED INDEX IX_Variant_PwMasterVariantId   
    ON profitwisevariant (PwMasterVariantId ASC);   
GO 

IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_Variant_IsPrimary')   
    DROP INDEX IX_Variant_IsPrimary ON profitwisevariant;   
GO  
CREATE NONCLUSTERED INDEX IX_Variant_IsPrimary   
    ON profitwisevariant (PwShopId, PwMasterVariantId) INCLUDE (IsPrimary);
GO 

-- Covering index for commonly accessed fields
IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_Variant_RetrieveVariants')   
    DROP INDEX IX_Variant_RetrieveVariants ON profitwisevariant;   
GO  
CREATE NONCLUSTERED INDEX IX_Variant_RetrieveVariants   
    ON profitwisevariant (PwShopId, IsPrimary) INCLUDE (PwProductId, PwVariantId, PwMasterVariantId, Sku, Title);
GO 

IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_Variant_PwProductId_And_PwMasterVariantId')   
    DROP INDEX IX_Variant_PwProductId_And_PwMasterVariantId ON profitwisevariant;   
GO  
CREATE NONCLUSTERED INDEX IX_Variant_PwProductId_And_PwMasterVariantId   
    ON profitwisevariant (PwShopId, IsPrimary) INCLUDE ( PwProductId, PwMasterVariantId )
GO 

-- This serves the Inventory Valuation Report query
IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_Variant_IsActive_And_Inventory')   
    DROP INDEX IX_Variant_IsActive_And_Inventory ON profitwisevariant;   
GO  
CREATE NONCLUSTERED INDEX IX_Variant_IsActive_And_Inventory   
    ON profitwisevariant (PwShopId, IsActive, Inventory) INCLUDE ( PwMasterVariantId, PwProductId, HighPrice )
GO 




-- **** profitwiseproduct Index Scripting

IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_Product_PwMasterProductId')   
    DROP INDEX IX_Product_PwMasterProductId ON profitwiseproduct;   
GO  
CREATE NONCLUSTERED INDEX IX_Product_PwMasterProductId   
    ON profitwiseproduct (PwMasterProductId)
GO 

IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_Product_Vendor')   
    DROP INDEX IX_Product_Vendor ON profitwiseproduct;   
GO  
CREATE NONCLUSTERED INDEX IX_Product_Vendor   
    ON profitwiseproduct (Vendor)
GO 

IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_Product_ProductType')   
    DROP INDEX IX_Product_ProductType ON profitwiseproduct;   
GO  
CREATE NONCLUSTERED INDEX IX_Product_ProductType   
    ON profitwiseproduct (ProductType)
GO 

IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_Product_IsPrimary')   
    DROP INDEX IX_Product_IsPrimary ON profitwiseproduct;   
GO  
CREATE NONCLUSTERED INDEX IX_Product_IsPrimary   
    ON profitwiseproduct (IsPrimary)
GO

IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_Product_Title_And_Vendor')   
    DROP INDEX IX_Product_Title_And_Vendor ON profitwiseproduct;   
GO  
CREATE NONCLUSTERED INDEX IX_Product_Title_And_Vendor   
    ON profitwiseproduct (PwMasterProductId, IsPrimary) INCLUDE (Title, Vendor)
GO






-- *** profitwisemastervariant Index Scripting
IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_MasterVariant_PwMasterProductId')   
    DROP INDEX IX_MasterVariant_PwMasterProductId ON profitwisemastervariant;   
GO  
CREATE NONCLUSTERED INDEX IX_MasterVariant_PwMasterProductId   
    ON profitwisemastervariant (PwMasterProductId)
GO 




-- *** shopifyorderlineitem Indexes

-- This serves the Inventory Valuation Report query
IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_OrderLineItem_PickListUpdate')   
    DROP INDEX IX_OrderLineItem_PickListUpdate ON shopifyorderlineitem;   
GO  
CREATE NONCLUSTERED INDEX IX_OrderLineItem_PickListUpdate   
    ON shopifyorderlineitem (PwShopId) INCLUDE ( ShopifyOrderId, ShopifyOrderLineId, OrderDate, PwProductId, PwVariantId, UnitPrice )
GO 


IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_OrderLineItem_LedgerUpdate')   
    DROP INDEX IX_OrderLineItem_LedgerUpdate ON shopifyorderlineitem;   
GO  
CREATE NONCLUSTERED INDEX IX_OrderLineItem_LedgerUpdate   
    ON shopifyorderlineitem (PwShopId) INCLUDE ( ShopifyOrderId, ShopifyOrderLineId, UnitCogs, PwVariantId, FinancialStatus )
GO 




-- *** profitwiseprofitreportentry Indexes

-- This serves the Inventory Valuation Report query
IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_ProfitReportEntry_UpdateIndex')   
    DROP INDEX IX_ProfitReportEntry_UpdateIndex ON shopifyorderlineitem;   
GO  
CREATE NONCLUSTERED INDEX IX_ProfitReportEntry_UpdateIndex   
    ON profitwiseprofitreportentry (PwShopId) INCLUDE ( EntryDate, EntryType, ShopifyOrderId, SourceId, Quantity )
GO 







-- Are these truly necessary...? We have a ton of database writes...
/*
IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_Variant_PwMasterVariantId_And_HighLowPrice_AndInventory')   
    DROP INDEX IX_Variant_PwMasterVariantId_And_HighLowPrice_AndInventory ON profitwisevariant;   
GO  
CREATE NONCLUSTERED INDEX IX_Variant_PwMasterVariantId_And_HighLowPrice_AndInventory   
    ON profitwisevariant (PwShopId) INCLUDE ( PwMasterVariantId, HighPrice, LowPrice, Inventory )
GO 
IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_Variant_PwMasterVariantId_And_Sku_And_Title')   
    DROP INDEX IX_Variant_PwMasterVariantId_And_Sku_And_Title ON profitwisevariant;   
GO  
CREATE NONCLUSTERED INDEX IX_Variant_PwMasterVariantId_And_Sku_And_Title   
    ON profitwisevariant (PwShopId, PwProductId, IsPrimary) INCLUDE ( PwMasterVariantId, Sku, Title )
GO 
*/

/**** TODO - Revisit
IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_Product_Tags')   
    DROP INDEX IX_Product_Tags ON profitwiseproduct;   
GO  
CREATE NONCLUSTERED INDEX IX_Product_Tags   
    ON profitwiseproduct (Tags)
GO 
*/



