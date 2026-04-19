-- ============================================================
--  FIDS Production Database Schema  v2.0
--  SQL Server 2019+  |  Collation: Thai_100_CI_AS_SC_UTF8
-- ============================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'FidsDB')
BEGIN
    CREATE DATABASE FidsDB COLLATE Thai_100_CI_AS_SC_UTF8;
END
GO

USE FidsDB;
GO

-- ─────────────────────────────────────────────
--  REFERENCE TABLES
-- ─────────────────────────────────────────────

IF OBJECT_ID('airlines') IS NULL
CREATE TABLE airlines (
    code        VARCHAR(10)    NOT NULL PRIMARY KEY,
    name        NVARCHAR(100)  NOT NULL,
    name_th     NVARCHAR(100),
    logo_url    NVARCHAR(255),
    bg_color    VARCHAR(7)     DEFAULT '#1a1a2e',
    is_active   BIT            DEFAULT 1,
    created_at  DATETIME2      DEFAULT GETDATE()
);

IF OBJECT_ID('terminals') IS NULL
CREATE TABLE terminals (
    id          INT IDENTITY(1,1) PRIMARY KEY,
    code        VARCHAR(10)   NOT NULL UNIQUE,
    name        NVARCHAR(100) NOT NULL,
    is_active   BIT DEFAULT 1
);

IF OBJECT_ID('gates') IS NULL
CREATE TABLE gates (
    id          INT IDENTITY(1,1) PRIMARY KEY,
    gate_code   VARCHAR(10)   NOT NULL UNIQUE,
    terminal_id INT           REFERENCES terminals(id),
    concourse   VARCHAR(10),
    is_active   BIT DEFAULT 1
);

IF OBJECT_ID('belts') IS NULL
CREATE TABLE belts (
    id          INT IDENTITY(1,1) PRIMARY KEY,
    belt_code   VARCHAR(10)   NOT NULL UNIQUE,
    terminal_id INT           REFERENCES terminals(id),
    is_active   BIT DEFAULT 1
);

IF OBJECT_ID('zones') IS NULL
CREATE TABLE zones (
    id          INT IDENTITY(1,1) PRIMARY KEY,
    zone_code   VARCHAR(20)   NOT NULL UNIQUE,
    name        NVARCHAR(100) NOT NULL,
    description NVARCHAR(255),
    is_active   BIT DEFAULT 1
);

-- ─────────────────────────────────────────────
--  USERS & AUTH
-- ─────────────────────────────────────────────

IF OBJECT_ID('users') IS NULL
CREATE TABLE users (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    username        VARCHAR(50)    NOT NULL UNIQUE,
    password_hash   VARCHAR(255)   NOT NULL,
    full_name       NVARCHAR(100),
    email           VARCHAR(100),
    role            VARCHAR(20)    NOT NULL DEFAULT 'viewer'
                    CHECK (role IN ('admin','operator','viewer')),
    is_active       BIT            DEFAULT 1,
    last_login      DATETIME2,
    created_at      DATETIME2      DEFAULT GETDATE(),
    updated_at      DATETIME2      DEFAULT GETDATE()
);

IF OBJECT_ID('refresh_tokens') IS NULL
CREATE TABLE refresh_tokens (
    id          INT IDENTITY(1,1) PRIMARY KEY,
    user_id     INT          NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token_hash  VARCHAR(255) NOT NULL UNIQUE,
    expires_at  DATETIME2    NOT NULL,
    revoked     BIT          DEFAULT 0,
    ip_address  VARCHAR(45),
    created_at  DATETIME2    DEFAULT GETDATE()
);

-- ─────────────────────────────────────────────
--  FLIGHTS
-- ─────────────────────────────────────────────

IF OBJECT_ID('flights') IS NULL
CREATE TABLE flights (
    id                  INT IDENTITY(1,1) PRIMARY KEY,
    flight_no           VARCHAR(20)    NOT NULL,
    airline_code        VARCHAR(10)    REFERENCES airlines(code),
    flight_type         VARCHAR(3)     NOT NULL CHECK (flight_type IN ('DEP','ARR')),
    origin              VARCHAR(10),
    destination         VARCHAR(10),
    origin_name         NVARCHAR(100),
    destination_name    NVARCHAR(100),

    -- Departure times (for DEP flights)
    std                 DATETIME2,
    etd                 DATETIME2,
    atd                 DATETIME2,

    -- Arrival times (for ARR flights)
    sta                 DATETIME2,
    eta                 DATETIME2,
    ata                 DATETIME2,

    gate_id             INT            REFERENCES gates(id),
    belt_id             INT            REFERENCES belts(id),
    checkin_counter     NVARCHAR(50),
    status              VARCHAR(30)    NOT NULL DEFAULT 'Scheduled'
                        CHECK (status IN (
                            'Scheduled','On Time','Check-In Open','Boarding',
                            'Gate Closed','Final Call','Departed','Delayed',
                            'Cancelled','Landed','Expected','Diverted','No Show'
                        )),
    remark              NVARCHAR(500),
    delay_minutes       INT            DEFAULT 0,
    delay_reason        NVARCHAR(255),
    flight_date         DATE           NOT NULL,
    aircraft_type       VARCHAR(20),
    terminal            VARCHAR(10),
    is_international    BIT            DEFAULT 0,
    is_codeshare        BIT            DEFAULT 0,
    codeshare_no        VARCHAR(20),
    passenger_count     INT,
    created_at          DATETIME2      DEFAULT GETDATE(),
    updated_at          DATETIME2      DEFAULT GETDATE(),
    created_by          INT            REFERENCES users(id),
    updated_by          INT            REFERENCES users(id)
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_flights_date')
    CREATE INDEX idx_flights_date   ON flights(flight_date);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_flights_status')
    CREATE INDEX idx_flights_status ON flights(status);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_flights_type')
    CREATE INDEX idx_flights_type   ON flights(flight_type);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_flights_no')
    CREATE INDEX idx_flights_no     ON flights(flight_no);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_flights_airline')
    CREATE INDEX idx_flights_airline ON flights(airline_code);
GO

IF OBJECT_ID('flight_status_history') IS NULL
CREATE TABLE flight_status_history (
    id          BIGINT IDENTITY(1,1) PRIMARY KEY,
    flight_id   INT           NOT NULL REFERENCES flights(id) ON DELETE CASCADE,
    old_status  VARCHAR(30),
    new_status  VARCHAR(30)   NOT NULL,
    changed_by  INT           REFERENCES users(id),
    reason      NVARCHAR(500),
    timestamp   DATETIME2     DEFAULT GETDATE()
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_fsh_flight')
    CREATE INDEX idx_fsh_flight ON flight_status_history(flight_id);
GO

-- ─────────────────────────────────────────────
--  DISPLAY TEMPLATES
-- ─────────────────────────────────────────────

IF OBJECT_ID('templates') IS NULL
CREATE TABLE templates (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    name            NVARCHAR(100)  NOT NULL,
    type            VARCHAR(20)    NOT NULL
                    CHECK (type IN ('departure','arrival','gate','baggage','checkin','mixed','ads')),
    layout_json     NVARCHAR(MAX),
    css_vars        NVARCHAR(MAX),
    preview_url     NVARCHAR(255),
    rows_per_page   INT            DEFAULT 12,
    show_clock      BIT            DEFAULT 1,
    show_logo       BIT            DEFAULT 1,
    show_date       BIT            DEFAULT 1,
    font_scale      DECIMAL(3,1)   DEFAULT 1.0,
    is_active       BIT            DEFAULT 1,
    created_at      DATETIME2      DEFAULT GETDATE(),
    updated_at      DATETIME2      DEFAULT GETDATE()
);

-- ─────────────────────────────────────────────
--  SCREENS
-- ─────────────────────────────────────────────

IF OBJECT_ID('screens') IS NULL
CREATE TABLE screens (
    id               INT IDENTITY(1,1) PRIMARY KEY,
    screen_code      VARCHAR(50)    NOT NULL UNIQUE,
    name             NVARCHAR(100)  NOT NULL,
    type             VARCHAR(20)    NOT NULL
                     CHECK (type IN ('departure','arrival','gate','baggage','checkin','mixed','ads')),
    location         NVARCHAR(255),
    zone_id          INT            REFERENCES zones(id),
    template_id      INT            REFERENCES templates(id),
    gate_id          INT            REFERENCES gates(id),
    resolution       VARCHAR(20)    DEFAULT '1920x1080',
    orientation      VARCHAR(10)    DEFAULT 'landscape'
                     CHECK (orientation IN ('landscape','portrait')),
    config_json      NVARCHAR(MAX),
    status           VARCHAR(20)    DEFAULT 'offline'
                     CHECK (status IN ('online','offline','slow','error')),
    last_seen        DATETIME2,
    ip_address       VARCHAR(45),
    is_active        BIT            DEFAULT 1,
    rows_per_page    INT            DEFAULT 12,
    refresh_interval INT            DEFAULT 30,
    show_clock       BIT            DEFAULT 1,
    show_logo        BIT            DEFAULT 1,
    filter_json      NVARCHAR(MAX),
    created_at       DATETIME2      DEFAULT GETDATE(),
    updated_at       DATETIME2      DEFAULT GETDATE()
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_screens_code')
    CREATE INDEX idx_screens_code   ON screens(screen_code);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_screens_status')
    CREATE INDEX idx_screens_status ON screens(status);
GO

IF OBJECT_ID('screen_heartbeat') IS NULL
CREATE TABLE screen_heartbeat (
    id          INT IDENTITY(1,1) PRIMARY KEY,
    screen_id   INT           NOT NULL REFERENCES screens(id) ON DELETE CASCADE,
    last_ping   DATETIME2     DEFAULT GETDATE(),
    latency_ms  INT,
    status      VARCHAR(20)   DEFAULT 'online',
    ip_address  VARCHAR(45),
    user_agent  NVARCHAR(500)
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_hb_screen')
    CREATE INDEX idx_hb_screen ON screen_heartbeat(screen_id);
GO

-- ─────────────────────────────────────────────
--  ADVERTISEMENTS
-- ─────────────────────────────────────────────

IF OBJECT_ID('ads') IS NULL
CREATE TABLE ads (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    title           NVARCHAR(100)  NOT NULL,
    file_url        NVARCHAR(500)  NOT NULL,
    file_type       VARCHAR(10)    NOT NULL CHECK (file_type IN ('image','video')),
    file_size       BIGINT,
    duration        INT            DEFAULT 10,
    width           INT,
    height          INT,
    thumbnail_url   NVARCHAR(500),
    tags            NVARCHAR(255),
    is_active       BIT            DEFAULT 1,
    created_at      DATETIME2      DEFAULT GETDATE(),
    updated_at      DATETIME2      DEFAULT GETDATE()
);

IF OBJECT_ID('playlists') IS NULL
CREATE TABLE playlists (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    name            NVARCHAR(100)  NOT NULL,
    description     NVARCHAR(500),
    total_duration  INT            DEFAULT 0,
    is_active       BIT            DEFAULT 1,
    created_at      DATETIME2      DEFAULT GETDATE(),
    updated_at      DATETIME2      DEFAULT GETDATE()
);

IF OBJECT_ID('playlist_items') IS NULL
CREATE TABLE playlist_items (
    id          INT IDENTITY(1,1) PRIMARY KEY,
    playlist_id INT NOT NULL REFERENCES playlists(id)  ON DELETE CASCADE,
    ad_id       INT NOT NULL REFERENCES ads(id),
    order_no    INT NOT NULL DEFAULT 1,
    duration    INT,
    UNIQUE (playlist_id, order_no)
);

IF OBJECT_ID('screen_playlists') IS NULL
CREATE TABLE screen_playlists (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    screen_id       INT NOT NULL REFERENCES screens(id),
    playlist_id     INT NOT NULL REFERENCES playlists(id),
    start_time      TIME,
    end_time        TIME,
    days_of_week    VARCHAR(20),
    is_active       BIT DEFAULT 1,
    priority        INT DEFAULT 1,
    UNIQUE (screen_id, playlist_id)
);

-- ─────────────────────────────────────────────
--  SYSTEM & AUDIT
-- ─────────────────────────────────────────────

IF OBJECT_ID('system_settings') IS NULL
CREATE TABLE system_settings (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    setting_key     VARCHAR(100)   NOT NULL UNIQUE,
    setting_value   NVARCHAR(MAX),
    setting_group   VARCHAR(50)    DEFAULT 'general',
    description     NVARCHAR(500),
    updated_at      DATETIME2      DEFAULT GETDATE(),
    updated_by      INT            REFERENCES users(id)
);

IF OBJECT_ID('logs') IS NULL
CREATE TABLE logs (
    id          BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id     INT            REFERENCES users(id),
    username    VARCHAR(50),
    action      VARCHAR(100)   NOT NULL,
    entity_type VARCHAR(50),
    entity_id   VARCHAR(50),
    data        NVARCHAR(MAX),
    ip_address  VARCHAR(45),
    user_agent  NVARCHAR(500),
    status      VARCHAR(20)    DEFAULT 'success',
    created_at  DATETIME2      DEFAULT GETDATE()
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_logs_created')
    CREATE INDEX idx_logs_created ON logs(created_at DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_logs_action')
    CREATE INDEX idx_logs_action  ON logs(action);
GO

IF OBJECT_ID('alerts') IS NULL
CREATE TABLE alerts (
    id          INT IDENTITY(1,1) PRIMARY KEY,
    type        VARCHAR(50)    NOT NULL
                CHECK (type IN ('flight_delay','gate_change','screen_offline','system','custom')),
    title       NVARCHAR(200)  NOT NULL,
    message     NVARCHAR(MAX),
    entity_type VARCHAR(50),
    entity_id   INT,
    severity    VARCHAR(20)    DEFAULT 'info'
                CHECK (severity IN ('info','warning','critical')),
    is_read     BIT            DEFAULT 0,
    is_resolved BIT            DEFAULT 0,
    resolved_at DATETIME2,
    resolved_by INT            REFERENCES users(id),
    created_at  DATETIME2      DEFAULT GETDATE()
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_alerts_read')
    CREATE INDEX idx_alerts_read    ON alerts(is_read);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_alerts_created')
    CREATE INDEX idx_alerts_created ON alerts(created_at DESC);
GO

-- ─────────────────────────────────────────────
--  STORED PROCEDURES
-- ─────────────────────────────────────────────

GO
CREATE OR ALTER PROCEDURE sp_UpdateFlightStatus
    @flight_id  INT,
    @new_status VARCHAR(30),
    @reason     NVARCHAR(500) = NULL,
    @changed_by INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @old_status VARCHAR(30);
    SELECT @old_status = status FROM flights WHERE id = @flight_id;

    UPDATE flights
    SET status     = @new_status,
        updated_at = GETDATE(),
        updated_by = @changed_by
    WHERE id = @flight_id;

    INSERT INTO flight_status_history (flight_id, old_status, new_status, changed_by, reason)
    VALUES (@flight_id, @old_status, @new_status, @changed_by, @reason);
END;
GO

CREATE OR ALTER PROCEDURE sp_GetFlights
    @flight_date DATE        = NULL,
    @flight_type VARCHAR(3)  = NULL,
    @status      VARCHAR(30) = NULL,
    @airline     VARCHAR(10) = NULL,
    @search      NVARCHAR(50)= NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        f.id, f.flight_no, f.airline_code,
        a.name AS airline_name, a.logo_url AS airline_logo, a.bg_color AS airline_color,
        f.flight_type, f.origin, f.destination, f.origin_name, f.destination_name,
        f.std, f.etd, f.atd, f.sta, f.eta, f.ata,
        f.status, f.remark, f.delay_minutes, f.delay_reason,
        f.flight_date, f.aircraft_type, f.terminal, f.is_international,
        f.checkin_counter,
        g.gate_code, b.belt_code,
        f.created_at, f.updated_at
    FROM flights f
    LEFT JOIN airlines a  ON f.airline_code = a.code
    LEFT JOIN gates   g  ON f.gate_id      = g.id
    LEFT JOIN belts   b  ON f.belt_id      = b.id
    WHERE
        (@flight_date IS NULL OR f.flight_date  = @flight_date)
        AND (@flight_type IS NULL OR f.flight_type  = @flight_type)
        AND (@status      IS NULL OR f.status       = @status)
        AND (@airline     IS NULL OR f.airline_code = @airline)
        AND (@search      IS NULL OR f.flight_no LIKE '%' + @search + '%'
                                  OR f.origin    LIKE '%' + @search + '%'
                                  OR f.destination LIKE '%' + @search + '%')
    ORDER BY
        CASE f.flight_type WHEN 'DEP' THEN ISNULL(f.etd, f.std)
                           ELSE ISNULL(f.eta, f.sta) END;
END;
GO

CREATE OR ALTER PROCEDURE sp_UpdateScreenHeartbeat
    @screen_code VARCHAR(50),
    @ip_address  VARCHAR(45) = NULL,
    @latency_ms  INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @screen_id INT;
    SELECT @screen_id = id FROM screens WHERE screen_code = @screen_code AND is_active = 1;

    IF @screen_id IS NULL RETURN;

    UPDATE screens
    SET status    = 'online',
        last_seen = GETDATE(),
        ip_address = ISNULL(@ip_address, ip_address)
    WHERE id = @screen_id;

    INSERT INTO screen_heartbeat (screen_id, latency_ms, ip_address)
    VALUES (@screen_id, @latency_ms, @ip_address);

    -- Delete old heartbeat records (keep last 100)
    DELETE FROM screen_heartbeat
    WHERE screen_id = @screen_id
      AND id NOT IN (
          SELECT TOP 100 id FROM screen_heartbeat
          WHERE screen_id = @screen_id
          ORDER BY last_ping DESC
      );
END;
GO

-- ─────────────────────────────────────────────
--  SEED DATA
-- ─────────────────────────────────────────────

IF NOT EXISTS (SELECT 1 FROM terminals)
BEGIN
    INSERT INTO terminals (code, name) VALUES ('T1','Terminal 1'),('T2','Terminal 2');

    INSERT INTO gates (gate_code, terminal_id) VALUES
        ('A1',1),('A2',1),('A3',1),('A4',1),('A5',1),
        ('B1',1),('B2',1),('B3',1),('B4',1),('B5',1),
        ('C1',2),('C2',2),('C3',2),('C4',2),('C5',2);

    INSERT INTO belts (belt_code, terminal_id) VALUES
        ('Belt 1',1),('Belt 2',1),('Belt 3',1),('Belt 4',1),
        ('Belt 5',2),('Belt 6',2),('Belt 7',2),('Belt 8',2);

    INSERT INTO airlines (code,name,name_th,bg_color) VALUES
        ('TG','Thai Airways','การบินไทย','#4a1942'),
        ('PG','Bangkok Airways','บางกอกแอร์เวย์ส','#003087'),
        ('FD','Thai AirAsia','ไทยแอร์เอเชีย','#ff0000'),
        ('WE','Thai Smile','ไทยสมายล์','#9c27b0'),
        ('SL','Thai Lion Air','ไทยไลอ้อนแอร์','#ff6b00'),
        ('QR','Qatar Airways','กาตาร์ แอร์เวย์ส','#5c0632'),
        ('EK','Emirates','เอมิเรตส์','#003366'),
        ('SQ','Singapore Airlines','สิงคโปร์แอร์ไลน์ส','#00274d'),
        ('TZ','Scoot','สกู๊ต','#ffcc00'),
        ('MH','Malaysia Airlines','มาเลเซียแอร์ไลน์ส','#003399');

    INSERT INTO zones (zone_code,name) VALUES
        ('Z1','Zone A - Departure Hall'),
        ('Z2','Zone B - Arrival Hall'),
        ('Z3','Zone C - Gate Area'),
        ('Z4','Zone D - Baggage Claim'),
        ('Z5','Zone E - Check-in Area');

    INSERT INTO system_settings (setting_key,setting_value,setting_group,description) VALUES
        ('airport_name',    N'Suvarnabhumi Airport',   'display',  'Airport name on screens'),
        ('airport_name_th', N'ท่าอากาศยานสุวรรณภูมิ', 'display',  'Airport name in Thai'),
        ('airport_code',    'BKK',                      'display',  'IATA airport code'),
        ('timezone',        'Asia/Bangkok',              'system',   'System timezone'),
        ('date_format',     'DD/MM/YYYY',                'display',  'Display date format'),
        ('time_format',     'HH:mm',                     'display',  'Display time format'),
        ('default_lang',    'th',                        'display',  'Default display language'),
        ('alert_delay_min', '30',                        'alert',    'Minutes before delay alert'),
        ('screen_offline_s','120',                       'alert',    'Seconds before screen offline alert'),
        ('logo_url',        '/assets/images/logo.png',  'display',  'Airport logo path');
END;
GO

-- Default admin  (password: Admin@1234 — change on first login)
IF NOT EXISTS (SELECT 1 FROM users WHERE username = 'admin')
    INSERT INTO users (username,password_hash,full_name,role)
    VALUES ('admin',
            '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY3WLKO0vKqW9mm',
            'System Administrator',
            'admin');
GO
