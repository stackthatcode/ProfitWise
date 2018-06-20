USE ProfitWise;
GO


IF EXISTS ( SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME = 'FK_profitwiseupload_PwShopId' )
BEGIN
	ALTER TABLE [dbo].[profitwiseuploads] DROP CONSTRAINT [FK_profitwiseupload_PwShopId];
END

GO
IF (EXISTS ( SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'profitwiseuploads' ))
BEGIN
	DROP TABLE [profitwiseuploads]
END


CREATE TABLE [profitwiseuploads]
( 
	FileUploadId BIGINT NOT NULL IDENTITY(1, 1),
	PwShopId BIGINT NOT NULL,
	FileLockerId UNIQUEIDENTIFIER NOT NULL,
	UploadFileName VARCHAR(100) NOT NULL,
	FeedbackFileName VARCHAR(100) NULL,
	UploadStatus INT NOT NULL,
	DateCreated DATETIME NOT NULL,
	LastUpdated DATETIME NOT NULL,
	TotalNumberOfRows INT NULL,
	RowsProcessed INT NULL,
	PRIMARY KEY (FileUploadId)
);

ALTER TABLE [profitwiseuploads]  
	ADD CONSTRAINT [FK_profitwiseupload_PwShopId]
	FOREIGN KEY (PwShopId) REFERENCES profitwiseshop (PwShopId);



