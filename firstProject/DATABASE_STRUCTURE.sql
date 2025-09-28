-- =============================================
-- Takhleesak API Database Script (Structure Only)
-- Database: MySQL/MariaDB
-- Version: 1.0.0
-- Created: January 2025
-- =============================================

-- Create Database
CREATE DATABASE IF NOT EXISTS `takhleesak_db` 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;

USE `takhleesak_db`;

-- =============================================
-- ASP.NET Identity Tables
-- =============================================

-- AspNetRoles Table
CREATE TABLE `AspNetRoles` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Name` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `NormalizedName` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `ConcurrencyStamp` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    PRIMARY KEY (`Id`),
    UNIQUE INDEX `RoleNameIndex` (`NormalizedName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- AspNetUsers Table (Custom User Table)
CREATE TABLE `AspNetUsers` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserName` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `NormalizedUserName` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `Email` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `NormalizedEmail` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `EmailConfirmed` tinyint(1) NOT NULL DEFAULT 0,
    `PasswordHash` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `SecurityStamp` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `ConcurrencyStamp` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `PhoneNumber` varchar(15) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `PhoneNumberConfirmed` tinyint(1) NOT NULL DEFAULT 0,
    `TwoFactorEnabled` tinyint(1) NOT NULL DEFAULT 0,
    `LockoutEnd` datetime(6) NULL,
    `LockoutEnabled` tinyint(1) NOT NULL DEFAULT 1,
    `AccessFailedCount` int NOT NULL DEFAULT 0,
    
    -- Custom User Properties
    `fullName` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
    `Identity` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
    `taxRecord` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `InsuranceNumber` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `license` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `isBlocked` tinyint(1) NOT NULL DEFAULT 0,
    `isActive` tinyint(1) NOT NULL DEFAULT 0,
    `lastLogin` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    
    PRIMARY KEY (`Id`),
    UNIQUE INDEX `IX_AspNetUsers_Email` (`Email`),
    UNIQUE INDEX `IX_AspNetUsers_PhoneNumber` (`PhoneNumber`),
    UNIQUE INDEX `IX_AspNetUsers_Identity` (`Identity`),
    UNIQUE INDEX `EmailIndex` (`NormalizedEmail`),
    UNIQUE INDEX `UserNameIndex` (`NormalizedUserName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- AspNetUserRoles Table
CREATE TABLE `AspNetUserRoles` (
    `UserId` int NOT NULL,
    `RoleId` int NOT NULL,
    PRIMARY KEY (`UserId`, `RoleId`),
    INDEX `IX_AspNetUserRoles_RoleId` (`RoleId`),
    CONSTRAINT `FK_AspNetUserRoles_AspNetRoles_RoleId` 
        FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_AspNetUserRoles_AspNetUsers_UserId` 
        FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- AspNetUserClaims Table
CREATE TABLE `AspNetUserClaims` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` int NOT NULL,
    `ClaimType` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `ClaimValue` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    PRIMARY KEY (`Id`),
    INDEX `IX_AspNetUserClaims_UserId` (`UserId`),
    CONSTRAINT `FK_AspNetUserClaims_AspNetUsers_UserId` 
        FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- AspNetUserLogins Table
CREATE TABLE `AspNetUserLogins` (
    `LoginProvider` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `ProviderKey` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `ProviderDisplayName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `UserId` int NOT NULL,
    PRIMARY KEY (`LoginProvider`, `ProviderKey`),
    INDEX `IX_AspNetUserLogins_UserId` (`UserId`),
    CONSTRAINT `FK_AspNetUserLogins_AspNetUsers_UserId` 
        FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- AspNetUserTokens Table
CREATE TABLE `AspNetUserTokens` (
    `UserId` int NOT NULL,
    `LoginProvider` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `Name` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `Value` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    PRIMARY KEY (`UserId`, `LoginProvider`, `Name`),
    CONSTRAINT `FK_AspNetUserTokens_AspNetUsers_UserId` 
        FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- AspNetRoleClaims Table
CREATE TABLE `AspNetRoleClaims` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `RoleId` int NOT NULL,
    `ClaimType` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `ClaimValue` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    PRIMARY KEY (`Id`),
    INDEX `IX_AspNetRoleClaims_RoleId` (`RoleId`),
    CONSTRAINT `FK_AspNetRoleClaims_AspNetRoles_RoleId` 
        FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- Custom Application Tables
-- =============================================

-- TwoFactorVerify Table
CREATE TABLE `TwoFactorVerify` (
    `UserId` int NOT NULL,
    `TypeOfGenerate` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
    `Value` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
    `Date` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `PeriodStartTime` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `Attempts` int NOT NULL DEFAULT 0,
    `IsUsed` tinyint(1) NOT NULL DEFAULT 0,
    PRIMARY KEY (`UserId`, `TypeOfGenerate`),
    CONSTRAINT `FK_TwoFactorVerify_AspNetUsers_UserId` 
        FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- Create Indexes for Performance
-- =============================================

-- Additional indexes for better performance
CREATE INDEX `IX_AspNetUsers_isBlocked` ON `AspNetUsers` (`isBlocked`);
CREATE INDEX `IX_AspNetUsers_isActive` ON `AspNetUsers` (`isActive`);
CREATE INDEX `IX_AspNetUsers_lastLogin` ON `AspNetUsers` (`lastLogin`);
CREATE INDEX `IX_TwoFactorVerify_Date` ON `TwoFactorVerify` (`Date`);
CREATE INDEX `IX_TwoFactorVerify_IsUsed` ON `TwoFactorVerify` (`IsUsed`);