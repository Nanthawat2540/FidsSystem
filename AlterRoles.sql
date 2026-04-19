-- ============================================================
-- AlterRoles.sql — Add AIRLINE role support
-- Run AFTER CreateFidsDB.sql + AlterFidsDB.sql
-- ============================================================

USE FidsDB;
GO

-- Add AirlineCode column to AdminUsers
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('AdminUsers') AND name='AirlineCode')
    ALTER TABLE [AdminUsers] ADD [AirlineCode] NVARCHAR(10) NULL;
GO

-- Update Role column to accept AIRLINE
-- (SQL Server NVARCHAR has no enum, just document: ADMIN | STAFF | AIRLINE)

-- Recreate sp_Admin_GetAll to include AirlineCode
IF OBJECT_ID('sp_Admin_GetAll', 'P') IS NOT NULL DROP PROCEDURE sp_Admin_GetAll; GO
CREATE PROCEDURE sp_Admin_GetAll
AS
BEGIN
    SELECT Id, Username, Role, Status, AirlineCode, CreatedAt FROM AdminUsers ORDER BY Id;
END
GO

IF OBJECT_ID('sp_Admin_GetById', 'P') IS NOT NULL DROP PROCEDURE sp_Admin_GetById; GO
CREATE PROCEDURE sp_Admin_GetById @Id INT
AS
BEGIN
    SELECT Id, Username, Role, Status, AirlineCode, CreatedAt FROM AdminUsers WHERE Id = @Id;
END
GO

IF OBJECT_ID('sp_Admin_Create', 'P') IS NOT NULL DROP PROCEDURE sp_Admin_Create; GO
CREATE PROCEDURE sp_Admin_Create
    @Username     NVARCHAR(50),
    @PasswordHash NVARCHAR(MAX),
    @Role         NVARCHAR(20) = 'STAFF',
    @AirlineCode  NVARCHAR(10) = NULL
AS
BEGIN
    IF EXISTS (SELECT 1 FROM AdminUsers WHERE Username = @Username)
    BEGIN
        SELECT -1 AS NewId; RETURN;
    END
    INSERT INTO AdminUsers (Username, PasswordHash, Role, Status, AirlineCode)
    VALUES (@Username, @PasswordHash, @Role, 'ACTIVE', @AirlineCode);
    SELECT SCOPE_IDENTITY() AS NewId;
END
GO

IF OBJECT_ID('sp_Admin_Update', 'P') IS NOT NULL DROP PROCEDURE sp_Admin_Update; GO
CREATE PROCEDURE sp_Admin_Update
    @Id           INT,
    @Role         NVARCHAR(20),
    @Status       NVARCHAR(20),
    @AirlineCode  NVARCHAR(10) = NULL,
    @PasswordHash NVARCHAR(MAX) = NULL
AS
BEGIN
    UPDATE AdminUsers
    SET Role        = @Role,
        Status      = @Status,
        AirlineCode = @AirlineCode,
        PasswordHash = CASE WHEN @PasswordHash IS NOT NULL THEN @PasswordHash ELSE PasswordHash END
    WHERE Id = @Id;
END
GO

-- Update sp_User_Authenticate to return AirlineCode
IF OBJECT_ID('sp_User_Authenticate', 'P') IS NOT NULL DROP PROCEDURE sp_User_Authenticate; GO
CREATE PROCEDURE sp_User_Authenticate
    @Username     NVARCHAR(50),
    @PasswordHash NVARCHAR(MAX)
AS
BEGIN
    SELECT Id, Username, Role, Status, AirlineCode
    FROM AdminUsers
    WHERE Username = @Username
      AND PasswordHash = @PasswordHash
      AND Status = 'ACTIVE';
END
GO
