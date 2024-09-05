-- Create the demo database
CREATE DATABASE DemoDB;

-- Switch to the new database
USE DemoDB;

-- Create a table named 'SampleData'
CREATE TABLE SampleData (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Insert some initial data into the table
INSERT INTO SampleData (Name)
VALUES ('Alice'),
       ('Bob'),
       ('Charlie');

