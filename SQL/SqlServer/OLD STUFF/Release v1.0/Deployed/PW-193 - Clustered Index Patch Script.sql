
USE ProfitWise
GO


/*** Patch queries to fix sloppy ProfitWise indexes that make deleting childless Master Products anti-performant ***/


/*** Step 1 - Remove FK references from Product and Master Variant to Master Product ***/

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwisemastervariant_PwMasterProductId' )
BEGIN
	ALTER TABLE profitwisemastervariant
	DROP CONSTRAINT FK_profitwisemastervariant_PwMasterProductId
END
GO

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwiseproduct_PwMasterProductId' )
BEGIN
	ALTER TABLE profitwiseproduct
	DROP CONSTRAINT FK_profitwiseproduct_PwMasterProductId
END
GO



/*** Step 2 - Remove Master Product Primary Key, and add back with composite of PwShopId and PwMasterProductId ***/

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwiseproduct_PwMasterProductId' )
BEGIN
	ALTER TABLE profitwisemasterproduct
	DROP CONSTRAINT PK_profitwisemasterproduct_PwMasterProductId

	ALTER TABLE profitwisemasterproduct
	ADD CONSTRAINT PK_profitwisemasterproduct_PwMasterProductId PRIMARY KEY CLUSTERED (PwShopId, PwMasterProductId);  
END
GO

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='UNIQ_profitwisemasterproduct_PwMasterProductId' )
BEGIN
	ALTER TABLE profitwisemasterproduct
	DROP CONSTRAINT UNIQ_profitwisemasterproduct_PwMasterProductId
END
GO
ALTER TABLE profitwisemasterproduct
ADD CONSTRAINT UNIQ_profitwisemasterproduct_PwMasterProductId UNIQUE(PwMasterProductId);  
GO


/*** Step 3 - rebuild FK references from Product and Master Variant to Master Product ***/

IF NOT EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwisemastervariant_PwMasterProductId' )
BEGIN
	ALTER TABLE [dbo].[profitwisemastervariant]
	ADD CONSTRAINT FK_profitwisemastervariant_PwMasterProductId
	FOREIGN KEY (PwMasterProductId) REFERENCES [dbo].[profitwisemasterproduct](PwMasterProductId);
END
GO

IF NOT EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwiseproduct_PwMasterProductId' )
BEGIN
	ALTER TABLE [dbo].[profitwiseproduct]
	ADD CONSTRAINT FK_profitwiseproduct_PwMasterProductId
	FOREIGN KEY (PwMasterProductId) REFERENCES [dbo].[profitwisemasterproduct](PwMasterProductId);
END
GO



/*** Step 4 - (unrelated) Correction to Product Table  - missing FK to Shop -> Shop Id ***/

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwiseproduct_pwshopid' )
BEGIN
	ALTER TABLE [dbo].[profitwiseproduct]
	DROP CONSTRAINT FK_profitwiseproduct_pwshopid
END

ALTER TABLE [dbo].[profitwiseproduct]
ADD CONSTRAINT FK_profitwiseproduct_pwshopid
FOREIGN KEY (PwShopId) REFERENCES [dbo].[profitwiseshop](PwShopId);




/*** Step 5 - covering index to eliminate Index Scans when deleting childless Master Products ***/

/*** Covering index on Master Product for (Shop) **/
IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_profitwisemasterproduct_PwShopId')   
    DROP INDEX IX_profitwisemasterproduct_PwShopId ON profitwisemasterproduct;   
GO  
CREATE NONCLUSTERED INDEX IX_profitwisemasterproduct_PwShopId   
    ON profitwisemasterproduct (PwShopId)
GO 


/*** Covering indexes on Product of (Shop) and (Shop, MasterProduct) **/
IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_Product_PwShopIdAndMasterProductId')   
    DROP INDEX IX_Product_PwShopIdAndMasterProductId ON profitwiseproduct;   
GO  
CREATE NONCLUSTERED INDEX IX_Product_PwShopIdAndMasterProductId   
    ON profitwiseproduct (PwShopId, PwMasterProductId)
GO

IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_Product_PwMasterProductId')   
    DROP INDEX IX_Product_PwMasterProductId ON profitwiseproduct;   
GO

IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_Product_MasterProductId')   
    DROP INDEX IX_Product_MasterProductId ON profitwiseproduct;   
GO  
CREATE NONCLUSTERED INDEX IX_Product_MasterProductId   
    ON profitwiseproduct (PwMasterProductId)
GO



/*** Covering indexes on Product of (Shop) and (Shop, MasterProduct) **/
-- NOT SURE IF THIS WORKS...?
IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_MasterVariant_PwMasterProductId')   
    DROP INDEX IX_MasterVariant_PwMasterProductId ON profitwisemastervariant;   
GO  
CREATE NONCLUSTERED INDEX IX_MasterVariant_PwMasterProductId   
    ON profitwisemastervariant (PwMasterProductId)
GO 


