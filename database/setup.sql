-- ==========================================
-- LABORATORY MANAGEMENT SYSTEM SETUP SCRIPT
-- Database: LaboratoryDb
-- Target: Microsoft SQL Server
-- ==========================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'LaboratoryDb')
BEGIN
    CREATE DATABASE LaboratoryDb;
END
GO

USE LaboratoryDb;
GO

-- ------------------------------------------
-- 1. DROP EXISTING CONSTRAINTS & TABLES
-- ------------------------------------------
IF OBJECT_ID('dbo.AuditLogs', 'U') IS NOT NULL DROP TABLE dbo.AuditLogs;
IF OBJECT_ID('dbo.Feedback', 'U') IS NOT NULL DROP TABLE dbo.Feedback;
IF OBJECT_ID('dbo.Notifications', 'U') IS NOT NULL DROP TABLE dbo.Notifications;
IF OBJECT_ID('dbo.Invoices', 'U') IS NOT NULL DROP TABLE dbo.Invoices;
IF OBJECT_ID('dbo.Payments', 'U') IS NOT NULL DROP TABLE dbo.Payments;
IF OBJECT_ID('dbo.Reports', 'U') IS NOT NULL DROP TABLE dbo.Reports;
IF OBJECT_ID('dbo.AppointmentPackages', 'U') IS NOT NULL DROP TABLE dbo.AppointmentPackages;
IF OBJECT_ID('dbo.AppointmentTests', 'U') IS NOT NULL DROP TABLE dbo.AppointmentTests;
IF OBJECT_ID('dbo.Appointments', 'U') IS NOT NULL DROP TABLE dbo.Appointments;
IF OBJECT_ID('dbo.PackageTests', 'U') IS NOT NULL DROP TABLE dbo.PackageTests;
IF OBJECT_ID('dbo.Packages', 'U') IS NOT NULL DROP TABLE dbo.Packages;
IF OBJECT_ID('dbo.Tests', 'U') IS NOT NULL DROP TABLE dbo.Tests;
IF OBJECT_ID('dbo.TestCategories', 'U') IS NOT NULL DROP TABLE dbo.TestCategories;
IF OBJECT_ID('dbo.Holidays', 'U') IS NOT NULL DROP TABLE dbo.Holidays;
IF OBJECT_ID('dbo.TimeSlots', 'U') IS NOT NULL DROP TABLE dbo.TimeSlots;
IF OBJECT_ID('dbo.Doctors', 'U') IS NOT NULL DROP TABLE dbo.Doctors;
IF OBJECT_ID('dbo.Staff', 'U') IS NOT NULL DROP TABLE dbo.Staff;
IF OBJECT_ID('dbo.Patients', 'U') IS NOT NULL DROP TABLE dbo.Patients;
IF OBJECT_ID('dbo.Branches', 'U') IS NOT NULL DROP TABLE dbo.Branches;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID('dbo.Roles', 'U') IS NOT NULL DROP TABLE dbo.Roles;
GO

-- ------------------------------------------
-- 2. CREATE TABLES
-- ------------------------------------------

CREATE TABLE dbo.Roles (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE dbo.Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Salt NVARCHAR(100) NOT NULL,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    PhoneNumber NVARCHAR(20) NULL,
    RoleId INT NOT NULL FOREIGN KEY REFERENCES dbo.Roles(RoleId),
    IsEmailVerified BIT NOT NULL DEFAULT 0,
    VerificationToken NVARCHAR(100) NULL,
    ResetToken NVARCHAR(100) NULL,
    ResetTokenExpiry DATETIME NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE dbo.Branches (
    BranchId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Address NVARCHAR(255) NOT NULL,
    City NVARCHAR(50) NOT NULL,
    ContactNumber NVARCHAR(20) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE dbo.Patients (
    PatientId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL UNIQUE FOREIGN KEY REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
    DateOfBirth DATE NOT NULL,
    Gender NVARCHAR(20) NOT NULL,
    BloodGroup NVARCHAR(5) NULL,
    Address NVARCHAR(255) NULL
);

CREATE TABLE dbo.Doctors (
    DoctorId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL UNIQUE FOREIGN KEY REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
    Specialization NVARCHAR(100) NOT NULL,
    Designation NVARCHAR(100) NOT NULL,
    CommissionRate DECIMAL(5,2) NOT NULL DEFAULT 0.00
);

CREATE TABLE dbo.Staff (
    StaffId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL UNIQUE FOREIGN KEY REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
    BranchId INT NOT NULL FOREIGN KEY REFERENCES dbo.Branches(BranchId),
    Designation NVARCHAR(100) NOT NULL
);

CREATE TABLE dbo.TimeSlots (
    SlotId INT IDENTITY(1,1) PRIMARY KEY,
    BranchId INT NOT NULL FOREIGN KEY REFERENCES dbo.Branches(BranchId) ON DELETE CASCADE,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    MaxBookings INT NOT NULL DEFAULT 5,
    IsActive BIT NOT NULL DEFAULT 1
);

CREATE TABLE dbo.Holidays (
    HolidayId INT IDENTITY(1,1) PRIMARY KEY,
    BranchId INT NOT NULL FOREIGN KEY REFERENCES dbo.Branches(BranchId) ON DELETE CASCADE,
    HolidayDate DATE NOT NULL,
    Description NVARCHAR(255) NULL,
    CONSTRAINT UQ_Branch_Holiday UNIQUE (BranchId, HolidayDate)
);

CREATE TABLE dbo.TestCategories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255) NULL
);

CREATE TABLE dbo.Tests (
    TestId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryId INT NOT NULL FOREIGN KEY REFERENCES dbo.TestCategories(CategoryId),
    Name NVARCHAR(150) NOT NULL,
    Code NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(500) NULL,
    Price DECIMAL(10,2) NOT NULL,
    SampleType NVARCHAR(100) NOT NULL,
    Preparation NVARCHAR(500) NULL,
    NormalRange NVARCHAR(255) NULL,
    DeliveryTimeHours INT NOT NULL DEFAULT 24,
    IsActive BIT NOT NULL DEFAULT 1
);

CREATE TABLE dbo.Packages (
    PackageId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(150) NOT NULL,
    Code NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(500) NULL,
    Price DECIMAL(10,2) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

CREATE TABLE dbo.PackageTests (
    PackageId INT NOT NULL FOREIGN KEY REFERENCES dbo.Packages(PackageId) ON DELETE CASCADE,
    TestId INT NOT NULL FOREIGN KEY REFERENCES dbo.Tests(TestId) ON DELETE CASCADE,
    PRIMARY KEY (PackageId, TestId)
);

CREATE TABLE dbo.Appointments (
    AppointmentId INT IDENTITY(1,1) PRIMARY KEY,
    PatientId INT NOT NULL FOREIGN KEY REFERENCES dbo.Patients(PatientId),
    BranchId INT NOT NULL FOREIGN KEY REFERENCES dbo.Branches(BranchId),
    AppointmentDate DATE NOT NULL,
    SlotId INT NOT NULL FOREIGN KEY REFERENCES dbo.TimeSlots(SlotId),
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Confirmed, SampleCollected, ResultUploaded, Completed, Cancelled
    TotalAmount DECIMAL(10,2) NOT NULL,
    DiscountAmount DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    PaidAmount DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    PaymentStatus NVARCHAR(50) NOT NULL DEFAULT 'Unpaid', -- Unpaid, Paid, Refunded
    Notes NVARCHAR(500) NULL,
    DoctorId INT NULL FOREIGN KEY REFERENCES dbo.Doctors(DoctorId),
    StaffId INT NULL FOREIGN KEY REFERENCES dbo.Staff(StaffId),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE dbo.AppointmentTests (
    AppointmentId INT NOT NULL FOREIGN KEY REFERENCES dbo.Appointments(AppointmentId) ON DELETE CASCADE,
    TestId INT NOT NULL FOREIGN KEY REFERENCES dbo.Tests(TestId),
    Price DECIMAL(10,2) NOT NULL,
    PRIMARY KEY (AppointmentId, TestId)
);

CREATE TABLE dbo.AppointmentPackages (
    AppointmentId INT NOT NULL FOREIGN KEY REFERENCES dbo.Appointments(AppointmentId) ON DELETE CASCADE,
    PackageId INT NOT NULL FOREIGN KEY REFERENCES dbo.Packages(PackageId),
    Price DECIMAL(10,2) NOT NULL,
    PRIMARY KEY (AppointmentId, PackageId)
);

CREATE TABLE dbo.Reports (
    ReportId INT IDENTITY(1,1) PRIMARY KEY,
    AppointmentId INT NOT NULL FOREIGN KEY REFERENCES dbo.Appointments(AppointmentId) ON DELETE CASCADE,
    TestId INT NOT NULL FOREIGN KEY REFERENCES dbo.Tests(TestId),
    ResultValue NVARCHAR(255) NULL,
    Remarks NVARCHAR(500) NULL,
    UploadedByStaffId INT NULL FOREIGN KEY REFERENCES dbo.Staff(StaffId),
    UploadedAt DATETIME NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending' -- Pending, Completed
);

CREATE TABLE dbo.Payments (
    PaymentId INT IDENTITY(1,1) PRIMARY KEY,
    AppointmentId INT NOT NULL FOREIGN KEY REFERENCES dbo.Appointments(AppointmentId) ON DELETE CASCADE,
    TransactionId NVARCHAR(100) NULL,
    Amount DECIMAL(10,2) NOT NULL,
    PaymentMethod NVARCHAR(50) NOT NULL, -- Razorpay, Stripe, Cash, Online
    PaymentStatus NVARCHAR(50) NOT NULL, -- Success, Failed, Refunded
    PaymentDate DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE dbo.Invoices (
    InvoiceId INT IDENTITY(1,1) PRIMARY KEY,
    AppointmentId INT NOT NULL UNIQUE FOREIGN KEY REFERENCES dbo.Appointments(AppointmentId) ON DELETE CASCADE,
    InvoiceNumber NVARCHAR(50) NOT NULL UNIQUE,
    TotalAmount DECIMAL(10,2) NOT NULL,
    DiscountAmount DECIMAL(10,2) NOT NULL,
    TaxAmount DECIMAL(10,2) NOT NULL,
    FinalAmount DECIMAL(10,2) NOT NULL,
    GeneratedAt DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE dbo.Notifications (
    NotificationId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
    Message NVARCHAR(500) NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE dbo.Feedback (
    FeedbackId INT IDENTITY(1,1) PRIMARY KEY,
    PatientId INT NOT NULL FOREIGN KEY REFERENCES dbo.Patients(PatientId) ON DELETE CASCADE,
    Rating INT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
    Comments NVARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE dbo.AuditLogs (
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NULL FOREIGN KEY REFERENCES dbo.Users(UserId) ON DELETE SET NULL,
    Action NVARCHAR(50) NOT NULL,
    TableName NVARCHAR(50) NOT NULL,
    RecordId INT NULL,
    OldValues NVARCHAR(MAX) NULL,
    NewValues NVARCHAR(MAX) NULL,
    Timestamp DATETIME NOT NULL DEFAULT GETDATE(),
    IpAddress NVARCHAR(50) NULL
);
GO

-- ------------------------------------------
-- 3. SEED INITIAL DATA
-- ------------------------------------------
INSERT INTO dbo.Roles (Name) VALUES ('Admin'), ('Staff'), ('Patient'), ('Doctor');

-- Password is 'Admin@123' for seeded Admin & 'Staff@123' for Staff, etc.
-- Salt is 'SEED_SALT_VALUE'
-- Hash of "Admin@123SEED_SALT": 240ea27389270e53a2db7956a297e68bc653d3cb6bc0816cf628be1b702ec4cd (SHA256)
INSERT INTO dbo.Users (Email, PasswordHash, Salt, FirstName, LastName, PhoneNumber, RoleId, IsEmailVerified)
VALUES ('admin@lab.com', '240ea27389270e53a2db7956a297e68bc653d3cb6bc0816cf628be1b702ec4cd', 'SEED_SALT', 'System', 'Admin', '1234567890', 1, 1);

-- Seed Branches
INSERT INTO dbo.Branches (Name, Address, City, ContactNumber, Email, IsActive)
VALUES 
('Main Branch', '101 Healthcare Avenue', 'New York', '555-0199', 'main@lab.com', 1),
('Suburban Clinic', '404 Wellness Street', 'New Jersey', '555-0299', 'suburban@lab.com', 1);

-- Seed Staff User (Password: Staff@123, email: staff@lab.com, Salt: SEED_SALT)
-- Hash: eca32d6dbdfc3c0bc695121b6d5b0a3c22de5d117a0225d2c672b1464df19183
INSERT INTO dbo.Users (Email, PasswordHash, Salt, FirstName, LastName, PhoneNumber, RoleId, IsEmailVerified)
VALUES ('staff@lab.com', 'eca32d6dbdfc3c0bc695121b6d5b0a3c22de5d117a0225d2c672b1464df19183', 'SEED_SALT', 'John', 'Staff', '1234567891', 2, 1);

INSERT INTO dbo.Staff (UserId, BranchId, Designation)
VALUES (2, 1, 'Senior Lab Technician');

-- Seed Doctor User (Password: Doctor@123, email: doctor@lab.com, Salt: SEED_SALT)
-- Hash: f36ee9fcf9dfc3b5d2ff1fcd84c6c0e5a8f465c4ef6c54784a92c4cd7aef484d
INSERT INTO dbo.Users (Email, PasswordHash, Salt, FirstName, LastName, PhoneNumber, RoleId, IsEmailVerified)
VALUES ('doctor@lab.com', 'f36ee9fcf9dfc3b5d2ff1fcd84c6c0e5a8f465c4ef6c54784a92c4cd7aef484d', 'SEED_SALT', 'Sarah', 'Connor', '1234567892', 4, 1);

INSERT INTO dbo.Doctors (UserId, Specialization, Designation, CommissionRate)
VALUES (3, 'Pathology', 'Consultant Pathologist', 15.00);

-- Seed Patient User (Password: Patient@123, email: patient@lab.com, Salt: SEED_SALT)
-- Hash: 3be568ccf5d4a132924cd8c2901dbd3184b256bc8b3d6790b8fde8a01bf26e25
INSERT INTO dbo.Users (Email, PasswordHash, Salt, FirstName, LastName, PhoneNumber, RoleId, IsEmailVerified)
VALUES ('patient@lab.com', '3be568ccf5d4a132924cd8c2901dbd3184b256bc8b3d6790b8fde8a01bf26e25', 'SEED_SALT', 'Alice', 'Smith', '1234567893', 3, 1);

INSERT INTO dbo.Patients (UserId, DateOfBirth, Gender, BloodGroup, Address)
VALUES (4, '1990-05-15', 'Female', 'A+', '12 Park Lane, NY');

-- Seed Time Slots
INSERT INTO dbo.TimeSlots (BranchId, StartTime, EndTime, MaxBookings, IsActive)
VALUES 
(1, '09:00:00', '10:00:00', 5, 1),
(1, '10:00:00', '11:00:00', 5, 1),
(1, '11:00:00', '12:00:00', 5, 1),
(1, '14:00:00', '15:00:00', 5, 1),
(1, '15:00:00', '16:00:00', 5, 1),
(2, '09:00:00', '10:00:00', 5, 1),
(2, '10:00:00', '11:00:00', 5, 1);

-- Seed Test Categories
INSERT INTO dbo.TestCategories (Name, Description)
VALUES 
('Hematology', 'Complete blood counts and blood related tests'),
('Biochemistry', 'Chemical analysis of bodily fluids'),
('Immunology', 'Study of the immune system and antibody tests'),
('Microbiology', 'Culture and identification of microorganisms');

-- Seed Tests with Comprehensive Parameters
INSERT INTO dbo.Tests (CategoryId, Name, Code, Description, Price, SampleType, Preparation, NormalRange, DeliveryTimeHours, IsActive)
VALUES 
(1, 'Complete Blood Count (CBC)', 'CBC001', 'Evaluates overall health and detects wide range of disorders including anemia and infection.', 450.00, 'Whole Blood (EDTA)', 'No fasting required', 'Hb: 13.0-17.5 g/dL, WBC: 4.0-11.0 k/uL, RBC: 4.5-5.5 M/uL, Platelets: 1.5-4.5 Lakhs/uL, HCT: 40-50%, MCV: 80-100 fL, MCH: 27-32 pg, MCHC: 32-36 g/dL', 12, 1),
(2, 'Liver Function Test (LFT)', 'LFT002', 'Evaluates liver health, enzymes, proteins, and bilirubin levels.', 750.00, 'Blood (Serum)', 'Fasting required for 8-10 hours', 'Bilirubin: 0.2-1.2 mg/dL, SGOT (AST): 5-40 U/L, SGPT (ALT): 7-56 U/L, ALP: 44-147 U/L, Albumin: 3.4-5.4 g/dL', 24, 1),
(2, 'Kidney Function Test (KFT/RFT)', 'KFT003', 'Assesses renal health and filtration rate using urea and creatinine values.', 650.00, 'Blood (Serum)', 'No fasting required', 'Blood Urea: 15-45 mg/dL, Serum Creatinine: 0.6-1.2 mg/dL, Uric Acid: 3.5-7.2 mg/dL', 12, 1),
(2, 'Lipid Profile', 'LIP004', 'Comprehensive lipid panel measuring total cholesterol, HDL, LDL, and triglycerides.', 800.00, 'Blood (Serum)', 'Fasting required for 10-12 hours', 'Cholesterol: <200 mg/dL, HDL: >40 mg/dL, LDL: <100 mg/dL, Triglycerides: <150 mg/dL', 24, 1),
(3, 'Thyroid Profile (T3, T4, TSH)', 'THY005', 'Evaluates thyroid gland activity and hormonal balance.', 1200.00, 'Blood (Serum)', 'No fasting required', 'T3: 0.8-2.0 ng/mL, T4: 5.1-14.1 ug/dL, TSH: 0.4-4.0 mIU/L', 24, 1),
(2, 'HbA1c (Glycated Hemoglobin)', 'HBA1C', 'Measures average blood sugar levels over the past 2-3 months.', 500.00, 'Whole Blood (EDTA)', 'No fasting required', 'HbA1c: <5.7% (Normal), eAG: 70-126 mg/dL', 12, 1),
(4, 'Urine Routine Examination', 'URI007', 'Chemical and microscopic analysis of urine sample.', 200.00, 'Urine', 'First morning sample preferred', 'Clear, pH: 5.0-8.0, Sugar: Nil, Protein: Nil', 6, 1);

-- Seed Packages
INSERT INTO dbo.Packages (Name, Code, Description, Price, IsActive)
VALUES 
('Executive Health Package', 'PKG001', 'Comprehensive health checkup including CBC, Lipid Profile, and Blood Glucose', 1100.00, 1),
('Thyroid & Sugar Basic', 'PKG002', 'Basic package covering Blood Sugar and Thyroid parameters', 1250.00, 1);

-- Link Packages to Tests
INSERT INTO dbo.PackageTests (PackageId, TestId) VALUES (1, 1), (1, 2), (1, 3);
INSERT INTO dbo.PackageTests (PackageId, TestId) VALUES (2, 3), (2, 4);
GO

-- ------------------------------------------
-- 4. CREATE STORED PROCEDURES
-- ------------------------------------------

CREATE OR ALTER PROCEDURE dbo.sp_CreateUser
    @Email NVARCHAR(100),
    @PasswordHash NVARCHAR(255),
    @Salt NVARCHAR(100),
    @FirstName NVARCHAR(50),
    @LastName NVARCHAR(50),
    @PhoneNumber NVARCHAR(20),
    @RoleId INT,
    @IsEmailVerified BIT = 0,
    @VerificationToken NVARCHAR(100) = NULL
AS
BEGIN
    INSERT INTO dbo.Users (Email, PasswordHash, Salt, FirstName, LastName, PhoneNumber, RoleId, IsEmailVerified, VerificationToken)
    VALUES (@Email, @PasswordHash, @Salt, @FirstName, @LastName, @PhoneNumber, @RoleId, @IsEmailVerified, @VerificationToken);
    SELECT SCOPE_IDENTITY() AS NewUserId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetUserById
    @UserId INT
AS
BEGIN
    SELECT u.*, r.Name AS RoleName 
    FROM dbo.Users u
    INNER JOIN dbo.Roles r ON u.RoleId = r.RoleId
    WHERE u.UserId = @UserId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetUserByEmail
    @Email NVARCHAR(100)
AS
BEGIN
    SELECT u.*, r.Name AS RoleName 
    FROM dbo.Users u
    INNER JOIN dbo.Roles r ON u.RoleId = r.RoleId
    WHERE u.Email = @Email;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateUser
    @UserId INT,
    @FirstName NVARCHAR(50),
    @LastName NVARCHAR(50),
    @PhoneNumber NVARCHAR(20)
AS
BEGIN
    UPDATE dbo.Users
    SET FirstName = @FirstName,
        LastName = @LastName,
        PhoneNumber = @PhoneNumber
    WHERE UserId = @UserId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeleteUser
    @UserId INT
AS
BEGIN
    DELETE FROM dbo.Users WHERE UserId = @UserId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdatePassword
    @UserId INT,
    @PasswordHash NVARCHAR(255),
    @Salt NVARCHAR(100)
AS
BEGIN
    UPDATE dbo.Users
    SET PasswordHash = @PasswordHash,
        Salt = @Salt,
        ResetToken = NULL,
        ResetTokenExpiry = NULL
    WHERE UserId = @UserId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_VerifyEmail
    @VerificationToken NVARCHAR(100)
AS
BEGIN
    UPDATE dbo.Users
    SET IsEmailVerified = 1,
        VerificationToken = NULL
    WHERE VerificationToken = @VerificationToken;
    SELECT @@ROWCOUNT AS RowsUpdated;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_SetResetToken
    @Email NVARCHAR(100),
    @ResetToken NVARCHAR(100),
    @Expiry DATETIME
AS
BEGIN
    UPDATE dbo.Users
    SET ResetToken = @ResetToken,
        ResetTokenExpiry = @Expiry
    WHERE Email = @Email;
    SELECT @@ROWCOUNT AS RowsUpdated;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetResetToken
    @ResetToken NVARCHAR(100)
AS
BEGIN
    SELECT * FROM dbo.Users 
    WHERE ResetToken = @ResetToken AND ResetTokenExpiry > GETDATE();
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetUsersWithRoles
AS
BEGIN
    SELECT u.UserId, u.Email, u.FirstName, u.LastName, u.PhoneNumber, u.RoleId, r.Name AS RoleName, u.IsEmailVerified, u.CreatedAt
    FROM dbo.Users u
    INNER JOIN dbo.Roles r ON u.RoleId = r.RoleId
    ORDER BY u.CreatedAt DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreatePatient
    @UserId INT,
    @DateOfBirth DATE,
    @Gender NVARCHAR(20),
    @BloodGroup NVARCHAR(5),
    @Address NVARCHAR(255)
AS
BEGIN
    INSERT INTO dbo.Patients (UserId, DateOfBirth, Gender, BloodGroup, Address)
    VALUES (@UserId, @DateOfBirth, @Gender, @BloodGroup, @Address);
    SELECT SCOPE_IDENTITY() AS NewPatientId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetPatientById
    @PatientId INT
AS
BEGIN
    SELECT p.*, u.Email, u.FirstName, u.LastName, u.PhoneNumber
    FROM dbo.Patients p
    INNER JOIN dbo.Users u ON p.UserId = u.UserId
    WHERE p.PatientId = @PatientId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetPatientByUserId
    @UserId INT
AS
BEGIN
    SELECT p.*, u.Email, u.FirstName, u.LastName, u.PhoneNumber
    FROM dbo.Patients p
    INNER JOIN dbo.Users u ON p.UserId = u.UserId
    WHERE p.UserId = @UserId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdatePatient
    @PatientId INT,
    @DateOfBirth DATE,
    @Gender NVARCHAR(20),
    @BloodGroup NVARCHAR(5),
    @Address NVARCHAR(255),
    @FirstName NVARCHAR(50),
    @LastName NVARCHAR(50),
    @PhoneNumber NVARCHAR(20)
AS
BEGIN
    DECLARE @UserId INT;
    SELECT @UserId = UserId FROM dbo.Patients WHERE PatientId = @PatientId;
    
    UPDATE dbo.Patients
    SET DateOfBirth = @DateOfBirth,
        Gender = @Gender,
        BloodGroup = @BloodGroup,
        Address = @Address
    WHERE PatientId = @PatientId;

    UPDATE dbo.Users
    SET FirstName = @FirstName,
        LastName = @LastName,
        PhoneNumber = @PhoneNumber
    WHERE UserId = @UserId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeletePatient
    @PatientId INT
AS
BEGIN
    DECLARE @UserId INT;
    SELECT @UserId = UserId FROM dbo.Patients WHERE PatientId = @PatientId;
    DELETE FROM dbo.Patients WHERE PatientId = @PatientId;
    DELETE FROM dbo.Users WHERE UserId = @UserId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetPatientsList
AS
BEGIN
    SELECT p.PatientId, p.DateOfBirth, p.Gender, p.BloodGroup, p.Address, u.UserId, u.Email, u.FirstName, u.LastName, u.PhoneNumber
    FROM dbo.Patients p
    INNER JOIN dbo.Users u ON p.UserId = u.UserId
    ORDER BY u.LastName, u.FirstName;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateDoctor
    @UserId INT,
    @Specialization NVARCHAR(100),
    @Designation NVARCHAR(100),
    @CommissionRate DECIMAL(5,2)
AS
BEGIN
    INSERT INTO dbo.Doctors (UserId, Specialization, Designation, CommissionRate)
    VALUES (@UserId, @Specialization, @Designation, @CommissionRate);
    SELECT SCOPE_IDENTITY() AS NewDoctorId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetDoctorById
    @DoctorId INT
AS
BEGIN
    SELECT d.*, u.Email, u.FirstName, u.LastName, u.PhoneNumber
    FROM dbo.Doctors d
    INNER JOIN dbo.Users u ON d.UserId = u.UserId
    WHERE d.DoctorId = @DoctorId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateDoctor
    @DoctorId INT,
    @Specialization NVARCHAR(100),
    @Designation NVARCHAR(100),
    @CommissionRate DECIMAL(5,2),
    @FirstName NVARCHAR(50),
    @LastName NVARCHAR(50),
    @PhoneNumber NVARCHAR(20)
AS
BEGIN
    DECLARE @UserId INT;
    SELECT @UserId = UserId FROM dbo.Doctors WHERE DoctorId = @DoctorId;

    UPDATE dbo.Doctors
    SET Specialization = @Specialization,
        Designation = @Designation,
        CommissionRate = @CommissionRate
    WHERE DoctorId = @DoctorId;

    UPDATE dbo.Users
    SET FirstName = @FirstName,
        LastName = @LastName,
        PhoneNumber = @PhoneNumber
    WHERE UserId = @UserId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeleteDoctor
    @DoctorId INT
AS
BEGIN
    DECLARE @UserId INT;
    SELECT @UserId = UserId FROM dbo.Doctors WHERE DoctorId = @DoctorId;
    DELETE FROM dbo.Doctors WHERE DoctorId = @DoctorId;
    DELETE FROM dbo.Users WHERE UserId = @UserId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetDoctorsList
AS
BEGIN
    SELECT d.DoctorId, d.Specialization, d.Designation, d.CommissionRate, u.UserId, u.Email, u.FirstName, u.LastName, u.PhoneNumber
    FROM dbo.Doctors d
    INNER JOIN dbo.Users u ON d.UserId = u.UserId
    ORDER BY u.LastName, u.FirstName;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateStaff
    @UserId INT,
    @BranchId INT,
    @Designation NVARCHAR(100)
AS
BEGIN
    INSERT INTO dbo.Staff (UserId, BranchId, Designation)
    VALUES (@UserId, @BranchId, @Designation);
    SELECT SCOPE_IDENTITY() AS NewStaffId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetStaffById
    @StaffId INT
AS
BEGIN
    SELECT s.*, u.Email, u.FirstName, u.LastName, u.PhoneNumber, b.Name AS BranchName
    FROM dbo.Staff s
    INNER JOIN dbo.Users u ON s.UserId = u.UserId
    INNER JOIN dbo.Branches b ON s.BranchId = b.BranchId
    WHERE s.StaffId = @StaffId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateStaff
    @StaffId INT,
    @BranchId INT,
    @Designation NVARCHAR(100),
    @FirstName NVARCHAR(50),
    @LastName NVARCHAR(50),
    @PhoneNumber NVARCHAR(20)
AS
BEGIN
    DECLARE @UserId INT;
    SELECT @UserId = UserId FROM dbo.Staff WHERE StaffId = @StaffId;

    UPDATE dbo.Staff
    SET BranchId = @BranchId,
        Designation = @Designation
    WHERE StaffId = @StaffId;

    UPDATE dbo.Users
    SET FirstName = @FirstName,
        LastName = @LastName,
        PhoneNumber = @PhoneNumber
    WHERE UserId = @UserId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeleteStaff
    @StaffId INT
AS
BEGIN
    DECLARE @UserId INT;
    SELECT @UserId = UserId FROM dbo.Staff WHERE StaffId = @StaffId;
    DELETE FROM dbo.Staff WHERE StaffId = @StaffId;
    DELETE FROM dbo.Users WHERE UserId = @UserId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetStaffList
AS
BEGIN
    SELECT s.StaffId, s.BranchId, b.Name AS BranchName, s.Designation, u.UserId, u.Email, u.FirstName, u.LastName, u.PhoneNumber
    FROM dbo.Staff s
    INNER JOIN dbo.Users u ON s.UserId = u.UserId
    INNER JOIN dbo.Branches b ON s.BranchId = b.BranchId
    ORDER BY u.LastName, u.FirstName;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateBranch
    @Name NVARCHAR(100),
    @Address NVARCHAR(255),
    @City NVARCHAR(50),
    @ContactNumber NVARCHAR(20),
    @Email NVARCHAR(100)
AS
BEGIN
    INSERT INTO dbo.Branches (Name, Address, City, ContactNumber, Email, IsActive)
    VALUES (@Name, @Address, @City, @ContactNumber, @Email, 1);
    SELECT SCOPE_IDENTITY() AS NewBranchId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetBranchById
    @BranchId INT
AS
BEGIN
    SELECT * FROM dbo.Branches WHERE BranchId = @BranchId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateBranch
    @BranchId INT,
    @Name NVARCHAR(100),
    @Address NVARCHAR(255),
    @City NVARCHAR(50),
    @ContactNumber NVARCHAR(20),
    @Email NVARCHAR(100),
    @IsActive BIT
AS
BEGIN
    UPDATE dbo.Branches
    SET Name = @Name,
        Address = @Address,
        City = @City,
        ContactNumber = @ContactNumber,
        Email = @Email,
        IsActive = @IsActive
    WHERE BranchId = @BranchId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeleteBranch
    @BranchId INT
AS
BEGIN
    DELETE FROM dbo.Branches WHERE BranchId = @BranchId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAllBranches
AS
BEGIN
    SELECT * FROM dbo.Branches ORDER BY Name;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateTimeSlot
    @BranchId INT,
    @StartTime TIME,
    @EndTime TIME,
    @MaxBookings INT
AS
BEGIN
    INSERT INTO dbo.TimeSlots (BranchId, StartTime, EndTime, MaxBookings, IsActive)
    VALUES (@BranchId, @StartTime, @EndTime, @MaxBookings, 1);
    SELECT SCOPE_IDENTITY() AS NewSlotId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetSlotsByBranch
    @BranchId INT
AS
BEGIN
    SELECT * FROM dbo.TimeSlots 
    WHERE BranchId = @BranchId AND IsActive = 1
    ORDER BY StartTime;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateTimeSlot
    @SlotId INT,
    @StartTime TIME,
    @EndTime TIME,
    @MaxBookings INT,
    @IsActive BIT
AS
BEGIN
    UPDATE dbo.TimeSlots
    SET StartTime = @StartTime,
        EndTime = @EndTime,
        MaxBookings = @MaxBookings,
        IsActive = @IsActive
    WHERE SlotId = @SlotId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeleteTimeSlot
    @SlotId INT
AS
BEGIN
    DELETE FROM dbo.TimeSlots WHERE SlotId = @SlotId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateHoliday
    @BranchId INT,
    @HolidayDate DATE,
    @Description NVARCHAR(255)
AS
BEGIN
    INSERT INTO dbo.Holidays (BranchId, HolidayDate, Description)
    VALUES (@BranchId, @HolidayDate, @Description);
    SELECT SCOPE_IDENTITY() AS NewHolidayId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetHolidaysByBranch
    @BranchId INT
AS
BEGIN
    SELECT * FROM dbo.Holidays 
    WHERE BranchId = @BranchId AND HolidayDate >= CAST(GETDATE() AS DATE)
    ORDER BY HolidayDate;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeleteHoliday
    @HolidayId INT
AS
BEGIN
    DELETE FROM dbo.Holidays WHERE HolidayId = @HolidayId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateTestCategory
    @Name NVARCHAR(100),
    @Description NVARCHAR(255)
AS
BEGIN
    INSERT INTO dbo.TestCategories (Name, Description)
    VALUES (@Name, @Description);
    SELECT SCOPE_IDENTITY() AS NewCategoryId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetTestCategoryById
    @CategoryId INT
AS
BEGIN
    SELECT * FROM dbo.TestCategories WHERE CategoryId = @CategoryId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateTestCategory
    @CategoryId INT,
    @Name NVARCHAR(100),
    @Description NVARCHAR(255)
AS
BEGIN
    UPDATE dbo.TestCategories
    SET Name = @Name,
        Description = @Description
    WHERE CategoryId = @CategoryId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeleteTestCategory
    @CategoryId INT
AS
BEGIN
    DELETE FROM dbo.TestCategories WHERE CategoryId = @CategoryId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAllTestCategories
AS
BEGIN
    SELECT * FROM dbo.TestCategories ORDER BY Name;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateTest
    @CategoryId INT,
    @Name NVARCHAR(150),
    @Code NVARCHAR(50),
    @Description NVARCHAR(500),
    @Price DECIMAL(10,2),
    @SampleType NVARCHAR(100),
    @Preparation NVARCHAR(500),
    @NormalRange NVARCHAR(255),
    @DeliveryTimeHours INT
AS
BEGIN
    INSERT INTO dbo.Tests (CategoryId, Name, Code, Description, Price, SampleType, Preparation, NormalRange, DeliveryTimeHours, IsActive)
    VALUES (@CategoryId, @Name, @Code, @Description, @Price, @SampleType, @Preparation, @NormalRange, @DeliveryTimeHours, 1);
    SELECT SCOPE_IDENTITY() AS NewTestId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetTestById
    @TestId INT
AS
BEGIN
    SELECT t.*, c.Name AS CategoryName
    FROM dbo.Tests t
    INNER JOIN dbo.TestCategories c ON t.CategoryId = c.CategoryId
    WHERE t.TestId = @TestId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateTest
    @TestId INT,
    @CategoryId INT,
    @Name NVARCHAR(150),
    @Code NVARCHAR(50),
    @Description NVARCHAR(500),
    @Price DECIMAL(10,2),
    @SampleType NVARCHAR(100),
    @Preparation NVARCHAR(500),
    @NormalRange NVARCHAR(255),
    @DeliveryTimeHours INT,
    @IsActive BIT
AS
BEGIN
    UPDATE dbo.Tests
    SET CategoryId = @CategoryId,
        Name = @Name,
        Code = @Code,
        Description = @Description,
        Price = @Price,
        SampleType = @SampleType,
        Preparation = @Preparation,
        NormalRange = @NormalRange,
        DeliveryTimeHours = @DeliveryTimeHours,
        IsActive = @IsActive
    WHERE TestId = @TestId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeleteTest
    @TestId INT
AS
BEGIN
    DELETE FROM dbo.Tests WHERE TestId = @TestId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAllTests
AS
BEGIN
    SELECT t.*, c.Name AS CategoryName
    FROM dbo.Tests t
    INNER JOIN dbo.TestCategories c ON t.CategoryId = c.CategoryId
    ORDER BY t.Name;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreatePackage
    @Name NVARCHAR(150),
    @Code NVARCHAR(50),
    @Description NVARCHAR(500),
    @Price DECIMAL(10,2)
AS
BEGIN
    INSERT INTO dbo.Packages (Name, Code, Description, Price, IsActive)
    VALUES (@Name, @Code, @Description, @Price, 1);
    SELECT SCOPE_IDENTITY() AS NewPackageId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetPackageById
    @PackageId INT
AS
BEGIN
    SELECT * FROM dbo.Packages WHERE PackageId = @PackageId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdatePackage
    @PackageId INT,
    @Name NVARCHAR(150),
    @Code NVARCHAR(50),
    @Description NVARCHAR(500),
    @Price DECIMAL(10,2),
    @IsActive BIT
AS
BEGIN
    UPDATE dbo.Packages
    SET Name = @Name,
        Code = @Code,
        Description = @Description,
        Price = @Price,
        IsActive = @IsActive
    WHERE PackageId = @PackageId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeletePackage
    @PackageId INT
AS
BEGIN
    DELETE FROM dbo.Packages WHERE PackageId = @PackageId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAllPackages
AS
BEGIN
    SELECT * FROM dbo.Packages ORDER BY Name;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_AddTestToPackage
    @PackageId INT,
    @TestId INT
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.PackageTests WHERE PackageId = @PackageId AND TestId = @TestId)
    BEGIN
        INSERT INTO dbo.PackageTests (PackageId, TestId) VALUES (@PackageId, @TestId);
    END
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_RemoveTestFromPackage
    @PackageId INT,
    @TestId INT
AS
BEGIN
    DELETE FROM dbo.PackageTests WHERE PackageId = @PackageId AND TestId = @TestId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetPackageTests
    @PackageId INT
AS
BEGIN
    SELECT t.*, c.Name AS CategoryName
    FROM dbo.Tests t
    INNER JOIN dbo.PackageTests pt ON t.TestId = pt.TestId
    INNER JOIN dbo.TestCategories c ON t.CategoryId = c.CategoryId
    WHERE pt.PackageId = @PackageId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateAppointment
    @PatientId INT,
    @BranchId INT,
    @AppointmentDate DATE,
    @SlotId INT,
    @TotalAmount DECIMAL(10,2),
    @DiscountAmount DECIMAL(10,2),
    @PaidAmount DECIMAL(10,2),
    @PaymentStatus NVARCHAR(50),
    @Notes NVARCHAR(500),
    @DoctorId INT = NULL,
    @StaffId INT = NULL
AS
BEGIN
    INSERT INTO dbo.Appointments (PatientId, BranchId, AppointmentDate, SlotId, Status, TotalAmount, DiscountAmount, PaidAmount, PaymentStatus, Notes, DoctorId, StaffId)
    VALUES (@PatientId, @BranchId, @AppointmentDate, @SlotId, 'Pending', @TotalAmount, @DiscountAmount, @PaidAmount, @PaymentStatus, @Notes, @DoctorId, @StaffId);
    SELECT SCOPE_IDENTITY() AS NewAppointmentId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_AddAppointmentTest
    @AppointmentId INT,
    @TestId INT,
    @Price DECIMAL(10,2)
AS
BEGIN
    INSERT INTO dbo.AppointmentTests (AppointmentId, TestId, Price)
    VALUES (@AppointmentId, @TestId, @Price);
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_AddAppointmentPackage
    @AppointmentId INT,
    @PackageId INT,
    @Price DECIMAL(10,2)
AS
BEGIN
    INSERT INTO dbo.AppointmentPackages (AppointmentId, PackageId, Price)
    VALUES (@AppointmentId, @PackageId, @Price);
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAppointmentById
    @AppointmentId INT
AS
BEGIN
    SELECT a.*, 
           b.Name AS BranchName, b.Address AS BranchAddress, b.City AS BranchCity,
           ts.StartTime, ts.EndTime,
           up.FirstName AS PatientFirstName, up.LastName AS PatientLastName, up.Email AS PatientEmail, up.PhoneNumber AS PatientPhone,
           pat.DateOfBirth, pat.Gender,
           ud.FirstName AS DoctorFirstName, ud.LastName AS DoctorLastName,
           us.FirstName AS StaffFirstName, us.LastName AS StaffLastName
    FROM dbo.Appointments a
    INNER JOIN dbo.Branches b ON a.BranchId = b.BranchId
    INNER JOIN dbo.TimeSlots ts ON a.SlotId = ts.SlotId
    INNER JOIN dbo.Patients pat ON a.PatientId = pat.PatientId
    INNER JOIN dbo.Users up ON pat.UserId = up.UserId
    LEFT JOIN dbo.Doctors d ON a.DoctorId = d.DoctorId
    LEFT JOIN dbo.Users ud ON d.UserId = ud.UserId
    LEFT JOIN dbo.Staff s ON a.StaffId = s.StaffId
    LEFT JOIN dbo.Users us ON s.UserId = us.UserId
    WHERE a.AppointmentId = @AppointmentId;

    -- Get Selected Tests
    SELECT t.*, at.Price AS OrderedPrice, c.Name AS CategoryName
    FROM dbo.Tests t
    INNER JOIN dbo.AppointmentTests at ON t.TestId = at.TestId
    INNER JOIN dbo.TestCategories c ON t.CategoryId = c.CategoryId
    WHERE at.AppointmentId = @AppointmentId;

    -- Get Selected Packages
    SELECT p.*, ap.Price AS OrderedPrice
    FROM dbo.Packages p
    INNER JOIN dbo.AppointmentPackages ap ON p.PackageId = ap.PackageId
    WHERE ap.AppointmentId = @AppointmentId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAppointmentsByPatient
    @PatientId INT
AS
BEGIN
    SELECT a.*, b.Name AS BranchName, ts.StartTime, ts.EndTime,
           ud.FirstName AS DoctorFirstName, ud.LastName AS DoctorLastName
    FROM dbo.Appointments a
    INNER JOIN dbo.Branches b ON a.BranchId = b.BranchId
    INNER JOIN dbo.TimeSlots ts ON a.SlotId = ts.SlotId
    LEFT JOIN dbo.Doctors d ON a.DoctorId = d.DoctorId
    LEFT JOIN dbo.Users ud ON d.UserId = ud.UserId
    WHERE a.PatientId = @PatientId
    ORDER BY a.AppointmentDate DESC, ts.StartTime DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAppointmentsByBranch
    @BranchId INT
AS
BEGIN
    SELECT a.*, ts.StartTime, ts.EndTime,
           up.FirstName AS PatientFirstName, up.LastName AS PatientLastName, up.Email AS PatientEmail,
           ud.FirstName AS DoctorFirstName, ud.LastName AS DoctorLastName
    FROM dbo.Appointments a
    INNER JOIN dbo.TimeSlots ts ON a.SlotId = ts.SlotId
    INNER JOIN dbo.Patients pat ON a.PatientId = pat.PatientId
    INNER JOIN dbo.Users up ON pat.UserId = up.UserId
    LEFT JOIN dbo.Doctors d ON a.DoctorId = d.DoctorId
    LEFT JOIN dbo.Users ud ON d.UserId = ud.UserId
    WHERE a.BranchId = @BranchId
    ORDER BY a.AppointmentDate DESC, ts.StartTime DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAllAppointments
AS
BEGIN
    SELECT a.*, b.Name AS BranchName, ts.StartTime, ts.EndTime,
           up.FirstName AS PatientFirstName, up.LastName AS PatientLastName,
           ud.FirstName AS DoctorFirstName, ud.LastName AS DoctorLastName,
           us.FirstName AS StaffFirstName, us.LastName AS StaffLastName
    FROM dbo.Appointments a
    INNER JOIN dbo.Branches b ON a.BranchId = b.BranchId
    INNER JOIN dbo.TimeSlots ts ON a.SlotId = ts.SlotId
    INNER JOIN dbo.Patients pat ON a.PatientId = pat.PatientId
    INNER JOIN dbo.Users up ON pat.UserId = up.UserId
    LEFT JOIN dbo.Doctors d ON a.DoctorId = d.DoctorId
    LEFT JOIN dbo.Users ud ON d.UserId = ud.UserId
    LEFT JOIN dbo.Staff s ON a.StaffId = s.StaffId
    LEFT JOIN dbo.Users us ON s.UserId = us.UserId
    ORDER BY a.AppointmentDate DESC, ts.StartTime DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateAppointmentStatus
    @AppointmentId INT,
    @Status NVARCHAR(50),
    @StaffId INT = NULL
AS
BEGIN
    UPDATE dbo.Appointments
    SET Status = @Status,
        StaffId = CASE WHEN @StaffId IS NOT NULL THEN @StaffId ELSE StaffId END
    WHERE AppointmentId = @AppointmentId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateAppointmentPaymentStatus
    @AppointmentId INT,
    @PaymentStatus NVARCHAR(50),
    @PaidAmount DECIMAL(10,2)
AS
BEGIN
    UPDATE dbo.Appointments
    SET PaymentStatus = @PaymentStatus,
        PaidAmount = @PaidAmount
    WHERE AppointmentId = @AppointmentId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_RescheduleAppointment
    @AppointmentId INT,
    @AppointmentDate DATE,
    @SlotId INT
AS
BEGIN
    UPDATE dbo.Appointments
    SET AppointmentDate = @AppointmentDate,
        SlotId = @SlotId,
        Status = 'Confirmed'
    WHERE AppointmentId = @AppointmentId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_CancelAppointment
    @AppointmentId INT
AS
BEGIN
    UPDATE dbo.Appointments
    SET Status = 'Cancelled'
    WHERE AppointmentId = @AppointmentId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateReport
    @AppointmentId INT,
    @TestId INT,
    @ResultValue NVARCHAR(255),
    @Remarks NVARCHAR(500),
    @UploadedByStaffId INT
AS
BEGIN
    INSERT INTO dbo.Reports (AppointmentId, TestId, ResultValue, Remarks, UploadedByStaffId, UploadedAt, Status)
    VALUES (@AppointmentId, @TestId, @ResultValue, @Remarks, @UploadedByStaffId, GETDATE(), 'Completed');
    
    DECLARE @TotalTests INT;
    DECLARE @CompletedReports INT;
    
    SELECT @TotalTests = COUNT(*) FROM dbo.AppointmentTests WHERE AppointmentId = @AppointmentId;
    SELECT @CompletedReports = COUNT(*) FROM dbo.Reports WHERE AppointmentId = @AppointmentId AND Status = 'Completed';
    
    IF @TotalTests = @CompletedReports
    BEGIN
        UPDATE dbo.Appointments SET Status = 'ResultUploaded' WHERE AppointmentId = @AppointmentId;
    END

    SELECT SCOPE_IDENTITY() AS NewReportId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetReportById
    @ReportId INT
AS
BEGIN
    SELECT r.*, t.Name AS TestName, t.Code AS TestCode, t.NormalRange, t.SampleType,
           u.FirstName AS StaffFirstName, u.LastName AS StaffLastName
    FROM dbo.Reports r
    INNER JOIN dbo.Tests t ON r.TestId = t.TestId
    LEFT JOIN dbo.Staff s ON r.UploadedByStaffId = s.StaffId
    LEFT JOIN dbo.Users u ON s.UserId = u.UserId
    WHERE r.ReportId = @ReportId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetReportsByAppointment
    @AppointmentId INT
AS
BEGIN
    SELECT r.*, t.Name AS TestName, t.Code AS TestCode, t.NormalRange, t.SampleType, t.Preparation,
           u.FirstName AS StaffFirstName, u.LastName AS StaffLastName
    FROM dbo.Reports r
    INNER JOIN dbo.Tests t ON r.TestId = t.TestId
    LEFT JOIN dbo.Staff s ON r.UploadedByStaffId = s.StaffId
    LEFT JOIN dbo.Users u ON s.UserId = u.UserId
    WHERE r.AppointmentId = @AppointmentId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreatePayment
    @AppointmentId INT,
    @TransactionId NVARCHAR(100),
    @Amount DECIMAL(10,2),
    @PaymentMethod NVARCHAR(50),
    @PaymentStatus NVARCHAR(50)
AS
BEGIN
    INSERT INTO dbo.Payments (AppointmentId, TransactionId, Amount, PaymentMethod, PaymentStatus, PaymentDate)
    VALUES (@AppointmentId, @TransactionId, @Amount, @PaymentMethod, @PaymentStatus, GETDATE());

    IF @PaymentStatus = 'Success'
    BEGIN
        UPDATE dbo.Appointments
        SET PaymentStatus = 'Paid',
            PaidAmount = PaidAmount + @Amount
        WHERE AppointmentId = @AppointmentId;
    END
    
    SELECT SCOPE_IDENTITY() AS NewPaymentId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetPaymentsByAppointment
    @AppointmentId INT
AS
BEGIN
    SELECT * FROM dbo.Payments WHERE AppointmentId = @AppointmentId ORDER BY PaymentDate DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAllPayments
AS
BEGIN
    SELECT p.*, a.PatientId, u.FirstName + ' ' + u.LastName AS PatientName
    FROM dbo.Payments p
    INNER JOIN dbo.Appointments a ON p.AppointmentId = a.AppointmentId
    INNER JOIN dbo.Patients pat ON a.PatientId = pat.PatientId
    INNER JOIN dbo.Users u ON pat.UserId = u.UserId
    ORDER BY p.PaymentDate DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateInvoice
    @AppointmentId INT,
    @InvoiceNumber NVARCHAR(50),
    @TotalAmount DECIMAL(10,2),
    @DiscountAmount DECIMAL(10,2),
    @TaxAmount DECIMAL(10,2),
    @FinalAmount DECIMAL(10,2)
AS
BEGIN
    INSERT INTO dbo.Invoices (AppointmentId, InvoiceNumber, TotalAmount, DiscountAmount, TaxAmount, FinalAmount, GeneratedAt)
    VALUES (@AppointmentId, @InvoiceNumber, @TotalAmount, @DiscountAmount, @TaxAmount, @FinalAmount, GETDATE());
    SELECT SCOPE_IDENTITY() AS NewInvoiceId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetInvoiceByAppointment
    @AppointmentId INT
AS
BEGIN
    SELECT * FROM dbo.Invoices WHERE AppointmentId = @AppointmentId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateNotification
    @UserId INT,
    @Message NVARCHAR(500),
    @Type NVARCHAR(50)
AS
BEGIN
    INSERT INTO dbo.Notifications (UserId, Message, Type, IsRead, CreatedAt)
    VALUES (@UserId, @Message, @Type, 0, GETDATE());
    SELECT SCOPE_IDENTITY() AS NewNotificationId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetNotificationsByUser
    @UserId INT
AS
BEGIN
    SELECT * FROM dbo.Notifications 
    WHERE UserId = @UserId 
    ORDER BY CreatedAt DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_MarkNotificationRead
    @NotificationId INT
AS
BEGIN
    UPDATE dbo.Notifications SET IsRead = 1 WHERE NotificationId = @NotificationId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateFeedback
    @PatientId INT,
    @Rating INT,
    @Comments NVARCHAR(500)
AS
BEGIN
    INSERT INTO dbo.Feedback (PatientId, Rating, Comments, CreatedAt)
    VALUES (@PatientId, @Rating, @Comments, GETDATE());
    SELECT SCOPE_IDENTITY() AS NewFeedbackId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAllFeedback
AS
BEGIN
    SELECT f.*, u.FirstName + ' ' + u.LastName AS PatientName
    FROM dbo.Feedback f
    INNER JOIN dbo.Patients p ON f.PatientId = p.PatientId
    INNER JOIN dbo.Users u ON p.UserId = u.UserId
    ORDER BY f.CreatedAt DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateAuditLog
    @UserId INT = NULL,
    @Action NVARCHAR(50),
    @TableName NVARCHAR(50),
    @RecordId INT = NULL,
    @OldValues NVARCHAR(MAX) = NULL,
    @NewValues NVARCHAR(MAX) = NULL,
    @IpAddress NVARCHAR(50) = NULL
AS
BEGIN
    INSERT INTO dbo.AuditLogs (UserId, Action, TableName, RecordId, OldValues, NewValues, Timestamp, IpAddress)
    VALUES (@UserId, @Action, @TableName, @RecordId, @OldValues, @NewValues, GETDATE(), @IpAddress);
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAuditLogs
AS
BEGIN
    SELECT l.*, u.Email AS UserEmail, u.FirstName + ' ' + u.LastName AS UserName
    FROM dbo.AuditLogs l
    LEFT JOIN dbo.Users u ON l.UserId = u.UserId
    ORDER BY l.Timestamp DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAdminDashboardStats
AS
BEGIN
    DECLARE @TotalPatients INT;
    DECLARE @TotalAppointments INT;
    DECLARE @TodayBookings INT;
    DECLARE @PendingReports INT;
    DECLARE @TotalRevenue DECIMAL(18,2);

    SELECT @TotalPatients = COUNT(*) FROM dbo.Patients;
    SELECT @TotalAppointments = COUNT(*) FROM dbo.Appointments;
    SELECT @TodayBookings = COUNT(*) FROM dbo.Appointments WHERE AppointmentDate = CAST(GETDATE() AS DATE);
    SELECT @PendingReports = COUNT(*) FROM dbo.Appointments WHERE Status IN ('Pending', 'Confirmed', 'SampleCollected');
    SELECT @TotalRevenue = ISNULL(SUM(PaidAmount), 0) FROM dbo.Appointments WHERE PaymentStatus = 'Paid';

    SELECT @TotalPatients AS TotalPatients, 
           @TotalAppointments AS TotalAppointments, 
           @TodayBookings AS TodayBookings, 
           @PendingReports AS PendingReports, 
           @TotalRevenue AS TotalRevenue;

    SELECT TOP 5 t.Name AS TestName, COUNT(at.TestId) AS BookingCount, SUM(at.Price) AS RevenueGenerated
    FROM dbo.AppointmentTests at
    INNER JOIN dbo.Tests t ON at.TestId = t.TestId
    GROUP BY t.Name
    ORDER BY BookingCount DESC;

    SELECT TOP 6 
        FORMAT(a.AppointmentDate, 'yyyy-MM') AS MonthName, 
        SUM(a.PaidAmount) AS Revenue
    FROM dbo.Appointments a
    WHERE a.PaymentStatus = 'Paid' AND a.AppointmentDate >= DATEADD(month, -6, GETDATE())
    GROUP BY FORMAT(a.AppointmentDate, 'yyyy-MM')
    ORDER BY MonthName ASC;

    SELECT TOP 10 
        a.AppointmentId, 
        a.AppointmentDate, 
        a.Status, 
        u.FirstName + ' ' + u.LastName AS PatientName,
        a.TotalAmount
    FROM dbo.Appointments a
    INNER JOIN dbo.Patients p ON a.PatientId = p.PatientId
    INNER JOIN dbo.Users u ON p.UserId = u.UserId
    ORDER BY a.CreatedAt DESC;
END;
GO
