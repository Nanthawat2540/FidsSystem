-- ============================================================
-- FidsDB - Full Schema + Stored Procedures + Seed Data
-- Server: 58.8.92.109,1433
-- ============================================================

USE FidsDB;
GO

-- ============================================================
-- TABLES
-- ============================================================

-- Zones
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Zones' AND xtype='U')
CREATE TABLE [Zones] (
    [Id]        INT IDENTITY(1,1) PRIMARY KEY,
    [ZoneName]  NVARCHAR(100) NOT NULL,
    [Remark]    NVARCHAR(500) NULL,
    [CreatedAt] DATETIME DEFAULT GETDATE()
);
GO

-- Templates
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Templates' AND xtype='U')
CREATE TABLE [Templates] (
    [Id]         INT IDENTITY(1,1) PRIMARY KEY,
    [Name]       NVARCHAR(100) NOT NULL,
    [DeviceType] NVARCHAR(100) NOT NULL,
    [Ratio]      NVARCHAR(20)  NOT NULL,
    [CreatedAt]  DATETIME DEFAULT GETDATE()
);
GO

-- Devices (physical devices assigned to zones)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Devices' AND xtype='U')
CREATE TABLE [Devices] (
    [Id]         INT IDENTITY(1,1) PRIMARY KEY,
    [ZoneId]     INT NULL,
    [ZoneName]   NVARCHAR(100) NULL,
    [DeviceName] NVARCHAR(100) NOT NULL,
    [DeviceType] NVARCHAR(100) NULL,
    [IPAddress]  NVARCHAR(50)  NULL,
    [Ratio]      NVARCHAR(20)  NULL,
    [CreatedAt]  DATETIME DEFAULT GETDATE()
);
GO

-- DisplayDevices (display screens mapped to zone+template)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DisplayDevices' AND xtype='U')
CREATE TABLE [DisplayDevices] (
    [Id]          INT IDENTITY(1,1) PRIMARY KEY,
    [ZoneId]      INT NULL,
    [ZoneName]    NVARCHAR(100) NULL,
    [DeviceName]  NVARCHAR(100) NOT NULL,
    [TemplateId]  INT NULL,
    [TemplateName] NVARCHAR(100) NULL,
    [Ratio]       NVARCHAR(20)  NULL,
    [DataSet]     NVARCHAR(200) NULL,
    [IsDisplayOn] BIT DEFAULT 0,
    [CreatedAt]   DATETIME DEFAULT GETDATE()
);
GO

-- Advertisements
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Advertisements' AND xtype='U')
CREATE TABLE [Advertisements] (
    [Id]         INT IDENTITY(1,1) PRIMARY KEY,
    [Title]      NVARCHAR(200) NOT NULL,
    [FileType]   NVARCHAR(20)  NOT NULL,   -- Video / Image
    [FilePath]   NVARCHAR(500) NULL,
    [Duration]   INT NULL,                 -- seconds
    [FileSizeMB] DECIMAL(10,2) NULL,
    [Month]      INT NOT NULL,
    [Year]       INT NOT NULL,
    [UploadDate] DATETIME DEFAULT GETDATE()
);
GO

-- Playlists
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Playlists' AND xtype='U')
CREATE TABLE [Playlists] (
    [Id]         INT IDENTITY(1,1) PRIMARY KEY,
    [Name]       NVARCHAR(200) NOT NULL,
    [CreatedAt]  DATETIME DEFAULT GETDATE()
);
GO

-- PlaylistItems
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PlaylistItems' AND xtype='U')
CREATE TABLE [PlaylistItems] (
    [Id]              INT IDENTITY(1,1) PRIMARY KEY,
    [PlaylistId]      INT NOT NULL,
    [AdvertisementId] INT NOT NULL,
    [SortOrder]       INT DEFAULT 0,
    CONSTRAINT FK_PlaylistItems_Playlist FOREIGN KEY ([PlaylistId]) REFERENCES [Playlists]([Id]) ON DELETE CASCADE,
    CONSTRAINT FK_PlaylistItems_Ad       FOREIGN KEY ([AdvertisementId]) REFERENCES [Advertisements]([Id])
);
GO

-- MatchDisplays (playlist <-> display device mapping)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='MatchDisplays' AND xtype='U')
CREATE TABLE [MatchDisplays] (
    [Id]             INT IDENTITY(1,1) PRIMARY KEY,
    [PlaylistId]     INT NOT NULL,
    [PlaylistName]   NVARCHAR(200) NULL,
    [DisplayId]      INT NULL,
    [DisplayName]    NVARCHAR(100) NULL,
    [IsMatched]      BIT DEFAULT 0,
    [MatchedAt]      DATETIME NULL,
    CONSTRAINT FK_MatchDisplays_Playlist FOREIGN KEY ([PlaylistId]) REFERENCES [Playlists]([Id])
);
GO

-- AdminUsers
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AdminUsers' AND xtype='U')
CREATE TABLE [AdminUsers] (
    [Id]           INT IDENTITY(1,1) PRIMARY KEY,
    [Username]     NVARCHAR(50)  NOT NULL UNIQUE,
    [PasswordHash] NVARCHAR(MAX) NOT NULL,
    [Role]         NVARCHAR(20)  NOT NULL DEFAULT 'STAFF',  -- ADMIN | STAFF
    [Status]       NVARCHAR(20)  NOT NULL DEFAULT 'ACTIVE', -- ACTIVE | INACTIVE
    [CreatedAt]    DATETIME DEFAULT GETDATE()
);
GO

-- AirlineLogos
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AirlineLogos' AND xtype='U')
CREATE TABLE [AirlineLogos] (
    [Id]              INT IDENTITY(1,1) PRIMARY KEY,
    [AirlineName]     NVARCHAR(200) NOT NULL,
    [IATACode]        NVARCHAR(10)  NOT NULL,
    [BackgroundColor] NVARCHAR(20)  NULL DEFAULT '#FFFFFF',
    [ImageUrl]        NVARCHAR(500) NULL,
    [CreatedAt]       DATETIME DEFAULT GETDATE()
);
GO

-- SystemSettings (key-value)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SystemSettings' AND xtype='U')
CREATE TABLE [SystemSettings] (
    [Id]           INT IDENTITY(1,1) PRIMARY KEY,
    [SettingKey]   NVARCHAR(100) NOT NULL UNIQUE,
    [SettingValue] NVARCHAR(500) NOT NULL,
    [UpdatedAt]    DATETIME DEFAULT GETDATE()
);
GO

-- Keep existing Flights table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Flights' AND xtype='U')
CREATE TABLE [Flights] (
    [Id]            INT IDENTITY(1,1) PRIMARY KEY,
    [FlightNumber]  NVARCHAR(10)  NOT NULL,
    [Airline]       NVARCHAR(50)  NULL,
    [Origin]        NVARCHAR(100) NULL,
    [Destination]   NVARCHAR(100) NULL,
    [ScheduledTime] DATETIME      NOT NULL,
    [EstimatedTime] DATETIME      NULL,
    [Status]        NVARCHAR(50)  NULL,
    [Gate]          NVARCHAR(10)  NULL,
    [Belt]          NVARCHAR(10)  NULL,
    [FlightType]    NVARCHAR(20)  NULL
);
GO

-- ============================================================
-- STORED PROCEDURES - ZONES
-- ============================================================

IF OBJECT_ID('sp_Zone_GetAll', 'P') IS NOT NULL DROP PROCEDURE sp_Zone_GetAll; GO
CREATE PROCEDURE sp_Zone_GetAll
    @Search NVARCHAR(100) = NULL
AS
BEGIN
    SELECT Id, ZoneName, Remark, CreatedAt
    FROM Zones
    WHERE (@Search IS NULL OR ZoneName LIKE '%' + @Search + '%' OR Remark LIKE '%' + @Search + '%')
    ORDER BY Id;
END
GO

IF OBJECT_ID('sp_Zone_GetById', 'P') IS NOT NULL DROP PROCEDURE sp_Zone_GetById; GO
CREATE PROCEDURE sp_Zone_GetById @Id INT
AS
BEGIN
    SELECT Id, ZoneName, Remark, CreatedAt FROM Zones WHERE Id = @Id;
END
GO

IF OBJECT_ID('sp_Zone_Create', 'P') IS NOT NULL DROP PROCEDURE sp_Zone_Create; GO
CREATE PROCEDURE sp_Zone_Create
    @ZoneName  NVARCHAR(100),
    @Remark    NVARCHAR(500) = NULL
AS
BEGIN
    INSERT INTO Zones (ZoneName, Remark) VALUES (@ZoneName, @Remark);
    SELECT SCOPE_IDENTITY() AS NewId;
END
GO

IF OBJECT_ID('sp_Zone_Update', 'P') IS NOT NULL DROP PROCEDURE sp_Zone_Update; GO
CREATE PROCEDURE sp_Zone_Update
    @Id        INT,
    @ZoneName  NVARCHAR(100),
    @Remark    NVARCHAR(500) = NULL
AS
BEGIN
    UPDATE Zones SET ZoneName = @ZoneName, Remark = @Remark WHERE Id = @Id;
END
GO

IF OBJECT_ID('sp_Zone_Delete', 'P') IS NOT NULL DROP PROCEDURE sp_Zone_Delete; GO
CREATE PROCEDURE sp_Zone_Delete @Id INT
AS
BEGIN
    DELETE FROM Zones WHERE Id = @Id;
END
GO

-- ============================================================
-- STORED PROCEDURES - TEMPLATES
-- ============================================================

IF OBJECT_ID('sp_Template_GetAll', 'P') IS NOT NULL DROP PROCEDURE sp_Template_GetAll; GO
CREATE PROCEDURE sp_Template_GetAll
    @Search     NVARCHAR(100) = NULL,
    @DeviceType NVARCHAR(100) = NULL
AS
BEGIN
    SELECT Id, Name, DeviceType, Ratio, CreatedAt
    FROM Templates
    WHERE (@Search IS NULL OR Name LIKE '%' + @Search + '%')
      AND (@DeviceType IS NULL OR DeviceType = @DeviceType)
    ORDER BY Id;
END
GO

IF OBJECT_ID('sp_Template_GetById', 'P') IS NOT NULL DROP PROCEDURE sp_Template_GetById; GO
CREATE PROCEDURE sp_Template_GetById @Id INT
AS
BEGIN
    SELECT Id, Name, DeviceType, Ratio, CreatedAt FROM Templates WHERE Id = @Id;
END
GO

IF OBJECT_ID('sp_Template_Create', 'P') IS NOT NULL DROP PROCEDURE sp_Template_Create; GO
CREATE PROCEDURE sp_Template_Create
    @Name       NVARCHAR(100),
    @DeviceType NVARCHAR(100),
    @Ratio      NVARCHAR(20)
AS
BEGIN
    INSERT INTO Templates (Name, DeviceType, Ratio) VALUES (@Name, @DeviceType, @Ratio);
    SELECT SCOPE_IDENTITY() AS NewId;
END
GO

IF OBJECT_ID('sp_Template_Update', 'P') IS NOT NULL DROP PROCEDURE sp_Template_Update; GO
CREATE PROCEDURE sp_Template_Update
    @Id         INT,
    @Name       NVARCHAR(100),
    @DeviceType NVARCHAR(100),
    @Ratio      NVARCHAR(20)
AS
BEGIN
    UPDATE Templates SET Name = @Name, DeviceType = @DeviceType, Ratio = @Ratio WHERE Id = @Id;
END
GO

IF OBJECT_ID('sp_Template_Delete', 'P') IS NOT NULL DROP PROCEDURE sp_Template_Delete; GO
CREATE PROCEDURE sp_Template_Delete @Id INT
AS
BEGIN
    DELETE FROM Templates WHERE Id = @Id;
END
GO

-- ============================================================
-- STORED PROCEDURES - DEVICES
-- ============================================================

IF OBJECT_ID('sp_Device_GetAll', 'P') IS NOT NULL DROP PROCEDURE sp_Device_GetAll; GO
CREATE PROCEDURE sp_Device_GetAll
    @Search NVARCHAR(100) = NULL
AS
BEGIN
    SELECT Id, ZoneId, ZoneName, DeviceName, DeviceType, IPAddress, Ratio, CreatedAt
    FROM Devices
    WHERE (@Search IS NULL OR DeviceName LIKE '%' + @Search + '%' OR ZoneName LIKE '%' + @Search + '%')
    ORDER BY Id;
END
GO

IF OBJECT_ID('sp_Device_GetById', 'P') IS NOT NULL DROP PROCEDURE sp_Device_GetById; GO
CREATE PROCEDURE sp_Device_GetById @Id INT
AS
BEGIN
    SELECT Id, ZoneId, ZoneName, DeviceName, DeviceType, IPAddress, Ratio, CreatedAt FROM Devices WHERE Id = @Id;
END
GO

IF OBJECT_ID('sp_Device_Create', 'P') IS NOT NULL DROP PROCEDURE sp_Device_Create; GO
CREATE PROCEDURE sp_Device_Create
    @ZoneId     INT = NULL,
    @ZoneName   NVARCHAR(100) = NULL,
    @DeviceName NVARCHAR(100),
    @DeviceType NVARCHAR(100) = NULL,
    @IPAddress  NVARCHAR(50)  = NULL,
    @Ratio      NVARCHAR(20)  = NULL
AS
BEGIN
    INSERT INTO Devices (ZoneId, ZoneName, DeviceName, DeviceType, IPAddress, Ratio)
    VALUES (@ZoneId, @ZoneName, @DeviceName, @DeviceType, @IPAddress, @Ratio);
    SELECT SCOPE_IDENTITY() AS NewId;
END
GO

IF OBJECT_ID('sp_Device_Update', 'P') IS NOT NULL DROP PROCEDURE sp_Device_Update; GO
CREATE PROCEDURE sp_Device_Update
    @Id         INT,
    @ZoneId     INT = NULL,
    @ZoneName   NVARCHAR(100) = NULL,
    @DeviceName NVARCHAR(100),
    @DeviceType NVARCHAR(100) = NULL,
    @IPAddress  NVARCHAR(50)  = NULL,
    @Ratio      NVARCHAR(20)  = NULL
AS
BEGIN
    UPDATE Devices SET ZoneId=@ZoneId, ZoneName=@ZoneName, DeviceName=@DeviceName,
        DeviceType=@DeviceType, IPAddress=@IPAddress, Ratio=@Ratio WHERE Id=@Id;
END
GO

IF OBJECT_ID('sp_Device_Delete', 'P') IS NOT NULL DROP PROCEDURE sp_Device_Delete; GO
CREATE PROCEDURE sp_Device_Delete @Id INT
AS
BEGIN
    DELETE FROM Devices WHERE Id = @Id;
END
GO

-- ============================================================
-- STORED PROCEDURES - DISPLAY DEVICES
-- ============================================================

IF OBJECT_ID('sp_Display_GetAll', 'P') IS NOT NULL DROP PROCEDURE sp_Display_GetAll; GO
CREATE PROCEDURE sp_Display_GetAll
    @Search NVARCHAR(100) = NULL
AS
BEGIN
    SELECT Id, ZoneId, ZoneName, DeviceName, TemplateId, TemplateName, Ratio, DataSet, IsDisplayOn, CreatedAt
    FROM DisplayDevices
    WHERE (@Search IS NULL OR DeviceName LIKE '%' + @Search + '%' OR ZoneName LIKE '%' + @Search + '%')
    ORDER BY Id;
END
GO

IF OBJECT_ID('sp_Display_GetById', 'P') IS NOT NULL DROP PROCEDURE sp_Display_GetById; GO
CREATE PROCEDURE sp_Display_GetById @Id INT
AS
BEGIN
    SELECT Id, ZoneId, ZoneName, DeviceName, TemplateId, TemplateName, Ratio, DataSet, IsDisplayOn, CreatedAt
    FROM DisplayDevices WHERE Id = @Id;
END
GO

IF OBJECT_ID('sp_Display_Create', 'P') IS NOT NULL DROP PROCEDURE sp_Display_Create; GO
CREATE PROCEDURE sp_Display_Create
    @ZoneId       INT = NULL,
    @ZoneName     NVARCHAR(100) = NULL,
    @DeviceName   NVARCHAR(100),
    @TemplateId   INT = NULL,
    @TemplateName NVARCHAR(100) = NULL,
    @Ratio        NVARCHAR(20)  = NULL,
    @DataSet      NVARCHAR(200) = NULL,
    @IsDisplayOn  BIT = 0
AS
BEGIN
    INSERT INTO DisplayDevices (ZoneId, ZoneName, DeviceName, TemplateId, TemplateName, Ratio, DataSet, IsDisplayOn)
    VALUES (@ZoneId, @ZoneName, @DeviceName, @TemplateId, @TemplateName, @Ratio, @DataSet, @IsDisplayOn);
    SELECT SCOPE_IDENTITY() AS NewId;
END
GO

IF OBJECT_ID('sp_Display_Update', 'P') IS NOT NULL DROP PROCEDURE sp_Display_Update; GO
CREATE PROCEDURE sp_Display_Update
    @Id           INT,
    @ZoneId       INT = NULL,
    @ZoneName     NVARCHAR(100) = NULL,
    @DeviceName   NVARCHAR(100),
    @TemplateId   INT = NULL,
    @TemplateName NVARCHAR(100) = NULL,
    @Ratio        NVARCHAR(20)  = NULL,
    @DataSet      NVARCHAR(200) = NULL,
    @IsDisplayOn  BIT = 0
AS
BEGIN
    UPDATE DisplayDevices SET ZoneId=@ZoneId, ZoneName=@ZoneName, DeviceName=@DeviceName,
        TemplateId=@TemplateId, TemplateName=@TemplateName, Ratio=@Ratio,
        DataSet=@DataSet, IsDisplayOn=@IsDisplayOn WHERE Id=@Id;
END
GO

IF OBJECT_ID('sp_Display_ToggleOn', 'P') IS NOT NULL DROP PROCEDURE sp_Display_ToggleOn; GO
CREATE PROCEDURE sp_Display_ToggleOn @Id INT
AS
BEGIN
    UPDATE DisplayDevices SET IsDisplayOn = CASE WHEN IsDisplayOn=1 THEN 0 ELSE 1 END WHERE Id=@Id;
    SELECT IsDisplayOn FROM DisplayDevices WHERE Id=@Id;
END
GO

IF OBJECT_ID('sp_Display_Delete', 'P') IS NOT NULL DROP PROCEDURE sp_Display_Delete; GO
CREATE PROCEDURE sp_Display_Delete @Id INT
AS
BEGIN
    DELETE FROM DisplayDevices WHERE Id = @Id;
END
GO

-- ============================================================
-- STORED PROCEDURES - ADVERTISEMENTS
-- ============================================================

IF OBJECT_ID('sp_Ad_GetAll', 'P') IS NOT NULL DROP PROCEDURE sp_Ad_GetAll; GO
CREATE PROCEDURE sp_Ad_GetAll
    @Search NVARCHAR(200) = NULL,
    @Month  INT = NULL,
    @Year   INT = NULL
AS
BEGIN
    SELECT Id, Title, FileType, FilePath, Duration, FileSizeMB, Month, Year, UploadDate
    FROM Advertisements
    WHERE (@Search IS NULL OR Title LIKE '%' + @Search + '%')
      AND (@Month IS NULL OR Month = @Month)
      AND (@Year  IS NULL OR Year  = @Year)
    ORDER BY UploadDate DESC;
END
GO

IF OBJECT_ID('sp_Ad_GetById', 'P') IS NOT NULL DROP PROCEDURE sp_Ad_GetById; GO
CREATE PROCEDURE sp_Ad_GetById @Id INT
AS
BEGIN
    SELECT Id, Title, FileType, FilePath, Duration, FileSizeMB, Month, Year, UploadDate
    FROM Advertisements WHERE Id = @Id;
END
GO

IF OBJECT_ID('sp_Ad_Create', 'P') IS NOT NULL DROP PROCEDURE sp_Ad_Create; GO
CREATE PROCEDURE sp_Ad_Create
    @Title      NVARCHAR(200),
    @FileType   NVARCHAR(20),
    @FilePath   NVARCHAR(500) = NULL,
    @Duration   INT = NULL,
    @FileSizeMB DECIMAL(10,2) = NULL,
    @Month      INT,
    @Year       INT
AS
BEGIN
    INSERT INTO Advertisements (Title, FileType, FilePath, Duration, FileSizeMB, Month, Year)
    VALUES (@Title, @FileType, @FilePath, @Duration, @FileSizeMB, @Month, @Year);
    SELECT SCOPE_IDENTITY() AS NewId;
END
GO

IF OBJECT_ID('sp_Ad_Delete', 'P') IS NOT NULL DROP PROCEDURE sp_Ad_Delete; GO
CREATE PROCEDURE sp_Ad_Delete @Id INT
AS
BEGIN
    DELETE FROM PlaylistItems WHERE AdvertisementId = @Id;
    DELETE FROM Advertisements WHERE Id = @Id;
END
GO

-- ============================================================
-- STORED PROCEDURES - PLAYLISTS
-- ============================================================

IF OBJECT_ID('sp_Playlist_GetAll', 'P') IS NOT NULL DROP PROCEDURE sp_Playlist_GetAll; GO
CREATE PROCEDURE sp_Playlist_GetAll
    @Search NVARCHAR(200) = NULL
AS
BEGIN
    SELECT p.Id, p.Name, p.CreatedAt,
           SUM(CASE WHEN a.FileType='Video' THEN 1 ELSE 0 END) AS VideoCount,
           SUM(CASE WHEN a.FileType='Image' THEN 1 ELSE 0 END) AS ImageCount,
           COUNT(pi2.Id) AS TotalCount
    FROM Playlists p
    LEFT JOIN PlaylistItems pi2 ON pi2.PlaylistId = p.Id
    LEFT JOIN Advertisements a ON a.Id = pi2.AdvertisementId
    WHERE (@Search IS NULL OR p.Name LIKE '%' + @Search + '%')
    GROUP BY p.Id, p.Name, p.CreatedAt
    ORDER BY p.Id;
END
GO

IF OBJECT_ID('sp_Playlist_GetById', 'P') IS NOT NULL DROP PROCEDURE sp_Playlist_GetById; GO
CREATE PROCEDURE sp_Playlist_GetById @Id INT
AS
BEGIN
    SELECT p.Id, p.Name, p.CreatedAt,
           SUM(CASE WHEN a.FileType='Video' THEN 1 ELSE 0 END) AS VideoCount,
           SUM(CASE WHEN a.FileType='Image' THEN 1 ELSE 0 END) AS ImageCount,
           COUNT(pi2.Id) AS TotalCount
    FROM Playlists p
    LEFT JOIN PlaylistItems pi2 ON pi2.PlaylistId = p.Id
    LEFT JOIN Advertisements a ON a.Id = pi2.AdvertisementId
    WHERE p.Id = @Id
    GROUP BY p.Id, p.Name, p.CreatedAt;
END
GO

IF OBJECT_ID('sp_Playlist_Create', 'P') IS NOT NULL DROP PROCEDURE sp_Playlist_Create; GO
CREATE PROCEDURE sp_Playlist_Create
    @Name NVARCHAR(200)
AS
BEGIN
    INSERT INTO Playlists (Name) VALUES (@Name);
    SELECT SCOPE_IDENTITY() AS NewId;
END
GO

IF OBJECT_ID('sp_Playlist_Update', 'P') IS NOT NULL DROP PROCEDURE sp_Playlist_Update; GO
CREATE PROCEDURE sp_Playlist_Update
    @Id   INT,
    @Name NVARCHAR(200)
AS
BEGIN
    UPDATE Playlists SET Name = @Name WHERE Id = @Id;
END
GO

IF OBJECT_ID('sp_Playlist_Delete', 'P') IS NOT NULL DROP PROCEDURE sp_Playlist_Delete; GO
CREATE PROCEDURE sp_Playlist_Delete @Id INT
AS
BEGIN
    DELETE FROM MatchDisplays WHERE PlaylistId = @Id;
    DELETE FROM PlaylistItems  WHERE PlaylistId = @Id;
    DELETE FROM Playlists      WHERE Id = @Id;
END
GO

-- ============================================================
-- STORED PROCEDURES - MATCH DISPLAYS
-- ============================================================

IF OBJECT_ID('sp_Match_GetAll', 'P') IS NOT NULL DROP PROCEDURE sp_Match_GetAll; GO
CREATE PROCEDURE sp_Match_GetAll
    @Search NVARCHAR(200) = NULL
AS
BEGIN
    SELECT Id, PlaylistId, PlaylistName, DisplayId, DisplayName, IsMatched, MatchedAt
    FROM MatchDisplays
    WHERE (@Search IS NULL OR PlaylistName LIKE '%' + @Search + '%')
    ORDER BY Id;
END
GO

IF OBJECT_ID('sp_Match_GetById', 'P') IS NOT NULL DROP PROCEDURE sp_Match_GetById; GO
CREATE PROCEDURE sp_Match_GetById @Id INT
AS
BEGIN
    SELECT Id, PlaylistId, PlaylistName, DisplayId, DisplayName, IsMatched, MatchedAt
    FROM MatchDisplays WHERE Id = @Id;
END
GO

IF OBJECT_ID('sp_Match_Create', 'P') IS NOT NULL DROP PROCEDURE sp_Match_Create; GO
CREATE PROCEDURE sp_Match_Create
    @PlaylistId   INT,
    @PlaylistName NVARCHAR(200) = NULL,
    @DisplayId    INT = NULL,
    @DisplayName  NVARCHAR(100) = NULL
AS
BEGIN
    INSERT INTO MatchDisplays (PlaylistId, PlaylistName, DisplayId, DisplayName, IsMatched)
    VALUES (@PlaylistId, @PlaylistName, @DisplayId, @DisplayName, 0);
    SELECT SCOPE_IDENTITY() AS NewId;
END
GO

IF OBJECT_ID('sp_Match_SetMatched', 'P') IS NOT NULL DROP PROCEDURE sp_Match_SetMatched; GO
CREATE PROCEDURE sp_Match_SetMatched @Id INT
AS
BEGIN
    UPDATE MatchDisplays SET IsMatched=1, MatchedAt=GETDATE() WHERE Id=@Id;
END
GO

IF OBJECT_ID('sp_Match_Delete', 'P') IS NOT NULL DROP PROCEDURE sp_Match_Delete; GO
CREATE PROCEDURE sp_Match_Delete @Id INT
AS
BEGIN
    DELETE FROM MatchDisplays WHERE Id = @Id;
END
GO

-- ============================================================
-- STORED PROCEDURES - ADMIN USERS
-- ============================================================

IF OBJECT_ID('sp_Admin_GetAll', 'P') IS NOT NULL DROP PROCEDURE sp_Admin_GetAll; GO
CREATE PROCEDURE sp_Admin_GetAll
AS
BEGIN
    SELECT Id, Username, Role, Status, CreatedAt FROM AdminUsers ORDER BY Id;
END
GO

IF OBJECT_ID('sp_Admin_GetById', 'P') IS NOT NULL DROP PROCEDURE sp_Admin_GetById; GO
CREATE PROCEDURE sp_Admin_GetById @Id INT
AS
BEGIN
    SELECT Id, Username, Role, Status, CreatedAt FROM AdminUsers WHERE Id = @Id;
END
GO

IF OBJECT_ID('sp_Admin_Create', 'P') IS NOT NULL DROP PROCEDURE sp_Admin_Create; GO
CREATE PROCEDURE sp_Admin_Create
    @Username     NVARCHAR(50),
    @PasswordHash NVARCHAR(MAX),
    @Role         NVARCHAR(20) = 'STAFF'
AS
BEGIN
    IF EXISTS (SELECT 1 FROM AdminUsers WHERE Username = @Username)
    BEGIN
        SELECT -1 AS NewId; RETURN;
    END
    INSERT INTO AdminUsers (Username, PasswordHash, Role, Status)
    VALUES (@Username, @PasswordHash, @Role, 'ACTIVE');
    SELECT SCOPE_IDENTITY() AS NewId;
END
GO

IF OBJECT_ID('sp_Admin_Update', 'P') IS NOT NULL DROP PROCEDURE sp_Admin_Update; GO
CREATE PROCEDURE sp_Admin_Update
    @Id           INT,
    @Role         NVARCHAR(20),
    @Status       NVARCHAR(20),
    @PasswordHash NVARCHAR(MAX) = NULL
AS
BEGIN
    UPDATE AdminUsers SET Role=@Role, Status=@Status,
        PasswordHash = CASE WHEN @PasswordHash IS NOT NULL THEN @PasswordHash ELSE PasswordHash END
    WHERE Id = @Id;
END
GO

IF OBJECT_ID('sp_Admin_Delete', 'P') IS NOT NULL DROP PROCEDURE sp_Admin_Delete; GO
CREATE PROCEDURE sp_Admin_Delete @Id INT
AS
BEGIN
    DELETE FROM AdminUsers WHERE Id = @Id AND Id > 1;
END
GO

-- ============================================================
-- STORED PROCEDURES - AIRLINE LOGOS
-- ============================================================

IF OBJECT_ID('sp_Logo_GetAll', 'P') IS NOT NULL DROP PROCEDURE sp_Logo_GetAll; GO
CREATE PROCEDURE sp_Logo_GetAll
    @Search NVARCHAR(100) = NULL
AS
BEGIN
    SELECT Id, AirlineName, IATACode, BackgroundColor, ImageUrl, CreatedAt
    FROM AirlineLogos
    WHERE (@Search IS NULL OR AirlineName LIKE '%' + @Search + '%' OR IATACode LIKE '%' + @Search + '%')
    ORDER BY AirlineName;
END
GO

IF OBJECT_ID('sp_Logo_GetById', 'P') IS NOT NULL DROP PROCEDURE sp_Logo_GetById; GO
CREATE PROCEDURE sp_Logo_GetById @Id INT
AS
BEGIN
    SELECT Id, AirlineName, IATACode, BackgroundColor, ImageUrl, CreatedAt FROM AirlineLogos WHERE Id = @Id;
END
GO

IF OBJECT_ID('sp_Logo_Create', 'P') IS NOT NULL DROP PROCEDURE sp_Logo_Create; GO
CREATE PROCEDURE sp_Logo_Create
    @AirlineName     NVARCHAR(200),
    @IATACode        NVARCHAR(10),
    @BackgroundColor NVARCHAR(20) = '#FFFFFF',
    @ImageUrl        NVARCHAR(500) = NULL
AS
BEGIN
    INSERT INTO AirlineLogos (AirlineName, IATACode, BackgroundColor, ImageUrl)
    VALUES (@AirlineName, @IATACode, @BackgroundColor, @ImageUrl);
    SELECT SCOPE_IDENTITY() AS NewId;
END
GO

IF OBJECT_ID('sp_Logo_Update', 'P') IS NOT NULL DROP PROCEDURE sp_Logo_Update; GO
CREATE PROCEDURE sp_Logo_Update
    @Id              INT,
    @AirlineName     NVARCHAR(200),
    @IATACode        NVARCHAR(10),
    @BackgroundColor NVARCHAR(20) = '#FFFFFF',
    @ImageUrl        NVARCHAR(500) = NULL
AS
BEGIN
    UPDATE AirlineLogos SET AirlineName=@AirlineName, IATACode=@IATACode,
        BackgroundColor=@BackgroundColor, ImageUrl=@ImageUrl WHERE Id=@Id;
END
GO

IF OBJECT_ID('sp_Logo_Delete', 'P') IS NOT NULL DROP PROCEDURE sp_Logo_Delete; GO
CREATE PROCEDURE sp_Logo_Delete @Id INT
AS
BEGIN
    DELETE FROM AirlineLogos WHERE Id = @Id;
END
GO

-- ============================================================
-- STORED PROCEDURES - SYSTEM SETTINGS
-- ============================================================

IF OBJECT_ID('sp_Setting_GetAll', 'P') IS NOT NULL DROP PROCEDURE sp_Setting_GetAll; GO
CREATE PROCEDURE sp_Setting_GetAll
AS
BEGIN
    SELECT Id, SettingKey, SettingValue, UpdatedAt FROM SystemSettings ORDER BY SettingKey;
END
GO

IF OBJECT_ID('sp_Setting_GetByKey', 'P') IS NOT NULL DROP PROCEDURE sp_Setting_GetByKey; GO
CREATE PROCEDURE sp_Setting_GetByKey @Key NVARCHAR(100)
AS
BEGIN
    SELECT SettingValue FROM SystemSettings WHERE SettingKey = @Key;
END
GO

IF OBJECT_ID('sp_Setting_Upsert', 'P') IS NOT NULL DROP PROCEDURE sp_Setting_Upsert; GO
CREATE PROCEDURE sp_Setting_Upsert
    @Key   NVARCHAR(100),
    @Value NVARCHAR(500)
AS
BEGIN
    IF EXISTS (SELECT 1 FROM SystemSettings WHERE SettingKey = @Key)
        UPDATE SystemSettings SET SettingValue=@Value, UpdatedAt=GETDATE() WHERE SettingKey=@Key;
    ELSE
        INSERT INTO SystemSettings (SettingKey, SettingValue) VALUES (@Key, @Value);
END
GO

-- ============================================================
-- STORED PROCEDURES - AUTHENTICATION
-- ============================================================

IF OBJECT_ID('sp_User_Authenticate', 'P') IS NOT NULL DROP PROCEDURE sp_User_Authenticate; GO
CREATE PROCEDURE sp_User_Authenticate
    @Username     NVARCHAR(50),
    @PasswordHash NVARCHAR(MAX)
AS
BEGIN
    SELECT Id, Username, Role, Status
    FROM AdminUsers
    WHERE Username = @Username
      AND PasswordHash = @PasswordHash
      AND Status = 'ACTIVE';
END
GO

-- ============================================================
-- SEED DATA
-- ============================================================

-- Default admin user: admin / admin123  (SHA256 hash)
-- SHA256("admin123") = 240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9
IF NOT EXISTS (SELECT 1 FROM AdminUsers WHERE Username = 'admin')
BEGIN
    INSERT INTO AdminUsers (Username, PasswordHash, Role, Status)
    VALUES ('admin', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 'ADMIN', 'ACTIVE');
END
GO

-- Default system settings
MERGE SystemSettings AS target
USING (VALUES
    ('BagFirstBag',    '60'),
    ('BagLastBag',     '30'),
    ('BagNoFirstBag',  '120'),
    ('ArrivalWindow',  '180'),
    ('ArrFirstBag',    '0'),
    ('ArrLastBag',     '30'),
    ('ArrNoFirstBag',  '120'),
    ('DelayThreshold', '180'),
    ('DepWindow',      '180'),
    ('DepRemove',      '30'),
    ('Language',       'Lao')
) AS source (SettingKey, SettingValue)
ON target.SettingKey = source.SettingKey
WHEN NOT MATCHED THEN
    INSERT (SettingKey, SettingValue) VALUES (source.SettingKey, source.SettingValue);
GO

-- Sample airline logos
IF NOT EXISTS (SELECT 1 FROM AirlineLogos WHERE IATACode = 'TG')
    INSERT INTO AirlineLogos (AirlineName, IATACode, BackgroundColor) VALUES (N'Thai Airways', 'TG', '#6D2077');
IF NOT EXISTS (SELECT 1 FROM AirlineLogos WHERE IATACode = 'FD')
    INSERT INTO AirlineLogos (AirlineName, IATACode, BackgroundColor) VALUES (N'Thai AirAsia', 'FD', '#FF0000');
IF NOT EXISTS (SELECT 1 FROM AirlineLogos WHERE IATACode = 'QV')
    INSERT INTO AirlineLogos (AirlineName, IATACode, BackgroundColor) VALUES (N'Lao Airlines', 'QV', '#003DA5');
IF NOT EXISTS (SELECT 1 FROM AirlineLogos WHERE IATACode = 'VZ')
    INSERT INTO AirlineLogos (AirlineName, IATACode, BackgroundColor) VALUES (N'Thai Vietjet Air', 'VZ', '#E60000');
IF NOT EXISTS (SELECT 1 FROM AirlineLogos WHERE IATACode = 'SQ')
    INSERT INTO AirlineLogos (AirlineName, IATACode, BackgroundColor) VALUES (N'Singapore Airlines', 'SQ', '#004B87');
GO
