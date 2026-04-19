-- ============================================================
-- AlterFidsDB.sql — Add new tables for production features
-- Run AFTER CreateFidsDB.sql
-- ============================================================

USE FidsDB;
GO

-- ── FlightStatusHistory ─────────────────────────────────────
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FlightStatusHistory' AND xtype='U')
CREATE TABLE [FlightStatusHistory] (
    [Id]           INT IDENTITY(1,1) PRIMARY KEY,
    [FlightId]     INT          NOT NULL,
    [FlightNumber] NVARCHAR(10) NOT NULL,
    [OldStatus]    NVARCHAR(50) NULL,
    [NewStatus]    NVARCHAR(50) NULL,
    [OldGate]      NVARCHAR(10) NULL,
    [NewGate]      NVARCHAR(10) NULL,
    [ChangedBy]    NVARCHAR(50) NULL,
    [ChangedAt]    DATETIME     NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_FlightHistory_Flight FOREIGN KEY ([FlightId]) REFERENCES [Flights]([Id]) ON DELETE CASCADE
);
GO

-- ── ScreenHeartbeat ──────────────────────────────────────────
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ScreenHeartbeat' AND xtype='U')
CREATE TABLE [ScreenHeartbeat] (
    [Id]        INT IDENTITY(1,1) PRIMARY KEY,
    [ScreenKey] NVARCHAR(100) NOT NULL UNIQUE,
    [LastPing]  DATETIME      NOT NULL DEFAULT GETDATE(),
    [IpAddress] NVARCHAR(50)  NULL,
    [UserAgent] NVARCHAR(500) NULL
);
GO

-- ── Alerts ──────────────────────────────────────────────────
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Alerts' AND xtype='U')
CREATE TABLE [Alerts] (
    [Id]             INT IDENTITY(1,1) PRIMARY KEY,
    [Type]           NVARCHAR(50)  NOT NULL,
    [Severity]       NVARCHAR(20)  NOT NULL DEFAULT 'Warning',
    [Title]          NVARCHAR(200) NOT NULL,
    [Message]        NVARCHAR(500) NOT NULL,
    [FlightNumber]   NVARCHAR(10)  NULL,
    [IsAcknowledged] BIT           NOT NULL DEFAULT 0,
    [CreatedAt]      DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

-- ── Add Remark column to Flights if missing ─────────────────
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Flights') AND name = 'Remark')
    ALTER TABLE [Flights] ADD [Remark] NVARCHAR(200) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Flights') AND name = 'UpdatedAt')
    ALTER TABLE [Flights] ADD [UpdatedAt] DATETIME NULL;
GO

-- ── Stored Procedures ────────────────────────────────────────

-- Log status/gate change
IF OBJECT_ID('sp_FlightHistory_Add', 'P') IS NOT NULL DROP PROCEDURE sp_FlightHistory_Add; GO
CREATE PROCEDURE sp_FlightHistory_Add
    @FlightId     INT,
    @FlightNumber NVARCHAR(10),
    @OldStatus    NVARCHAR(50) = NULL,
    @NewStatus    NVARCHAR(50) = NULL,
    @OldGate      NVARCHAR(10) = NULL,
    @NewGate      NVARCHAR(10) = NULL,
    @ChangedBy    NVARCHAR(50) = NULL
AS
BEGIN
    INSERT INTO FlightStatusHistory (FlightId, FlightNumber, OldStatus, NewStatus, OldGate, NewGate, ChangedBy)
    VALUES (@FlightId, @FlightNumber, @OldStatus, @NewStatus, @OldGate, @NewGate, @ChangedBy);
END
GO

-- Get history for a flight
IF OBJECT_ID('sp_FlightHistory_GetByFlight', 'P') IS NOT NULL DROP PROCEDURE sp_FlightHistory_GetByFlight; GO
CREATE PROCEDURE sp_FlightHistory_GetByFlight
    @FlightId INT
AS
BEGIN
    SELECT Id, FlightId, FlightNumber, OldStatus, NewStatus, OldGate, NewGate, ChangedBy, ChangedAt
    FROM FlightStatusHistory
    WHERE FlightId = @FlightId
    ORDER BY ChangedAt DESC;
END
GO

-- Get recent history (last N records)
IF OBJECT_ID('sp_FlightHistory_GetRecent', 'P') IS NOT NULL DROP PROCEDURE sp_FlightHistory_GetRecent; GO
CREATE PROCEDURE sp_FlightHistory_GetRecent
    @TopN INT = 50
AS
BEGIN
    SELECT TOP (@TopN) Id, FlightId, FlightNumber, OldStatus, NewStatus, OldGate, NewGate, ChangedBy, ChangedAt
    FROM FlightStatusHistory
    ORDER BY ChangedAt DESC;
END
GO

-- Heartbeat upsert
IF OBJECT_ID('sp_Heartbeat_Upsert', 'P') IS NOT NULL DROP PROCEDURE sp_Heartbeat_Upsert; GO
CREATE PROCEDURE sp_Heartbeat_Upsert
    @ScreenKey NVARCHAR(100),
    @IpAddress NVARCHAR(50)  = NULL,
    @UserAgent NVARCHAR(500) = NULL
AS
BEGIN
    IF EXISTS (SELECT 1 FROM ScreenHeartbeat WHERE ScreenKey = @ScreenKey)
        UPDATE ScreenHeartbeat SET LastPing=GETDATE(), IpAddress=@IpAddress, UserAgent=@UserAgent
        WHERE ScreenKey = @ScreenKey;
    ELSE
        INSERT INTO ScreenHeartbeat (ScreenKey, IpAddress, UserAgent)
        VALUES (@ScreenKey, @IpAddress, @UserAgent);
END
GO

-- Get all heartbeats
IF OBJECT_ID('sp_Heartbeat_GetAll', 'P') IS NOT NULL DROP PROCEDURE sp_Heartbeat_GetAll; GO
CREATE PROCEDURE sp_Heartbeat_GetAll
AS
BEGIN
    SELECT ScreenKey, LastPing, IpAddress,
           DATEDIFF(SECOND, LastPing, GETDATE()) AS SecondsSinceLastPing
    FROM ScreenHeartbeat
    ORDER BY LastPing DESC;
END
GO

-- Alert SPs
IF OBJECT_ID('sp_Alert_Add', 'P') IS NOT NULL DROP PROCEDURE sp_Alert_Add; GO
CREATE PROCEDURE sp_Alert_Add
    @Type         NVARCHAR(50),
    @Severity     NVARCHAR(20),
    @Title        NVARCHAR(200),
    @Message      NVARCHAR(500),
    @FlightNumber NVARCHAR(10) = NULL
AS
BEGIN
    -- Avoid duplicate active alerts for same flight + type
    IF NOT EXISTS (
        SELECT 1 FROM Alerts
        WHERE Type=@Type AND FlightNumber=@FlightNumber AND IsAcknowledged=0
          AND CreatedAt > DATEADD(HOUR,-1,GETDATE())
    )
    BEGIN
        INSERT INTO Alerts (Type, Severity, Title, Message, FlightNumber)
        VALUES (@Type, @Severity, @Title, @Message, @FlightNumber);
    END
END
GO

IF OBJECT_ID('sp_Alert_GetActive', 'P') IS NOT NULL DROP PROCEDURE sp_Alert_GetActive; GO
CREATE PROCEDURE sp_Alert_GetActive
AS
BEGIN
    SELECT Id, Type, Severity, Title, Message, FlightNumber, IsAcknowledged, CreatedAt
    FROM Alerts
    WHERE IsAcknowledged = 0
    ORDER BY CreatedAt DESC;
END
GO

IF OBJECT_ID('sp_Alert_GetAll', 'P') IS NOT NULL DROP PROCEDURE sp_Alert_GetAll; GO
CREATE PROCEDURE sp_Alert_GetAll
    @TopN INT = 100
AS
BEGIN
    SELECT TOP (@TopN) Id, Type, Severity, Title, Message, FlightNumber, IsAcknowledged, CreatedAt
    FROM Alerts
    ORDER BY CreatedAt DESC;
END
GO

IF OBJECT_ID('sp_Alert_Acknowledge', 'P') IS NOT NULL DROP PROCEDURE sp_Alert_Acknowledge; GO
CREATE PROCEDURE sp_Alert_Acknowledge
    @Id INT
AS
BEGIN
    UPDATE Alerts SET IsAcknowledged = 1 WHERE Id = @Id;
END
GO

IF OBJECT_ID('sp_Alert_AcknowledgeAll', 'P') IS NOT NULL DROP PROCEDURE sp_Alert_AcknowledgeAll; GO
CREATE PROCEDURE sp_Alert_AcknowledgeAll
AS
BEGIN
    UPDATE Alerts SET IsAcknowledged = 1 WHERE IsAcknowledged = 0;
END
GO

-- Flight status bulk update
IF OBJECT_ID('sp_Flight_BulkUpdateStatus', 'P') IS NOT NULL DROP PROCEDURE sp_Flight_BulkUpdateStatus; GO
CREATE PROCEDURE sp_Flight_BulkUpdateStatus
    @Ids    NVARCHAR(MAX),   -- comma-separated IDs
    @Status NVARCHAR(50)
AS
BEGIN
    UPDATE Flights
    SET Status = @Status, UpdatedAt = GETDATE()
    WHERE Id IN (
        SELECT value FROM STRING_SPLIT(@Ids, ',')
    );
END
GO

-- Flight report query
IF OBJECT_ID('sp_Report_FlightSummary', 'P') IS NOT NULL DROP PROCEDURE sp_Report_FlightSummary; GO
CREATE PROCEDURE sp_Report_FlightSummary
    @Date DATE = NULL
AS
BEGIN
    SET @Date = ISNULL(@Date, CAST(GETDATE() AS DATE));

    -- Summary
    SELECT
        SUM(CASE WHEN FlightType='Departure' THEN 1 ELSE 0 END) AS TotalDepartures,
        SUM(CASE WHEN FlightType='Arrival'   THEN 1 ELSE 0 END) AS TotalArrivals,
        SUM(CASE WHEN Status='On Time'       THEN 1 ELSE 0 END) AS OnTime,
        SUM(CASE WHEN Status='Delayed'       THEN 1 ELSE 0 END) AS Delayed,
        SUM(CASE WHEN Status='Cancelled'     THEN 1 ELSE 0 END) AS Cancelled,
        SUM(CASE WHEN Status IN('Boarding','Gate Closed','Departed','Landed') THEN 1 ELSE 0 END) AS Boarded
    FROM Flights
    WHERE CAST(ScheduledTime AS DATE) = @Date;

    -- By airline
    SELECT Airline, COUNT(*) AS Count
    FROM Flights
    WHERE CAST(ScheduledTime AS DATE) = @Date AND Airline IS NOT NULL
    GROUP BY Airline
    ORDER BY Count DESC;

    -- By status
    SELECT Status, COUNT(*) AS Count
    FROM Flights
    WHERE CAST(ScheduledTime AS DATE) = @Date AND Status IS NOT NULL
    GROUP BY Status
    ORDER BY Count DESC;

    -- Recent delays
    SELECT TOP 10 *
    FROM Flights
    WHERE CAST(ScheduledTime AS DATE) = @Date
      AND Status IN ('Delayed','Cancelled')
    ORDER BY ScheduledTime;
END
GO
