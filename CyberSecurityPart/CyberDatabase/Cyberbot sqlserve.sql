
--Create the database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'cyberbot_db')
BEGIN
    CREATE DATABASE cyberbot_db;
END
GO


USE cyberbot_db;
GO

-- Create the tasks table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tasks')
BEGIN
    CREATE TABLE tasks (
        id          INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
        title       NVARCHAR(200) NOT NULL,
        description NVARCHAR(MAX) NOT NULL,
        reminder_at DATETIME          NULL,
        status      TINYINT       NOT NULL DEFAULT 0,
        created_at  DATETIME      NOT NULL DEFAULT GETDATE()
    );
END
GO


SELECT * FROM sys.tables WHERE name = 'tasks';
GO