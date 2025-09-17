-- Setup User Database Tables
USE u676203545_Orders;

-- Create NewOrder table
CREATE TABLE IF NOT EXISTS NewOrder (
    Id int NOT NULL AUTO_INCREMENT,
    Location varchar(255) NOT NULL,
    Date datetime(6) NOT NULL,
    UserId varchar(255) NOT NULL,
    statuOrder varchar(255) NOT NULL,
    numberOfLicense varchar(255) NOT NULL,
    Accept varchar(255),
    AcceptCustomerService varchar(255),
    AcceptAccount varchar(255),
    Notes longtext,
    City varchar(255),
    Town varchar(255),
    zipCode varchar(255),
    step1 varchar(255),
    step2 varchar(255),
    step3 varchar(255),
    JopID varchar(255),
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Create PaymentDetails table
CREATE TABLE IF NOT EXISTS PaymentDetails (
    Id int NOT NULL AUTO_INCREMENT,
    OrderId varchar(255),
    UserId varchar(255),
    Status varchar(255),
    Amount decimal(18,2),
    DateTime datetime(6) NOT NULL,
    TransactionId varchar(255),
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Create UploadFile table
CREATE TABLE IF NOT EXISTS UploadFile (
    Id int NOT NULL AUTO_INCREMENT,
    FileName varchar(255) NOT NULL,
    FilePath varchar(500) NOT NULL,
    FileSize bigint NOT NULL,
    ContentType varchar(100) NOT NULL,
    OrderId int NOT NULL,
    UploadDate datetime(6) NOT NULL,
    PRIMARY KEY (Id),
    FOREIGN KEY (OrderId) REFERENCES NewOrder(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Create NumberOfTypeOrder table
CREATE TABLE IF NOT EXISTS NumberOfTypeOrder (
    Id int NOT NULL AUTO_INCREMENT,
    OrderId int NOT NULL,
    TypeName varchar(255) NOT NULL,
    Quantity int NOT NULL,
    PRIMARY KEY (Id),
    FOREIGN KEY (OrderId) REFERENCES NewOrder(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Create Values table
CREATE TABLE IF NOT EXISTS `Values` (
    Id int NOT NULL AUTO_INCREMENT,
    OrderId int NOT NULL,
    ValueName varchar(255) NOT NULL,
    ValueAmount decimal(18,2) NOT NULL,
    PRIMARY KEY (Id),
    FOREIGN KEY (OrderId) REFERENCES NewOrder(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Create NotesCustomerService table
CREATE TABLE IF NOT EXISTS NotesCustomerService (
    Id int NOT NULL AUTO_INCREMENT,
    OrderId int NOT NULL,
    Note longtext NOT NULL,
    CreatedDate datetime(6) NOT NULL,
    CreatedBy varchar(255) NOT NULL,
    PRIMARY KEY (Id),
    FOREIGN KEY (OrderId) REFERENCES NewOrder(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Create NotesAccounting table
CREATE TABLE IF NOT EXISTS NotesAccounting (
    Id int NOT NULL AUTO_INCREMENT,
    OrderId int NOT NULL,
    Note longtext NOT NULL,
    CreatedDate datetime(6) NOT NULL,
    CreatedBy varchar(255) NOT NULL,
    PRIMARY KEY (Id),
    FOREIGN KEY (OrderId) REFERENCES NewOrder(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
