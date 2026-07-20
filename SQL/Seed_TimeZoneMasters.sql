-- ============================================================
--  BUS TRACKING APPLICATION - Complete Global TimeZones Seed Script
--  Platform : SQL Server (T-SQL)
--  Description: Comprehensive list of global time zones.
-- ============================================================

PRINT 'Seeding TimeZoneMasters table with global time zones...';
GO

SET IDENTITY_INSERT TimeZoneMasters ON;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 1)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (1, N'(UTC+05:30) India Standard Time - Chennai, Kolkata, Mumbai, New Delhi', N'Asia/Kolkata', N'India Standard Time', N'+05:30', 1, 1);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 2)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (2, N'(UTC+04:00) Gulf Standard Time - Dubai, Abu Dhabi, Muscat', N'Asia/Dubai', N'Arabian Standard Time', N'+04:00', 1, 2);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 3)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (3, N'(UTC+03:00) Arabia Standard Time - Riyadh, Kuwait, Qatar, Baghdad', N'Asia/Riyadh', N'Arab Standard Time', N'+03:00', 1, 3);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 4)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (4, N'(UTC+00:00) Greenwich Mean Time - London, Dublin, Lisbon, Edinburgh', N'Europe/London', N'GMT Standard Time', N'+00:00', 1, 4);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 5)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (5, N'(UTC-05:00) Eastern Standard Time - New York, Washington, Toronto', N'America/New_York', N'Eastern Standard Time', N'-05:00', 1, 5);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 6)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (6, N'(UTC-08:00) Pacific Standard Time - Los Angeles, Vancouver, Seattle', N'America/Los_Angeles', N'Pacific Standard Time', N'-08:00', 1, 6);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 7)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (7, N'(UTC+08:00) Singapore Standard Time - Singapore, Beijing, Hong Kong, Kuala Lumpur', N'Asia/Singapore', N'Singapore Standard Time', N'+08:00', 1, 7);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 8)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (8, N'(UTC-12:00) International Date Line West', N'Etc/GMT+12', N'Dateline Standard Time', N'-12:00', 1, 8);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 9)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (9, N'(UTC-11:00) Coordinated Universal Time-11', N'Etc/GMT+11', N'UTC-11', N'-11:00', 1, 9);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 10)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (10, N'(UTC-10:00) Hawaii', N'Pacific/Honolulu', N'Hawaiian Standard Time', N'-10:00', 1, 10);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 11)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (11, N'(UTC-09:00) Alaska', N'America/Anchorage', N'Alaskan Standard Time', N'-09:00', 1, 11);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 12)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (12, N'(UTC-07:00) Mountain Standard Time - Denver, Phoenix, Calgary', N'America/Denver', N'Mountain Standard Time', N'-07:00', 1, 12);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 13)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (13, N'(UTC-06:00) Central Standard Time - Chicago, Mexico City, Winnipeg', N'America/Chicago', N'Central Standard Time', N'-06:00', 1, 13);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 14)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (14, N'(UTC-04:00) Atlantic Standard Time - Halifax, San Juan', N'America/Halifax', N'Atlantic Standard Time', N'-04:00', 1, 14);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 15)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (15, N'(UTC-03:30) Newfoundland', N'America/St_Johns', N'Newfoundland Standard Time', N'-03:30', 1, 15);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 16)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (16, N'(UTC-03:00) Brasilia, Buenos Aires, Montevideo', N'America/Sao_Paulo', N'E. South America Standard Time', N'-03:00', 1, 16);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 17)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (17, N'(UTC-02:00) Mid-Atlantic - UTC-02', N'Etc/GMT+2', N'UTC-02', N'-02:00', 1, 17);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 18)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (18, N'(UTC-01:00) Azores, Cape Verde Is.', N'Atlantic/Azores', N'Azores Standard Time', N'-01:00', 1, 18);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 19)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (19, N'(UTC+01:00) Central European Time - Berlin, Paris, Rome, Madrid', N'Europe/Berlin', N'W. Europe Standard Time', N'+01:00', 1, 19);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 20)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (20, N'(UTC+02:00) Eastern European Time - Athens, Cairo, Helsinki, Kyiv', N'Europe/Athens', N'GTB Standard Time', N'+02:00', 1, 20);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 21)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (21, N'(UTC+03:00) Moscow Standard Time - Moscow, St. Petersburg, Volgograd', N'Europe/Moscow', N'Russian Standard Time', N'+03:00', 1, 21);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 22)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (22, N'(UTC+03:30) Iran Standard Time - Tehran', N'Asia/Tehran', N'Iran Standard Time', N'+03:30', 1, 22);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 23)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (23, N'(UTC+04:30) Afghanistan Standard Time - Kabul', N'Asia/Kabul', N'Afghanistan Standard Time', N'+04:30', 1, 23);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 24)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (24, N'(UTC+05:00) Pakistan Standard Time - Islamabad, Karachi, Tashkent', N'Asia/Karachi', N'Pakistan Standard Time', N'+05:00', 1, 24);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 25)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (25, N'(UTC+05:45) Nepal Standard Time - Kathmandu', N'Asia/Kathmandu', N'Nepal Standard Time', N'+05:45', 1, 25);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 26)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (26, N'(UTC+06:00) Bangladesh Standard Time - Dhaka, Astana', N'Asia/Dhaka', N'Bangladesh Standard Time', N'+06:00', 1, 26);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 27)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (27, N'(UTC+06:30) Myanmar Standard Time - Yangon', N'Asia/Yangon', N'Myanmar Standard Time', N'+06:30', 1, 27);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 28)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (28, N'(UTC+07:00) Indochina Time - Bangkok, Hanoi, Jakarta', N'Asia/Bangkok', N'SE Asia Standard Time', N'+07:00', 1, 28);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 29)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (29, N'(UTC+08:45) Aus Central Western Standard Time - Eucla', N'Australia/Eucla', N'Aus Central W. Standard Time', N'+08:45', 1, 29);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 30)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (30, N'(UTC+09:00) Japan Standard Time - Tokyo, Seoul, Osaka', N'Asia/Tokyo', N'Tokyo Standard Time', N'+09:00', 1, 30);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 31)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (31, N'(UTC+09:30) Australian Central Standard Time - Adelaide, Darwin', N'Australia/Adelaide', N'Cen. Australia Standard Time', N'+09:30', 1, 31);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 32)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (32, N'(UTC+10:00) Australian Eastern Standard Time - Sydney, Melbourne, Brisbane', N'Australia/Sydney', N'AUS Eastern Standard Time', N'+10:00', 1, 32);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 33)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (33, N'(UTC+10:30) Lord Howe Island', N'Australia/Lord_Howe', N'Lord Howe Standard Time', N'+10:30', 1, 33);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 34)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (34, N'(UTC+11:00) Solomon Is., New Caledonia, Magadan', N'Pacific/Guadalcanal', N'Central Pacific Standard Time', N'+11:00', 1, 34);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 35)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (35, N'(UTC+12:00) New Zealand Standard Time - Auckland, Wellington, Fiji', N'Pacific/Auckland', N'New Zealand Standard Time', N'+12:00', 1, 35);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 36)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (36, N'(UTC+12:45) Chatham Islands', N'Pacific/Chatham', N'Chatham Islands Standard Time', N'+12:45', 1, 36);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 37)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (37, N'(UTC+13:00) Nuku''alofa, Samoa, Tonga', N'Pacific/Tongatapu', N'Tonga Standard Time', N'+13:00', 1, 37);
END;

IF NOT EXISTS (SELECT 1 FROM TimeZoneMasters WHERE TimeZoneId = 38)
BEGIN
    INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
    VALUES (38, N'(UTC+14:00) Kiritimati Island, Line Islands', N'Pacific/Kiritimati', N'Line Islands Standard Time', N'+14:00', 1, 38);
END;

SET IDENTITY_INSERT TimeZoneMasters OFF;
GO

PRINT 'TimeZoneMasters seeded with global time zones successfully.';
GO
