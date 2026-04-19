-- ============================================================
-- UpdateMatchDisplay.sql
-- Redesign Match Display: device-centric approach
-- Run: sqlcmd -S 58.8.92.109,1433 -U nanthawat -P "@Ps03032022" -d FidsDB -i UpdateMatchDisplay.sql
-- ============================================================

USE FidsDB;
GO

-- ============================================================
-- 1. New Stored Procedures for device-centric Match Display
-- ============================================================

CREATE OR ALTER PROCEDURE sp_Match_GetAllByDisplay
    @Search NVARCHAR(100) = NULL,
    @ZoneId INT = NULL,
    @Status NVARCHAR(20) = NULL    -- 'matched' | 'notmatch' | NULL=all
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        dd.Id           AS DisplayId,
        dd.DeviceName   AS DisplayName,
        z.ZoneName,
        dd.Ratio,
        md.PlaylistId,
        p.Name          AS PlaylistName,
        CASE WHEN md.Id IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsMatched,
        md.MatchedAt
    FROM DisplayDevices dd
    LEFT JOIN Zones z            ON z.Id  = dd.ZoneId
    LEFT JOIN MatchDisplays md   ON md.DisplayId = dd.Id
    LEFT JOIN Playlists p        ON p.Id  = md.PlaylistId
    WHERE
        (@Search IS NULL OR dd.DeviceName LIKE '%' + @Search + '%')
        AND (@ZoneId IS NULL OR dd.ZoneId = @ZoneId)
        AND (
            @Status IS NULL
            OR (@Status = 'matched'  AND md.Id IS NOT NULL)
            OR (@Status = 'notmatch' AND md.Id IS NULL)
        )
    ORDER BY z.ZoneName, dd.DeviceName;
END
GO

CREATE OR ALTER PROCEDURE sp_Match_SetByDisplay
    @DisplayId INT,
    @PlaylistId INT
AS
BEGIN
    SET NOCOUNT ON;
    -- Remove any existing match for this display
    DELETE FROM MatchDisplays WHERE DisplayId = @DisplayId;
    -- Insert new match
    INSERT INTO MatchDisplays (PlaylistId, PlaylistName, DisplayId, DisplayName, IsMatched, MatchedAt)
    SELECT
        p.Id,
        p.Name,
        dd.Id,
        dd.DeviceName,
        1,
        GETDATE()
    FROM Playlists p
    CROSS JOIN DisplayDevices dd
    WHERE p.Id = @PlaylistId AND dd.Id = @DisplayId;
END
GO

CREATE OR ALTER PROCEDURE sp_Match_RemoveByDisplay
    @DisplayId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM MatchDisplays WHERE DisplayId = @DisplayId;
END
GO

-- ============================================================
-- 2. Seed Data (Zones, Templates, DisplayDevices, Playlists, Ads, Matches)
-- ============================================================

-- Clear existing seed data (safe reset)
DELETE FROM MatchDisplays;
DELETE FROM PlaylistItems;
DELETE FROM Playlists;
DELETE FROM Advertisements;
DELETE FROM DisplayDevices;
DELETE FROM Devices;
DELETE FROM Templates;
DELETE FROM Zones;

DBCC CHECKIDENT ('Zones', RESEED, 0);
DBCC CHECKIDENT ('Templates', RESEED, 0);
DBCC CHECKIDENT ('Devices', RESEED, 0);
DBCC CHECKIDENT ('DisplayDevices', RESEED, 0);
DBCC CHECKIDENT ('Advertisements', RESEED, 0);
DBCC CHECKIDENT ('Playlists', RESEED, 0);
DBCC CHECKIDENT ('PlaylistItems', RESEED, 0);
DBCC CHECKIDENT ('MatchDisplays', RESEED, 0);

-- Zones
INSERT INTO Zones (ZoneName, Remark) VALUES
    ('A', 'Zone A - Departures Hall'),
    ('B', 'Zone B - Arrivals Hall'),
    ('C', 'Zone C - Gate Area');

-- Templates
INSERT INTO Templates (Name, DeviceType, Ratio) VALUES
    ('bc-h',         'Baggage Claim',           '16:9'),
    ('bc-v',         'Baggage Claim',           '9:16'),
    ('bci-h',        'Baggage Claim Info',       '16:9'),
    ('ai-h',         'Arrival Information',     '16:9'),
    ('ai-v',         'Arrival Information',     '9:16'),
    ('ci-common',    'Check-in',                '16:9'),
    ('ci-dedicate',  'Check-in',                '16:9'),
    ('di-h',         'Departures Information',  '16:9'),
    ('di-v',         'Departures Information',  '9:16'),
    ('dg-h',         'Departures Gate',         '16:9'),
    ('dg-v',         'Departures Gate',         '9:16'),
    ('g-h',          'Gate',                    '16:9');

-- Devices (physical screens)
INSERT INTO Devices (DeviceName, DeviceType, Ratio, IPAddress, ZoneId) VALUES
    ('BC-H',          'Baggage Claim',          '16:9', '192.168.206.11', 1),
    ('BC-V',          'Baggage Claim',          '9:16', '192.168.206.12', 1),
    ('BCI-H',         'Baggage Claim Info',      '16:9', '192.168.206.21', 1),
    ('AI-H',          'Arrival Information',    '16:9', '192.168.206.31', 1),
    ('AI-V',          'Arrival Information',    '9:16', '192.168.206.32', 1),
    ('CI-H-Dedicate', 'Check-in',               '16:9', '192.168.206.42', 1),
    ('DI-H',          'Departures Information', '16:9', '192.168.206.51', 1),
    ('DI-V',          'Departures Information', '9:16', '192.168.206.52', 1),
    ('DG-H',          'Departures Gate',        '16:9', '192.168.206.61', 1),
    ('DG-V',          'Departures Gate',        '9:16', '192.168.206.62', 1),
    ('G-H',           'Gate',                   '16:9', '192.168.206.71', 1),
    ('BC-H-B',        'Baggage Claim',          '16:9', '192.168.206.81', 2),
    ('AI-H-B',        'Arrival Information',    '16:9', '192.168.206.82', 2),
    ('DG-H-C',        'Departures Gate',        '16:9', '192.168.206.91', 3);

-- DisplayDevices (logical display config)
INSERT INTO DisplayDevices (ZoneId, DeviceName, TemplateId, Ratio, DataSet, IsDisplayOn) VALUES
    (1, 'BC-H',          1,  '16:9', 'Belt 1',     1),
    (1, 'BC-V',          2,  '9:16', 'Belt 1',     1),
    (1, 'BCI-H',         3,  '16:9', 'Page 1',     1),
    (1, 'AI-H',          4,  '16:9', 'Page 1',     1),
    (1, 'AI-V',          5,  '9:16', 'Page 1',     1),
    (1, 'CI-H-Dedicate', 7,  '16:9', 'Counter 11', 1),
    (1, 'DI-H',          8,  '16:9', 'Page 1',     1),
    (1, 'DI-V',          9,  '9:16', 'Page 1',     1),
    (1, 'DG-H',          10, '16:9', 'Page 1',     1),
    (1, 'DG-V',          11, '9:16', 'Page 1',     1),
    (1, 'G-H',           12, '16:9', 'Gate 2',     1),
    (2, 'BC-H-B',        1,  '16:9', 'Belt 1',     1),
    (2, 'AI-H-B',        4,  '16:9', 'Page 1',     1),
    (3, 'DG-H-C',        10, '16:9', 'Gate 1',     1);

-- Advertisements
INSERT INTO Advertisements (Title, FileType, Duration, FileSizeMB, Month, Year, UploadDate) VALUES
    ('ads-1',   'Video', 15, 12.5,  3, 2026, GETDATE()),
    ('ads-2',   'Video', 16, 18.3,  3, 2026, GETDATE()),
    ('paiev-1', 'Video', 63, 45.2,  3, 2026, GETDATE()),
    ('paiev-2', 'Video', 53, 38.7,  3, 2026, GETDATE()),
    ('fids',    'Video', 10,  5.1,  3, 2026, GETDATE()),
    ('test',    'Video', 10,  4.8,  3, 2026, GETDATE()),
    ('img-1',   'Image',  0,  0.8,  3, 2026, GETDATE()),
    ('img-2',   'Image',  0,  1.2,  3, 2026, GETDATE());

-- Playlists
INSERT INTO Playlists (Name, CreatedAt) VALUES
    ('pl1',         GETDATE()),
    ('pl2',         GETDATE()),
    ('pl3',         GETDATE()),
    ('pl4',         GETDATE()),
    ('pl5',         GETDATE()),
    ('laopay',      GETDATE()),
    ('BC playlist', GETDATE());

-- PlaylistItems (link ads to playlists)
INSERT INTO PlaylistItems (PlaylistId, AdvertisementId, SortOrder) VALUES
    (1, 1, 1),
    (2, 2, 1),
    (3, 3, 1),
    (4, 4, 1),
    (5, 5, 1),
    (6, 6, 1), (6, 7, 2), (6, 8, 3),
    (7, 1, 1), (7, 2, 2);

-- MatchDisplays (match some display devices to playlists)
INSERT INTO MatchDisplays (PlaylistId, PlaylistName, DisplayId, DisplayName, IsMatched, MatchedAt) VALUES
    (1, 'pl1',         1,  'BC-H',          1, GETDATE()),
    (2, 'pl2',         4,  'AI-H',          1, GETDATE()),
    (6, 'laopay',      6,  'CI-H-Dedicate', 1, GETDATE()),
    (3, 'pl3',         7,  'DI-H',          1, GETDATE()),
    (7, 'BC playlist', 12, 'BC-H-B',        1, GETDATE());

PRINT 'Seed data inserted successfully.';
GO
