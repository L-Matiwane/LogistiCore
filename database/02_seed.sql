/* Seed data for development and demo. Passwords are placeholders: the API seeder must replace PasswordHash with real hashes. */
USE LogistiCore;
GO
INSERT INTO Roles(Name) VALUES ('Admin'),('Dispatcher'),('WarehouseClerk'),('Driver'),('Billing'),('Customer');

INSERT INTO Users(Email, FullName, PasswordHash, RoleId) VALUES
('admin@logisticore.test','Nomsa Admin','PLACEHOLDER',1),
('dispatcher@logisticore.test','Sipho Dispatcher','PLACEHOLDER',2),
('clerk@logisticore.test','Thandi Clerk','PLACEHOLDER',3),
('driver1@logisticore.test','Lwazi Driver','PLACEHOLDER',4),
('driver2@logisticore.test','Thabo Driver','PLACEHOLDER',4),
('billing@logisticore.test','Ayanda Billing','PLACEHOLDER',5);

INSERT INTO Customers(AccountNo, Name, Email, Phone, BillingAddress) VALUES
('C0001','Zola''s Corner Shop','zola@example.com','0821110001','5 Main Rd, East London'),
('C0002','Buhle Boutique','buhle@example.com','0821110002','22 Oxford St, East London'),
('C0003','EL Hardware','info@elhardware.example','0821110003','9 Industrial Rd, East London');

INSERT INTO RateCards(ServiceLevel, MinWeightKg, MaxWeightKg, Price) VALUES
('Standard',0,2,60),('Standard',2,10,95),('Standard',10,30,160),
('Express',0,2,95),('Express',2,10,145),('Express',10,30,240),
('Overnight',0,2,130),('Overnight',2,10,195),('Overnight',10,30,320);

INSERT INTO WarehouseBins(Zone, Code, Capacity) VALUES
('Zone A - East London Central','A-01',50),('Zone A - East London Central','A-02',50),
('Zone B - Beacon Bay','B-01',40),('Zone C - Gonubie','C-01',40);

INSERT INTO Drivers(UserId, LicenceNo, Phone) VALUES (4,'LIC-1001','0831110001'),(5,'LIC-1002','0831110002');
INSERT INTO Vehicles(Registration, Description, CapacityKg) VALUES ('EL 123 EC','Panel van',500),('EL 456 EC','Bakkie',300);
GO
