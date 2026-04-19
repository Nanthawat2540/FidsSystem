-- ============================================================
-- Permissions System Migration
-- Run once on existing DB
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UserPermissions')
BEGIN
    CREATE TABLE UserPermissions (
        UserId    INT           NOT NULL,
        PermKey   NVARCHAR(50)  NOT NULL,
        IsEnabled BIT           NOT NULL DEFAULT 1,
        UpdatedAt DATETIME2     NOT NULL DEFAULT GETDATE(),
        CONSTRAINT PK_UserPermissions PRIMARY KEY (UserId, PermKey),
        CONSTRAINT FK_UserPermissions_User FOREIGN KEY (UserId) REFERENCES AdminUsers(Id) ON DELETE CASCADE
    );
END

-- ============================================================
-- sp_Perm_GetByUser  — returns all rows for a user
-- ============================================================
IF OBJECT_ID('sp_Perm_GetByUser', 'P') IS NOT NULL DROP PROCEDURE sp_Perm_GetByUser;
GO
CREATE PROCEDURE sp_Perm_GetByUser @UserId INT
AS
    SELECT PermKey, IsEnabled FROM UserPermissions WHERE UserId = @UserId;
GO

-- ============================================================
-- sp_Perm_Upsert  — insert or update one permission
-- ============================================================
IF OBJECT_ID('sp_Perm_Upsert', 'P') IS NOT NULL DROP PROCEDURE sp_Perm_Upsert;
GO
CREATE PROCEDURE sp_Perm_Upsert
    @UserId    INT,
    @PermKey   NVARCHAR(50),
    @IsEnabled BIT
AS
    MERGE UserPermissions AS T
    USING (SELECT @UserId AS UserId, @PermKey AS PermKey) AS S
    ON T.UserId = S.UserId AND T.PermKey = S.PermKey
    WHEN MATCHED    THEN UPDATE SET IsEnabled = @IsEnabled, UpdatedAt = GETDATE()
    WHEN NOT MATCHED THEN INSERT (UserId, PermKey, IsEnabled) VALUES (@UserId, @PermKey, @IsEnabled);
GO

-- ============================================================
-- sp_Perm_ResetUser  — delete all rows for user (revert to role defaults)
-- ============================================================
IF OBJECT_ID('sp_Perm_ResetUser', 'P') IS NOT NULL DROP PROCEDURE sp_Perm_ResetUser;
GO
CREATE PROCEDURE sp_Perm_ResetUser @UserId INT
AS
    DELETE FROM UserPermissions WHERE UserId = @UserId;
GO

-- ============================================================
-- sp_Perm_GetAll  — all rows for all users (for the panel grid)
-- ============================================================
IF OBJECT_ID('sp_Perm_GetAll', 'P') IS NOT NULL DROP PROCEDURE sp_Perm_GetAll;
GO
CREATE PROCEDURE sp_Perm_GetAll
AS
    SELECT UserId, PermKey, IsEnabled FROM UserPermissions;
GO
