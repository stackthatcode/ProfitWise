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






-- ### Primary Key corrections

ALTER TABLE [dbo].[profitwiseshop]
DROP CONSTRAINT [PK_profitwiseshop_PwShopId];

ALTER TABLE [dbo].[profitwiseshop]
ADD CONSTRAINT [PK_profitwiseshop_PwShopId] PRIMARY KEY (PwShopId);


ALTER TABLE [dbo].[profitwisemasterproduct]
DROP CONSTRAINT [PK_profitwisemasterproduct_PwMasterProductId];

ALTER TABLE [dbo].[profitwisemasterproduct]
ADD CONSTRAINT [PK_profitwisemasterproduct_PwMasterProductId] PRIMARY KEY (PwMasterProductId);


ALTER TABLE [dbo].[profitwisemasterproduct]
DROP CONSTRAINT [PK_profitwisemasterproduct_PwMasterProductId];

ALTER TABLE [dbo].[profitwisemasterproduct]
ADD CONSTRAINT [PK_profitwisemasterproduct_PwMasterProductId] PRIMARY KEY (PwMasterProductId);


ALTER TABLE [dbo].[profitwisemastervariant]
DROP CONSTRAINT [PK_profitwisemastervariant_PwMasterVariantId];

ALTER TABLE [dbo].[profitwisemastervariant]
ADD CONSTRAINT [PK_profitwisemastervariant_PwMasterVariantId] PRIMARY KEY (PwMasterVariantId);


ALTER TABLE [dbo].[profitwisevariant]
DROP CONSTRAINT [PK_profitwisevariant_PwVariantId];

ALTER TABLE [dbo].[profitwisevariant]
ADD CONSTRAINT [PK_profitwisevariant_PwVariantId] PRIMARY KEY (PwVariantId);


ALTER TABLE [dbo].[profitwiseproduct]
DROP CONSTRAINT [PK_profitwiseproduct_PwProductId];

ALTER TABLE [dbo].[profitwiseproduct]
ADD CONSTRAINT [PK_profitwiseproduct_PwProductId] PRIMARY KEY (PwProductId);




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
ADD CONSTRAINT FK_profitwisepicklistmasterproduct_PwShopId
FOREIGN KEY (PwShopId) REFERENCES [dbo].[profitwiseshop](PwShopId);

ALTER TABLE [dbo].[profitwisepicklistmasterproduct]
ADD CONSTRAINT FK_profitwisepicklistmasterproduct_PwPickListId
FOREIGN KEY (PwPickListId) REFERENCES [dbo].[profitwisepicklist](PwPickListId);

ALTER TABLE [dbo].[profitwisepicklistmasterproduct]
ADD CONSTRAINT FK_profitwisepicklistmasterproduct_PwMasterProductId
FOREIGN KEY (PwMasterProductId) REFERENCES [dbo].[profitwisemasterproduct](PwMasterProductId);



ALTER TABLE [dbo].[profitwiseproduct]
ADD CONSTRAINT FK_profitwiseproduct_PwShopId
FOREIGN KEY (PwShopId) REFERENCES [dbo].[profitwiseshop](PwShopId);

ALTER TABLE [dbo].[profitwiseproduct]
ADD CONSTRAINT FK_profitwiseproduct_PwMasterProductId
FOREIGN KEY (PwMasterProductId) REFERENCES [dbo].[profitwisemasterproduct](PwMasterProductId);



ALTER TABLE [dbo].[profitwisemastervariant]
ADD CONSTRAINT FK_profitwisemastervariant_PwShopId
FOREIGN KEY (PwShopId) REFERENCES [dbo].[profitwiseshop](PwShopId);

ALTER TABLE [dbo].[profitwisemastervariant]
ADD CONSTRAINT FK_profitwisemastervariant_PwMasterProductId
FOREIGN KEY (PwMasterProductId) REFERENCES [dbo].[profitwisemasterproduct](PwMasterProductId);

ALTER TABLE [dbo].[profitwisemastervariant]
ADD CONSTRAINT FK_profitwisemastervariant_CogsCurrencyId
FOREIGN KEY (CogsCurrencyId) REFERENCES [dbo].[currency](CurrencyId);



ALTER TABLE [dbo].[profitwisemastervariantcogsdetail]
ADD CONSTRAINT FK_profitwisemastervariantcogsdetail_PwShopId
FOREIGN KEY (PwShopId) REFERENCES [dbo].[profitwiseshop](PwShopId);

ALTER TABLE [dbo].[profitwisemastervariantcogsdetail]
ADD CONSTRAINT FK_profitwisemastervariantcogsdetail_PwMasterVariantId
FOREIGN KEY (PwMasterVariantId) REFERENCES [dbo].[profitwisemastervariant](PwMasterVariantId);

ALTER TABLE [dbo].[profitwisemastervariantcogsdetail]
ADD CONSTRAINT FK_profitwisemastervariantcogsdetail_CogsCurrencyId
FOREIGN KEY (CogsCurrencyId) REFERENCES [dbo].[currency](CurrencyId);



ALTER TABLE [dbo].[profitwisemastervariantcogscalc]
ADD CONSTRAINT FK_profitwisemastervariantcogscalc_PwShopId
FOREIGN KEY (PwShopId) REFERENCES [dbo].[profitwiseshop](PwShopId);

ALTER TABLE [dbo].[profitwisemastervariantcogscalc]
ADD CONSTRAINT FK_profitwisemastervariantcogscalc_PwMasterVariantId
FOREIGN KEY (PwMasterVariantId) REFERENCES [dbo].[profitwisemastervariant](PwMasterVariantId);

ALTER TABLE [dbo].[profitwisemastervariantcogscalc]
ADD CONSTRAINT FK_profitwisemastervariantcogscalc_SourceCurrencyId
FOREIGN KEY (SourceCurrencyId) REFERENCES [dbo].[currency](CurrencyId);



ALTER TABLE [dbo].[profitwisevariant]
ADD CONSTRAINT FK_profitwisevariant_PwShopId
FOREIGN KEY (PwShopId) REFERENCES [dbo].[profitwiseshop](PwShopId);

ALTER TABLE [dbo].[profitwisevariant]
ADD CONSTRAINT FK_profitwisevariant_PwMasterVariantId
FOREIGN KEY (PwMasterVariantId) REFERENCES [dbo].[profitwisemastervariant](PwMasterVariantId);

ALTER TABLE [dbo].[profitwisevariant]
ADD CONSTRAINT FK_profitwisevariant_PwProductId
FOREIGN KEY (PwProductId) REFERENCES [dbo].[profitwiseproduct](PwProductId);
