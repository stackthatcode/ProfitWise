USE ProfitWise
GO



-- ### ASP.NET Identity tables

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_AspNetUserRoles_RoleId' )
BEGIN
	ALTER TABLE [dbo].[AspNetUserRoles]
	DROP CONSTRAINT FK_AspNetUserRoles_RoleId;
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_AspNetUserRoles_UserId' )
BEGIN
	ALTER TABLE [dbo].[AspNetUserRoles]
	DROP CONSTRAINT FK_AspNetUserRoles_UserId;
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_AspNetUserClaims_UserId' )
BEGIN
	ALTER TABLE [dbo].[AspNetUserClaims]
	DROP CONSTRAINT FK_AspNetUserClaims_UserId;
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_AspNetUserLogins_UserId' )
BEGIN
	ALTER TABLE [dbo].[AspNetUserLogins]
	DROP CONSTRAINT FK_AspNetUserLogins_UserId;
END



ALTER TABLE [dbo].[AspNetUserRoles]
ADD CONSTRAINT FK_AspNetUserRoles_RoleId
FOREIGN KEY (RoleId) REFERENCES [dbo].[AspNetRoles](Id);

ALTER TABLE [dbo].[AspNetUserRoles]
ADD CONSTRAINT FK_AspNetUserRoles_UserId
FOREIGN KEY (UserId) REFERENCES [dbo].[AspNetUsers](Id);

ALTER TABLE [dbo].[AspNetUserClaims]
ADD CONSTRAINT FK_AspNetUserClaims_UserId
FOREIGN KEY (UserId) REFERENCES [dbo].[AspNetUsers](Id);

ALTER TABLE [dbo].[AspNetUserLogins]
ADD CONSTRAINT FK_AspNetUserLogins_UserId
FOREIGN KEY (UserId) REFERENCES [dbo].[AspNetUsers](Id);





-- ### Foreign Key DROP scripts for ProfitWise tables

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_exchangerate_SourceCurrencyId' )
BEGIN
	ALTER TABLE [dbo].[exchangerate]
	DROP CONSTRAINT FK_exchangerate_SourceCurrencyId;
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_exchangerate_DestinationCurrencyId' )
BEGIN
	ALTER TABLE [dbo].[exchangerate]
	DROP CONSTRAINT FK_exchangerate_DestinationCurrencyId;
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwiseshop_UserId' )
BEGIN
	ALTER TABLE [dbo].[profitwiseshop]
	DROP CONSTRAINT FK_profitwiseshop_UserId;
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwisebatchstate_PwShopId' )
BEGIN
	ALTER TABLE [dbo].[profitwisebatchstate]
	DROP CONSTRAINT FK_profitwisebatchstate_PwShopId;
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwiserecurringcharge_PwShopId' )
BEGIN
	ALTER TABLE [dbo].[profitwiserecurringcharge]
	DROP CONSTRAINT FK_profitwiserecurringcharge_PwShopId;
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwisetour_PwShopId' )
BEGIN
	ALTER TABLE [dbo].[profitwisetour]
	DROP CONSTRAINT FK_profitwisetour_PwShopId;
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwisemasterproduct_PwShopId' )
BEGIN
	ALTER TABLE [dbo].[profitwisemasterproduct]
	DROP CONSTRAINT FK_profitwisemasterproduct_PwShopId;
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwisepicklist_PwShopId' )
BEGIN
	ALTER TABLE [dbo].[profitwisepicklist]
	DROP CONSTRAINT FK_profitwisepicklist_PwShopId;
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwisepicklistmasterproduct_PwShopId' )
BEGIN
	ALTER TABLE [dbo].[profitwisepicklistmasterproduct]
	DROP CONSTRAINT FK_profitwisepicklistmasterproduct_PwShopId;
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwisepicklistmasterproduct_PwPickListId' )
BEGIN
	ALTER TABLE [dbo].[profitwisepicklistmasterproduct]
	DROP CONSTRAINT FK_profitwisepicklistmasterproduct_PwPickListId;
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwisepicklistmasterproduct_PwMasterProductId' )
BEGIN
	ALTER TABLE [dbo].[profitwisepicklistmasterproduct]
	DROP CONSTRAINT FK_profitwisepicklistmasterproduct_PwMasterProductId;
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwiseproduct_PwShopId' )
BEGIN
	ALTER TABLE [dbo].[profitwiseproduct]
	DROP CONSTRAINT FK_profitwiseproduct_PwShopId;
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwiseproduct_PwMasterProductId' )
BEGIN
	ALTER TABLE [dbo].[profitwiseproduct]
	DROP CONSTRAINT FK_profitwiseproduct_PwMasterProductId;
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwisemastervariant_PwShopId' )
BEGIN
	ALTER TABLE [dbo].[profitwisemastervariant]
	DROP CONSTRAINT FK_profitwisemastervariant_PwShopId;
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwisemastervariant_PwMasterProductId' )
BEGIN
	ALTER TABLE [dbo].[profitwisemastervariant]
	DROP CONSTRAINT FK_profitwisemastervariant_PwMasterProductId;
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwisemastervariant_CogsCurrencyId' )
BEGIN
	ALTER TABLE [dbo].[profitwisemastervariant]
	DROP CONSTRAINT FK_profitwisemastervariant_CogsCurrencyId;
END


IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwisemastervariantcogsdetail_PwShopId' )
BEGIN
	ALTER TABLE [dbo].[profitwisemastervariantcogsdetail]
	DROP CONSTRAINT FK_profitwisemastervariantcogsdetail_PwShopId
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwisemastervariantcogsdetail_PwMasterVariantId' )
BEGIN
	ALTER TABLE [dbo].[profitwisemastervariantcogsdetail]
	DROP CONSTRAINT FK_profitwisemastervariantcogsdetail_PwMasterVariantId
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwisemastervariantcogsdetail_CogsCurrencyId' )
BEGIN
	ALTER TABLE [dbo].[profitwisemastervariantcogsdetail]
	DROP CONSTRAINT FK_profitwisemastervariantcogsdetail_CogsCurrencyId
END


IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwisemastervariantcogscalc_PwShopId' )
BEGIN
	ALTER TABLE [dbo].[profitwisemastervariantcogscalc]
	DROP CONSTRAINT FK_profitwisemastervariantcogscalc_PwShopId
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwisemastervariantcogscalc_PwMasterVariantId' )
BEGIN
	ALTER TABLE [dbo].[profitwisemastervariantcogscalc]
	DROP CONSTRAINT FK_profitwisemastervariantcogscalc_PwMasterVariantId
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwisemastervariantcogscalc_SourceCurrencyId' )
BEGIN
	ALTER TABLE [dbo].[profitwisemastervariantcogscalc]
	DROP CONSTRAINT FK_profitwisemastervariantcogscalc_SourceCurrencyId
END


IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwisevariant_PwShopId' )
BEGIN
	ALTER TABLE [dbo].[profitwisevariant]
	DROP CONSTRAINT FK_profitwisevariant_PwShopId
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwisevariant_PwMasterVariantId' )
BEGIN
	ALTER TABLE [dbo].[profitwisevariant]
	DROP CONSTRAINT FK_profitwisevariant_PwMasterVariantId
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwisevariant_PwProductId' )
BEGIN
	ALTER TABLE [dbo].[profitwisevariant]
	DROP CONSTRAINT FK_profitwisevariant_PwProductId
END



IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_shopifyorder_PwShopId' )
BEGIN
	ALTER TABLE [dbo].[shopifyorder]
	DROP CONSTRAINT FK_shopifyorder_PwShopId
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_shopifyorderlineitem_PwShopId' )
BEGIN
	ALTER TABLE [dbo].[shopifyorderlineitem]
	DROP CONSTRAINT FK_shopifyorderlineitem_PwShopId
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_shopifyorderlineitem_PwShopId_And_ShopifyOrderId' )
BEGIN
	ALTER TABLE [dbo].[shopifyorderlineitem]
	DROP CONSTRAINT FK_shopifyorderlineitem_PwShopId_And_ShopifyOrderId
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_shopifyorderlineitem_PwProductId' )
BEGIN
	ALTER TABLE [dbo].[shopifyorderlineitem]
	DROP CONSTRAINT FK_shopifyorderlineitem_PwProductId
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_shopifyorderlineitem_PwVariantId' )
BEGIN
	ALTER TABLE [dbo].[shopifyorderlineitem]
	DROP CONSTRAINT FK_shopifyorderlineitem_PwVariantId
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_shopifyorderadjustment_PwShopId' )
BEGIN
	ALTER TABLE [dbo].[shopifyorderadjustment]
	DROP CONSTRAINT FK_shopifyorderadjustment_PwShopId
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_shopifyorderadjustment_PwShopId_And_ShopifyOrderId' )
BEGIN
	ALTER TABLE [dbo].[shopifyorderadjustment]
	DROP CONSTRAINT FK_shopifyorderadjustment_PwShopId_And_ShopifyOrderId
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_shopifyorderrefund_PwShopId' )
BEGIN
	ALTER TABLE [dbo].[shopifyorderrefund]
	DROP CONSTRAINT FK_shopifyorderrefund_PwShopId
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_shopifyorderrefund_PwShopId_And_ShopifyOrderId' )
BEGIN
	ALTER TABLE [dbo].[shopifyorderrefund]
	DROP CONSTRAINT FK_shopifyorderrefund_PwShopId_And_ShopifyOrderId
END


IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwiseprofitreportentry_PwShopId' )
BEGIN
	ALTER TABLE [dbo].[profitwiseprofitreportentry]
	DROP CONSTRAINT FK_profitwiseprofitreportentry_PwShopId
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwiseprofitreportentry_ShopifyOrderId' )
BEGIN
	ALTER TABLE [dbo].[profitwiseprofitreportentry]
	DROP CONSTRAINT FK_profitwiseprofitreportentry_ShopifyOrderId
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwiseprofitreportentry_PwVariantId' )
BEGIN
	ALTER TABLE [dbo].[profitwiseprofitreportentry]
	DROP CONSTRAINT FK_profitwiseprofitreportentry_PwVariantId
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwisereport_PwShopId' )
BEGIN
	ALTER TABLE [dbo].[profitwisereport]
	DROP CONSTRAINT FK_profitwisereport_PwShopId
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwisereportfilter_PwShopId' )
BEGIN
	ALTER TABLE [dbo].[profitwisereportfilter]
	DROP CONSTRAINT FK_profitwisereportfilter_PwShopId
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwiseprofitquerystub_PwReportId' )
BEGIN
	ALTER TABLE [dbo].[profitwiseprofitquerystub]
	DROP CONSTRAINT FK_profitwiseprofitquerystub_PwReportId
END

IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_profitwisegoodsonhandquerystub_PwReportId' )
BEGIN
	ALTER TABLE [dbo].[profitwisegoodsonhandquerystub]
	DROP CONSTRAINT FK_profitwisegoodsonhandquerystub_PwReportId
END





-- ### Old Table clean-up vendetta
IF OBJECT_ID('dbo.profitwisemasterproduct_import', 'U') IS NOT NULL 
  DROP TABLE dbo.profitwisemasterproduct_import; 

  
DROP FUNCTION IF EXISTS dbo.reportquerystub
GO

DROP TABLE IF EXISTS profitwisereportquerystub
GO




-- ### Primary Key corrections

ALTER TABLE [dbo].[profitwiseshop]
DROP CONSTRAINT [PK_profitwiseshop_PwShopId];

ALTER TABLE [dbo].[profitwiseshop]
ADD CONSTRAINT [PK_profitwiseshop_PwShopId] PRIMARY KEY (PwShopId);


-- ### Temporary fixes for DEV - yah, go figure!
ALTER TABLE [dbo].[profitwisemasterproduct]
DROP CONSTRAINT [PK_profitwisemasterproduct_PwMasterProductId];

ALTER TABLE [dbo].[profitwisemasterproduct]
ADD CONSTRAINT [PK_profitwisemasterproduct_PwMasterProductId] PRIMARY KEY (PwMasterProductId, PwShopId);


ALTER TABLE [dbo].[profitwisepicklist]
DROP CONSTRAINT [PK_profitwisepicklist_PwPickListId];

ALTER TABLE [dbo].[profitwisepicklist]
ADD CONSTRAINT [PK_profitwisepicklist_PwPickListId] PRIMARY KEY (PwPickListId, PwShopId);


ALTER TABLE [dbo].[profitwisemastervariant]
DROP CONSTRAINT [PK_profitwisemastervariant_PwMasterVariantId];

ALTER TABLE [dbo].[profitwisemastervariant]
ADD CONSTRAINT [PK_profitwisemastervariant_PwMasterVariantId] PRIMARY KEY (PwMasterVariantId, PwShopId);


ALTER TABLE [dbo].[profitwisevariant]
DROP CONSTRAINT [PK_profitwisevariant_PwVariantId];

ALTER TABLE [dbo].[profitwisevariant]
ADD CONSTRAINT [PK_profitwisevariant_PwVariantId] PRIMARY KEY (PwVariantId, PwShopId);


ALTER TABLE [dbo].[profitwiseproduct]
DROP CONSTRAINT [PK_profitwiseproduct_PwProductId];

ALTER TABLE [dbo].[profitwiseproduct]
ADD CONSTRAINT [PK_profitwiseproduct_PwProductId] PRIMARY KEY (PwProductId, PwShopId);



ALTER TABLE [dbo].[profitwisereport]
DROP CONSTRAINT [PK_profitwisereport_PwReportId];

ALTER TABLE [dbo].[profitwisereport]
ADD CONSTRAINT [PK_profitwisereport_PwReportId] PRIMARY KEY (PwReportId, PwShopId);


ALTER TABLE [dbo].[profitwisereportfilter]
DROP CONSTRAINT [PK_profitwisereportfilter_PwReportId];

ALTER TABLE [dbo].[profitwisereportfilter]
ADD CONSTRAINT [PK_profitwisereportfilter_PwReportId] PRIMARY KEY (PwReportId, PwShopId, PwFilterId);



/*
ALTER TABLE [dbo].[profitwiseprofitquerystub]
DROP CONSTRAINT [PK_profitwiseprofitquerystub_PwReportId];

ALTER TABLE [dbo].[profitwiseprofitquerystub]
ADD CONSTRAINT [PK_profitwiseprofitquerystub_PwReportId] PRIMARY KEY (PwReportId, PwFilterId);
*/



-- ## Currency system tables

ALTER TABLE [dbo].[exchangerate]
ADD CONSTRAINT FK_exchangerate_SourceCurrencyId
FOREIGN KEY (SourceCurrencyId) REFERENCES [dbo].[currency](CurrencyId);

ALTER TABLE [dbo].[exchangerate]
ADD CONSTRAINT FK_exchangerate_DestinationCurrencyId
FOREIGN KEY (DestinationCurrencyId) REFERENCES [dbo].[currency](CurrencyId);




-- ### Shop tables ###

ALTER TABLE [dbo].[profitwiseshop]
ADD CONSTRAINT FK_profitwiseshop_UserId
FOREIGN KEY (ShopOwnerUserId) REFERENCES [dbo].[AspNetUsers](Id);

ALTER TABLE [dbo].[profitwisebatchstate]
ADD CONSTRAINT FK_profitwisebatchstate_PwShopId
FOREIGN KEY (PwShopId) REFERENCES [dbo].[profitwiseshop](PwShopId);

ALTER TABLE [dbo].[profitwiserecurringcharge]
ADD CONSTRAINT FK_profitwiserecurringcharge_PwShopId
FOREIGN KEY (PwShopId) REFERENCES [dbo].[profitwiseshop](PwShopId);

ALTER TABLE [dbo].[profitwisetour]
ADD CONSTRAINT FK_profitwisetour_PwShopId
FOREIGN KEY (PwShopId) REFERENCES [dbo].[profitwiseshop](PwShopId);



-- ### Product and Variant tables

ALTER TABLE [dbo].[profitwisemasterproduct]
ADD CONSTRAINT FK_profitwisemasterproduct_PwShopId
FOREIGN KEY (PwShopId) REFERENCES [dbo].[profitwiseshop](PwShopId);

ALTER TABLE [dbo].[profitwisepicklist]
ADD CONSTRAINT FK_profitwisepicklist_PwShopId
FOREIGN KEY (PwShopId) REFERENCES [dbo].[profitwiseshop](PwShopId);



-- *** Data clean-up!
DELETE FROM [profitwisepicklistmasterproduct] WHERE PwMasterProductId NOT IN (SELECT PwMasterProductId FROM profitwisemasterproduct);

ALTER TABLE [dbo].[profitwisepicklistmasterproduct]
ADD CONSTRAINT FK_profitwisepicklistmasterproduct_PwPickListId
FOREIGN KEY (PwPickListId, PwShopId) REFERENCES [dbo].[profitwisepicklist](PwPickListId, PwShopId);

ALTER TABLE [dbo].[profitwisepicklistmasterproduct]
ADD CONSTRAINT FK_profitwisepicklistmasterproduct_PwMasterProductId
FOREIGN KEY (PwMasterProductId, PwShopId) REFERENCES [dbo].[profitwisemasterproduct](PwMasterProductId, PwShopId);



ALTER TABLE [dbo].[profitwiseproduct]
ADD CONSTRAINT FK_profitwiseproduct_PwMasterProductId
FOREIGN KEY (PwMasterProductId, PwShopId) REFERENCES [dbo].[profitwisemasterproduct](PwMasterProductId, PwShopId);


ALTER TABLE [dbo].[profitwisemastervariant]
ADD CONSTRAINT FK_profitwisemastervariant_PwShopId
FOREIGN KEY (PwShopId) REFERENCES [dbo].[profitwiseshop](PwShopId);

ALTER TABLE [dbo].[profitwisemastervariant]
ADD CONSTRAINT FK_profitwisemastervariant_PwMasterProductId
FOREIGN KEY (PwMasterProductId, PwShopId) REFERENCES [dbo].[profitwisemasterproduct](PwMasterProductId, PwShopId);

ALTER TABLE [dbo].[profitwisemastervariant]
ADD CONSTRAINT FK_profitwisemastervariant_CogsCurrencyId
FOREIGN KEY (CogsCurrencyId) REFERENCES [dbo].[currency](CurrencyId);



ALTER TABLE [dbo].[profitwisemastervariantcogsdetail]
ADD CONSTRAINT FK_profitwisemastervariantcogsdetail_PwMasterVariantId
FOREIGN KEY (PwMasterVariantId, PwShopId) REFERENCES [dbo].[profitwisemastervariant](PwMasterVariantId, PwShopId);

ALTER TABLE [dbo].[profitwisemastervariantcogsdetail]
ADD CONSTRAINT FK_profitwisemastervariantcogsdetail_CogsCurrencyId
FOREIGN KEY (CogsCurrencyId) REFERENCES [dbo].[currency](CurrencyId);



ALTER TABLE [dbo].[profitwisemastervariantcogscalc]
ADD CONSTRAINT FK_profitwisemastervariantcogscalc_PwMasterVariantId
FOREIGN KEY (PwMasterVariantId, PwShopId) REFERENCES [dbo].[profitwisemastervariant](PwMasterVariantId, PwShopId);

ALTER TABLE [dbo].[profitwisemastervariantcogscalc]
ADD CONSTRAINT FK_profitwisemastervariantcogscalc_SourceCurrencyId
FOREIGN KEY (SourceCurrencyId) REFERENCES [dbo].[currency](CurrencyId);



ALTER TABLE [dbo].[profitwisevariant]
ADD CONSTRAINT FK_profitwisevariant_PwShopId
FOREIGN KEY (PwShopId) REFERENCES [dbo].[profitwiseshop](PwShopId);

ALTER TABLE [dbo].[profitwisevariant]
ADD CONSTRAINT FK_profitwisevariant_PwMasterVariantId
FOREIGN KEY (PwMasterVariantId, PwShopId) REFERENCES [dbo].[profitwisemastervariant](PwMasterVariantId, PwShopId);

ALTER TABLE [dbo].[profitwisevariant]
ADD CONSTRAINT FK_profitwisevariant_PwProductId
FOREIGN KEY (PwProductId, PwShopId) REFERENCES [dbo].[profitwiseproduct](PwProductId, PwShopId);




-- ### Shopify Order, Line Item, Adjustment and Refund tables

ALTER TABLE [dbo].[shopifyorder]
ADD CONSTRAINT FK_shopifyorder_PwShopId
FOREIGN KEY (PwShopId) REFERENCES [dbo].[profitwiseshop](PwShopId);


ALTER TABLE [dbo].[shopifyorderlineitem]
ADD CONSTRAINT FK_shopifyorderlineitem_PwShopId_And_ShopifyOrderId
FOREIGN KEY (PwShopId, ShopifyOrderId) REFERENCES [dbo].[shopifyorder](PwShopId, ShopifyOrderId);

ALTER TABLE [dbo].[shopifyorderlineitem]
ADD CONSTRAINT FK_shopifyorderlineitem_PwProductId
FOREIGN KEY (PwProductId, PwShopId) REFERENCES [dbo].[profitwiseproduct](PwProductId, PwShopId);

ALTER TABLE [dbo].[shopifyorderlineitem]
ADD CONSTRAINT FK_shopifyorderlineitem_PwVariantId
FOREIGN KEY (PwVariantId, PwShopId) REFERENCES [dbo].[profitwisevariant](PwVariantId, PwShopId);


ALTER TABLE [dbo].[shopifyorderadjustment]
ADD CONSTRAINT FK_shopifyorderadjustment_PwShopId_And_ShopifyOrderId
FOREIGN KEY (PwShopId, ShopifyOrderId) REFERENCES [dbo].[shopifyorder](PwShopId, ShopifyOrderId);


ALTER TABLE [dbo].[shopifyorderrefund]
ADD CONSTRAINT FK_shopifyorderrefund_PwShopId_And_ShopifyOrderId
FOREIGN KEY (PwShopId, ShopifyOrderId) REFERENCES [dbo].[shopifyorder](PwShopId, ShopifyOrderId);


ALTER TABLE [dbo].[profitwiseprofitreportentry]
ADD CONSTRAINT FK_profitwiseprofitreportentry_ShopifyOrderId
FOREIGN KEY (PwShopId, ShopifyOrderId) REFERENCES [dbo].[shopifyorder](PwShopId, ShopifyOrderId);

ALTER TABLE [dbo].[profitwiseprofitreportentry]
ADD CONSTRAINT FK_profitwiseprofitreportentry_PwVariantId
FOREIGN KEY (PwVariantId, PwShopId) REFERENCES [dbo].[profitwisevariant](PwVariantId, PwShopId);




-- ## Report tables

ALTER TABLE [dbo].[profitwisereport]
ADD CONSTRAINT FK_profitwisereport_PwShopId
FOREIGN KEY (PwShopId) REFERENCES [dbo].[profitwiseshop](PwShopId);

ALTER TABLE [dbo].[profitwisereportfilter]
ADD CONSTRAINT FK_profitwisereportfilter_PwShopId
FOREIGN KEY (PwReportId, PwShopId) REFERENCES [dbo].[profitwisereport](PwReportId, PwShopId);


ALTER TABLE [dbo].[profitwiseprofitquerystub]
ADD CONSTRAINT FK_profitwiseprofitquerystub_PwReportId
FOREIGN KEY (PwReportId, PwShopId) REFERENCES [dbo].[profitwisereport](PwReportId, PwShopId);

-- Clean-up of old data
DELETE FROM [profitwisegoodsonhandquerystub] WHERE PwReportId NOT IN (SELECT PwReportId FROM profitwisereport);

ALTER TABLE [dbo].[profitwisegoodsonhandquerystub]
ADD CONSTRAINT FK_profitwisegoodsonhandquerystub_PwReportId
FOREIGN KEY (PwReportId, PwShopId) REFERENCES [dbo].[profitwisereport](PwReportId, PwShopId);


