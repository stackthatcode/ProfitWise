USE ProfitWise
GO

SELECT * FROM AspNetUsers

-- The same as External Login
SELECT * FROM AspNetUserLogins

SELECT * FROM AspNetRoles

SELECT * FROM AspNetUserClaims

SELECT * FROM AspNetUserRoles



/*
DECLARE @UserID varchar(255) = '41d73e38-1302-4a4b-8fea-c046898b3665'

DELETE FROM AspNetUserClaims WHERE UserId = @UserID

DELETE FROM AspNetUserLogins WHERE UserId = @UserID

DELETE FROM AspNetUsers WHERE Id = @UserID
*/

/*
DELETE FROM AspNetUserClaims
DELETE FROM AspNetUserLogins
DELETE FROM AspNetUsers
DELETE FROM AspNetRoles
DELETE FROM AspNetUserRoles
*/





