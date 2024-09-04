-- Check if the DemoTable already exists, and drop it if it does
IF OBJECT_ID('dbo.DemoTable', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.DemoTable;
END

-- Create a new table called DemoTable
CREATE TABLE dbo.DemoTable
(
    ID INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Insert a single row into the table
INSERT INTO dbo.DemoTable (Name)
VALUES ('Test Entry');

-- Select data from the table to verify insertion
SELECT * FROM dbo.DemoTable;
