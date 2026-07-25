-- ============================================================
--  BUS TRACKING APPLICATION - Exhaustive Global TimeZones Seed Script
--  Platform   : SQL Server (T-SQL)
--  Description: Truncates TimeZoneMasters and seeds all 141 global time zones.
-- ============================================================

PRINT 'Seeding TimeZoneMasters table with all global time zones...';
GO

-- 1. Disable Foreign Key constraints referencing TimeZoneMasters
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Schools_TimeZoneMasters')
BEGIN
    ALTER TABLE Schools NOCHECK CONSTRAINT FK_Schools_TimeZoneMasters;
END;
GO

-- 2. Clear existing records and reset Identity seed
DELETE FROM TimeZoneMasters;
DBCC CHECKIDENT ('TimeZoneMasters', RESEED, 0);
GO

-- 3. Insert all global time zone records with Identity Insert
SET IDENTITY_INSERT TimeZoneMasters ON;
GO

INSERT INTO TimeZoneMasters (TimeZoneId, TimeZoneName, IanaTimeZoneId, WindowsTimeZoneId, UtcOffset, IsActive, DisplayOrder)
VALUES
(1, N'(UTC-12:00) International Date Line West', N'Etc/GMT+12', N'Dateline Standard Time', N'-12:00', 1, 1),
(2, N'(UTC-11:00) Coordinated Universal Time-11', N'Etc/GMT+11', N'UTC-11', N'-11:00', 1, 2),
(3, N'(UTC-10:00) Aleutian Islands', N'America/Adak', N'Aleutian Standard Time', N'-10:00', 1, 3),
(4, N'(UTC-10:00) Hawaii', N'Pacific/Honolulu', N'Hawaiian Standard Time', N'-10:00', 1, 4),
(5, N'(UTC-09:30) Marquesas Islands', N'Pacific/Marquesas', N'Marquesas Standard Time', N'-09:30', 1, 5),
(6, N'(UTC-09:00) Alaska', N'America/Anchorage', N'Alaskan Standard Time', N'-09:00', 1, 6),
(7, N'(UTC-09:00) Coordinated Universal Time-09', N'Etc/GMT+9', N'UTC-09', N'-09:00', 1, 7),
(8, N'(UTC-08:00) Baja California', N'America/Tijuana', N'Pacific Standard Time (Mexico)', N'-08:00', 1, 8),
(9, N'(UTC-08:00) Coordinated Universal Time-08', N'Etc/GMT+8', N'UTC-08', N'-08:00', 1, 9),
(10, N'(UTC-08:00) Pacific Time (US & Canada)', N'America/Los_Angeles', N'Pacific Standard Time', N'-08:00', 1, 10),
(11, N'(UTC-07:00) Arizona', N'America/Phoenix', N'US Mountain Standard Time', N'-07:00', 1, 11),
(12, N'(UTC-07:00) La Paz, Mazatlan', N'America/Chihuahua', N'Mountain Standard Time (Mexico)', N'-07:00', 1, 12),
(13, N'(UTC-07:00) Mountain Time (US & Canada)', N'America/Denver', N'Mountain Standard Time', N'-07:00', 1, 13),
(14, N'(UTC-07:00) Yukon', N'America/Whitehorse', N'Yukon Standard Time', N'-07:00', 1, 14),
(15, N'(UTC-06:00) Central America', N'America/Guatemala', N'Central America Standard Time', N'-06:00', 1, 15),
(16, N'(UTC-06:00) Central Time (US & Canada)', N'America/Chicago', N'Central Standard Time', N'-06:00', 1, 16),
(17, N'(UTC-06:00) Easter Island', N'Pacific/Easter', N'Easter Island Standard Time', N'-06:00', 1, 17),
(18, N'(UTC-06:00) Guadalajara, Mexico City, Monterrey', N'America/Mexico_City', N'Central Standard Time (Mexico)', N'-06:00', 1, 18),
(19, N'(UTC-06:00) Saskatchewan', N'America/Regina', N'Canada Central Standard Time', N'-06:00', 1, 19),
(20, N'(UTC-05:00) Bogota, Lima, Quito, Rio Branco', N'America/Bogota', N'SA Pacific Standard Time', N'-05:00', 1, 20),
(21, N'(UTC-05:00) Chetumal', N'America/Cancun', N'Eastern Standard Time (Mexico)', N'-05:00', 1, 21),
(22, N'(UTC-05:00) Eastern Time (US & Canada)', N'America/New_York', N'Eastern Standard Time', N'-05:00', 1, 22),
(23, N'(UTC-05:00) Haiti', N'America/Port-au-Prince', N'Haiti Standard Time', N'-05:00', 1, 23),
(24, N'(UTC-05:00) Havana', N'America/Havana', N'Cuba Standard Time', N'-05:00', 1, 24),
(25, N'(UTC-05:00) Indiana (East)', N'America/Indianapolis', N'US Eastern Standard Time', N'-05:00', 1, 25),
(26, N'(UTC-05:00) Turks and Caicos', N'America/Grand_Turk', N'Turks And Caicos Standard Time', N'-05:00', 1, 26),
(27, N'(UTC-04:00) Asuncion', N'America/Asuncion', N'Paraguay Standard Time', N'-04:00', 1, 27),
(28, N'(UTC-04:00) Atlantic Time (Canada)', N'America/Halifax', N'Atlantic Standard Time', N'-04:00', 1, 28),
(29, N'(UTC-04:00) Caracas', N'America/Caracas', N'Venezuela Standard Time', N'-04:00', 1, 29),
(30, N'(UTC-04:00) Cuiaba', N'America/Cuiaba', N'Central Brazilian Standard Time', N'-04:00', 1, 30),
(31, N'(UTC-04:00) Georgetown, La Paz, Manaus, San Juan', N'America/La_Paz', N'SA Western Standard Time', N'-04:00', 1, 31),
(32, N'(UTC-04:00) Santiago', N'America/Santiago', N'Pacific SA Standard Time', N'-04:00', 1, 32),
(33, N'(UTC-03:30) Newfoundland', N'America/St_Johns', N'Newfoundland Standard Time', N'-03:30', 1, 33),
(34, N'(UTC-03:00) Araguaina', N'America/Araguaina', N'Tocantins Standard Time', N'-03:00', 1, 34),
(35, N'(UTC-03:00) Brasilia', N'America/Sao_Paulo', N'E. South America Standard Time', N'-03:00', 1, 35),
(36, N'(UTC-03:00) Buenos Aires', N'America/Buenos_Aires', N'Argentina Standard Time', N'-03:00', 1, 36),
(37, N'(UTC-03:00) Cayenne, Fortaleza', N'America/Cayenne', N'SA Eastern Standard Time', N'-03:00', 1, 37),
(38, N'(UTC-03:00) Montevideo', N'America/Montevideo', N'Montevideo Standard Time', N'-03:00', 1, 38),
(39, N'(UTC-03:00) Punta Arenas', N'America/Punta_Arenas', N'Magallanes Standard Time', N'-03:00', 1, 39),
(40, N'(UTC-03:00) Salvador', N'America/Bahia', N'Bahia Standard Time', N'-03:00', 1, 40),
(41, N'(UTC-03:00) Miquelon, St. Pierre', N'America/Miquelon', N'Saint Pierre Standard Time', N'-03:00', 1, 41),
(42, N'(UTC-02:00) Greenland', N'America/Nuuk', N'Greenland Standard Time', N'-02:00', 1, 42),
(43, N'(UTC-02:00) Coordinated Universal Time-02', N'Etc/GMT+2', N'UTC-02', N'-02:00', 1, 43),
(44, N'(UTC-02:00) Mid-Atlantic - Old', N'Atlantic/South_Georgia', N'Mid-Atlantic Standard Time', N'-02:00', 1, 44),
(45, N'(UTC-01:00) Azores', N'Atlantic/Azores', N'Azores Standard Time', N'-01:00', 1, 45),
(46, N'(UTC-01:00) Cape Verde Is.', N'Atlantic/Cape_Verde', N'Cape Verde Standard Time', N'-01:00', 1, 46),
(47, N'(UTC+00:00) Coordinated Universal Time', N'Etc/UTC', N'UTC', N'+00:00', 1, 47),
(48, N'(UTC+00:00) Dublin, Edinburgh, Lisbon, London', N'Europe/London', N'GMT Standard Time', N'+00:00', 1, 48),
(49, N'(UTC+00:00) Monrovia, Reykjavik', N'Africa/Monrovia', N'Greenwich Standard Time', N'+00:00', 1, 49),
(50, N'(UTC+00:00) Sao Tome', N'Africa/Sao_Tome', N'Sao Tome Standard Time', N'+00:00', 1, 50),
(51, N'(UTC+01:00) Casablanca', N'Africa/Casablanca', N'Morocco Standard Time', N'+01:00', 1, 51),
(52, N'(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna', N'Europe/Berlin', N'W. Europe Standard Time', N'+01:00', 1, 52),
(53, N'(UTC+01:00) Belgrade, Bratislava, Budapest, Ljubljana, Prague', N'Europe/Budapest', N'Central Europe Standard Time', N'+01:00', 1, 53),
(54, N'(UTC+01:00) Brussels, Copenhagen, Madrid, Paris', N'Europe/Paris', N'Romance Standard Time', N'+01:00', 1, 54),
(55, N'(UTC+01:00) Sarajevo, Skopje, Warsaw, Zagreb', N'Europe/Warsaw', N'Central European Standard Time', N'+01:00', 1, 55),
(56, N'(UTC+01:00) West Central Africa', N'Africa/Lagos', N'W. Central Africa Standard Time', N'+01:00', 1, 56),
(57, N'(UTC+02:00) Amman', N'Asia/Amman', N'Jordan Standard Time', N'+02:00', 1, 57),
(58, N'(UTC+02:00) Athens, Bucharest', N'Europe/Athens', N'GTB Standard Time', N'+02:00', 1, 58),
(59, N'(UTC+02:00) Beirut', N'Asia/Beirut', N'Middle East Standard Time', N'+02:00', 1, 59),
(60, N'(UTC+02:00) Cairo', N'Africa/Cairo', N'Egypt Standard Time', N'+02:00', 1, 60),
(61, N'(UTC+02:00) Chisinau', N'Europe/Chisinau', N'E. Europe Standard Time', N'+02:00', 1, 61),
(62, N'(UTC+02:00) Harare, Pretoria', N'Africa/Johannesburg', N'South Africa Standard Time', N'+02:00', 1, 62),
(63, N'(UTC+02:00) Helsinki, Kyiv, Riga, Sofia, Tallinn, Vilnius', N'Europe/Helsinki', N'FLE Standard Time', N'+02:00', 1, 63),
(64, N'(UTC+02:00) Jerusalem', N'Asia/Jerusalem', N'Jerusalem Standard Time', N'+02:00', 1, 64),
(65, N'(UTC+02:00) Juba', N'Africa/Juba', N'South Sudan Standard Time', N'+02:00', 1, 65),
(66, N'(UTC+02:00) Khartoum', N'Africa/Khartoum', N'Sudan Standard Time', N'+02:00', 1, 66),
(67, N'(UTC+02:00) Windhoek', N'Africa/Windhoek', N'Namibia Standard Time', N'+02:00', 1, 67),
(68, N'(UTC+03:00) Baghdad', N'Asia/Baghdad', N'Arabic Standard Time', N'+03:00', 1, 68),
(69, N'(UTC+03:00) Istanbul', N'Europe/Istanbul', N'Turkey Standard Time', N'+03:00', 1, 69),
(70, N'(UTC+03:00) Kuwait, Riyadh', N'Asia/Riyadh', N'Arab Standard Time', N'+03:00', 1, 70),
(71, N'(UTC+03:00) Minsk', N'Europe/Minsk', N'Belarus Standard Time', N'+03:00', 1, 71),
(72, N'(UTC+03:00) Moscow, St. Petersburg', N'Europe/Moscow', N'Russian Standard Time', N'+03:00', 1, 72),
(73, N'(UTC+03:00) Nairobi', N'Africa/Nairobi', N'E. Africa Standard Time', N'+03:00', 1, 73),
(74, N'(UTC+03:00) Volgograd', N'Europe/Volgograd', N'Volgograd Standard Time', N'+03:00', 1, 74),
(75, N'(UTC+03:30) Tehran', N'Asia/Tehran', N'Iran Standard Time', N'+03:30', 1, 75),
(76, N'(UTC+04:00) Abu Dhabi, Muscat', N'Asia/Dubai', N'Arabian Standard Time', N'+04:00', 1, 76),
(77, N'(UTC+04:00) Astrakhan, Ulyanovsk', N'Europe/Astrakhan', N'Astrakhan Standard Time', N'+04:00', 1, 77),
(78, N'(UTC+04:00) Baku', N'Asia/Baku', N'Azerbaijan Standard Time', N'+04:00', 1, 78),
(79, N'(UTC+04:00) Samara, Ufa', N'Europe/Samara', N'Russia Time Zone 3', N'+04:00', 1, 79),
(80, N'(UTC+04:00) Port Louis', N'Indian/Mauritius', N'Mauritius Standard Time', N'+04:00', 1, 80),
(81, N'(UTC+04:00) Saratov', N'Europe/Saratov', N'Saratov Standard Time', N'+04:00', 1, 81),
(82, N'(UTC+04:00) Tbilisi', N'Asia/Tbilisi', N'Georgian Standard Time', N'+04:00', 1, 82),
(83, N'(UTC+04:00) Yerevan', N'Asia/Yerevan', N'Caucasus Standard Time', N'+04:00', 1, 83),
(84, N'(UTC+04:30) Kabul', N'Asia/Kabul', N'Afghanistan Standard Time', N'+04:30', 1, 84),
(85, N'(UTC+05:00) Tashkent, Ashgabat', N'Asia/Tashkent', N'West Asia Standard Time', N'+05:00', 1, 85),
(86, N'(UTC+05:00) Ekaterinburg', N'Asia/Yekaterinburg', N'Ekaterinburg Standard Time', N'+05:00', 1, 86),
(87, N'(UTC+05:00) Islamabad, Karachi', N'Asia/Karachi', N'Pakistan Standard Time', N'+05:00', 1, 87),
(88, N'(UTC+05:00) Qyzylorda', N'Asia/Qyzylorda', N'Qyzylorda Standard Time', N'+05:00', 1, 88),
(89, N'(UTC+05:30) Chennai, Kolkata, Mumbai, New Delhi', N'Asia/Kolkata', N'India Standard Time', N'+05:30', 1, 89),
(90, N'(UTC+05:30) Sri Jayawardenepura', N'Asia/Colombo', N'Sri Lanka Standard Time', N'+05:30', 1, 90),
(91, N'(UTC+05:45) Kathmandu', N'Asia/Kathmandu', N'Nepal Standard Time', N'+05:45', 1, 91),
(92, N'(UTC+06:00) Astana, Almaty', N'Asia/Almaty', N'Central Asia Standard Time', N'+06:00', 1, 92),
(93, N'(UTC+06:00) Dhaka', N'Asia/Dhaka', N'Bangladesh Standard Time', N'+06:00', 1, 93),
(94, N'(UTC+06:00) Omsk', N'Asia/Omsk', N'Omsk Standard Time', N'+06:00', 1, 94),
(95, N'(UTC+06:30) Yangon (Rangoon)', N'Asia/Yangon', N'Myanmar Standard Time', N'+06:30', 1, 95),
(96, N'(UTC+07:00) Bangkok, Hanoi, Jakarta', N'Asia/Bangkok', N'SE Asia Standard Time', N'+07:00', 1, 96),
(97, N'(UTC+07:00) Barnaul, Gorno-Altaysk', N'Asia/Barnaul', N'Altai Standard Time', N'+07:00', 1, 97),
(98, N'(UTC+07:00) Hovd', N'Asia/Hovd', N'W. Mongolia Standard Time', N'+07:00', 1, 98),
(99, N'(UTC+07:00) Novosibirsk', N'Asia/Novosibirsk', N'N. Central Asia Standard Time', N'+07:00', 1, 99),
(100, N'(UTC+07:00) Tomsk', N'Asia/Tomsk', N'Tomsk Standard Time', N'+07:00', 1, 100),
(101, N'(UTC+07:00) Krasnoyarsk', N'Asia/Krasnoyarsk', N'North Asia Standard Time', N'+07:00', 1, 101),
(102, N'(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi', N'Asia/Shanghai', N'China Standard Time', N'+08:00', 1, 102),
(103, N'(UTC+08:00) Irkutsk', N'Asia/Irkutsk', N'North Asia East Standard Time', N'+08:00', 1, 103),
(104, N'(UTC+08:00) Kuala Lumpur, Singapore', N'Asia/Singapore', N'Singapore Standard Time', N'+08:00', 1, 104),
(105, N'(UTC+08:00) Perth', N'Australia/Perth', N'W. Australia Standard Time', N'+08:00', 1, 105),
(106, N'(UTC+08:00) Taipei', N'Asia/Taipei', N'Taipei Standard Time', N'+08:00', 1, 106),
(107, N'(UTC+08:00) Ulaanbaatar', N'Asia/Ulaanbaatar', N'Ulaanbaatar Standard Time', N'+08:00', 1, 107),
(108, N'(UTC+08:45) Eucla', N'Australia/Eucla', N'Aus Central W. Standard Time', N'+08:45', 1, 108),
(109, N'(UTC+09:00) Chita', N'Asia/Chita', N'Transbaikal Standard Time', N'+09:00', 1, 109),
(110, N'(UTC+09:00) Osaka, Sapporo, Tokyo', N'Asia/Tokyo', N'Tokyo Standard Time', N'+09:00', 1, 110),
(111, N'(UTC+09:00) Pyongyang', N'Asia/Pyongyang', N'North Korea Standard Time', N'+09:00', 1, 111),
(112, N'(UTC+09:00) Seoul', N'Asia/Seoul', N'Korea Standard Time', N'+09:00', 1, 112),
(113, N'(UTC+09:00) Yakutsk', N'Asia/Yakutsk', N'Yakutsk Standard Time', N'+09:00', 1, 113),
(114, N'(UTC+09:30) Adelaide', N'Australia/Adelaide', N'Cen. Australia Standard Time', N'+09:30', 1, 114),
(115, N'(UTC+09:30) Darwin', N'Australia/Darwin', N'AUS Central Standard Time', N'+09:30', 1, 115),
(116, N'(UTC+10:00) Brisbane', N'Australia/Brisbane', N'E. Australia Standard Time', N'+10:00', 1, 116),
(117, N'(UTC+10:00) Canberra, Melbourne, Sydney', N'Australia/Sydney', N'AUS Eastern Standard Time', N'+10:00', 1, 117),
(118, N'(UTC+10:00) Guam, Port Moresby', N'Pacific/Port_Moresby', N'West Pacific Standard Time', N'+10:00', 1, 118),
(119, N'(UTC+10:00) Hobart', N'Australia/Hobart', N'Tasmania Standard Time', N'+10:00', 1, 119),
(120, N'(UTC+10:00) Vladivostok', N'Asia/Vladivostok', N'Vladivostok Standard Time', N'+10:00', 1, 120),
(121, N'(UTC+10:30) Lord Howe Island', N'Australia/Lord_Howe', N'Lord Howe Standard Time', N'+10:30', 1, 121),
(122, N'(UTC+11:00) Bougainville Island', N'Pacific/Bougainville', N'Bougainville Standard Time', N'+11:00', 1, 122),
(123, N'(UTC+11:00) Chokurdakh', N'Asia/Srednekolymsk', N'Russia Time Zone 10', N'+11:00', 1, 123),
(124, N'(UTC+11:00) Magadan', N'Asia/Magadan', N'Magadan Standard Time', N'+11:00', 1, 124),
(125, N'(UTC+11:00) Norfolk Island', N'Pacific/Norfolk', N'Norfolk Standard Time', N'+11:00', 1, 125),
(126, N'(UTC+11:00) Sakhalin', N'Asia/Sakhalin', N'Sakhalin Standard Time', N'+11:00', 1, 126),
(127, N'(UTC+11:00) Solomon Is., New Caledonia', N'Pacific/Guadalcanal', N'Central Pacific Standard Time', N'+11:00', 1, 127),
(128, N'(UTC+12:00) Anadyr, Petropavlovsk-Kamchatsky', N'Asia/Kamchatka', N'Russia Time Zone 11', N'+12:00', 1, 128),
(129, N'(UTC+12:00) Auckland, Wellington', N'Pacific/Auckland', N'New Zealand Standard Time', N'+12:00', 1, 129),
(130, N'(UTC+12:00) Coordinated Universal Time+12', N'Etc/GMT-12', N'UTC+12', N'+12:00', 1, 130),
(131, N'(UTC+12:00) Fiji', N'Pacific/Fiji', N'Fiji Standard Time', N'+12:00', 1, 131),
(132, N'(UTC+12:00) Petropavlovsk-Kamchatsky - Old', N'UTC', N'Kamchatka Standard Time', N'+12:00', 1, 132),
(133, N'(UTC+12:45) Chatham Islands', N'Pacific/Chatham', N'Chatham Islands Standard Time', N'+12:45', 1, 133),
(134, N'(UTC+13:00) Coordinated Universal Time+13', N'Etc/GMT-13', N'UTC+13', N'+13:00', 1, 134),
(135, N'(UTC+13:00) Nuku''alofa', N'Pacific/Tongatapu', N'Tonga Standard Time', N'+13:00', 1, 135),
(136, N'(UTC+13:00) Samoa', N'Pacific/Apia', N'Samoa Standard Time', N'+13:00', 1, 136),
(137, N'(UTC+14:00) Kiritimati Island', N'Pacific/Kiritimati', N'Line Islands Standard Time', N'+14:00', 1, 137);

SET IDENTITY_INSERT TimeZoneMasters OFF;
GO

-- 4. Re-enable Foreign Key constraints
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Schools_TimeZoneMasters')
BEGIN
    ALTER TABLE Schools WITH CHECK CHECK CONSTRAINT FK_Schools_TimeZoneMasters;
END;
GO

PRINT 'TimeZoneMasters truncated and seeded with all global time zones successfully.';
GO
