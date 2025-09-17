-- Setup Customer Service Database Tables
USE u676203545_CustomerServic;

-- Create Form table
CREATE TABLE IF NOT EXISTS Form (
    Id int NOT NULL AUTO_INCREMENT,
    Message longtext,
    Email varchar(255),
    fullName varchar(255),
    phoneNumber varchar(255),
    createdAt datetime(6) NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Create ChatMessage table
CREATE TABLE IF NOT EXISTS ChatMessage (
    Id int NOT NULL AUTO_INCREMENT,
    SenderId varchar(255) NOT NULL,
    ReceiverId varchar(255) NOT NULL,
    Message longtext NOT NULL,
    Timestamp datetime(6) NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Create ChatSummary table
CREATE TABLE IF NOT EXISTS ChatSummary (
    Id int NOT NULL AUTO_INCREMENT,
    UserId varchar(255) NOT NULL,
    OtherUserId varchar(255) NOT NULL,
    LastMessage longtext,
    LastMessageTime datetime(6),
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Create saberCertificate table
CREATE TABLE IF NOT EXISTS saberCertificate (
    Id int NOT NULL AUTO_INCREMENT,
    CertificateNumber varchar(255) NOT NULL,
    CompanyName varchar(255) NOT NULL,
    IssueDate datetime(6) NOT NULL,
    ExpiryDate datetime(6) NOT NULL,
    Status varchar(50) NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

