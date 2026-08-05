-- ============================================================
--  SCHOOL ERP APPLICATION - Complete Database Creation Script
--  Platform : SQL Server (T-SQL)
--  Database : SchoolErpDB
-- ============================================================

USE master;
GO

IF DB_ID('SchoolErpDB') IS NOT NULL
BEGIN
    ALTER DATABASE SchoolErpDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE SchoolErpDB;
END
GO

CREATE DATABASE SchoolErpDB;
GO

USE SchoolErpDB;
GO

-- ============================================================
-- PART 1: MASTER & CORE INFRASTRUCTURE TABLES
-- ============================================================

-- 1. TIME ZONE MASTERS
CREATE TABLE TimeZoneMasters (
    TimeZoneId         INT           NOT NULL IDENTITY(1,1),
    TimeZoneName       NVARCHAR(200) NOT NULL,
    IanaTimeZoneId     NVARCHAR(100) NOT NULL,
    WindowsTimeZoneId  NVARCHAR(100) NOT NULL,
    UtcOffset          NVARCHAR(20)  NOT NULL,
    IsActive           BIT           NOT NULL CONSTRAINT DF_TimeZoneMasters_IsActive DEFAULT 1,
    DisplayOrder       INT           NOT NULL CONSTRAINT DF_TimeZoneMasters_DisplayOrder DEFAULT 0,
    CONSTRAINT PK_TimeZoneMasters PRIMARY KEY (TimeZoneId)
);
GO

-- 2. COUNTRY MASTERS
CREATE TABLE CountryMasters (
    CountryId      INT           NOT NULL IDENTITY(1,1),
    CountryName    NVARCHAR(150) NOT NULL,
    ISO2           NVARCHAR(10)  NULL,
    PhoneCode      NVARCHAR(10)  NULL,
    CurrencyCode   NVARCHAR(10)  NULL,
    CurrencySymbol NVARCHAR(10)  NULL,
    IsActive       BIT           NOT NULL CONSTRAINT DF_CountryMasters_IsActive DEFAULT 1,
    CreatedAt      DATETIME2     NOT NULL CONSTRAINT DF_CountryMasters_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt      DATETIME2     NOT NULL CONSTRAINT DF_CountryMasters_UpdatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_CountryMasters PRIMARY KEY (CountryId),
    CONSTRAINT UQ_CountryMasters_Name UNIQUE (CountryName)
);
GO

-- 3. REGION MASTERS (States / Provinces)
CREATE TABLE RegionMasters (
    RegionId    INT           NOT NULL IDENTITY(1,1),
    CountryId   INT           NOT NULL,
    RegionName  NVARCHAR(150) NOT NULL,
    RegionCode  NVARCHAR(20)  NULL,
    IsActive    BIT           NOT NULL CONSTRAINT DF_RegionMasters_IsActive DEFAULT 1,
    CreatedAt   DATETIME2     NOT NULL CONSTRAINT DF_RegionMasters_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt   DATETIME2     NOT NULL CONSTRAINT DF_RegionMasters_UpdatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_RegionMasters PRIMARY KEY (RegionId),
    CONSTRAINT FK_RegionMasters_CountryMasters FOREIGN KEY (CountryId) REFERENCES CountryMasters(CountryId) ON DELETE CASCADE,
    CONSTRAINT UQ_RegionMasters_Country_Name UNIQUE (CountryId, RegionName)
);
GO

-- 4. SCHOOLS (Multi-Tenant Core)
CREATE TABLE Schools (
    SchoolId       INT           NOT NULL IDENTITY(1,1),
    SchoolName     NVARCHAR(200) NOT NULL,
    SchoolCode     NVARCHAR(50)  NOT NULL,
    SchoolLogo     NVARCHAR(500) NULL,
    SchoolAddress  NVARCHAR(500) NOT NULL,
    ContactNumber  NVARCHAR(20)  NOT NULL,
    EmailAddress   NVARCHAR(100) NOT NULL,
    PrincipalName  NVARCHAR(150) NOT NULL,
    Website        NVARCHAR(200) NULL,
    CountryId      INT           NULL,
    RegionId       INT           NULL,
    City           NVARCHAR(150) NULL,
    TimeZoneId     INT           NULL,
    TimeZoneInfoId NVARCHAR(100) NULL CONSTRAINT DF_Schools_TimeZoneInfoId DEFAULT 'India Standard Time',
    IsActive       BIT           NOT NULL CONSTRAINT DF_Schools_IsActive DEFAULT 1,
    CreatedAt      DATETIME2     NOT NULL CONSTRAINT DF_Schools_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt      DATETIME2     NOT NULL CONSTRAINT DF_Schools_UpdatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Schools PRIMARY KEY (SchoolId),
    CONSTRAINT UQ_Schools_SchoolCode UNIQUE (SchoolCode),
    CONSTRAINT FK_Schools_CountryMasters FOREIGN KEY (CountryId) REFERENCES CountryMasters(CountryId),
    CONSTRAINT FK_Schools_RegionMasters FOREIGN KEY (RegionId) REFERENCES RegionMasters(RegionId),
    CONSTRAINT FK_Schools_TimeZoneMasters FOREIGN KEY (TimeZoneId) REFERENCES TimeZoneMasters(TimeZoneId)
);
GO

-- 5. SYSTEM ADMINISTRATORS
CREATE TABLE SystemAdministrators (
    AdminId      INT           NOT NULL IDENTITY(1,1),
    FullName     NVARCHAR(150) NOT NULL,
    UserName     NVARCHAR(100) NOT NULL,
    Email        NVARCHAR(255) NULL,
    PasswordHash NVARCHAR(512) NOT NULL,
    PasswordSalt NVARCHAR(256) NOT NULL,
    CreatedAt    DATETIME2     NOT NULL CONSTRAINT DF_SysAdmins_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt    DATETIME2     NOT NULL CONSTRAINT DF_SysAdmins_UpdatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_SystemAdministrators PRIMARY KEY (AdminId),
    CONSTRAINT UQ_SystemAdministrators_UserName UNIQUE (UserName)
);
GO

-- 6. ROLES
CREATE TABLE Roles (
    RoleId      INT           NOT NULL IDENTITY(1,1),
    RoleName    NVARCHAR(50)  NOT NULL,
    Description NVARCHAR(255) NULL,
    IsActive    BIT           NOT NULL CONSTRAINT DF_Roles_IsActive DEFAULT 1,
    CreatedAt   DATETIME2     NOT NULL CONSTRAINT DF_Roles_CreatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Roles PRIMARY KEY (RoleId)
);
GO

-- 7. PERMISSIONS CATALOGUE
CREATE TABLE Permissions (
    PermissionId   INT           NOT NULL IDENTITY(1,1),
    ModuleName     NVARCHAR(100) NOT NULL,
    PermissionKey  NVARCHAR(100) NOT NULL,
    Description    NVARCHAR(255) NULL,
    CreatedAt      DATETIME2     NOT NULL CONSTRAINT DF_Permissions_CreatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Permissions PRIMARY KEY (PermissionId),
    CONSTRAINT UQ_Permissions_PermissionKey UNIQUE (PermissionKey)
);
GO

-- ============================================================
-- PART 2: USERS & AUTHENTICATION
-- ============================================================

-- 8. USERS
CREATE TABLE Users (
    SchoolId        INT            NULL,
    UserId          INT            NOT NULL IDENTITY(1,1),
    RoleId          INT            NOT NULL,
    FullName        NVARCHAR(150)  NOT NULL,
    UserName        NVARCHAR(100)  NOT NULL,
    Email           NVARCHAR(255)  NULL,
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
    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    CONSTRAINT FK_Users_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId)
);
GO

-- 9. PASSWORD RESET TOKENS
CREATE TABLE PasswordResetTokens (
    TokenId   INT           NOT NULL IDENTITY(1,1),
    UserId    INT           NOT NULL,
    Token     NVARCHAR(256) NOT NULL,
    ExpiresAt DATETIME2     NOT NULL,
    IsUsed    BIT           NOT NULL CONSTRAINT DF_PasswordResetTokens_IsUsed DEFAULT 0,
    CreatedAt DATETIME2     NOT NULL CONSTRAINT DF_PasswordResetTokens_CreatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_PasswordResetTokens PRIMARY KEY (TokenId),
    CONSTRAINT FK_PasswordResetTokens_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT UQ_PasswordResetTokens_Token UNIQUE (Token)
);
GO

-- 10. SUB-ADMIN PERMISSIONS MAPPING
CREATE TABLE SubAdminPermissions (
    SchoolId      INT       NULL,
    UserId        INT       NOT NULL,
    PermissionId  INT       NOT NULL,
    GrantedAt     DATETIME2 NOT NULL CONSTRAINT DF_SubAdminPermissions_GrantedAt DEFAULT GETUTCDATE(),
    GrantedBy     INT       NULL,
    CONSTRAINT PK_SubAdminPermissions PRIMARY KEY (UserId, PermissionId),
    CONSTRAINT FK_SubAdminPermissions_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_SubAdminPermissions_Permissions FOREIGN KEY (PermissionId) REFERENCES Permissions(PermissionId) ON DELETE CASCADE,
    CONSTRAINT FK_SubAdminPermissions_GrantedBy FOREIGN KEY (GrantedBy) REFERENCES Users(UserId),
    CONSTRAINT FK_SubAdminPermissions_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId)
);
GO

-- ============================================================
-- PART 3: ACADEMICS & CLASS STRUCTURE
-- ============================================================

-- 11. STANDARD MASTERS (Class 1, Class 2, ...)
CREATE TABLE StandardMasters (
    SchoolId     INT           NULL,
    StandardId   INT           NOT NULL IDENTITY(1,1),
    StandardName NVARCHAR(100) NOT NULL,
    IsActive     BIT           NOT NULL CONSTRAINT DF_StandardMasters_IsActive DEFAULT 1,
    CreatedAt    DATETIME2     NOT NULL CONSTRAINT DF_StandardMasters_CreatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_StandardMasters PRIMARY KEY (StandardId),
    CONSTRAINT UQ_StandardMasters_StandardName UNIQUE (SchoolId, StandardName),
    CONSTRAINT FK_StandardMasters_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId)
);
GO

-- 12. ACADEMIC YEARS (Session Switcher)
CREATE TABLE AcademicYears (
    AcademicYearId INT           NOT NULL IDENTITY(1,1),
    SchoolId       INT           NOT NULL,
    YearName       NVARCHAR(50)  NOT NULL,
    StartDate      DATE          NOT NULL,
    EndDate        DATE          NOT NULL,
    IsActive       BIT           NOT NULL CONSTRAINT DF_AcademicYears_IsActive DEFAULT 1,
    IsCurrent      BIT           NOT NULL CONSTRAINT DF_AcademicYears_IsCurrent DEFAULT 0,
    CreatedAt      DATETIME2     NOT NULL CONSTRAINT DF_AcademicYears_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt      DATETIME2     NOT NULL CONSTRAINT DF_AcademicYears_UpdatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_AcademicYears PRIMARY KEY (AcademicYearId),
    CONSTRAINT FK_AcademicYears_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId) ON DELETE CASCADE,
    CONSTRAINT UQ_AcademicYears_School_Year UNIQUE (SchoolId, YearName)
);
GO

-- 13. CLASS SECTIONS (Auto Section 'A' Rule)
CREATE TABLE Sections (
    SectionId   INT           NOT NULL IDENTITY(1,1),
    SchoolId    INT           NOT NULL,
    StandardId  INT           NOT NULL,
    SectionName NVARCHAR(50)  NOT NULL CONSTRAINT DF_Sections_SectionName DEFAULT 'A',
    IsDefault   BIT           NOT NULL CONSTRAINT DF_Sections_IsDefault DEFAULT 1,
    IsActive    BIT           NOT NULL CONSTRAINT DF_Sections_IsActive DEFAULT 1,
    CreatedAt   DATETIME2     NOT NULL CONSTRAINT DF_Sections_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt   DATETIME2     NOT NULL CONSTRAINT DF_Sections_UpdatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Sections PRIMARY KEY (SectionId),
    CONSTRAINT FK_Sections_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId),
    CONSTRAINT FK_Sections_StandardMasters FOREIGN KEY (StandardId) REFERENCES StandardMasters(StandardId) ON DELETE CASCADE,
    CONSTRAINT UQ_Sections_School_Standard_Section UNIQUE (SchoolId, StandardId, SectionName)
);
GO

-- Trigger: Automatic Section 'A' Rule on Standard Creation
CREATE OR ALTER TRIGGER trg_StandardMasters_AutoSectionA
ON StandardMasters
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Sections (SchoolId, StandardId, SectionName, IsDefault, IsActive, CreatedAt, UpdatedAt)
    SELECT i.SchoolId, i.StandardId, 'A', 1, 1, GETUTCDATE(), GETUTCDATE()
    FROM inserted i
    WHERE i.SchoolId IS NOT NULL
      AND NOT EXISTS (
        SELECT 1 FROM Sections s WHERE s.SchoolId = i.SchoolId AND s.StandardId = i.StandardId AND s.SectionName = 'A'
    );
END;
GO

-- 13.1 SUBJECT MASTERS
CREATE TABLE Subjects (
    SubjectId   INT           NOT NULL IDENTITY(1,1),
    SchoolId    INT           NOT NULL,
    SubjectName NVARCHAR(150) NOT NULL,
    SubjectCode NVARCHAR(50)  NULL,
    IsActive    BIT           NOT NULL CONSTRAINT DF_Subjects_IsActive DEFAULT 1,
    CreatedAt   DATETIME2     NOT NULL CONSTRAINT DF_Subjects_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt   DATETIME2     NOT NULL CONSTRAINT DF_Subjects_UpdatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Subjects PRIMARY KEY (SubjectId),
    CONSTRAINT FK_Subjects_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId) ON DELETE CASCADE,
    CONSTRAINT UQ_Subjects_School_Name UNIQUE (SchoolId, SubjectName)
);
GO

-- 13.2 CLASS SUBJECT TEACHERS MAPPING
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ClassSubjectTeachers')
BEGIN
    CREATE TABLE ClassSubjectTeachers (
        ClassSubjectTeacherId INT       NOT NULL IDENTITY(1,1),
        SchoolId              INT       NOT NULL,
        AcademicYearId        INT       NOT NULL,
        StandardId            INT       NOT NULL,
        SectionId             INT       NULL,
        SubjectId             INT       NOT NULL,
        TeacherId             INT       NOT NULL,
        IsActive              BIT       NOT NULL CONSTRAINT DF_ClassSubjectTeachers_IsActive DEFAULT 1,
        CreatedAt             DATETIME2 NOT NULL CONSTRAINT DF_ClassSubjectTeachers_CreatedAt DEFAULT GETUTCDATE(),
        UpdatedAt             DATETIME2 NOT NULL CONSTRAINT DF_ClassSubjectTeachers_UpdatedAt DEFAULT GETUTCDATE(),
        CONSTRAINT PK_ClassSubjectTeachers PRIMARY KEY (ClassSubjectTeacherId),
        CONSTRAINT FK_ClassSubjectTeachers_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId),
        CONSTRAINT FK_ClassSubjectTeachers_AcademicYears FOREIGN KEY (AcademicYearId) REFERENCES AcademicYears(AcademicYearId),
        CONSTRAINT FK_ClassSubjectTeachers_Standards FOREIGN KEY (StandardId) REFERENCES StandardMasters(StandardId),
        CONSTRAINT FK_ClassSubjectTeachers_Sections FOREIGN KEY (SectionId) REFERENCES Sections(SectionId),
        CONSTRAINT FK_ClassSubjectTeachers_Subjects FOREIGN KEY (SubjectId) REFERENCES Subjects(SubjectId),
        CONSTRAINT FK_ClassSubjectTeachers_Teachers FOREIGN KEY (TeacherId) REFERENCES Users(UserId)
    );
END
GO

-- ============================================================
-- PART 4: BUS TRACKING & TRANSPORT MODULE
-- ============================================================

-- 14. ROUTES
CREATE TABLE Routes (
    SchoolId      INT           NULL,
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
    CONSTRAINT UQ_Routes_RouteCode UNIQUE (SchoolId, RouteCode),
    CONSTRAINT FK_Routes_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId)
);
GO

-- 15. STOPS
CREATE TABLE Stops (
    SchoolId    INT            NULL,
    StopId      INT            NOT NULL IDENTITY(1,1),
    RouteId     INT            NOT NULL,
    StopName    NVARCHAR(150)  NOT NULL,
    StopOrder   INT            NOT NULL,
    Latitude    DECIMAL(10,7)  NULL,
    Longitude   DECIMAL(10,7)  NULL,
    MorningTime TIME           NULL,
    EveningTime TIME           NULL,
    IsActive    BIT            NOT NULL CONSTRAINT DF_Stops_IsActive DEFAULT 1,
    CreatedAt   DATETIME2      NOT NULL CONSTRAINT DF_Stops_CreatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Stops PRIMARY KEY (StopId),
    CONSTRAINT FK_Stops_Routes FOREIGN KEY (RouteId) REFERENCES Routes(RouteId),
    CONSTRAINT UQ_Route_StopOrder UNIQUE (RouteId, StopOrder),
    CONSTRAINT FK_Stops_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId)
);
GO

-- 16. BUS TYPE MASTERS
CREATE TABLE BusTypeMasters (
    SchoolId  INT           NULL,
    Id        INT           NOT NULL IDENTITY(1,1),
    Name      NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2     NOT NULL CONSTRAINT DF_BusTypeMasters_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2     NOT NULL CONSTRAINT DF_BusTypeMasters_UpdatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_BusTypeMasters PRIMARY KEY (Id),
    CONSTRAINT UQ_BusTypeMasters_Name UNIQUE (SchoolId, Name),
    CONSTRAINT FK_BusTypeMasters_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId)
);
GO

-- 17. BUSES
CREATE TABLE Buses (
    SchoolId            INT           NULL,
    BusId               INT           NOT NULL IDENTITY(1,1),
    BusName             NVARCHAR(100) NOT NULL,
    BusNumber           NVARCHAR(50)  NOT NULL,
    BusTypeId           INT           NOT NULL,
    Capacity            INT           NULL,
    InsuranceExpiryDate DATE          NULL,
    FitnessExpiryDate   DATE          NULL,
    PucExpiryDate       DATE          NULL,
    LastServiceDate     DATE          NULL,
    IsActive            BIT           NOT NULL CONSTRAINT DF_Buses_IsActive DEFAULT 1,
    CreatedAt           DATETIME2     NOT NULL CONSTRAINT DF_Buses_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt           DATETIME2     NOT NULL CONSTRAINT DF_Buses_UpdatedAt DEFAULT GETUTCDATE(),
    CreatedBy           INT           NULL,
    CONSTRAINT PK_Buses PRIMARY KEY (BusId),
    CONSTRAINT FK_Buses_BusTypeMasters FOREIGN KEY (BusTypeId) REFERENCES BusTypeMasters(Id),
    CONSTRAINT FK_Buses_Users FOREIGN KEY (CreatedBy) REFERENCES Users(UserId),
    CONSTRAINT UQ_Buses_BusNumber UNIQUE (SchoolId, BusNumber),
    CONSTRAINT FK_Buses_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId)
);
GO

-- 18. DRIVER DETAILS
CREATE TABLE DriverDetails (
    SchoolId       INT           NULL,
    DriverDetailId INT           NOT NULL IDENTITY(1,1),
    UserId         INT           NOT NULL,
    LicenseNumber  NVARCHAR(100) NULL,
    LicenseExpiry  DATE          NULL,
    CreatedAt      DATETIME2     NOT NULL CONSTRAINT DF_DriverDetails_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt      DATETIME2     NOT NULL CONSTRAINT DF_DriverDetails_UpdatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_DriverDetails PRIMARY KEY (DriverDetailId),
    CONSTRAINT FK_DriverDetails_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT UQ_DriverDetails_UserId UNIQUE (UserId),
    CONSTRAINT FK_DriverDetails_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId)
);
GO

-- 18.1 TEACHERS
CREATE TABLE Teachers (
    SchoolId         INT           NULL,
    TeacherId        INT           NOT NULL IDENTITY(1,1),
    UserId           INT           NOT NULL,
    EmployeeCode     NVARCHAR(50)  NULL,
    Qualification    NVARCHAR(150) NULL,
    Designation      NVARCHAR(100) NULL,
    Department       NVARCHAR(100) NULL,
    JoiningDate      DATE          NULL,
    Gender           NVARCHAR(20)  NULL,
    EmergencyContact NVARCHAR(20)  NULL,
    CreatedAt        DATETIME2     NOT NULL CONSTRAINT DF_Teachers_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt        DATETIME2     NOT NULL CONSTRAINT DF_Teachers_UpdatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Teachers PRIMARY KEY (TeacherId),
    CONSTRAINT FK_Teachers_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT FK_Teachers_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId),
    CONSTRAINT UQ_Teachers_UserId UNIQUE (UserId)
);
GO

-- 18.2 CLASS SUBJECT TEACHER MAPPINGS
CREATE TABLE ClassSubjectTeachers (
    ClassSubjectTeacherId INT       NOT NULL IDENTITY(1,1),
    SchoolId              INT       NOT NULL,
    AcademicYearId        INT       NOT NULL,
    StandardId            INT       NOT NULL,
    SectionId             INT       NOT NULL,
    SubjectId             INT       NOT NULL,
    TeacherId             INT       NOT NULL,
    IsActive              BIT       NOT NULL CONSTRAINT DF_ClassSubjectTeachers_IsActive DEFAULT 1,
    CreatedAt             DATETIME2 NOT NULL CONSTRAINT DF_ClassSubjectTeachers_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt             DATETIME2 NOT NULL CONSTRAINT DF_ClassSubjectTeachers_UpdatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_ClassSubjectTeachers PRIMARY KEY (ClassSubjectTeacherId),
    CONSTRAINT FK_ClassSubjectTeachers_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId),
    CONSTRAINT FK_ClassSubjectTeachers_AcademicYears FOREIGN KEY (AcademicYearId) REFERENCES AcademicYears(AcademicYearId),
    CONSTRAINT FK_ClassSubjectTeachers_StandardMasters FOREIGN KEY (StandardId) REFERENCES StandardMasters(StandardId),
    CONSTRAINT FK_ClassSubjectTeachers_Sections FOREIGN KEY (SectionId) REFERENCES Sections(SectionId),
    CONSTRAINT FK_ClassSubjectTeachers_Subjects FOREIGN KEY (SubjectId) REFERENCES Subjects(SubjectId),
    CONSTRAINT FK_ClassSubjectTeachers_Teachers FOREIGN KEY (TeacherId) REFERENCES Teachers(TeacherId),
    CONSTRAINT UQ_ClassSubjectTeachers UNIQUE (SchoolId, AcademicYearId, StandardId, SectionId, SubjectId)
);
GO

-- ============================================================
-- PART 5: STUDENTS & PARENTS
-- ============================================================

-- 19. STUDENTS
CREATE TABLE Students (
    SchoolId           INT            NULL,
    StudentId          INT           NOT NULL IDENTITY(1,1),
    UserId             INT           NOT NULL,
    StudentCode        NVARCHAR(50)  NOT NULL,
    AcademicYearId     INT           NULL,
    StandardId         INT           NULL,
    SectionId          INT           NULL,
    BusId              INT           NULL,
    StopId             INT           NULL,
    TransportFeeStatus NVARCHAR(20) NOT NULL CONSTRAINT DF_Students_TransportFeeStatus DEFAULT 'Paid',
    FeeExpiryDate      DATE          NULL,
    CreatedAt          DATETIME2     NOT NULL CONSTRAINT DF_Students_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt          DATETIME2     NOT NULL CONSTRAINT DF_Students_UpdatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Students PRIMARY KEY (StudentId),
    CONSTRAINT FK_Students_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT FK_Students_AcademicYears FOREIGN KEY (AcademicYearId) REFERENCES AcademicYears(AcademicYearId),
    CONSTRAINT FK_Students_StandardMasters FOREIGN KEY (StandardId) REFERENCES StandardMasters(StandardId),
    CONSTRAINT FK_Students_Sections FOREIGN KEY (SectionId) REFERENCES Sections(SectionId),
    CONSTRAINT FK_Students_Buses FOREIGN KEY (BusId) REFERENCES Buses(BusId),
    CONSTRAINT FK_Students_Stops FOREIGN KEY (StopId) REFERENCES Stops(StopId),
    CONSTRAINT UQ_Students_UserId UNIQUE (UserId),
    CONSTRAINT UQ_Students_StudentCode UNIQUE (SchoolId, StudentCode),
    CONSTRAINT FK_Students_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId)
);
GO

-- 20. PARENTS
CREATE TABLE Parents (
    SchoolId  INT       NULL,
    ParentId  INT       NOT NULL IDENTITY(1,1),
    UserId    INT       NOT NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Parents_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_Parents_UpdatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Parents PRIMARY KEY (ParentId),
    CONSTRAINT FK_Parents_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT UQ_Parents_UserId UNIQUE (UserId),
    CONSTRAINT FK_Parents_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId)
);
GO

-- 21. PARENT-STUDENT MAPPING
CREATE TABLE ParentStudents (
    SchoolId         INT          NULL,
    ParentId         INT          NOT NULL,
    StudentId        INT          NOT NULL,
    Relationship     NVARCHAR(50) NULL CONSTRAINT DF_ParentStudents_Relationship DEFAULT 'Parent',
    IsPrimaryContact BIT          NOT NULL CONSTRAINT DF_ParentStudents_IsPrimary DEFAULT 1,
    CreatedAt        DATETIME2    NOT NULL CONSTRAINT DF_ParentStudents_CreatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_ParentStudents PRIMARY KEY (ParentId, StudentId),
    CONSTRAINT FK_ParentStudents_Parents FOREIGN KEY (ParentId) REFERENCES Parents(ParentId) ON DELETE CASCADE,
    CONSTRAINT FK_ParentStudents_Students FOREIGN KEY (StudentId) REFERENCES Students(StudentId) ON DELETE CASCADE,
    CONSTRAINT FK_ParentStudents_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId)
);
GO

-- 22. STUDENT AVAILABILITY / LEAVE REQUESTS
CREATE TABLE StudentAvailabilities (
    SchoolId         INT           NULL,
    AvailabilityId   INT           NOT NULL IDENTITY(1,1),
    StudentId        INT           NOT NULL,
    FromDate         DATE          NOT NULL,
    ToDate           DATE          NOT NULL,
    AvailabilityType NVARCHAR(20)  NOT NULL,
    Remarks          NVARCHAR(500) NULL,
    CreatedAt        DATETIME2     NOT NULL CONSTRAINT DF_StudentAvailabilities_CreatedAt DEFAULT GETUTCDATE(),
    MarkedBy         INT           NOT NULL,
    CONSTRAINT PK_StudentAvailabilities PRIMARY KEY (AvailabilityId),
    CONSTRAINT FK_StudentAvailabilities_Students FOREIGN KEY (StudentId) REFERENCES Students(StudentId) ON DELETE CASCADE,
    CONSTRAINT FK_StudentAvailabilities_Users FOREIGN KEY (MarkedBy) REFERENCES Users(UserId),
    CONSTRAINT FK_StudentAvailabilities_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId)
);
GO

-- ============================================================
-- PART 6: BUS TRIPS & LIVE TELEMETRY
-- ============================================================

-- 23. BUS TRIPS
CREATE TABLE BusTrips (
    SchoolId  INT           NULL,
    TripId    INT           NOT NULL IDENTITY(1,1),
    BusId     INT           NOT NULL,
    DriverId  INT           NOT NULL,
    RouteId   INT           NULL,
    TripType  NVARCHAR(20)  NOT NULL,
    TripDate  DATE          NOT NULL,
    Status    NVARCHAR(20)  NOT NULL CONSTRAINT DF_BusTrips_Status DEFAULT 'Scheduled',
    StartedAt DATETIME2     NULL,
    EndedAt   DATETIME2     NULL,
    Remarks   NVARCHAR(500) NULL,
    CreatedAt DATETIME2     NOT NULL CONSTRAINT DF_BusTrips_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2     NOT NULL CONSTRAINT DF_BusTrips_UpdatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_BusTrips PRIMARY KEY (TripId),
    CONSTRAINT FK_BusTrips_Buses FOREIGN KEY (BusId) REFERENCES Buses(BusId),
    CONSTRAINT FK_BusTrips_Users FOREIGN KEY (DriverId) REFERENCES Users(UserId),
    CONSTRAINT FK_BusTrips_Routes FOREIGN KEY (RouteId) REFERENCES Routes(RouteId),
    CONSTRAINT UQ_Bus_TripInstance UNIQUE (BusId, TripDate, TripType),
    CONSTRAINT FK_BusTrips_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId)
);
GO

-- 24. BUS ROUTE MAPPINGS
CREATE TABLE BusRouteMappings (
    SchoolId  INT       NULL,
    BusId     INT       NOT NULL,
    RouteId   INT       NOT NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_BusRouteMappings_CreatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_BusRouteMappings PRIMARY KEY (BusId, RouteId),
    CONSTRAINT FK_BusRouteMappings_Buses FOREIGN KEY (BusId) REFERENCES Buses(BusId) ON DELETE CASCADE,
    CONSTRAINT FK_BusRouteMappings_Routes FOREIGN KEY (RouteId) REFERENCES Routes(RouteId) ON DELETE CASCADE,
    CONSTRAINT FK_BusRouteMappings_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId)
);
GO

-- 25. BUS DRIVER MAPPINGS
CREATE TABLE BusDriverMappings (
    SchoolId  INT       NULL,
    BusId     INT       NOT NULL,
    DriverId  INT       NOT NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_BusDriverMappings_CreatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_BusDriverMappings PRIMARY KEY (BusId, DriverId),
    CONSTRAINT FK_BusDriverMappings_Buses FOREIGN KEY (BusId) REFERENCES Buses(BusId) ON DELETE CASCADE,
    CONSTRAINT FK_BusDriverMappings_Users FOREIGN KEY (DriverId) REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_BusDriverMappings_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId)
);
GO

-- 26. TRIP STOP EVENTS
CREATE TABLE TripStopEvents (
    SchoolId        INT          NULL,
    TripStopEventId INT          NOT NULL IDENTITY(1,1),
    TripId          INT          NOT NULL,
    StopId          INT          NOT NULL,
    ReachedAt       DATETIME2    NULL,
    DepartedAt      DATETIME2    NULL,
    Status          NVARCHAR(20) NOT NULL CONSTRAINT DF_TripStopEvents_Status DEFAULT 'Pending',
    CreatedAt       DATETIME2    NOT NULL CONSTRAINT DF_TripStopEvents_CreatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_TripStopEvents PRIMARY KEY (TripStopEventId),
    CONSTRAINT FK_TripStopEvents_BusTrips FOREIGN KEY (TripId) REFERENCES BusTrips(TripId) ON DELETE CASCADE,
    CONSTRAINT FK_TripStopEvents_Stops FOREIGN KEY (StopId) REFERENCES Stops(StopId),
    CONSTRAINT UQ_Trip_Stop UNIQUE (TripId, StopId),
    CONSTRAINT FK_TripStopEvents_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId)
);
GO

-- 27. STUDENT TRIP STATUS
CREATE TABLE StudentTripStatus (
    SchoolId            INT          NULL,
    StudentTripStatusId INT          NOT NULL IDENTITY(1,1),
    TripId              INT          NOT NULL,
    StudentId           INT          NOT NULL,
    StopId              INT          NOT NULL,
    BoardingStatus      NVARCHAR(20) NOT NULL CONSTRAINT DF_StudentTripStatus_Status DEFAULT 'Pending',
    BoardedAt           DATETIME2    NULL,
    DroppedAt           DATETIME2    NULL,
    UpdatedAt           DATETIME2    NOT NULL CONSTRAINT DF_StudentTripStatus_UpdatedAt DEFAULT GETUTCDATE(),
    UpdatedBy           INT          NULL,
    CONSTRAINT PK_StudentTripStatus PRIMARY KEY (StudentTripStatusId),
    CONSTRAINT FK_StudentTripStatus_BusTrips FOREIGN KEY (TripId) REFERENCES BusTrips(TripId) ON DELETE CASCADE,
    CONSTRAINT FK_StudentTripStatus_Students FOREIGN KEY (StudentId) REFERENCES Students(StudentId),
    CONSTRAINT FK_StudentTripStatus_Stops FOREIGN KEY (StopId) REFERENCES Stops(StopId),
    CONSTRAINT FK_StudentTripStatus_Users FOREIGN KEY (UpdatedBy) REFERENCES Users(UserId),
    CONSTRAINT UQ_Trip_Student UNIQUE (TripId, StudentId),
    CONSTRAINT FK_StudentTripStatus_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId)
);
GO

-- 28. BUS LIVE LOCATION
CREATE TABLE BusLiveLocation (
    SchoolId   INT           NULL,
    LocationId INT           NOT NULL IDENTITY(1,1),
    TripId     INT           NOT NULL,
    BusId      INT           NOT NULL,
    Latitude   DECIMAL(10,7) NOT NULL,
    Longitude  DECIMAL(10,7) NOT NULL,
    Speed      DECIMAL(6,2)  NULL,
    Heading    DECIMAL(6,2)  NULL,
    RecordedAt DATETIME2     NOT NULL CONSTRAINT DF_BusLiveLocation_RecordedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_BusLiveLocation PRIMARY KEY (LocationId),
    CONSTRAINT FK_BusLiveLocation_BusTrips FOREIGN KEY (TripId) REFERENCES BusTrips(TripId) ON DELETE CASCADE,
    CONSTRAINT FK_BusLiveLocation_Buses FOREIGN KEY (BusId) REFERENCES Buses(BusId),
    CONSTRAINT FK_BusLiveLocation_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId)
);
GO

-- 29. BUS IMAGES
CREATE TABLE BusImages (
    SchoolId     INT           NULL,
    BusImageId   INT           NOT NULL IDENTITY(1,1),
    BusId        INT           NOT NULL,
    ImageUrl     NVARCHAR(500) NOT NULL,
    DisplayOrder INT           NOT NULL CONSTRAINT DF_BusImages_DisplayOrder DEFAULT 0,
    IsPrimary    BIT           NOT NULL CONSTRAINT DF_BusImages_IsPrimary DEFAULT 0,
    UploadedAt   DATETIME2     NOT NULL CONSTRAINT DF_BusImages_UploadedAt DEFAULT GETUTCDATE(),
    UploadedBy   INT           NULL,
    CONSTRAINT PK_BusImages PRIMARY KEY (BusImageId),
    CONSTRAINT FK_BusImages_Buses FOREIGN KEY (BusId) REFERENCES Buses(BusId) ON DELETE CASCADE,
    CONSTRAINT FK_BusImages_Users FOREIGN KEY (UploadedBy) REFERENCES Users(UserId),
    CONSTRAINT FK_BusImages_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId)
);
GO

-- 30. BUS FUEL LOGS
CREATE TABLE BusFuelLogs (
    SchoolId        INT             NULL,
    FuelLogId       INT             NOT NULL IDENTITY(1,1),
    BusId           INT             NOT NULL,
    DriverId        INT             NULL,
    FuelDate        DATE            NOT NULL,
    FuelLiters      DECIMAL(10,2)   NOT NULL,
    TotalCost       DECIMAL(10,2)   NOT NULL,
    OdometerReading DECIMAL(10,2)   NOT NULL,
    ReceiptImage    NVARCHAR(500)   NULL,
    Notes           NVARCHAR(500)   NULL,
    CreatedAt       DATETIME2       NOT NULL CONSTRAINT DF_BusFuelLogs_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL CONSTRAINT DF_BusFuelLogs_UpdatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_BusFuelLogs PRIMARY KEY (FuelLogId),
    CONSTRAINT FK_BusFuelLogs_Buses FOREIGN KEY (BusId) REFERENCES Buses(BusId) ON DELETE CASCADE,
    CONSTRAINT FK_BusFuelLogs_Users FOREIGN KEY (DriverId) REFERENCES Users(UserId),
    CONSTRAINT FK_BusFuelLogs_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId)
);
GO

-- ============================================================
-- PART 7: ERP ATTENDANCE, PAYMENT GATEWAYS & FEES
-- ============================================================

-- 31. PAYMENT GATEWAY CONFIGS (PhonePe / Razorpay)
CREATE TABLE PaymentGatewayConfigs (
    ConfigId    INT           NOT NULL IDENTITY(1,1),
    SchoolId    INT           NOT NULL,
    GatewayType NVARCHAR(50)  NOT NULL,
    MerchantId  NVARCHAR(200) NULL,
    ApiKey      NVARCHAR(500) NULL,
    SecretKey   NVARCHAR(500) NULL,
    IsActive    BIT           NOT NULL CONSTRAINT DF_PaymentGatewayConfigs_IsActive DEFAULT 1,
    CreatedAt   DATETIME2     NOT NULL CONSTRAINT DF_PaymentGatewayConfigs_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt   DATETIME2     NOT NULL CONSTRAINT DF_PaymentGatewayConfigs_UpdatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_PaymentGatewayConfigs PRIMARY KEY (ConfigId),
    CONSTRAINT FK_PaymentGatewayConfigs_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId) ON DELETE CASCADE,
    CONSTRAINT UQ_PaymentGatewayConfigs_School_Gateway UNIQUE (SchoolId, GatewayType)
);
GO

-- 32. PAYMENT TRANSACTIONS (Audit Trail & Webhooks)
CREATE TABLE PaymentTransactions (
    TransactionId        BIGINT        NOT NULL IDENTITY(1,1),
    SchoolId             INT           NOT NULL,
    AcademicYearId       INT           NULL,
    StudentId            INT           NULL,
    Amount               DECIMAL(18,2) NOT NULL,
    GatewayType          NVARCHAR(50)  NOT NULL,
    GatewayTransactionId NVARCHAR(200) NULL,
    Status               NVARCHAR(50)  NOT NULL,
    Checksum             NVARCHAR(500) NULL,
    RawResponse          NVARCHAR(MAX) NULL,
    CreatedAt            DATETIME2     NOT NULL CONSTRAINT DF_PaymentTransactions_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt            DATETIME2     NOT NULL CONSTRAINT DF_PaymentTransactions_UpdatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_PaymentTransactions PRIMARY KEY (TransactionId),
    CONSTRAINT FK_PaymentTransactions_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId),
    CONSTRAINT FK_PaymentTransactions_AcademicYears FOREIGN KEY (AcademicYearId) REFERENCES AcademicYears(AcademicYearId),
    CONSTRAINT FK_PaymentTransactions_Students FOREIGN KEY (StudentId) REFERENCES Students(StudentId)
);
GO

-- 33. DAILY CLASSROOM ATTENDANCE (Manual Checklist & Face Scan)
CREATE TABLE DailyAttendances (
    AttendanceId   BIGINT        NOT NULL IDENTITY(1,1),
    SchoolId       INT           NOT NULL,
    AcademicYearId INT           NOT NULL,
    StandardId     INT           NOT NULL,
    SectionId      INT           NULL,
    SubjectId      INT           NULL,
    StudentId      INT           NOT NULL,
    AttendanceDate DATE          NOT NULL,
    Status         NVARCHAR(20)  NOT NULL,
    IsFaceScanned  BIT           NOT NULL CONSTRAINT DF_DailyAttendances_IsFaceScanned DEFAULT 0,
    PhotoUrl       NVARCHAR(500) NULL,
    MarkedByUserId INT           NULL,
    CreatedAt      DATETIME2     NOT NULL CONSTRAINT DF_DailyAttendances_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt      DATETIME2     NOT NULL CONSTRAINT DF_DailyAttendances_UpdatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_DailyAttendances PRIMARY KEY (AttendanceId),
    CONSTRAINT FK_DailyAttendances_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId),
    CONSTRAINT FK_DailyAttendances_AcademicYears FOREIGN KEY (AcademicYearId) REFERENCES AcademicYears(AcademicYearId),
    CONSTRAINT FK_DailyAttendances_StandardMasters FOREIGN KEY (StandardId) REFERENCES StandardMasters(StandardId),
    CONSTRAINT FK_DailyAttendances_Sections FOREIGN KEY (SectionId) REFERENCES Sections(SectionId),
    CONSTRAINT FK_DailyAttendances_Subjects FOREIGN KEY (SubjectId) REFERENCES Subjects(SubjectId),
    CONSTRAINT FK_DailyAttendances_Students FOREIGN KEY (StudentId) REFERENCES Students(StudentId),
    CONSTRAINT FK_DailyAttendances_MarkedBy FOREIGN KEY (MarkedByUserId) REFERENCES Users(UserId),
    CONSTRAINT UQ_DailyAttendances_School_Year_Student_Date UNIQUE (SchoolId, AcademicYearId, StudentId, AttendanceDate)
);
GO

-- 34. FEE STRUCTURES
CREATE TABLE FeeStructures (
    FeeStructureId INT           NOT NULL IDENTITY(1,1),
    SchoolId       INT           NOT NULL,
    AcademicYearId INT           NOT NULL,
    StandardId     INT           NOT NULL,
    FeeTitle       NVARCHAR(150) NOT NULL,
    Amount         DECIMAL(18,2) NOT NULL,
    DueDate        DATE          NULL,
    IsActive       BIT           NOT NULL CONSTRAINT DF_FeeStructures_IsActive DEFAULT 1,
    CreatedAt      DATETIME2     NOT NULL CONSTRAINT DF_FeeStructures_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt      DATETIME2     NOT NULL CONSTRAINT DF_FeeStructures_UpdatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_FeeStructures PRIMARY KEY (FeeStructureId),
    CONSTRAINT FK_FeeStructures_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId),
    CONSTRAINT FK_FeeStructures_AcademicYears FOREIGN KEY (AcademicYearId) REFERENCES AcademicYears(AcademicYearId),
    CONSTRAINT FK_FeeStructures_StandardMasters FOREIGN KEY (StandardId) REFERENCES StandardMasters(StandardId)
);
GO

-- 35. FEE PAYMENTS (Receipts & Dues Reports)
CREATE TABLE FeePayments (
    FeePaymentId         BIGINT        NOT NULL IDENTITY(1,1),
    SchoolId             INT           NOT NULL,
    AcademicYearId       INT           NOT NULL,
    StudentId            INT           NOT NULL,
    FeeStructureId       INT           NULL,
    PaymentTransactionId BIGINT        NULL,
    AmountPaid           DECIMAL(18,2) NOT NULL,
    PaymentDate          DATETIME2     NOT NULL CONSTRAINT DF_FeePayments_PaymentDate DEFAULT GETUTCDATE(),
    PaymentMode          NVARCHAR(50)  NOT NULL,
    ReceiptNumber        NVARCHAR(100) NOT NULL,
    ReceiptPdfUrl        NVARCHAR(500) NULL,
    Remarks              NVARCHAR(500) NULL,
    CreatedAt            DATETIME2     NOT NULL CONSTRAINT DF_FeePayments_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt            DATETIME2     NOT NULL CONSTRAINT DF_FeePayments_UpdatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_FeePayments PRIMARY KEY (FeePaymentId),
    CONSTRAINT FK_FeePayments_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId),
    CONSTRAINT FK_FeePayments_AcademicYears FOREIGN KEY (AcademicYearId) REFERENCES AcademicYears(AcademicYearId),
    CONSTRAINT FK_FeePayments_Students FOREIGN KEY (StudentId) REFERENCES Students(StudentId),
    CONSTRAINT FK_FeePayments_FeeStructures FOREIGN KEY (FeeStructureId) REFERENCES FeeStructures(FeeStructureId),
    CONSTRAINT FK_FeePayments_PaymentTransactions FOREIGN KEY (PaymentTransactionId) REFERENCES PaymentTransactions(TransactionId)
);
GO

-- ============================================================
-- PART 8: SYSTEM NOTIFICATIONS, FEEDBACK & LOGS
-- ============================================================

-- 36. NOTIFICATIONS
CREATE TABLE Notifications (
    SchoolId        INT            NULL,
    NotificationId  INT            NOT NULL IDENTITY(1,1),
    RecipientUserId INT            NOT NULL,
    Title           NVARCHAR(200)  NOT NULL,
    Body            NVARCHAR(1000) NOT NULL,
    NotificationType NVARCHAR(50)  NOT NULL,
    ReferenceId     INT            NULL,
    ReferenceType   NVARCHAR(50)   NULL,
    IsRead          BIT            NOT NULL CONSTRAINT DF_Notifications_IsRead DEFAULT 0,
    SentAt          DATETIME2      NOT NULL CONSTRAINT DF_Notifications_SentAt DEFAULT GETUTCDATE(),
    ReadAt          DATETIME2      NULL,
    CONSTRAINT PK_Notifications PRIMARY KEY (NotificationId),
    CONSTRAINT FK_Notifications_Users FOREIGN KEY (RecipientUserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_Notifications_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId)
);
GO

-- 37. NOTIFICATION SETTINGS
CREATE TABLE NotificationSettings (
    SchoolId      INT       NULL,
    SettingId     INT       NOT NULL IDENTITY(1,1),
    UserId        INT       NOT NULL,
    BusApproach   BIT       NOT NULL CONSTRAINT DF_Notif_BusApproach DEFAULT 1,
    BoardedAlert  BIT       NOT NULL CONSTRAINT DF_Notif_BoardedAlert DEFAULT 1,
    DroppedAlert  BIT       NOT NULL CONSTRAINT DF_Notif_DroppedAlert DEFAULT 1,
    LeaveAlert    BIT       NOT NULL CONSTRAINT DF_Notif_LeaveAlert DEFAULT 1,
    PushEnabled   BIT       NOT NULL CONSTRAINT DF_Notif_PushEnabled DEFAULT 1,
    UpdatedAt     DATETIME2 NOT NULL CONSTRAINT DF_Notif_UpdatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_NotificationSettings PRIMARY KEY (SettingId),
    CONSTRAINT FK_NotificationSettings_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT UQ_NotificationSettings_UserId UNIQUE (UserId),
    CONSTRAINT FK_NotificationSettings_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId)
);
GO

-- 38. DEVICE TOKENS (FCM Tokens)
CREATE TABLE DeviceTokens (
    SchoolId    INT           NULL,
    TokenId     INT           NOT NULL IDENTITY(1,1),
    UserId      INT           NOT NULL,
    FcmToken    NVARCHAR(500) NOT NULL,
    DeviceType  NVARCHAR(20)  NOT NULL,
    UpdatedAt   DATETIME2     NOT NULL CONSTRAINT DF_DeviceTokens_UpdatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_DeviceTokens PRIMARY KEY (TokenId),
    CONSTRAINT FK_DeviceTokens_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT UQ_DeviceTokens_UserDevice UNIQUE (UserId, DeviceType),
    CONSTRAINT FK_DeviceTokens_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId)
);
GO

-- 39. FEEDBACKS
CREATE TABLE Feedbacks (
    SchoolId    INT            NULL,
    FeedbackId  INT            NOT NULL IDENTITY(1,1),
    UserId      INT            NOT NULL,
    Category    NVARCHAR(50)   NULL,
    Email       NVARCHAR(255)  NULL,
    PhoneNumber NVARCHAR(20)   NULL,
    Subject     NVARCHAR(200)  NOT NULL,
    Description NVARCHAR(2000) NOT NULL,
    Status      NVARCHAR(20)   NOT NULL CONSTRAINT DF_Feedbacks_Status DEFAULT 'Open',
    AdminReply  NVARCHAR(2000) NULL,
    RepliedBy   INT            NULL,
    RepliedAt   DATETIME2      NULL,
    ResolvedBy  INT            NULL,
    ResolvedAt  DATETIME2      NULL,
    CreatedAt   DATETIME2      NOT NULL CONSTRAINT DF_Feedbacks_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt   DATETIME2      NOT NULL CONSTRAINT DF_Feedbacks_UpdatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Feedbacks PRIMARY KEY (FeedbackId),
    CONSTRAINT FK_Feedbacks_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT FK_Feedbacks_RepliedBy FOREIGN KEY (RepliedBy) REFERENCES Users(UserId),
    CONSTRAINT FK_Feedbacks_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId)
);
GO

-- 40. AUDIT LOGS
CREATE TABLE AuditLogs (
    SchoolId   INT            NULL,
    LogId      INT            NOT NULL IDENTITY(1,1),
    UserId     INT            NULL,
    Action     NVARCHAR(100)  NOT NULL,
    EntityName NVARCHAR(100)  NOT NULL,
    EntityId   INT            NULL,
    OldValues  NVARCHAR(MAX)  NULL,
    NewValues  NVARCHAR(MAX)  NULL,
    IpAddress  NVARCHAR(50)   NULL,
    CreatedAt  DATETIME2      NOT NULL CONSTRAINT DF_AuditLogs_CreatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_AuditLogs PRIMARY KEY (LogId),
    CONSTRAINT FK_AuditLogs_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE SET NULL,
    CONSTRAINT FK_AuditLogs_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId)
);
GO

-- 41. LOGGER
CREATE TABLE Logger (
    LogId              INT            NOT NULL IDENTITY(1,1),
    Timestamp          DATETIME2      NOT NULL CONSTRAINT DF_Logger_Timestamp DEFAULT GETUTCDATE(),
    LogLevel           NVARCHAR(20)   NULL,
    Message            NVARCHAR(MAX)  NULL,
    Exception          NVARCHAR(MAX)  NULL,
    LoggerName         NVARCHAR(250)  NULL,
    SchoolId           INT            NULL,
    UserId             INT            NULL,
    Platform           NVARCHAR(50)   NULL,
    ExceptionMessage   NVARCHAR(MAX)  NULL,
    StackTrace         NVARCHAR(MAX)  NULL,
    RequestUrl         NVARCHAR(2083) NULL,
    Username           NVARCHAR(256)  NULL,
    Role               NVARCHAR(50)   NULL,
    ModuleName         NVARCHAR(100)  NULL,
    ActionName         NVARCHAR(100)  NULL,
    AdditionalDetails  NVARCHAR(MAX)  NULL,
    CONSTRAINT PK_Logger PRIMARY KEY (LogId)
);
GO

-- 42. APP CONFIGURATIONS
CREATE TABLE AppConfigurations (
    SchoolId    INT            NULL,
    ConfigId    INT            NOT NULL IDENTITY(1,1),
    ConfigKey   NVARCHAR(100)  NOT NULL,
    ConfigValue NVARCHAR(MAX)  NULL,
    Description NVARCHAR(500)  NULL,
    Platform    NVARCHAR(20)   NOT NULL CONSTRAINT DF_AppConfigurations_Platform DEFAULT 'Both',
    IsActive    BIT            NOT NULL CONSTRAINT DF_AppConfigurations_IsActive DEFAULT 1,
    CreatedAt   DATETIME2      NOT NULL CONSTRAINT DF_AppConfigurations_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt   DATETIME2      NOT NULL CONSTRAINT DF_AppConfigurations_UpdatedAt DEFAULT GETUTCDATE(),
    CreatedBy   INT            NULL,
    CONSTRAINT PK_AppConfigurations PRIMARY KEY (ConfigId),
    CONSTRAINT UQ_AppConfigurations_Key_Platform UNIQUE (SchoolId, ConfigKey, Platform),
    CONSTRAINT FK_AppConfigurations_Users FOREIGN KEY (CreatedBy) REFERENCES Users(UserId),
    CONSTRAINT FK_AppConfigurations_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(SchoolId)
);
GO

-- 43. GLOBAL CONFIGURATIONS
CREATE TABLE GlobalConfigurations (
    GlobalConfigId    INT           NOT NULL IDENTITY(1,1),
    GlobalConfigKey   NVARCHAR(100) NOT NULL,
    GlobalConfigValue NVARCHAR(MAX) NULL,
    Description       NVARCHAR(500) NULL,
    IsActive          BIT           NOT NULL CONSTRAINT DF_GlobalConfigurations_IsActive DEFAULT 1,
    CreatedAt         DATETIME2     NOT NULL CONSTRAINT DF_GlobalConfigurations_CreatedAt DEFAULT GETUTCDATE(),
    UpdatedAt         DATETIME2     NOT NULL CONSTRAINT DF_GlobalConfigurations_UpdatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT PK_GlobalConfigurations PRIMARY KEY (GlobalConfigId),
    CONSTRAINT UQ_GlobalConfigurations_Key UNIQUE (GlobalConfigKey)
);
GO

-- ============================================================
-- PART 9: VIEWS & STORED PROCEDURES
-- ============================================================

-- V1: Active students with bus and stop info
CREATE VIEW vw_StudentBusInfo AS
SELECT
    s.StudentId,
    s.StudentCode,
    u.FullName          AS StudentName,
    u.Email             AS StudentEmail,
    sm.StandardName     AS Standard,
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
LEFT JOIN StandardMasters sm  ON sm.StandardId = s.StandardId
LEFT JOIN Buses            b  ON b.BusId      = s.BusId
LEFT JOIN BusRouteMappings brm ON brm.BusId    = b.BusId
LEFT JOIN Routes           r  ON r.RouteId    = brm.RouteId
LEFT JOIN Stops            st ON st.StopId    = s.StopId
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
LEFT JOIN BusRouteMappings  brm ON brm.BusId = b.BusId
LEFT JOIN Routes            r   ON r.RouteId = brm.RouteId
LEFT JOIN BusDriverMappings bdm ON bdm.BusId = b.BusId
LEFT JOIN Users             u   ON u.UserId  = bdm.DriverId
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
-- PART 10: INDEXES
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
CREATE NONCLUSTERED INDEX IX_CountryMasters_Name        ON CountryMasters (CountryName);
CREATE NONCLUSTERED INDEX IX_RegionMasters_CountryId     ON RegionMasters (CountryId);
CREATE NONCLUSTERED INDEX IX_AcademicYears_SchoolId     ON AcademicYears (SchoolId, IsCurrent);
CREATE NONCLUSTERED INDEX IX_Sections_School_Standard   ON Sections (SchoolId, StandardId);
CREATE NONCLUSTERED INDEX IX_Subjects_School            ON Subjects (SchoolId);
CREATE NONCLUSTERED INDEX IX_ClassSubjectTeachers_Lookup ON ClassSubjectTeachers (SchoolId, AcademicYearId, StandardId, SectionId);
CREATE NONCLUSTERED INDEX IX_PaymentTransactions_School ON PaymentTransactions (SchoolId, StudentId, Status);
CREATE NONCLUSTERED INDEX IX_DailyAttendances_Date      ON DailyAttendances (SchoolId, AttendanceDate, StandardId, SectionId);
CREATE NONCLUSTERED INDEX IX_FeeStructures_School       ON FeeStructures (SchoolId, AcademicYearId, StandardId);
CREATE NONCLUSTERED INDEX IX_FeePayments_School_Student ON FeePayments (SchoolId, AcademicYearId, StudentId);
GO

-- ============================================================
-- PART 11: SEED RECORDS & INITIAL CONFIGURATIONS (ALL AT THE END)
-- ============================================================

-- 1. SEED DEFAULT SCHOOL
SET IDENTITY_INSERT Schools ON;
INSERT INTO Schools (SchoolId, SchoolName, SchoolCode, SchoolAddress, ContactNumber, EmailAddress, PrincipalName, IsActive, CreatedAt, UpdatedAt)
VALUES (1, 'Default School', 'SCH01', '123 Education Way', '555-0100', 'info@defaultschool.com', 'Dr. John Doe', 1, GETUTCDATE(), GETUTCDATE());
SET IDENTITY_INSERT Schools OFF;
GO

-- 2. SEED DEFAULT SYSTEM ADMIN (Username: sysadmin, Password: Admin123!)
INSERT INTO SystemAdministrators (FullName, UserName, Email, PasswordHash, PasswordSalt, CreatedAt, UpdatedAt)
VALUES ('System Admin', 'sysadmin', 'sysadmin@bustracking.com', '$2a$12$gRiCpH9Cj4ztBpZsTgntH.BM2d/G9mO6VmcbIKD7gRdkk4vT3PpoW', '$2a$12$gRiCpH9Cj4ztBpZsTgntH.', GETUTCDATE(), GETUTCDATE());
GO

-- 3. SEED ROLES
INSERT INTO Roles (RoleName, Description) VALUES
('SuperAdmin',      'Full system access'),
('BusCoordinator',  'Sub-admin with limited permissions assigned by SuperAdmin'),
('Driver',          'Mobile app user – manages bus trips'),
('Teacher',         'Web user – academic staff / class teacher'),
('Parent',          'Web user – tracks kids and manages availability'),
('Student',         'Web user – tracks bus and manages own availability');
GO

-- 4. SEED PERMISSIONS CATALOGUE
INSERT INTO Permissions (ModuleName, PermissionKey, Description) VALUES
('Dashboard',           'dashboard.view',           'View dashboard'),
('Teachers',            'teachers.view',            'View teachers directory and details'),
('Teachers',            'teachers.add',             'Register new teacher account'),
('Teachers',            'teachers.edit',            'Edit teacher profile and toggle active status'),
('Teachers',            'teachers.delete',          'Delete teacher profile'),
('AppConfig',           'appconfig.view',           'View app configurations'),
('AppConfig',           'appconfig.add',            'Add app configuration'),
('AppConfig',           'appconfig.edit',           'Edit app configuration'),
('AppConfig',           'appconfig.delete',         'Delete app configuration'),
('ManageGeographics',   'geo.view',                 'View geographic masters'),
('ManageGeographics',   'geo.manage',               'Manage country and state masters'),
('ManageAcademicYears', 'academicyear.view',        'View academic years & session switcher'),
('ManageAcademicYears', 'academicyear.add',         'Add academic year'),
('ManageAcademicYears', 'academicyear.edit',        'Edit academic year'),
('ManageSections',      'section.view',             'View class sections'),
('ManageSections',      'section.add',              'Add class section'),
('ManageSections',      'section.edit',             'Edit class section'),
('ManageSections',      'section.delete',           'Delete class section'),
('ManageSubjects',      'subject.view',             'View subjects master'),
('ManageSubjects',      'subject.add',              'Add new subject'),
('ManageSubjects',      'subject.edit',             'Edit subject details'),
('ManageSubjects',      'subject.delete',           'Delete subject'),
('ManageClassMappings', 'classmapping.view',        'View subject and teacher assignments'),
('ManageClassMappings', 'classmapping.add',         'Assign subjects and teachers to class sections'),
('ManageClassMappings', 'classmapping.edit',        'Edit class subject teacher assignments'),
('ManageClassMappings', 'classmapping.delete',      'Unassign subject and teacher from class sections'),
('ManageStandards',     'standard.view',            'View standards'),
('ManageStandards',     'standard.add',             'Add standard'),
('ManageStandards',     'standard.edit',            'Edit standard'),
('ManageStandards',     'standard.delete',          'Delete standard'),
('ManageAttendance',    'attendance.view',          'View daily attendance reports'),
('ManageAttendance',    'attendance.mark',          'Mark manual classroom attendance'),
('ManageAttendance',    'attendance.facescan',      'Mark attendance using ONNX face scan'),
('ManagePayments',      'payment.view',             'View payment transaction logs'),
('ManagePayments',      'payment.manage',           'Manage payment transaction audit'),
('ManagePayments',      'payment.config',           'Configure school payment gateways (PhonePe / Razorpay)'),
('ManageFees',          'fee.view',                 'View fee structures & payment history'),
('ManageFees',          'fee.manage',               'Create fee structures & collect payments'),
('ManageFees',          'fee.report',               'View paid vs unpaid student fee reports'),
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
('BroadcastMessage',    'broadcast.manage',         'Can compose and send broadcast notifications to user roles and specific members'),
('HelpSupport',         'helpsupport.view',         'View help & support requests'),
('HelpSupport',         'helpsupport.manage',       'Manage help & support status'),
('ManageLogs',          'logs.view',                'View system logs'),
('ManageFuelLogs',      'fuellog.view',             'View fuel logs'),
('ManageFuelLogs',      'fuellog.manage',           'Manage fuel logs'),
('ManageReports',       'report.view',              'View reports & analytics');
GO

-- 5. SEED DEFAULT SUPER ADMIN USER (Username: superadmin, Password: Admin@123)
INSERT INTO Users (SchoolId, RoleId, FullName, UserName, Email, PhoneNumber, PasswordHash, PasswordSalt, IsActive, IsEmailVerified, CreatedAt, UpdatedAt)
VALUES (1, 1, 'Super Admin', 'superadmin', 'admin@bustracking.com', '555-0199',
        '$2a$12$gRiCpH9Cj4ztBpZsTgntH.BM2d/G9mO6VmcbIKD7gRdkk4vT3PpoW',
        '$2a$12$gRiCpH9Cj4ztBpZsTgntH.', 1, 1, GETUTCDATE(), GETUTCDATE());
GO

-- 6. SEED GLOBAL CONFIGURATIONS
MERGE INTO GlobalConfigurations AS Target
USING (VALUES 
    ('IsMaintencePage',    'false',                                                      'Global Maintenance Mode switch. Set to true to show maintenance screen in mobile app before login.'),
    ('MandatoryUpdateApp', 'false',                                                      'Mandatory Version Update switch. Set to true to force update without cancel option.'),
    ('AndroidVersion',     '1.0.0',                                                      'Minimum required Android app version.'),
    ('iOSVersion',         '1.0.0',                                                      'Minimum required iOS app version.'),
    ('Android_Update_Url', 'https://play.google.com/store/apps/details?id=com.bustrack.bustracking', 'Play Store update link for Android app.'),
    ('iOS_Update_Url',     'https://apps.apple.com/app/id123456789',                     'App Store update link for iOS app.')
) AS Source (GlobalConfigKey, GlobalConfigValue, Description)
ON Target.GlobalConfigKey = Source.GlobalConfigKey
WHEN MATCHED THEN
    UPDATE SET Target.Description = Source.Description, Target.UpdatedAt = GETUTCDATE()
WHEN NOT MATCHED THEN
    INSERT (GlobalConfigKey, GlobalConfigValue, Description, IsActive, CreatedAt, UpdatedAt)
    VALUES (Source.GlobalConfigKey, Source.GlobalConfigValue, Source.Description, 1, GETUTCDATE(), GETUTCDATE());
GO

-- 7. SEED APP CONFIGURATIONS
INSERT INTO AppConfigurations (ConfigKey, ConfigValue, SchoolId, Description, Platform, IsActive, CreatedBy)
VALUES
('GpsIntervalSeconds', '10',  1, 'How often the driver app sends GPS pings (seconds)',        'Mobile', 1, 1),
('SupportEmail',       '',    1, 'Support email shown inside the mobile app',                'Mobile', 1, 1),
('SupportPhone',       '',    1, 'Support phone number shown inside the mobile app',         'Mobile', 1, 1),
('IsMobileUpdateImage',       '0',    1, 'When true: app uploads images via API and shows Upload/Remove buttons',         'Mobile', 1, 1),
('WebsiteImageUrl',       'https://10.0.2.2:7001',    1, 'Used to construct full image URLs when IsMobileUpdateImage = 1',         'Mobile', 1, 1),
('AppConfigPageSize',       '10',    1, 'Number of rows per page on the App Configuration list (Web & Mobile)',         'Both', 1, 1),
('GoogleMapApiKey',       '',    1, 'this is used for connect google map Api',         'Both', 1, 1),
('TrackingHubUrl',       'https://10.0.2.2:7001',    1, '',         'Both', 1, 1),
('IsUseGoogleMap',       '0',    1, 'If you are use paid google map in app then it should be set 1 else 0',         'Both', 1, 1);
GO

-- 8. SEED ACADEMIC YEARS
SET IDENTITY_INSERT AcademicYears ON;
INSERT INTO AcademicYears (AcademicYearId, SchoolId, YearName, StartDate, EndDate, IsActive, IsCurrent, CreatedAt, UpdatedAt)
VALUES (1, 1, '2026-2027', '2026-04-01', '2027-03-31', 1, 1, GETUTCDATE(), GETUTCDATE());
SET IDENTITY_INSERT AcademicYears OFF;
GO

-- 9. SEED PAYMENT GATEWAY CONFIGURATIONS
INSERT INTO PaymentGatewayConfigs (SchoolId, GatewayType, MerchantId, ApiKey, SecretKey, IsActive) VALUES
(1, 'PhonePe',  'MERCHANT_PHONEPE_DEFAULT',  'key_phonepe_test_123',  'secret_phonepe_test_123', 1),
(1, 'Razorpay', 'MERCHANT_RAZORPAY_DEFAULT', 'rzp_test_key_123456',   'rzp_test_secret_123456', 1);
GO

PRINT 'SchoolErpDB database creation and seed completed successfully!';
GO
