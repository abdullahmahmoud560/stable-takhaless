-- Setup FirstProject Database Tables
USE u676203545_mainData;

-- Create AspNetRoles table
CREATE TABLE IF NOT EXISTS AspNetRoles (
    Id varchar(255) NOT NULL,
    Name varchar(256),
    NormalizedName varchar(256),
    ConcurrencyStamp longtext,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Create AspNetUsers table
CREATE TABLE IF NOT EXISTS AspNetUsers (
    Id varchar(255) NOT NULL,
    fullName longtext,
    Identity longtext,
    taxRecord longtext,
    InsuranceNumber longtext,
    license longtext,
    isBlocked tinyint(1),
    isActive tinyint(1),
    lastLogin datetime(6),
    UserName varchar(256),
    NormalizedUserName varchar(256),
    Email varchar(256),
    NormalizedEmail varchar(256),
    EmailConfirmed tinyint(1) NOT NULL,
    PasswordHash longtext,
    SecurityStamp longtext,
    ConcurrencyStamp longtext,
    PhoneNumber varchar(255),
    PhoneNumberConfirmed tinyint(1) NOT NULL,
    TwoFactorEnabled tinyint(1) NOT NULL,
    LockoutEnd datetime,
    LockoutEnabled tinyint(1) NOT NULL,
    AccessFailedCount int NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Create AspNetRoleClaims table
CREATE TABLE IF NOT EXISTS AspNetRoleClaims (
    Id int NOT NULL AUTO_INCREMENT,
    RoleId varchar(255) NOT NULL,
    ClaimType longtext,
    ClaimValue longtext,
    PRIMARY KEY (Id),
    FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Create AspNetUserClaims table
CREATE TABLE IF NOT EXISTS AspNetUserClaims (
    Id int NOT NULL AUTO_INCREMENT,
    UserId varchar(255) NOT NULL,
    ClaimType longtext,
    ClaimValue longtext,
    PRIMARY KEY (Id),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Create AspNetUserLogins table
CREATE TABLE IF NOT EXISTS AspNetUserLogins (
    LoginProvider varchar(128) NOT NULL,
    ProviderKey varchar(128) NOT NULL,
    ProviderDisplayName longtext,
    UserId varchar(255) NOT NULL,
    PRIMARY KEY (LoginProvider, ProviderKey),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Create AspNetUserRoles table
CREATE TABLE IF NOT EXISTS AspNetUserRoles (
    UserId varchar(255) NOT NULL,
    RoleId varchar(255) NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Create AspNetUserTokens table
CREATE TABLE IF NOT EXISTS AspNetUserTokens (
    UserId varchar(255) NOT NULL,
    LoginProvider varchar(128) NOT NULL,
    Name varchar(128) NOT NULL,
    Value longtext,
    PRIMARY KEY (UserId, LoginProvider, Name),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Create TwoFactorVerify table
CREATE TABLE IF NOT EXISTS TwoFactorVerify (
    Id int NOT NULL AUTO_INCREMENT,
    UserId varchar(255) NOT NULL,
    Value longtext,
    Name longtext,
    Date datetime(6),
    PRIMARY KEY (Id),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Insert default roles
INSERT IGNORE INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp) VALUES
('1', 'Admin', 'ADMIN', UUID()),
('2', 'User', 'USER', UUID()),
('3', 'Company', 'COMPANY', UUID()),
('4', 'Broker', 'BROKER', UUID()),
('5', 'Manager', 'MANAGER', UUID()),
('6', 'CustomerService', 'CUSTOMERSERVICE', UUID());
