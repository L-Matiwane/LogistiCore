/* LogistiCore migration 04: extra Parcels columns for the intake screen.
   Safe to run more than once. Run it in SSMS after 01, 02 and 03. */
USE LogistiCoreDb;
GO

IF COL_LENGTH('dbo.Parcels', 'City') IS NULL
    ALTER TABLE dbo.Parcels ADD City NVARCHAR(100) NOT NULL CONSTRAINT DF_Parcels_City DEFAULT '';
GO
IF COL_LENGTH('dbo.Parcels', 'PostalCode') IS NULL
    ALTER TABLE dbo.Parcels ADD PostalCode NVARCHAR(10) NOT NULL CONSTRAINT DF_Parcels_PostalCode DEFAULT '';
GO
IF COL_LENGTH('dbo.Parcels', 'DeliveryInstructions') IS NULL
    ALTER TABLE dbo.Parcels ADD DeliveryInstructions NVARCHAR(300) NULL;
GO
IF COL_LENGTH('dbo.Parcels', 'SpecialHandling') IS NULL
    ALTER TABLE dbo.Parcels ADD SpecialHandling NVARCHAR(200) NULL;
GO
IF COL_LENGTH('dbo.Parcels', 'BilledWeightKg') IS NULL
    ALTER TABLE dbo.Parcels ADD BilledWeightKg DECIMAL(8,2) NOT NULL CONSTRAINT DF_Parcels_BilledWeight DEFAULT 0;
GO

SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Parcels'
ORDER BY ORDINAL_POSITION;
GO
