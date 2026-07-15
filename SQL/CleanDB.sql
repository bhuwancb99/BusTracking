-- ============================================================
--  BUS TRACKING APPLICATION - Database Cleanup Script
--  Platform : SQL Server (T-SQL)
--  Description:
--    1. Disables all foreign key constraints.
--    2. Deletes all records from all tables.
--    3. Drops all views, stored procedures, and tables.
-- ============================================================

USE BusTrackingDB;
GO

PRINT 'Starting database cleanup...';
GO

-- ────────────────────────────────────────────────────────────
-- 1. Disable all Foreign Key and Check Constraints
-- ────────────────────────────────────────────────────────────
PRINT 'Disabling all constraints...';
EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all";
GO

-- ────────────────────────────────────────────────────────────
-- 2. Delete all records from all tables
-- ────────────────────────────────────────────────────────────
PRINT 'Deleting all records from all tables...';
BEGIN TRANSACTION;
BEGIN TRY
    -- Order designed to minimize constraint checks (even though disabled)
    IF OBJECT_ID('AuditLogs', 'U') IS NOT NULL DELETE FROM AuditLogs;
    IF OBJECT_ID('BusLiveLocation', 'U') IS NOT NULL DELETE FROM BusLiveLocation;
    IF OBJECT_ID('StudentTripStatus', 'U') IS NOT NULL DELETE FROM StudentTripStatus;
    IF OBJECT_ID('TripStopEvents', 'U') IS NOT NULL DELETE FROM TripStopEvents;
    IF OBJECT_ID('BusTrips', 'U') IS NOT NULL DELETE FROM BusTrips;
    IF OBJECT_ID('StudentAvailabilities', 'U') IS NOT NULL DELETE FROM StudentAvailabilities;
    IF OBJECT_ID('ParentStudents', 'U') IS NOT NULL DELETE FROM ParentStudents;
    IF OBJECT_ID('Parents', 'U') IS NOT NULL DELETE FROM Parents;
    IF OBJECT_ID('Students', 'U') IS NOT NULL DELETE FROM Students;
    IF OBJECT_ID('DriverDetails', 'U') IS NOT NULL DELETE FROM DriverDetails;
    IF OBJECT_ID('BusImages', 'U') IS NOT NULL DELETE FROM BusImages;
    IF OBJECT_ID('Buses', 'U') IS NOT NULL DELETE FROM Buses;
    IF OBJECT_ID('BusTypeMasters', 'U') IS NOT NULL DELETE FROM BusTypeMasters;
    IF OBJECT_ID('Stops', 'U') IS NOT NULL DELETE FROM Stops;
    IF OBJECT_ID('Routes', 'U') IS NOT NULL DELETE FROM Routes;
    IF OBJECT_ID('SubAdminPermissions', 'U') IS NOT NULL DELETE FROM SubAdminPermissions;
    IF OBJECT_ID('PasswordResetTokens', 'U') IS NOT NULL DELETE FROM PasswordResetTokens;
    IF OBJECT_ID('AppConfigurations', 'U') IS NOT NULL DELETE FROM AppConfigurations;
    IF OBJECT_ID('DeviceTokens', 'U') IS NOT NULL DELETE FROM DeviceTokens;
    IF OBJECT_ID('Feedbacks', 'U') IS NOT NULL DELETE FROM Feedbacks;
    IF OBJECT_ID('Notifications', 'U') IS NOT NULL DELETE FROM Notifications;
    IF OBJECT_ID('NotificationSettings', 'U') IS NOT NULL DELETE FROM NotificationSettings;
    IF OBJECT_ID('Users', 'U') IS NOT NULL DELETE FROM Users;
    IF OBJECT_ID('Permissions', 'U') IS NOT NULL DELETE FROM Permissions;
    IF OBJECT_ID('Roles', 'U') IS NOT NULL DELETE FROM Roles;

    COMMIT TRANSACTION;
    PRINT 'All records deleted successfully.';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Error occurred during deletion: ' + ERROR_MESSAGE();
END CATCH;
GO

-- ────────────────────────────────────────────────────────────
-- 3. Dynamically drop all Foreign Key constraints
-- ────────────────────────────────────────────────────────────
PRINT 'Dropping all foreign key constraints...';
DECLARE @sql_fk NVARCHAR(MAX) = N'';
SELECT @sql_fk += N'
ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' + QUOTENAME(OBJECT_NAME(parent_object_id)) + 
' DROP CONSTRAINT ' + QUOTENAME(name) + ';'
FROM sys.foreign_keys;

IF @sql_fk <> N''
BEGIN
    EXEC sp_executesql @sql_fk;
    PRINT 'All foreign key constraints dropped.';
END
ELSE
BEGIN
    PRINT 'No foreign key constraints found to drop.';
END
GO

-- ────────────────────────────────────────────────────────────
-- 4. Drop all Views
-- ────────────────────────────────────────────────────────────
PRINT 'Dropping all views...';
IF OBJECT_ID('vw_StudentBusInfo', 'V') IS NOT NULL DROP VIEW vw_StudentBusInfo;
IF OBJECT_ID('vw_BusDriverInfo', 'V') IS NOT NULL DROP VIEW vw_BusDriverInfo;
IF OBJECT_ID('vw_BusLatestLocation', 'V') IS NOT NULL DROP VIEW vw_BusLatestLocation;
PRINT 'All views dropped.';
GO

-- ────────────────────────────────────────────────────────────
-- 5. Drop all Stored Procedures
-- ────────────────────────────────────────────────────────────
PRINT 'Dropping all stored procedures...';
IF OBJECT_ID('sp_GetTripStudents', 'P') IS NOT NULL DROP PROCEDURE sp_GetTripStudents;
IF OBJECT_ID('sp_UpdateStudentBoardingStatus', 'P') IS NOT NULL DROP PROCEDURE sp_UpdateStudentBoardingStatus;
IF OBJECT_ID('sp_InsertBusLocation', 'P') IS NOT NULL DROP PROCEDURE sp_InsertBusLocation;
IF OBJECT_ID('sp_GetDashboardSummary', 'P') IS NOT NULL DROP PROCEDURE sp_GetDashboardSummary;
PRINT 'All stored procedures dropped.';
GO

-- ────────────────────────────────────────────────────────────
-- 6. Drop all Tables
-- ────────────────────────────────────────────────────────────
PRINT 'Dropping all tables...';
-- Drop tables in reverse dependency order to ensure clean execution
IF OBJECT_ID('AuditLogs', 'U') IS NOT NULL DROP TABLE AuditLogs;
IF OBJECT_ID('BusLiveLocation', 'U') IS NOT NULL DROP TABLE BusLiveLocation;
IF OBJECT_ID('StudentTripStatus', 'U') IS NOT NULL DROP TABLE StudentTripStatus;
IF OBJECT_ID('TripStopEvents', 'U') IS NOT NULL DROP TABLE TripStopEvents;
IF OBJECT_ID('BusTrips', 'U') IS NOT NULL DROP TABLE BusTrips;
IF OBJECT_ID('StudentAvailabilities', 'U') IS NOT NULL DROP TABLE StudentAvailabilities;
IF OBJECT_ID('ParentStudents', 'U') IS NOT NULL DROP TABLE ParentStudents;
IF OBJECT_ID('Parents', 'U') IS NOT NULL DROP TABLE Parents;
IF OBJECT_ID('Students', 'U') IS NOT NULL DROP TABLE Students;
IF OBJECT_ID('DriverDetails', 'U') IS NOT NULL DROP TABLE DriverDetails;
IF OBJECT_ID('BusImages', 'U') IS NOT NULL DROP TABLE BusImages;
IF OBJECT_ID('Buses', 'U') IS NOT NULL DROP TABLE Buses;
IF OBJECT_ID('BusTypeMasters', 'U') IS NOT NULL DROP TABLE BusTypeMasters;
IF OBJECT_ID('Stops', 'U') IS NOT NULL DROP TABLE Stops;
IF OBJECT_ID('Routes', 'U') IS NOT NULL DROP TABLE Routes;
IF OBJECT_ID('SubAdminPermissions', 'U') IS NOT NULL DROP TABLE SubAdminPermissions;
IF OBJECT_ID('PasswordResetTokens', 'U') IS NOT NULL DROP TABLE PasswordResetTokens;
IF OBJECT_ID('AppConfigurations', 'U') IS NOT NULL DROP TABLE AppConfigurations;
IF OBJECT_ID('DeviceTokens', 'U') IS NOT NULL DROP TABLE DeviceTokens;
IF OBJECT_ID('Feedbacks', 'U') IS NOT NULL DROP TABLE Feedbacks;
IF OBJECT_ID('Notifications', 'U') IS NOT NULL DROP TABLE Notifications;
IF OBJECT_ID('NotificationSettings', 'U') IS NOT NULL DROP TABLE NotificationSettings;
IF OBJECT_ID('Users', 'U') IS NOT NULL DROP TABLE Users;
IF OBJECT_ID('Permissions', 'U') IS NOT NULL DROP TABLE Permissions;
IF OBJECT_ID('Roles', 'U') IS NOT NULL DROP TABLE Roles;
PRINT 'All tables dropped.';
GO

PRINT 'Database cleanup completed successfully. Database is now empty.';
GO
