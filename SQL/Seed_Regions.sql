-- ============================================================
--  GEOGRAPHIC MASTERS - Regions Master Seed Script (States / Provinces)
--  Platform : SQL Server (T-SQL - Full Unicode Support)
--  Description:
--    Inserts states, provinces, and regions into RegionMasters mapped dynamically by CountryName.
--    Safe & Idempotent (uses MERGE to prevent duplicate records).
-- ============================================================

USE SchoolErpDB;
GO

PRINT 'Seeding Comprehensive Regions into RegionMasters...';
GO

-- 1. INDIA (28 States + 8 UTs)
DECLARE @IndiaId INT = (SELECT CountryId FROM CountryMasters WHERE CountryName = N'India');
IF @IndiaId IS NOT NULL
BEGIN
    MERGE INTO RegionMasters AS Target
    USING (VALUES
        (@IndiaId, N'Andhra Pradesh',                 N'AP'),
        (@IndiaId, N'Arunachal Pradesh',              N'AR'),
        (@IndiaId, N'Assam',                          N'AS'),
        (@IndiaId, N'Bihar',                          N'BR'),
        (@IndiaId, N'Chhattisgarh',                   N'CG'),
        (@IndiaId, N'Goa',                            N'GA'),
        (@IndiaId, N'Gujarat',                        N'GJ'),
        (@IndiaId, N'Haryana',                        N'HR'),
        (@IndiaId, N'Himachal Pradesh',               N'HP'),
        (@IndiaId, N'Jharkhand',                      N'JH'),
        (@IndiaId, N'Karnataka',                      N'KA'),
        (@IndiaId, N'Kerala',                         N'KL'),
        (@IndiaId, N'Madhya Pradesh',                 N'MP'),
        (@IndiaId, N'Maharashtra',                    N'MH'),
        (@IndiaId, N'Manipur',                        N'MN'),
        (@IndiaId, N'Meghalaya',                      N'ML'),
        (@IndiaId, N'Mizoram',                        N'MZ'),
        (@IndiaId, N'Nagaland',                       N'NL'),
        (@IndiaId, N'Odisha',                         N'OR'),
        (@IndiaId, N'Punjab',                         N'PB'),
        (@IndiaId, N'Rajasthan',                      N'RJ'),
        (@IndiaId, N'Sikkim',                         N'SK'),
        (@IndiaId, N'Tamil Nadu',                     N'TN'),
        (@IndiaId, N'Telangana',                      N'TS'),
        (@IndiaId, N'Tripura',                        N'TR'),
        (@IndiaId, N'Uttar Pradesh',                  N'UP'),
        (@IndiaId, N'Uttarakhand',                    N'UK'),
        (@IndiaId, N'West Bengal',                    N'WB'),
        (@IndiaId, N'Andaman and Nicobar Islands',    N'AN'),
        (@IndiaId, N'Chandigarh',                     N'CH'),
        (@IndiaId, N'Dadra and Nagar Haveli and Daman and Diu', N'DN'),
        (@IndiaId, N'Delhi',                          N'DL'),
        (@IndiaId, N'Jammu and Kashmir',              N'JK'),
        (@IndiaId, N'Ladakh',                         N'LA'),
        (@IndiaId, N'Lakshadweep',                    N'LD'),
        (@IndiaId, N'Puducherry',                     N'PY')
    ) AS Source (CountryId, RegionName, RegionCode)
    ON Target.CountryId = Source.CountryId AND Target.RegionName = Source.RegionName
    WHEN NOT MATCHED THEN
        INSERT (CountryId, RegionName, RegionCode, IsActive, CreatedAt, UpdatedAt)
        VALUES (Source.CountryId, Source.RegionName, Source.RegionCode, 1, GETUTCDATE(), GETUTCDATE());
END

-- 2. UNITED STATES (50 States + DC)
DECLARE @USAId INT = (SELECT CountryId FROM CountryMasters WHERE CountryName = N'United States');
IF @USAId IS NOT NULL
BEGIN
    MERGE INTO RegionMasters AS Target
    USING (VALUES
        (@USAId, N'Alabama',          N'AL'),
        (@USAId, N'Alaska',           N'AK'),
        (@USAId, N'Arizona',          N'AZ'),
        (@USAId, N'Arkansas',         N'AR'),
        (@USAId, N'California',       N'CA'),
        (@USAId, N'Colorado',         N'CO'),
        (@USAId, N'Connecticut',      N'CT'),
        (@USAId, N'Delaware',         N'DE'),
        (@USAId, N'District of Columbia', N'DC'),
        (@USAId, N'Florida',          N'FL'),
        (@USAId, N'Georgia',          N'GA'),
        (@USAId, N'Hawaii',           N'HI'),
        (@USAId, N'Idaho',            N'ID'),
        (@USAId, N'Illinois',         N'IL'),
        (@USAId, N'Indiana',          N'IN'),
        (@USAId, N'Iowa',             N'IA'),
        (@USAId, N'Kansas',           N'KS'),
        (@USAId, N'Kentucky',         N'KY'),
        (@USAId, N'Louisiana',        N'LA'),
        (@USAId, N'Maine',            N'ME'),
        (@USAId, N'Maryland',         N'MD'),
        (@USAId, N'Massachusetts',    N'MA'),
        (@USAId, N'Michigan',         N'MI'),
        (@USAId, N'Minnesota',        N'MN'),
        (@USAId, N'Mississippi',      N'MS'),
        (@USAId, N'Missouri',         N'MO'),
        (@USAId, N'Montana',          N'MT'),
        (@USAId, N'Nebraska',         N'NE'),
        (@USAId, N'Nevada',           N'NV'),
        (@USAId, N'New Hampshire',    N'NH'),
        (@USAId, N'New Jersey',       N'NJ'),
        (@USAId, N'New Mexico',       N'NM'),
        (@USAId, N'New York',         N'NY'),
        (@USAId, N'North Carolina',   N'NC'),
        (@USAId, N'North Dakota',     N'ND'),
        (@USAId, N'Ohio',             N'OH'),
        (@USAId, N'Oklahoma',         N'OK'),
        (@USAId, N'Oregon',           N'OR'),
        (@USAId, N'Pennsylvania',     N'PA'),
        (@USAId, N'Rhode Island',     N'RI'),
        (@USAId, N'South Carolina',   N'SC'),
        (@USAId, N'South Dakota',     N'SD'),
        (@USAId, N'Tennessee',        N'TN'),
        (@USAId, N'Texas',            N'TX'),
        (@USAId, N'Utah',             N'UT'),
        (@USAId, N'Vermont',          N'VT'),
        (@USAId, N'Virginia',         N'VA'),
        (@USAId, N'Washington',       N'WA'),
        (@USAId, N'West Virginia',    N'WV'),
        (@USAId, N'Wisconsin',        N'WI'),
        (@USAId, N'Wyoming',          N'WY')
    ) AS Source (CountryId, RegionName, RegionCode)
    ON Target.CountryId = Source.CountryId AND Target.RegionName = Source.RegionName
    WHEN NOT MATCHED THEN
        INSERT (CountryId, RegionName, RegionCode, IsActive, CreatedAt, UpdatedAt)
        VALUES (Source.CountryId, Source.RegionName, Source.RegionCode, 1, GETUTCDATE(), GETUTCDATE());
END

-- 3. UNITED ARAB EMIRATES (7 Emirates)
DECLARE @UAEId INT = (SELECT CountryId FROM CountryMasters WHERE CountryName = N'United Arab Emirates');
IF @UAEId IS NOT NULL
BEGIN
    MERGE INTO RegionMasters AS Target
    USING (VALUES
        (@UAEId, N'Abu Dhabi',        N'AZ'),
        (@UAEId, N'Ajman',            N'AJ'),
        (@UAEId, N'Dubai',            N'DU'),
        (@UAEId, N'Fujairah',         N'FU'),
        (@UAEId, N'Ras Al Khaimah',   N'RK'),
        (@UAEId, N'Sharjah',          N'SH'),
        (@UAEId, N'Umm Al Quwain',    N'UQ')
    ) AS Source (CountryId, RegionName, RegionCode)
    ON Target.CountryId = Source.CountryId AND Target.RegionName = Source.RegionName
    WHEN NOT MATCHED THEN
        INSERT (CountryId, RegionName, RegionCode, IsActive, CreatedAt, UpdatedAt)
        VALUES (Source.CountryId, Source.RegionName, Source.RegionCode, 1, GETUTCDATE(), GETUTCDATE());
END

-- 4. CANADA (10 Provinces + 3 Territories)
DECLARE @CanadaId INT = (SELECT CountryId FROM CountryMasters WHERE CountryName = N'Canada');
IF @CanadaId IS NOT NULL
BEGIN
    MERGE INTO RegionMasters AS Target
    USING (VALUES
        (@CanadaId, N'Alberta',                   N'AB'),
        (@CanadaId, N'British Columbia',          N'BC'),
        (@CanadaId, N'Manitoba',                  N'MB'),
        (@CanadaId, N'New Brunswick',             N'NB'),
        (@CanadaId, N'Newfoundland and Labrador', N'NL'),
        (@CanadaId, N'Nova Scotia',               N'NS'),
        (@CanadaId, N'Ontario',                   N'ON'),
        (@CanadaId, N'Prince Edward Island',      N'PE'),
        (@CanadaId, N'Quebec',                    N'QC'),
        (@CanadaId, N'Saskatchewan',              N'SK'),
        (@CanadaId, N'Northwest Territories',     N'NT'),
        (@CanadaId, N'Nunavut',                   N'NU'),
        (@CanadaId, N'Yukon',                     N'YT')
    ) AS Source (CountryId, RegionName, RegionCode)
    ON Target.CountryId = Source.CountryId AND Target.RegionName = Source.RegionName
    WHEN NOT MATCHED THEN
        INSERT (CountryId, RegionName, RegionCode, IsActive, CreatedAt, UpdatedAt)
        VALUES (Source.CountryId, Source.RegionName, Source.RegionCode, 1, GETUTCDATE(), GETUTCDATE());
END

-- 5. AUSTRALIA (6 States + 2 Territories)
DECLARE @AustraliaId INT = (SELECT CountryId FROM CountryMasters WHERE CountryName = N'Australia');
IF @AustraliaId IS NOT NULL
BEGIN
    MERGE INTO RegionMasters AS Target
    USING (VALUES
        (@AustraliaId, N'New South Wales',             N'NSW'),
        (@AustraliaId, N'Victoria',                    N'VIC'),
        (@AustraliaId, N'Queensland',                  N'QLD'),
        (@AustraliaId, N'Western Australia',           N'WA'),
        (@AustraliaId, N'South Australia',             N'SA'),
        (@AustraliaId, N'Tasmania',                    N'TAS'),
        (@AustraliaId, N'Australian Capital Territory',N'ACT'),
        (@AustraliaId, N'Northern Territory',          N'NT')
    ) AS Source (CountryId, RegionName, RegionCode)
    ON Target.CountryId = Source.CountryId AND Target.RegionName = Source.RegionName
    WHEN NOT MATCHED THEN
        INSERT (CountryId, RegionName, RegionCode, IsActive, CreatedAt, UpdatedAt)
        VALUES (Source.CountryId, Source.RegionName, Source.RegionCode, 1, GETUTCDATE(), GETUTCDATE());
END

-- 6. UNITED KINGDOM (4 Countries / Regions)
DECLARE @UKId INT = (SELECT CountryId FROM CountryMasters WHERE CountryName = N'United Kingdom');
IF @UKId IS NOT NULL
BEGIN
    MERGE INTO RegionMasters AS Target
    USING (VALUES
        (@UKId, N'England',          N'ENG'),
        (@UKId, N'Scotland',         N'SCT'),
        (@UKId, N'Wales',            N'WLS'),
        (@UKId, N'Northern Ireland', N'NIR')
    ) AS Source (CountryId, RegionName, RegionCode)
    ON Target.CountryId = Source.CountryId AND Target.RegionName = Source.RegionName
    WHEN NOT MATCHED THEN
        INSERT (CountryId, RegionName, RegionCode, IsActive, CreatedAt, UpdatedAt)
        VALUES (Source.CountryId, Source.RegionName, Source.RegionCode, 1, GETUTCDATE(), GETUTCDATE());
END

-- 7. SAUDI ARABIA (13 Regions)
DECLARE @SaudiId INT = (SELECT CountryId FROM CountryMasters WHERE CountryName = N'Saudi Arabia');
IF @SaudiId IS NOT NULL
BEGIN
    MERGE INTO RegionMasters AS Target
    USING (VALUES
        (@SaudiId, N'Riyadh',             N'01'),
        (@SaudiId, N'Makkah',             N'02'),
        (@SaudiId, N'Madinah',            N'03'),
        (@SaudiId, N'Eastern Province',   N'04'),
        (@SaudiId, N'Al-Qassim',          N'05'),
        (@SaudiId, N'Asir',               N'06'),
        (@SaudiId, N'Tabuk',              N'07'),
        (@SaudiId, N'Hail',               N'08'),
        (@SaudiId, N'Northern Borders',   N'09'),
        (@SaudiId, N'Jazan',              N'10'),
        (@SaudiId, N'Najran',             N'11'),
        (@SaudiId, N'Al-Baha',            N'12'),
        (@SaudiId, N'Al-Jowf',            N'13')
    ) AS Source (CountryId, RegionName, RegionCode)
    ON Target.CountryId = Source.CountryId AND Target.RegionName = Source.RegionName
    WHEN NOT MATCHED THEN
        INSERT (CountryId, RegionName, RegionCode, IsActive, CreatedAt, UpdatedAt)
        VALUES (Source.CountryId, Source.RegionName, Source.RegionCode, 1, GETUTCDATE(), GETUTCDATE());
END

-- 8. GERMANY (16 Federal States)
DECLARE @GermanyId INT = (SELECT CountryId FROM CountryMasters WHERE CountryName = N'Germany');
IF @GermanyId IS NOT NULL
BEGIN
    MERGE INTO RegionMasters AS Target
    USING (VALUES
        (@GermanyId, N'Baden-Württemberg',        N'BW'),
        (@GermanyId, N'Bavaria',                  N'BY'),
        (@GermanyId, N'Berlin',                   N'BE'),
        (@GermanyId, N'Brandenburg',              N'BB'),
        (@GermanyId, N'Bremen',                   N'HB'),
        (@GermanyId, N'Hamburg',                  N'HH'),
        (@GermanyId, N'Hesse',                    N'HE'),
        (@GermanyId, N'Mecklenburg-Vorpommern',   N'MV'),
        (@GermanyId, N'Lower Saxony',             N'NI'),
        (@GermanyId, N'North Rhine-Westphalia',   N'NW'),
        (@GermanyId, N'Rhineland-Palatinate',     N'RP'),
        (@GermanyId, N'Saarland',                 N'SL'),
        (@GermanyId, N'Saxony',                   N'SN'),
        (@GermanyId, N'Saxony-Anhalt',            N'ST'),
        (@GermanyId, N'Schleswig-Holstein',       N'SH'),
        (@GermanyId, N'Thuringia',                N'TH')
    ) AS Source (CountryId, RegionName, RegionCode)
    ON Target.CountryId = Source.CountryId AND Target.RegionName = Source.RegionName
    WHEN NOT MATCHED THEN
        INSERT (CountryId, RegionName, RegionCode, IsActive, CreatedAt, UpdatedAt)
        VALUES (Source.CountryId, Source.RegionName, Source.RegionCode, 1, GETUTCDATE(), GETUTCDATE());
END

-- 9. NEPAL (7 Provinces)
DECLARE @NepalId INT = (SELECT CountryId FROM CountryMasters WHERE CountryName = N'Nepal');
IF @NepalId IS NOT NULL
BEGIN
    MERGE INTO RegionMasters AS Target
    USING (VALUES
        (@NepalId, N'Koshi Province',         N'P1'),
        (@NepalId, N'Madhesh Province',       N'P2'),
        (@NepalId, N'Bagmati Province',       N'P3'),
        (@NepalId, N'Gandaki Province',       N'P4'),
        (@NepalId, N'Lumbini Province',       N'P5'),
        (@NepalId, N'Karnali Province',       N'P6'),
        (@NepalId, N'Sudurpashchim Province', N'P7')
    ) AS Source (CountryId, RegionName, RegionCode)
    ON Target.CountryId = Source.CountryId AND Target.RegionName = Source.RegionName
    WHEN NOT MATCHED THEN
        INSERT (CountryId, RegionName, RegionCode, IsActive, CreatedAt, UpdatedAt)
        VALUES (Source.CountryId, Source.RegionName, Source.RegionCode, 1, GETUTCDATE(), GETUTCDATE());
END

-- 10. SRI LANKA (9 Provinces)
DECLARE @SriLankaId INT = (SELECT CountryId FROM CountryMasters WHERE CountryName = N'Sri Lanka');
IF @SriLankaId IS NOT NULL
BEGIN
    MERGE INTO RegionMasters AS Target
    USING (VALUES
        (@SriLankaId, N'Western Province',       N'WP'),
        (@SriLankaId, N'Central Province',       N'CP'),
        (@SriLankaId, N'Southern Province',      N'SP'),
        (@SriLankaId, N'Northern Province',      N'NP'),
        (@SriLankaId, N'Eastern Province',       N'EP'),
        (@SriLankaId, N'North Western Province', N'NWP'),
        (@SriLankaId, N'North Central Province', N'NCP'),
        (@SriLankaId, N'Uva Province',           N'UP'),
        (@SriLankaId, N'Sabaragamuwa Province',  N'SGP')
    ) AS Source (CountryId, RegionName, RegionCode)
    ON Target.CountryId = Source.CountryId AND Target.RegionName = Source.RegionName
    WHEN NOT MATCHED THEN
        INSERT (CountryId, RegionName, RegionCode, IsActive, CreatedAt, UpdatedAt)
        VALUES (Source.CountryId, Source.RegionName, Source.RegionCode, 1, GETUTCDATE(), GETUTCDATE());
END

-- 11. BANGLADESH (8 Divisions)
DECLARE @BangladeshId INT = (SELECT CountryId FROM CountryMasters WHERE CountryName = N'Bangladesh');
IF @BangladeshId IS NOT NULL
BEGIN
    MERGE INTO RegionMasters AS Target
    USING (VALUES
        (@BangladeshId, N'Dhaka',       N'DHK'),
        (@BangladeshId, N'Chattogram',  N'CTG'),
        (@BangladeshId, N'Rajshahi',    N'RAJ'),
        (@BangladeshId, N'Khulna',      N'KHL'),
        (@BangladeshId, N'Barishal',    N'BAR'),
        (@BangladeshId, N'Sylhet',      N'SYL'),
        (@BangladeshId, N'Rangpur',     N'RAN'),
        (@BangladeshId, N'Mymensingh',  N'MYM')
    ) AS Source (CountryId, RegionName, RegionCode)
    ON Target.CountryId = Source.CountryId AND Target.RegionName = Source.RegionName
    WHEN NOT MATCHED THEN
        INSERT (CountryId, RegionName, RegionCode, IsActive, CreatedAt, UpdatedAt)
        VALUES (Source.CountryId, Source.RegionName, Source.RegionCode, 1, GETUTCDATE(), GETUTCDATE());
END

-- 12. PAKISTAN (5 Provinces & Territories)
DECLARE @PakistanId INT = (SELECT CountryId FROM CountryMasters WHERE CountryName = N'Pakistan');
IF @PakistanId IS NOT NULL
BEGIN
    MERGE INTO RegionMasters AS Target
    USING (VALUES
        (@PakistanId, N'Punjab',                      N'PB'),
        (@PakistanId, N'Sindh',                       N'SD'),
        (@PakistanId, N'Khyber Pakhtunkhwa',          N'KP'),
        (@PakistanId, N'Balochistan',                 N'BA'),
        (@PakistanId, N'Islamabad Capital Territory', N'ICT')
    ) AS Source (CountryId, RegionName, RegionCode)
    ON Target.CountryId = Source.CountryId AND Target.RegionName = Source.RegionName
    WHEN NOT MATCHED THEN
        INSERT (CountryId, RegionName, RegionCode, IsActive, CreatedAt, UpdatedAt)
        VALUES (Source.CountryId, Source.RegionName, Source.RegionCode, 1, GETUTCDATE(), GETUTCDATE());
END
GO

PRINT 'Comprehensive Regions Seed completed successfully with full Unicode support.';
GO
