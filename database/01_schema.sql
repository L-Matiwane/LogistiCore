/* LogistiCore - SQL Server schema (v0.1) */
IF DB_ID('LogistiCore') IS NULL CREATE DATABASE LogistiCore;
GO
USE LogistiCore;
GO

CREATE TABLE Roles (
  RoleId INT IDENTITY PRIMARY KEY,
  Name NVARCHAR(50) NOT NULL CONSTRAINT UQ_Roles_Name UNIQUE
);

CREATE TABLE Users (
  UserId INT IDENTITY PRIMARY KEY,
  Email NVARCHAR(256) NOT NULL CONSTRAINT UQ_Users_Email UNIQUE,
  FullName NVARCHAR(150) NOT NULL,
  PasswordHash NVARCHAR(500) NOT NULL,
  RoleId INT NOT NULL CONSTRAINT FK_Users_Roles REFERENCES Roles(RoleId),
  IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT 1,
  CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME()
);

CREATE TABLE Customers (
  CustomerId INT IDENTITY PRIMARY KEY,
  AccountNo NVARCHAR(20) NOT NULL CONSTRAINT UQ_Customers_AccountNo UNIQUE,
  Name NVARCHAR(150) NOT NULL,
  Email NVARCHAR(256) NULL,
  Phone NVARCHAR(30) NULL,
  BillingAddress NVARCHAR(300) NULL,
  UserId INT NULL CONSTRAINT FK_Customers_Users REFERENCES Users(UserId),
  IsActive BIT NOT NULL CONSTRAINT DF_Customers_IsActive DEFAULT 1
);

CREATE TABLE RateCards (
  RateCardId INT IDENTITY PRIMARY KEY,
  ServiceLevel NVARCHAR(20) NOT NULL,
  MinWeightKg DECIMAL(8,2) NOT NULL,
  MaxWeightKg DECIMAL(8,2) NOT NULL,
  Price DECIMAL(10,2) NOT NULL,
  CONSTRAINT CK_RateCards_Service CHECK (ServiceLevel IN ('Standard','Express','Overnight')),
  CONSTRAINT CK_RateCards_Weight CHECK (MinWeightKg >= 0 AND MaxWeightKg > MinWeightKg),
  CONSTRAINT CK_RateCards_Price CHECK (Price >= 0)
);

CREATE TABLE WarehouseBins (
  BinId INT IDENTITY PRIMARY KEY,
  Zone NVARCHAR(30) NOT NULL,
  Code NVARCHAR(20) NOT NULL CONSTRAINT UQ_WarehouseBins_Code UNIQUE,
  Capacity INT NOT NULL CONSTRAINT CK_WarehouseBins_Cap CHECK (Capacity > 0)
);

CREATE TABLE Parcels (
  ParcelId INT IDENTITY PRIMARY KEY,
  TrackingNo NVARCHAR(30) NOT NULL CONSTRAINT UQ_Parcels_TrackingNo UNIQUE,
  CustomerId INT NOT NULL CONSTRAINT FK_Parcels_Customers REFERENCES Customers(CustomerId),
  SenderName NVARCHAR(150) NULL,
  RecipientName NVARCHAR(150) NOT NULL,
  RecipientPhone NVARCHAR(30) NOT NULL,
  RecipientEmail NVARCHAR(256) NULL,
  DeliveryAddress NVARCHAR(300) NOT NULL,
  Zone NVARCHAR(30) NULL,
  WeightKg DECIMAL(8,2) NOT NULL CONSTRAINT CK_Parcels_Weight CHECK (WeightKg > 0),
  LengthCm DECIMAL(8,2) NULL,
  WidthCm DECIMAL(8,2) NULL,
  HeightCm DECIMAL(8,2) NULL,
  ServiceLevel NVARCHAR(20) NOT NULL CONSTRAINT CK_Parcels_Service CHECK (ServiceLevel IN ('Standard','Express','Overnight')),
  Price DECIMAL(10,2) NOT NULL CONSTRAINT CK_Parcels_Price CHECK (Price >= 0),
  CurrentStatus NVARCHAR(20) NOT NULL CONSTRAINT DF_Parcels_Status DEFAULT 'Registered',
  DeliveryAttempts INT NOT NULL CONSTRAINT DF_Parcels_Attempts DEFAULT 0,
  BinId INT NULL CONSTRAINT FK_Parcels_Bins REFERENCES WarehouseBins(BinId),
  IsInvoiced BIT NOT NULL CONSTRAINT DF_Parcels_IsInvoiced DEFAULT 0,
  RegisteredByUserId INT NOT NULL CONSTRAINT FK_Parcels_Users REFERENCES Users(UserId),
  RegisteredAt DATETIME2 NOT NULL CONSTRAINT DF_Parcels_RegisteredAt DEFAULT SYSUTCDATETIME(),
  CONSTRAINT CK_Parcels_Status CHECK (CurrentStatus IN
    ('Registered','AtWarehouse','Sorted','Assigned','OutForDelivery','Delivered','FailedAttempt','Returned','Lost','Damaged'))
);
CREATE INDEX IX_Parcels_Status ON Parcels(CurrentStatus);
CREATE INDEX IX_Parcels_Customer ON Parcels(CustomerId, RegisteredAt);

CREATE TABLE ParcelStatusHistory (
  HistoryId BIGINT IDENTITY PRIMARY KEY,
  ParcelId INT NOT NULL CONSTRAINT FK_History_Parcels REFERENCES Parcels(ParcelId),
  Status NVARCHAR(20) NOT NULL,
  ChangedByUserId INT NULL CONSTRAINT FK_History_Users REFERENCES Users(UserId),
  ChangedAt DATETIME2 NOT NULL CONSTRAINT DF_History_ChangedAt DEFAULT SYSUTCDATETIME(),
  Note NVARCHAR(300) NULL
);
CREATE INDEX IX_History_Parcel ON ParcelStatusHistory(ParcelId, ChangedAt);

CREATE TABLE ParcelExceptions (
  ExceptionId INT IDENTITY PRIMARY KEY,
  ParcelId INT NOT NULL CONSTRAINT FK_Exceptions_Parcels REFERENCES Parcels(ParcelId),
  Type NVARCHAR(10) NOT NULL CONSTRAINT CK_Exceptions_Type CHECK (Type IN ('Lost','Damaged')),
  Reason NVARCHAR(500) NOT NULL,
  ReportedByUserId INT NOT NULL CONSTRAINT FK_Exceptions_Users REFERENCES Users(UserId),
  ReportedAt DATETIME2 NOT NULL CONSTRAINT DF_Exceptions_ReportedAt DEFAULT SYSUTCDATETIME(),
  Resolved BIT NOT NULL CONSTRAINT DF_Exceptions_Resolved DEFAULT 0
);

CREATE TABLE Drivers (
  DriverId INT IDENTITY PRIMARY KEY,
  UserId INT NOT NULL CONSTRAINT UQ_Drivers_User UNIQUE CONSTRAINT FK_Drivers_Users REFERENCES Users(UserId),
  LicenceNo NVARCHAR(30) NOT NULL,
  Phone NVARCHAR(30) NULL,
  IsAvailable BIT NOT NULL CONSTRAINT DF_Drivers_Avail DEFAULT 1
);

CREATE TABLE Vehicles (
  VehicleId INT IDENTITY PRIMARY KEY,
  Registration NVARCHAR(20) NOT NULL CONSTRAINT UQ_Vehicles_Reg UNIQUE,
  Description NVARCHAR(100) NULL,
  CapacityKg DECIMAL(8,2) NOT NULL CONSTRAINT CK_Vehicles_Cap CHECK (CapacityKg > 0),
  IsActive BIT NOT NULL CONSTRAINT DF_Vehicles_Active DEFAULT 1
);

CREATE TABLE Routes (
  RouteId INT IDENTITY PRIMARY KEY,
  Name NVARCHAR(100) NOT NULL,
  RouteDate DATE NOT NULL,
  DriverId INT NOT NULL CONSTRAINT FK_Routes_Drivers REFERENCES Drivers(DriverId),
  VehicleId INT NOT NULL CONSTRAINT FK_Routes_Vehicles REFERENCES Vehicles(VehicleId),
  Status NVARCHAR(15) NOT NULL CONSTRAINT DF_Routes_Status DEFAULT 'Planned',
  CONSTRAINT CK_Routes_Status CHECK (Status IN ('Planned','Released','InProgress','Completed','Cancelled'))
);
-- One active route per driver and per vehicle per day (business rule 4)
CREATE UNIQUE INDEX UX_Routes_Driver_Day ON Routes(DriverId, RouteDate) WHERE Status <> 'Cancelled';
CREATE UNIQUE INDEX UX_Routes_Vehicle_Day ON Routes(VehicleId, RouteDate) WHERE Status <> 'Cancelled';

CREATE TABLE RouteParcels (
  RouteParcelId INT IDENTITY PRIMARY KEY,
  RouteId INT NOT NULL CONSTRAINT FK_RP_Routes REFERENCES Routes(RouteId),
  ParcelId INT NOT NULL CONSTRAINT FK_RP_Parcels REFERENCES Parcels(ParcelId),
  StopSequence INT NOT NULL,
  IsActive BIT NOT NULL CONSTRAINT DF_RP_Active DEFAULT 1,
  CONSTRAINT UQ_RP_Route_Stop UNIQUE (RouteId, StopSequence)
);
-- A parcel can be on only one active route (business rule 3)
CREATE UNIQUE INDEX UX_RP_ActiveParcel ON RouteParcels(ParcelId) WHERE IsActive = 1;

CREATE TABLE Deliveries (
  DeliveryId INT IDENTITY PRIMARY KEY,
  RouteParcelId INT NOT NULL CONSTRAINT FK_Deliveries_RP REFERENCES RouteParcels(RouteParcelId),
  Outcome NVARCHAR(10) NOT NULL CONSTRAINT CK_Deliveries_Outcome CHECK (Outcome IN ('Delivered','Failed')),
  RecipientName NVARCHAR(150) NULL,
  FailureReason NVARCHAR(300) NULL,
  Notes NVARCHAR(500) NULL,
  AttemptedAt DATETIME2 NOT NULL CONSTRAINT DF_Deliveries_At DEFAULT SYSUTCDATETIME(),
  CONSTRAINT CK_Deliveries_Details CHECK (
    (Outcome = 'Delivered' AND RecipientName IS NOT NULL) OR
    (Outcome = 'Failed' AND FailureReason IS NOT NULL))
);

CREATE TABLE Notifications (
  NotificationId BIGINT IDENTITY PRIMARY KEY,
  ParcelId INT NOT NULL CONSTRAINT FK_Notif_Parcels REFERENCES Parcels(ParcelId),
  Channel NVARCHAR(10) NOT NULL CONSTRAINT CK_Notif_Channel CHECK (Channel IN ('Email','SMS')),
  Message NVARCHAR(500) NOT NULL,
  Status NVARCHAR(10) NOT NULL CONSTRAINT DF_Notif_Status DEFAULT 'Queued',
  CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Notif_Created DEFAULT SYSUTCDATETIME(),
  SentAt DATETIME2 NULL,
  CONSTRAINT CK_Notif_Status CHECK (Status IN ('Queued','Sent','Failed'))
);

CREATE TABLE Invoices (
  InvoiceId INT IDENTITY PRIMARY KEY,
  InvoiceNo NVARCHAR(20) NOT NULL CONSTRAINT UQ_Invoices_No UNIQUE,
  CustomerId INT NOT NULL CONSTRAINT FK_Invoices_Customers REFERENCES Customers(CustomerId),
  IssueDate DATE NOT NULL,
  DueDate DATE NOT NULL,
  Subtotal DECIMAL(12,2) NOT NULL,
  Vat DECIMAL(12,2) NOT NULL,
  Total DECIMAL(12,2) NOT NULL,
  Status NVARCHAR(10) NOT NULL CONSTRAINT DF_Invoices_Status DEFAULT 'Issued',
  CONSTRAINT CK_Invoices_Status CHECK (Status IN ('Issued','Paid','Overdue','Void'))
);

CREATE TABLE InvoiceLines (
  LineId INT IDENTITY PRIMARY KEY,
  InvoiceId INT NOT NULL CONSTRAINT FK_Lines_Invoices REFERENCES Invoices(InvoiceId),
  ParcelId INT NOT NULL CONSTRAINT UQ_Lines_Parcel UNIQUE CONSTRAINT FK_Lines_Parcels REFERENCES Parcels(ParcelId),
  Amount DECIMAL(10,2) NOT NULL
);

CREATE TABLE Payments (
  PaymentId INT IDENTITY PRIMARY KEY,
  CustomerId INT NOT NULL CONSTRAINT FK_Payments_Customers REFERENCES Customers(CustomerId),
  InvoiceId INT NULL CONSTRAINT FK_Payments_Invoices REFERENCES Invoices(InvoiceId),
  Amount DECIMAL(12,2) NOT NULL CONSTRAINT CK_Payments_Amount CHECK (Amount > 0),
  Method NVARCHAR(20) NOT NULL,
  PaidAt DATETIME2 NOT NULL CONSTRAINT DF_Payments_PaidAt DEFAULT SYSUTCDATETIME()
);
GO
