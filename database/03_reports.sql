USE LogistiCoreDb;
GO
/* R1: Daily parcel movement */
CREATE OR ALTER PROCEDURE dbo.sp_DailyParcelMovement @Date DATE AS
BEGIN
  SET NOCOUNT ON;
  SELECT h.Status, COUNT(DISTINCT h.ParcelId) AS ParcelCount
  FROM ParcelStatusHistory h
  WHERE CAST(h.ChangedAt AS DATE) = @Date
  GROUP BY h.Status
  ORDER BY h.Status;
END
GO
/* R2: Driver performance */
CREATE OR ALTER PROCEDURE dbo.sp_DriverPerformance @From DATE, @To DATE AS
BEGIN
  SET NOCOUNT ON;
  SELECT d.DriverId, u.FullName AS Driver,
         SUM(CASE WHEN dl.Outcome = 'Delivered' THEN 1 ELSE 0 END) AS Delivered,
         SUM(CASE WHEN dl.Outcome = 'Failed' THEN 1 ELSE 0 END) AS FailedAttempts,
         CAST(100.0 * SUM(CASE WHEN dl.Outcome = 'Delivered' THEN 1 ELSE 0 END) / NULLIF(COUNT(*),0) AS DECIMAL(5,1)) AS SuccessRatePct
  FROM Deliveries dl
  JOIN RouteParcels rp ON rp.RouteParcelId = dl.RouteParcelId
  JOIN Routes r ON r.RouteId = rp.RouteId
  JOIN Drivers d ON d.DriverId = r.DriverId
  JOIN Users u ON u.UserId = d.UserId
  WHERE CAST(dl.AttemptedAt AS DATE) BETWEEN @From AND @To
  GROUP BY d.DriverId, u.FullName
  ORDER BY Delivered DESC;
END
GO
/* R3: Monthly revenue (invoiced) */
CREATE OR ALTER PROCEDURE dbo.sp_MonthlyRevenue @Year INT, @Month INT AS
BEGIN
  SET NOCOUNT ON;
  SELECT c.CustomerId, c.Name AS Customer, COUNT(DISTINCT i.InvoiceId) AS Invoices,
         SUM(i.Subtotal) AS Subtotal, SUM(i.Vat) AS Vat, SUM(i.Total) AS Total
  FROM Invoices i JOIN Customers c ON c.CustomerId = i.CustomerId
  WHERE YEAR(i.IssueDate) = @Year AND MONTH(i.IssueDate) = @Month AND i.Status <> 'Void'
  GROUP BY c.CustomerId, c.Name
  ORDER BY Total DESC;
END
GO
/* R4: Lost/damaged exceptions */
CREATE OR ALTER PROCEDURE dbo.sp_ExceptionsList @From DATE, @To DATE AS
BEGIN
  SET NOCOUNT ON;
  SELECT e.ExceptionId, p.TrackingNo, c.Name AS Customer, e.Type, e.Reason, e.ReportedAt, e.Resolved
  FROM ParcelExceptions e
  JOIN Parcels p ON p.ParcelId = e.ParcelId
  JOIN Customers c ON c.CustomerId = p.CustomerId
  WHERE CAST(e.ReportedAt AS DATE) BETWEEN @From AND @To
  ORDER BY e.ReportedAt DESC;
END
GO
