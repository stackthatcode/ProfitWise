USE ProfitWise;
GO


IF (EXISTS ( SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'systemwebhookinvocations' ))
BEGIN
	DROP TABLE systemwebhookinvocations
END

CREATE TABLE systemwebhookinvocations
( 	
	Id BIGINT NOT NULL IDENTITY(1, 1),
	Topic VARCHAR(100) NOT NULL,
	BodyText VARCHAR(MAX) NOT NULL,
	DateCreated DATETIME NOT NULL,
	Handled BIT NOT NULL
	PRIMARY KEY (Id)
);

-- SELECT * FROM systemwebhookinvocations;
---SELECT GETUTCDATE();
