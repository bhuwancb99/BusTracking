-- ============================================================
--  BUS TRACKING APPLICATION - Full Database Script
--  Platform : SQL Server (T-SQL)
--  Applications:
--    - Web  : .NET Web App  (Students, Parents, Super Admin, Bus Coordinator)
--    - Mobile: .NET MAUI App (Driver)
--    - API  : .NET Core API  (shared)
-- ============================================================

USE master;
GO

IF DB_ID('BusTrackingDB') IS NOT NULL
BEGIN
    ALTER DATABASE BusTrackingDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE BusTrackingDB;
END
GO

CREATE DATABASE BusTrackingDB;
GO

USE BusTrackingDB;
GO

-- ============================================================
-- 1. ROLES
-- ============================================================
CREATE TABLE Roles (
    RoleId      INT           NOT NULL IDENTITY(1,1),
    RoleName    NVARCHAR(50)  NOT NULL,                  -- SuperAdmin, BusCoordinator, Driver, Parent, Student
    Description NVARCHAR(255) NULL,
    IsActive    BIT           NOT NULL CONSTRAINT DF_Roles_IsActive DEFAULT 1,
    CreatedAt   DATETIME2     NOT NULL CONSTRAINT DF_Roles_CreatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Roles PRIMARY KEY (RoleId)
);
GO

INSERT INTO Roles (RoleName, Description) VALUES
('SuperAdmin',      'Full system access'),
('BusCoordinator',  'Sub-admin with limited permissions assigned by SuperAdmin'),
('Driver',          'Mobile app user – manages bus trips'),
('Parent',          'Web user – tracks kids and manages availability'),
('Student',         'Web user – tracks bus and manages own availability');
GO

-- ============================================================
-- 2. PERMISSIONS  (module-level permission catalogue)
-- ============================================================
CREATE TABLE Permissions (
    PermissionId   INT           NOT NULL IDENTITY(1,1),
    ModuleName     NVARCHAR(100) NOT NULL,   -- e.g. ManageBuses, ManageDrivers
    PermissionKey  NVARCHAR(100) NOT NULL,
    Description    NVARCHAR(255) NULL,
    CreatedAt      DATETIME2     NOT NULL CONSTRAINT DF_Permissions_CreatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Permissions PRIMARY KEY (PermissionId),
    CONSTRAINT UQ_Permissions_PermissionKey UNIQUE (PermissionKey)
);
GO

INSERT INTO Permissions (ModuleName, PermissionKey, Description) VALUES
('Dashboard',           'dashboard.view',           'View dashboard'),
('AppConfig',           'appconfig.view',           'View app configurations'),
('AppConfig',           'appconfig.add',            'Add app configuration'),
('AppConfig',           'appconfig.edit',           'Edit app configuration'),
('AppConfig',           'appconfig.delete',         'Delete app configuration'),
('ManageSubAdmins',     'subadmin.view',            'View sub-admins'),
('ManageSubAdmins',     'subadmin.add',             'Add sub-admin'),
('ManageSubAdmins',     'subadmin.edit',            'Edit sub-admin'),
('ManageSubAdmins',     'subadmin.delete',          'Delete sub-admin'),
('ManageRoutes',        'route.view',               'View routes'),
('ManageRoutes',        'route.add',                'Add route'),
('ManageRoutes',        'route.edit',               'Edit route'),
('ManageRoutes',        'route.delete',             'Delete route'),
('ManageBuses',         'bus.view',                 'View buses'),
('ManageBuses',         'bus.add',                  'Add bus'),
('ManageBuses',         'bus.edit',                 'Edit bus'),
('ManageBuses',         'bus.delete',               'Delete bus'),
('ManageBuses',         'bus.track',                'Track bus live'),
('ManageBusTypes',      'bustype.view',             'View bus types'),
('ManageBusTypes',      'bustype.add',              'Add bus type'),
('ManageBusTypes',      'bustype.edit',             'Edit bus type'),
('ManageBusTypes',      'bustype.delete',           'Delete bus type'),
('ManageDrivers',       'driver.view',              'View drivers'),
('ManageDrivers',       'driver.add',               'Add driver'),
('ManageDrivers',       'driver.edit',              'Edit driver'),
('ManageDrivers',       'driver.delete',            'Delete driver'),
('ManageDrivers',       'driver.track',             'Track driver live'),
('ManageParents',       'parent.view',              'View parents'),
('ManageParents',       'parent.add',               'Add parent'),
('ManageParents',       'parent.edit',              'Edit parent'),
('ManageParents',       'parent.delete',            'Delete parent'),
('ManageStudents',      'student.view',             'View students'),
('ManageStudents',      'student.add',              'Add student'),
('ManageStudents',      'student.edit',             'Edit student'),
('ManageStudents',      'student.delete',           'Delete student'),
('ManageTrips',         'trip.view',                'View trips'),
('ManageTrips',         'trip.manage',              'Manage trips (start/stop/track)'),
('ManageStudents',      'student.assignbus',        'Assign bus to student'),
('ManageNotifications', 'notification.manage',      'Enable/disable notifications'),
('HelpSupport',         'helpsupport.view',         'View help & support requests'),
('HelpSupport',         'helpsupport.manage',       'Manage help & support status'),
('ManageLogs',          'logs.view',                'View system logs');
GO

-- ============================================================
-- 3. USERS  (unified users table for all roles)
-- ============================================================
CREATE TABLE Users (
    UserId          INT            NOT NULL IDENTITY(1,1),
    RoleId          INT            NOT NULL,
    FullName        NVARCHAR(150)  NOT NULL,
    UserName        NVARCHAR(100)  NOT NULL,                  -- used for login (required)
    Email           NVARCHAR(255)  NULL,                      -- optional, for notifications/reset
    PhoneNumber     NVARCHAR(20)   NULL,
    PasswordHash    NVARCHAR(512)  NOT NULL,
    PasswordSalt    NVARCHAR(256)  NOT NULL,
    ProfileImageUrl NVARCHAR(500)  NULL,
    IsActive        BIT            NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT 1,
    IsEmailVerified BIT            NOT NULL CONSTRAINT DF_Users_IsEmailVerified DEFAULT 0,
    LastLoginAt     DATETIME2      NULL,
    CreatedAt       DATETIME2      NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2      NOT NULL CONSTRAINT DF_Users_UpdatedAt DEFAULT GETUTCDATE(),
    CreatedBy       INT            NULL,
    CONSTRAINT PK_Users PRIMARY KEY (UserId),
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId),
    CONSTRAINT FK_Users_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(UserId),
    CONSTRAINT UQ_Users_UserName UNIQUE (UserName),
    CONSTRAINT UQ_Users_Email UNIQUE (Email)
);
GO

SET IDENTITY_INSERT Users ON;
INSERT INTO Users (UserId, RoleId, FullName, UserName, Email, PhoneNumber, PasswordHash, PasswordSalt, ProfileImageUrl, IsActive, IsEmailVerified, LastLoginAt, CreatedAt, UpdatedAt, CreatedBy)
VALUES (1, 1, 'SuperAdmin', 'Admin', 'admin@bustracking.com', NULL, '$2a$12$gRiCpH9Cj4ztBpZsTgntH.BM2d/G9mO6VmcbIKD7gRdkk4vT3PpoW', '$2a$12$gRiCpH9Cj4ztBpZsTgntH.', NULL, 1, 1, GETDATE(), GETDATE(), GETDATE(), 1);
SET IDENTITY_INSERT Users OFF;
GO

-- ============================================================
-- 4. PASSWORD RESET TOKENS
-- ============================================================
CREATE TABLE PasswordResetTokens (
    TokenId    INT           NOT NULL IDENTITY(1,1),
    UserId     INT           NOT NULL,
    Token      NVARCHAR(512) NOT NULL,
    ExpiresAt  DATETIME2     NOT NULL,
    IsUsed     BIT           NOT NULL CONSTRAINT DF_PasswordResetTokens_IsUsed DEFAULT 0,
    CreatedAt  DATETIME2     NOT NULL CONSTRAINT DF_PasswordResetTokens_CreatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_PasswordResetTokens PRIMARY KEY (TokenId),
    CONSTRAINT FK_PasswordResetTokens_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT UQ_PasswordResetTokens_Token UNIQUE (Token)
);
GO

-- ============================================================
-- 5. SUB-ADMIN PERMISSIONS  (BusCoordinator role assignments)
-- ============================================================
CREATE TABLE SubAdminPermissions (
    SubAdminPermissionId INT       NOT NULL IDENTITY(1,1),
    UserId               INT       NOT NULL,                            -- must be BusCoordinator
    PermissionId         INT       NOT NULL,
    AssignedBy           INT       NOT NULL,                            -- SuperAdmin
    AssignedAt           DATETIME2 NOT NULL CONSTRAINT DF_SubAdminPermissions_AssignedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_SubAdminPermissions PRIMARY KEY (SubAdminPermissionId),
    CONSTRAINT FK_SubAdminPermissions_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT FK_SubAdminPermissions_Permissions FOREIGN KEY (PermissionId) REFERENCES Permissions(PermissionId),
    CONSTRAINT FK_SubAdminPermissions_AssignedBy FOREIGN KEY (AssignedBy) REFERENCES Users(UserId),
    CONSTRAINT UQ_SubAdmin_Permission UNIQUE (UserId, PermissionId)
);
GO

-- ============================================================
-- 6. ROUTES
-- ============================================================
CREATE TABLE Routes (
    RouteId       INT           NOT NULL IDENTITY(1,1),
    RouteName     NVARCHAR(150) NOT NULL,
    RouteCode     NVARCHAR(50)  NOT NULL,
    MorningTime   TIME          NULL,
    EveningTime   TIME          NULL,
    Description   NVARCHAR(500) NULL,
    IsActive      BIT           NOT NULL CONSTRAINT DF_Routes_IsActive DEFAULT 1,
    CreatedAt     DATETIME2     NOT NULL CONSTRAINT DF_Routes_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt     DATETIME2     NOT NULL CONSTRAINT DF_Routes_UpdatedAt DEFAULT GETUTCDATE(),
    CreatedBy     INT           NULL,
    CONSTRAINT PK_Routes PRIMARY KEY (RouteId),
    CONSTRAINT FK_Routes_Users FOREIGN KEY (CreatedBy) REFERENCES Users(UserId),
    CONSTRAINT UQ_Routes_RouteCode UNIQUE (RouteCode)
);
GO

-- ============================================================
-- 7. STOPS  (drop/pick-up points on a route)
-- ============================================================
CREATE TABLE Stops (
    StopId      INT            NOT NULL IDENTITY(1,1),
    RouteId     INT            NOT NULL,
    StopName    NVARCHAR(150)  NOT NULL,
    StopOrder   INT            NOT NULL,                            -- sequence on the route
    Latitude    DECIMAL(10,7)  NULL,
    Longitude   DECIMAL(10,7)  NULL,
    MorningTime TIME           NULL,                                -- expected arrival at stop (morning)
    EveningTime TIME           NULL,                                -- expected arrival at stop (evening)
    IsActive    BIT            NOT NULL CONSTRAINT DF_Stops_IsActive DEFAULT 1,
    CreatedAt   DATETIME2      NOT NULL CONSTRAINT DF_Stops_CreatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Stops PRIMARY KEY (StopId),
    CONSTRAINT FK_Stops_Routes FOREIGN KEY (RouteId) REFERENCES Routes(RouteId),
    CONSTRAINT UQ_Route_StopOrder UNIQUE (RouteId, StopOrder)
);
GO

-- ============================================================
-- 7a. BUS TYPE MASTERS  (Mini Bus, Standard Bus, Luxury Bus, ...)
-- ============================================================
CREATE TABLE BusTypeMasters (
    Id        INT           NOT NULL IDENTITY(1,1),
    Name      NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2     NOT NULL CONSTRAINT DF_BusTypeMasters_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2     NOT NULL CONSTRAINT DF_BusTypeMasters_UpdatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_BusTypeMasters PRIMARY KEY (Id),
    CONSTRAINT UQ_BusTypeMasters_Name UNIQUE (Name)
);
GO

-- ============================================================
-- 8. BUSES
-- ============================================================
CREATE TABLE Buses (
    BusId         INT           NOT NULL IDENTITY(1,1),
    BusName       NVARCHAR(100) NOT NULL,
    BusNumber     NVARCHAR(50)  NOT NULL,
    RouteId       INT           NULL,
    BusTypeId     INT           NOT NULL,
    Capacity      INT           NULL,
    IsActive      BIT           NOT NULL CONSTRAINT DF_Buses_IsActive DEFAULT 1,
    CreatedAt     DATETIME2     NOT NULL CONSTRAINT DF_Buses_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt     DATETIME2     NOT NULL CONSTRAINT DF_Buses_UpdatedAt DEFAULT GETUTCDATE(),
    CreatedBy     INT           NULL,
    CONSTRAINT PK_Buses PRIMARY KEY (BusId),
    CONSTRAINT FK_Buses_Routes FOREIGN KEY (RouteId) REFERENCES Routes(RouteId),
    CONSTRAINT FK_Buses_BusTypeMasters FOREIGN KEY (BusTypeId) REFERENCES BusTypeMasters(Id),
    CONSTRAINT FK_Buses_Users FOREIGN KEY (CreatedBy) REFERENCES Users(UserId),
    CONSTRAINT UQ_Buses_BusNumber UNIQUE (BusNumber)
);
GO

-- ============================================================
-- 9. DRIVER DETAILS  (extra fields for Driver role users)
-- ============================================================
CREATE TABLE DriverDetails (
    DriverDetailId  INT           NOT NULL IDENTITY(1,1),
    UserId          INT           NOT NULL,
    LicenseNumber   NVARCHAR(100) NULL,
    LicenseExpiry   DATE          NULL,
    BusId           INT           NULL,                              -- currently assigned bus
    CreatedAt       DATETIME2     NOT NULL CONSTRAINT DF_DriverDetails_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2     NOT NULL CONSTRAINT DF_DriverDetails_UpdatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_DriverDetails PRIMARY KEY (DriverDetailId),
    CONSTRAINT FK_DriverDetails_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT FK_DriverDetails_Buses FOREIGN KEY (BusId) REFERENCES Buses(BusId),
    CONSTRAINT UQ_DriverDetails_UserId UNIQUE (UserId)
);
GO

-- ============================================================
-- 10. STUDENTS
-- ============================================================
CREATE TABLE Students (
    StudentId     INT           NOT NULL IDENTITY(1,1),
    UserId        INT           NOT NULL,
    StudentCode   NVARCHAR(50)  NOT NULL,                            -- Student ID / Roll number
    Standard      NVARCHAR(50)  NULL,                                -- Class / Grade
    BusId         INT           NULL,
    StopId        INT           NULL,                                -- assigned pick-up/drop stop
    CreatedAt     DATETIME2     NOT NULL CONSTRAINT DF_Students_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt     DATETIME2     NOT NULL CONSTRAINT DF_Students_UpdatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Students PRIMARY KEY (StudentId),
    CONSTRAINT FK_Students_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT FK_Students_Buses FOREIGN KEY (BusId) REFERENCES Buses(BusId),
    CONSTRAINT FK_Students_Stops FOREIGN KEY (StopId) REFERENCES Stops(StopId),
    CONSTRAINT UQ_Students_UserId UNIQUE (UserId),
    CONSTRAINT UQ_Students_StudentCode UNIQUE (StudentCode)
);
GO

-- ============================================================
-- 11. PARENTS
-- ============================================================
CREATE TABLE Parents (
    ParentId   INT       NOT NULL IDENTITY(1,1),
    UserId     INT       NOT NULL,
    CreatedAt  DATETIME2 NOT NULL CONSTRAINT DF_Parents_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt  DATETIME2 NOT NULL CONSTRAINT DF_Parents_UpdatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Parents PRIMARY KEY (ParentId),
    CONSTRAINT FK_Parents_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT UQ_Parents_UserId UNIQUE (UserId)
);
GO

-- ============================================================
-- 12. PARENT-STUDENT  (a parent can have multiple kids)
-- ============================================================
CREATE TABLE ParentStudents (
    ParentStudentId INT       NOT NULL IDENTITY(1,1),
    ParentId        INT       NOT NULL,
    StudentId       INT       NOT NULL,
    CreatedAt       DATETIME2 NOT NULL CONSTRAINT DF_ParentStudents_CreatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_ParentStudents PRIMARY KEY (ParentStudentId),
    CONSTRAINT FK_ParentStudents_Parents FOREIGN KEY (ParentId) REFERENCES Parents(ParentId),
    CONSTRAINT FK_ParentStudents_Students FOREIGN KEY (StudentId) REFERENCES Students(StudentId),
    CONSTRAINT UQ_Parent_Student UNIQUE (ParentId, StudentId)
);
GO

-- ============================================================
-- 13. STUDENT AVAILABILITY
--     (Student or Parent marks leave / not going / parent-pick)
-- ============================================================
CREATE TABLE StudentAvailabilities (
    AvailabilityId   INT           NOT NULL IDENTITY(1,1),
    StudentId        INT           NOT NULL,
    AvailabilityType NVARCHAR(50)  NOT NULL,                        -- OnLeave | NotGoingToSchool | ParentPickup
    FromDate         DATE          NOT NULL,
    ToDate           DATE          NOT NULL,
    Remarks          NVARCHAR(500) NULL,
    MarkedBy         INT           NOT NULL,                        -- Student or Parent userId
    CreatedAt        DATETIME2     NOT NULL CONSTRAINT DF_StudentAvailabilities_CreatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_StudentAvailabilities PRIMARY KEY (AvailabilityId),
    CONSTRAINT FK_StudentAvailabilities_Students FOREIGN KEY (StudentId) REFERENCES Students(StudentId),
    CONSTRAINT FK_StudentAvailabilities_Users FOREIGN KEY (MarkedBy) REFERENCES Users(UserId),
    CONSTRAINT CK_AvailabilityType CHECK (AvailabilityType IN ('OnLeave','NotGoingToSchool','ParentPickup')),
    CONSTRAINT CK_DateRange CHECK (ToDate >= FromDate)
);
GO

-- ============================================================
-- 14. BUS TRIPS  (each tracking session started by a driver)
-- ============================================================
CREATE TABLE BusTrips (
    TripId      INT           NOT NULL IDENTITY(1,1),
    BusId       INT           NOT NULL,
    DriverId    INT           NOT NULL,
    RouteId     INT           NOT NULL,
    TripType    NVARCHAR(20)  NOT NULL,                              -- Morning | Evening
    TripDate    DATE          NOT NULL,
    StartedAt   DATETIME2     NULL,
    EndedAt     DATETIME2     NULL,
    Status      NVARCHAR(20)  NOT NULL CONSTRAINT DF_BusTrips_Status DEFAULT 'Scheduled',  -- Scheduled | InProgress | Completed | Cancelled
    CreatedAt   DATETIME2     NOT NULL CONSTRAINT DF_BusTrips_CreatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_BusTrips PRIMARY KEY (TripId),
    CONSTRAINT FK_BusTrips_Buses FOREIGN KEY (BusId) REFERENCES Buses(BusId),
    CONSTRAINT FK_BusTrips_Users FOREIGN KEY (DriverId) REFERENCES Users(UserId),
    CONSTRAINT FK_BusTrips_Routes FOREIGN KEY (RouteId) REFERENCES Routes(RouteId),
    CONSTRAINT CK_TripType   CHECK (TripType IN ('Morning','Evening')),
    CONSTRAINT CK_TripStatus CHECK (Status   IN ('Scheduled','InProgress','Completed','Cancelled'))
);
GO

-- ============================================================
-- 15. TRIP STOP EVENTS  (driver marks reached/departed per stop)
-- ============================================================
CREATE TABLE TripStopEvents (
    TripStopEventId INT           NOT NULL IDENTITY(1,1),
    TripId          INT           NOT NULL,
    StopId          INT           NOT NULL,
    ReachedAt       DATETIME2     NULL,
    DepartedAt      DATETIME2     NULL,
    Status          NVARCHAR(20)  NOT NULL CONSTRAINT DF_TripStopEvents_Status DEFAULT 'Pending',  -- Pending | Reached | Departed
    CreatedAt       DATETIME2     NOT NULL CONSTRAINT DF_TripStopEvents_CreatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_TripStopEvents PRIMARY KEY (TripStopEventId),
    CONSTRAINT FK_TripStopEvents_BusTrips FOREIGN KEY (TripId) REFERENCES BusTrips(TripId),
    CONSTRAINT FK_TripStopEvents_Stops FOREIGN KEY (StopId) REFERENCES Stops(StopId),
    CONSTRAINT CK_TripStopStatus CHECK (Status IN ('Pending','Reached','Departed'))
);
GO

-- ============================================================
-- 16. STUDENT TRIP STATUS  (per-student boarding status per trip)
-- ============================================================
CREATE TABLE StudentTripStatus (
    StudentTripStatusId INT          NOT NULL IDENTITY(1,1),
    TripId              INT          NOT NULL,
    StudentId           INT          NOT NULL,
    StopId              INT          NOT NULL,
    BoardingStatus      NVARCHAR(20) NOT NULL CONSTRAINT DF_StudentTripStatus_BoardingStatus DEFAULT 'Pending', -- Pending | PickedUp | NoShow | OnLeave
    UpdatedAt           DATETIME2    NOT NULL CONSTRAINT DF_StudentTripStatus_UpdatedAt DEFAULT GETUTCDATE(),
    UpdatedBy           INT          NULL,                                                  -- Driver userId
    CONSTRAINT PK_StudentTripStatus PRIMARY KEY (StudentTripStatusId),
    CONSTRAINT FK_StudentTripStatus_BusTrips FOREIGN KEY (TripId) REFERENCES BusTrips(TripId),
    CONSTRAINT FK_StudentTripStatus_Students FOREIGN KEY (StudentId) REFERENCES Students(StudentId),
    CONSTRAINT FK_StudentTripStatus_Stops FOREIGN KEY (StopId) REFERENCES Stops(StopId),
    CONSTRAINT FK_StudentTripStatus_Users FOREIGN KEY (UpdatedBy) REFERENCES Users(UserId),
    CONSTRAINT UQ_Trip_Student UNIQUE (TripId, StudentId),
    CONSTRAINT CK_BoardingStatus CHECK (BoardingStatus IN ('Pending','PickedUp','NoShow','OnLeave'))
);
GO

-- ============================================================
-- 17. BUS LIVE LOCATION  (GPS pings from driver mobile app)
-- ============================================================
CREATE TABLE BusLiveLocation (
    LocationId  BIGINT         NOT NULL IDENTITY(1,1),
    TripId      INT            NOT NULL,
    BusId       INT            NOT NULL,
    Latitude    DECIMAL(10,7)  NOT NULL,
    Longitude   DECIMAL(10,7)  NOT NULL,
    Speed       DECIMAL(6,2)   NULL,                                  -- km/h
    Heading     DECIMAL(6,2)   NULL,                                  -- degrees
    RecordedAt  DATETIME2      NOT NULL CONSTRAINT DF_BusLiveLocation_RecordedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_BusLiveLocation PRIMARY KEY (LocationId),
    CONSTRAINT FK_BusLiveLocation_BusTrips FOREIGN KEY (TripId) REFERENCES BusTrips(TripId),
    CONSTRAINT FK_BusLiveLocation_Buses FOREIGN KEY (BusId) REFERENCES Buses(BusId)
);
GO

-- Index for fast latest-location queries
CREATE NONCLUSTERED INDEX IX_BusLiveLocation_Trip_Time
    ON BusLiveLocation (TripId, RecordedAt DESC);
GO

-- ============================================================
-- 18. NOTIFICATIONS
-- ============================================================
CREATE TABLE Notifications (
    NotificationId   INT            NOT NULL IDENTITY(1,1),
    RecipientUserId  INT            NOT NULL,
    Title            NVARCHAR(200)  NOT NULL,
    Body             NVARCHAR(1000) NOT NULL,
    NotificationType NVARCHAR(50)   NOT NULL,                        -- BusApproaching | StudentPickedUp | NoShow | BusAssigned | RouteChanged | Broadcast
    ReferenceId      INT            NULL,                             -- e.g. TripId or BusId
    ReferenceType    NVARCHAR(50)   NULL,                             -- Trip | Bus | Route
    IsRead           BIT            NOT NULL CONSTRAINT DF_Notifications_IsRead DEFAULT 0,
    SentAt           DATETIME2      NOT NULL CONSTRAINT DF_Notifications_SentAt DEFAULT GETUTCDATE(),
    ReadAt           DATETIME2      NULL,
    CONSTRAINT PK_Notifications PRIMARY KEY (NotificationId),
    CONSTRAINT FK_Notifications_Users FOREIGN KEY (RecipientUserId) REFERENCES Users(UserId)
);
GO

CREATE NONCLUSTERED INDEX IX_Notifications_Recipient_Unread
    ON Notifications (RecipientUserId, IsRead, SentAt DESC);
GO

-- ============================================================
-- 19. NOTIFICATION SETTINGS  (enable/disable notification types)
-- ============================================================
CREATE TABLE NotificationSettings (
    NotificationSettingId INT           NOT NULL IDENTITY(1,1),
    NotificationType      NVARCHAR(50)  NOT NULL,
    IsEnabled             BIT           NOT NULL CONSTRAINT DF_NotificationSettings_IsEnabled DEFAULT 1,
    UpdatedAt             DATETIME2     NOT NULL CONSTRAINT DF_NotificationSettings_UpdatedAt DEFAULT GETUTCDATE(),
    UpdatedBy             INT           NULL,
    CONSTRAINT PK_NotificationSettings PRIMARY KEY (NotificationSettingId),
    CONSTRAINT FK_NotificationSettings_Users FOREIGN KEY (UpdatedBy) REFERENCES Users(UserId),
    CONSTRAINT UQ_NotificationSettings_NotificationType UNIQUE (NotificationType)
);
GO

INSERT INTO NotificationSettings (NotificationType, IsEnabled) VALUES
('BusApproaching',    1),
('StudentPickedUp',   1),
('NoShow',            1),
('BusAssigned',       1),
('RouteChanged',      1),
('Broadcast',         1),
('ContentUpdate',     1),
('ActivityUpdate',    1);
GO

-- ============================================================
-- 20. DEVICE TOKENS  (FCM / APNs push tokens per user device)
-- ============================================================
CREATE TABLE DeviceTokens (
    DeviceTokenId INT           NOT NULL IDENTITY(1,1),
    UserId        INT           NOT NULL,
    Token         NVARCHAR(512) NOT NULL,
    Platform      NVARCHAR(20)  NOT NULL,                            -- Android | iOS | Web
    IsActive      BIT           NOT NULL CONSTRAINT DF_DeviceTokens_IsActive DEFAULT 1,
    RegisteredAt  DATETIME2     NOT NULL CONSTRAINT DF_DeviceTokens_RegisteredAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_DeviceTokens PRIMARY KEY (DeviceTokenId),
    CONSTRAINT FK_DeviceTokens_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT CK_DevicePlatform CHECK (Platform IN ('Android','iOS','Web'))
);
GO

-- ============================================================
-- 21. FEEDBACK / HELP & SUPPORT
-- ============================================================
CREATE TABLE Feedbacks (
    FeedbackId   INT            NOT NULL IDENTITY(1,1),
    UserId       INT            NOT NULL,
    Category     NVARCHAR(50)   NOT NULL,                            -- Inquiry | Complaint
    Email        NVARCHAR(255)  NOT NULL,
    PhoneNumber  NVARCHAR(20)   NULL,
    Description  NVARCHAR(2000) NOT NULL,
    Status       NVARCHAR(50)   NOT NULL CONSTRAINT DF_Feedbacks_Status DEFAULT 'Open',  -- Open | InProgress | Resolved | Closed
    ResolvedBy   INT            NULL,
    ResolvedAt   DATETIME2      NULL,
    CreatedAt    DATETIME2      NOT NULL CONSTRAINT DF_Feedbacks_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt    DATETIME2      NOT NULL CONSTRAINT DF_Feedbacks_UpdatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Feedbacks PRIMARY KEY (FeedbackId),
    CONSTRAINT FK_Feedbacks_Users_User FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT FK_Feedbacks_Users_ResolvedBy FOREIGN KEY (ResolvedBy) REFERENCES Users(UserId),
    CONSTRAINT CK_FeedbackCategory CHECK (Category IN ('Inquiry','Complaint')),
    CONSTRAINT CK_FeedbackStatus   CHECK (Status   IN ('Open','InProgress','Resolved','Closed'))
);
GO

-- ============================================================
-- 22. AUDIT LOG  (optional – track key entity changes)
-- ============================================================
CREATE TABLE AuditLogs (
    AuditLogId  BIGINT        NOT NULL IDENTITY(1,1),
    UserId      INT           NULL,
    Action      NVARCHAR(100) NOT NULL,                              -- e.g. UserCreated, BusAssigned
    EntityName  NVARCHAR(100) NULL,
    EntityId    NVARCHAR(50)  NULL,
    OldValues   NVARCHAR(MAX) NULL,                                  -- JSON
    NewValues   NVARCHAR(MAX) NULL,                                  -- JSON
    IpAddress   NVARCHAR(50)  NULL,
    CreatedAt   DATETIME2     NOT NULL CONSTRAINT DF_AuditLogs_CreatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_AuditLogs PRIMARY KEY (AuditLogId),
    CONSTRAINT FK_AuditLogs_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

-- ============================================================
-- 22B. LOGGER
-- ============================================================
CREATE TABLE Logger (
    LogId             INT           NOT NULL IDENTITY(1,1),
    Platform          NVARCHAR(50)  NOT NULL,                  -- WEB, API, Android, iOS, Windows, macOS
    Timestamp         DATETIME2     NOT NULL CONSTRAINT DF_Logger_Timestamp DEFAULT GETUTCDATE(),
    ExceptionMessage  NVARCHAR(MAX) NULL,
    StackTrace        NVARCHAR(MAX) NULL,
    RequestUrl        NVARCHAR(2083) NULL,
    UserId            INT           NULL,
    Username          NVARCHAR(256) NULL,
    Role              NVARCHAR(50)  NULL,
    ModuleName        NVARCHAR(100) NULL,
    ActionName        NVARCHAR(100) NULL,
    AdditionalDetails NVARCHAR(MAX) NULL,
    CONSTRAINT PK_Logger PRIMARY KEY (LogId)
);
GO
CREATE NONCLUSTERED INDEX IX_Logger_Timestamp ON Logger (Timestamp DESC);
CREATE NONCLUSTERED INDEX IX_Logger_Platform  ON Logger (Platform);
GO

-- ============================================================
-- 23. APP CONFIGURATIONS
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AppConfigurations')
BEGIN
    CREATE TABLE AppConfigurations (
        ConfigId     INT IDENTITY(1,1),
        ConfigKey    NVARCHAR(100) NOT NULL,
        ConfigValue  NVARCHAR(500) NOT NULL,
        Description  NVARCHAR(200) NULL,
        Platform     NVARCHAR(20)  NOT NULL CONSTRAINT DF_AppConfigurations_Platform DEFAULT 'Both',  -- Web | Mobile | Both
        IsActive     BIT           NOT NULL CONSTRAINT DF_AppConfigurations_IsActive DEFAULT 1,
        CreatedBy    INT           NOT NULL,
        CreatedAt    DATETIME2     NOT NULL CONSTRAINT DF_AppConfigurations_CreatedAt DEFAULT GETUTCDATE(),
        UpdatedAt    DATETIME2     NOT NULL CONSTRAINT DF_AppConfigurations_UpdatedAt DEFAULT GETUTCDATE(),

        CONSTRAINT PK_AppConfigurations PRIMARY KEY (ConfigId),
        CONSTRAINT FK_AppConfigurations_Users FOREIGN KEY (CreatedBy) REFERENCES Users(UserId),
        CONSTRAINT UQ_AppConfigurations_Key_Platform UNIQUE (ConfigKey, Platform)
    );

    CREATE INDEX IX_AppConfigurations_Platform_Active
        ON AppConfigurations (Platform, IsActive);
END
GO

-- ============================================================
-- 24. BUS IMAGES
-- ============================================================

CREATE TABLE BusImages (
    BusImageId   INT           NOT NULL IDENTITY(1,1),
    BusId        INT           NOT NULL,
    ImageUrl     NVARCHAR(500) NOT NULL,
    DisplayOrder INT           NOT NULL CONSTRAINT DF_BusImages_DisplayOrder DEFAULT 0,
    IsPrimary    BIT           NOT NULL CONSTRAINT DF_BusImages_IsPrimary DEFAULT 0,
    UploadedAt   DATETIME2     NOT NULL CONSTRAINT DF_BusImages_UploadedAt DEFAULT GETUTCDATE(),
    UploadedBy   INT           NULL,
    CONSTRAINT PK_BusImages PRIMARY KEY (BusImageId),
    CONSTRAINT FK_BusImages_Buses FOREIGN KEY (BusId) REFERENCES Buses(BusId) ON DELETE CASCADE,
    CONSTRAINT FK_BusImages_Users FOREIGN KEY (UploadedBy) REFERENCES Users(UserId)
);
GO

CREATE INDEX IX_BusImages_BusId ON BusImages(BusId);
GO

-- ── Seed default Mobile config keys ──────────────────────────────────
-- (Only inserts if key doesn't already exist)

INSERT INTO AppConfigurations (ConfigKey, ConfigValue, Description, Platform, IsActive, CreatedBy)
SELECT * FROM (VALUES
    ('IsMaintencePage',    '0',   'Set to 1 to show maintenance screen on app launch',       'Mobile', 1, 1),
    ('MandatoryUpdateApp', '0',   'Set to 1 to force users to update the app',                'Mobile', 1, 1),
    ('AndroidVersion',      '1.0.0','','Mobile', 1, 1),
    ('iOSVersion',      '1.0.0','','Mobile', 1, 1),
    ('Android_Update_Url', '',    'Google Play Store URL for the Android app',                'Mobile', 1, 1),
    ('iOS_Update_Url',     '',    'Apple App Store URL for the iOS app',                      'Mobile', 1, 1),
    ('GpsIntervalSeconds', '10',  'How often the driver app sends GPS pings (seconds)',        'Mobile', 1, 1),
    ('SupportEmail',       '',    'Support email shown inside the mobile app',                'Mobile', 1, 1),
    ('SupportPhone',       '',    'Support phone number shown inside the mobile app',         'Mobile', 1, 1),
    ('IsMobileUpdateImage',       '1',    'When true: app uploads images via API and shows Upload/Remove buttons',         'Mobile', 1, 1),
    ('WebsiteImageUrl',       'https://10.0.2.2:7001',    'Used to construct full image URLs when IsMobileUpdateImage = 1',         'Mobile', 1, 1)
) AS v(ConfigKey, ConfigValue, Description, Platform, IsActive, CreatedBy)
WHERE NOT EXISTS (
    SELECT 1 FROM AppConfigurations
    WHERE ConfigKey = v.ConfigKey AND Platform = v.Platform
);
GO

-- ============================================================
-- VIEWS
-- ============================================================

-- V1: Active students with bus and stop info
CREATE VIEW vw_StudentBusInfo AS
SELECT
    s.StudentId,
    s.StudentCode,
    u.FullName          AS StudentName,
    u.Email             AS StudentEmail,
    s.Standard,
    b.BusId,
    b.BusName,
    b.BusNumber,
    r.RouteId,
    r.RouteName,
    r.RouteCode,
    st.StopId,
    st.StopName,
    st.StopOrder,
    st.MorningTime      AS StopMorningTime,
    st.EveningTime      AS StopEveningTime
FROM Students s
JOIN Users   u  ON u.UserId  = s.UserId
LEFT JOIN Buses  b  ON b.BusId   = s.BusId
LEFT JOIN Routes r  ON r.RouteId = b.RouteId
LEFT JOIN Stops  st ON st.StopId = s.StopId
WHERE u.IsActive = 1;
GO

-- V2: Active bus with assigned driver
CREATE VIEW vw_BusDriverInfo AS
SELECT
    b.BusId,
    b.BusName,
    b.BusNumber,
    b.Capacity,
    r.RouteId,
    r.RouteName,
    r.RouteCode,
    r.MorningTime       AS RouteMorningTime,
    r.EveningTime       AS RouteEveningTime,
    u.UserId            AS DriverUserId,
    u.FullName          AS DriverName,
    u.PhoneNumber       AS DriverPhone,
    u.Email             AS DriverEmail
FROM Buses b
LEFT JOIN Routes       r  ON r.RouteId = b.RouteId
LEFT JOIN DriverDetails dd ON dd.BusId  = b.BusId
LEFT JOIN Users        u  ON u.UserId  = dd.UserId
WHERE b.IsActive = 1;
GO

-- V3: Latest live location per active trip
CREATE VIEW vw_BusLatestLocation AS
SELECT
    bt.TripId,
    bt.BusId,
    bt.DriverId,
    bt.TripType,
    bt.TripDate,
    bt.Status           AS TripStatus,
    ll.Latitude,
    ll.Longitude,
    ll.Speed,
    ll.Heading,
    ll.RecordedAt
FROM BusTrips bt
CROSS APPLY (
    SELECT TOP 1 Latitude, Longitude, Speed, Heading, RecordedAt
    FROM BusLiveLocation
    WHERE TripId = bt.TripId
    ORDER BY RecordedAt DESC
) ll
WHERE bt.Status = 'InProgress';
GO

-- ============================================================
-- STORED PROCEDURES
-- ============================================================

-- SP1: Get students on a trip adjusted for today's availability
CREATE OR ALTER PROCEDURE sp_GetTripStudents
    @TripId   INT,
    @TripDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @TripDate IS NULL SET @TripDate = CAST(GETUTCDATE() AS DATE);

    SELECT
        s.StudentId,
        s.StudentCode,
        u.FullName      AS StudentName,
        s.StopId,
        st.StopName,
        st.StopOrder,
        ISNULL(sts.BoardingStatus, 
            CASE 
                WHEN sa.AvailabilityId IS NOT NULL THEN 'OnLeave'
                ELSE 'Pending'
            END)        AS BoardingStatus,
        CASE WHEN sa.AvailabilityId IS NOT NULL THEN 1 ELSE 0 END AS IsUnavailable,
        sa.AvailabilityType
    FROM BusTrips bt
    JOIN Buses    b   ON b.BusId    = bt.BusId
    JOIN Students s   ON s.BusId    = b.BusId
    JOIN Users    u   ON u.UserId   = s.UserId
    JOIN Stops    st  ON st.StopId  = s.StopId
    LEFT JOIN StudentTripStatus sts
           ON sts.TripId    = bt.TripId
          AND sts.StudentId = s.StudentId
    LEFT JOIN StudentAvailabilities sa
           ON sa.StudentId        = s.StudentId
          AND @TripDate BETWEEN sa.FromDate AND sa.ToDate
    WHERE bt.TripId  = @TripId
      AND u.IsActive = 1
    ORDER BY st.StopOrder, u.FullName;
END;
GO

-- SP2: Update student boarding status
CREATE OR ALTER PROCEDURE sp_UpdateStudentBoardingStatus
    @TripId         INT,
    @StudentId      INT,
    @StopId         INT,
    @BoardingStatus NVARCHAR(20),
    @UpdatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM StudentTripStatus WHERE TripId = @TripId AND StudentId = @StudentId)
        UPDATE StudentTripStatus
        SET BoardingStatus = @BoardingStatus,
            UpdatedAt      = GETUTCDATE(),
            UpdatedBy      = @UpdatedByUserId
        WHERE TripId = @TripId AND StudentId = @StudentId;
    ELSE
        INSERT INTO StudentTripStatus (TripId, StudentId, StopId, BoardingStatus, UpdatedBy)
        VALUES (@TripId, @StudentId, @StopId, @BoardingStatus, @UpdatedByUserId);
END;
GO

-- SP3: Insert GPS ping
CREATE OR ALTER PROCEDURE sp_InsertBusLocation
    @TripId    INT,
    @BusId     INT,
    @Latitude  DECIMAL(10,7),
    @Longitude DECIMAL(10,7),
    @Speed     DECIMAL(6,2) = NULL,
    @Heading   DECIMAL(6,2) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO BusLiveLocation (TripId, BusId, Latitude, Longitude, Speed, Heading)
    VALUES (@TripId, @BusId, @Latitude, @Longitude, @Speed, @Heading);
END;
GO

-- SP4: Dashboard summary for Super Admin
CREATE OR ALTER PROCEDURE sp_GetDashboardSummary
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        (SELECT COUNT(*) FROM Buses   WHERE IsActive = 1)                                    AS TotalBuses,
        (SELECT COUNT(*) FROM Users   WHERE RoleId = (SELECT RoleId FROM Roles WHERE RoleName='Driver')          AND IsActive=1) AS TotalDrivers,
        (SELECT COUNT(*) FROM Users   WHERE RoleId = (SELECT RoleId FROM Roles WHERE RoleName='BusCoordinator')  AND IsActive=1) AS TotalBusCoordinators,
        (SELECT COUNT(*) FROM Parents p JOIN Users u ON u.UserId=p.UserId WHERE u.IsActive=1) AS TotalParents,
        (SELECT COUNT(*) FROM Students s JOIN Users u ON u.UserId=s.UserId WHERE u.IsActive=1) AS TotalStudents,
        (SELECT COUNT(*) FROM BusTrips WHERE Status='InProgress')                             AS ActiveTrips;
END;
GO

-- ============================================================
-- INDEXES  (performance)
-- ============================================================
CREATE NONCLUSTERED INDEX IX_Users_RoleId         ON Users (RoleId);
CREATE NONCLUSTERED INDEX IX_Users_Email          ON Users (Email);
CREATE NONCLUSTERED INDEX IX_Users_UserName        ON Users (UserName);
CREATE NONCLUSTERED INDEX IX_Students_BusId       ON Students (BusId);
CREATE NONCLUSTERED INDEX IX_Students_StopId      ON Students (StopId);
CREATE NONCLUSTERED INDEX IX_Stops_RouteId        ON Stops (RouteId, StopOrder);
CREATE NONCLUSTERED INDEX IX_BusTrips_BusDate     ON BusTrips (BusId, TripDate, TripType);
CREATE NONCLUSTERED INDEX IX_TripStopEvents_Trip  ON TripStopEvents (TripId, StopId);
CREATE NONCLUSTERED INDEX IX_StudentTripStatus    ON StudentTripStatus (TripId, StudentId);
CREATE NONCLUSTERED INDEX IX_Availability_Student ON StudentAvailabilities (StudentId, FromDate, ToDate);
CREATE NONCLUSTERED INDEX IX_Feedbacks_Status     ON Feedbacks (Status, CreatedAt DESC);
CREATE NONCLUSTERED INDEX IX_Notifications_Read   ON Notifications (RecipientUserId, IsRead);
GO

PRINT 'BusTrackingDB created successfully with all tables, views, stored procedures, and indexes.';
GO