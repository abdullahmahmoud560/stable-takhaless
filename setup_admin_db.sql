-- Setup Admin Database Tables
USE u676203545_logs;

-- Create Logs table
CREATE TABLE IF NOT EXISTS Logs (
    Id int NOT NULL AUTO_INCREMENT,
    Message longtext NOT NULL,
    NewOrderId int NOT NULL,
    TimeStamp datetime(6),
    UserId varchar(255) NOT NULL,
    Notes longtext NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

