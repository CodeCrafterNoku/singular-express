-- Add authentication fields to Users table
-- Run this script against your database to add user locking functionality

ALTER TABLE Users
ADD FailedLoginAttempts INT NOT NULL DEFAULT 0;

ALTER TABLE Users
ADD IsLockedOut BIT NOT NULL DEFAULT 0;

ALTER TABLE Users
ADD LockoutEnd DATETIME2 NULL;

ALTER TABLE Users
ADD LastLoginAttempt DATETIME2 NULL;

-- Optional: Create an index on email for faster lookups
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Email')
BEGIN
    CREATE INDEX IX_Users_Email ON Users (Email);
END

-- Show current table structure
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Users'
ORDER BY ORDINAL_POSITION;