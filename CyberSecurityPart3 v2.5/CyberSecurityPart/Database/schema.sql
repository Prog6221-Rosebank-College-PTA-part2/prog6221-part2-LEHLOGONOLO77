-- CyberBot Task Assistant — MySQL schema
-- Run this once in MySQL Workbench / mysql CLI before first launch.
-- (CyberBot will also try to create the table automatically on startup,
--  but creating it yourself first guarantees you can see the database too.)

CREATE DATABASE IF NOT EXISTS cyberbot_db;
USE cyberbot_db;

CREATE TABLE IF NOT EXISTS tasks (
    Id          INT AUTO_INCREMENT PRIMARY KEY,
    Title       VARCHAR(255)  NOT NULL,
    Description TEXT         NOT NULL,
    ReminderAt  DATE          NULL,
    Status      VARCHAR(20)   NOT NULL DEFAULT 'Pending',
    CreatedAt   DATETIME      NOT NULL
);

-- A couple of sample rows so the Task Assistant isn't empty on first run.
INSERT INTO tasks (Title, Description, ReminderAt, Status, CreatedAt) VALUES
('Enable two-factor authentication', 'Turn on 2FA for your email and banking accounts.', CURDATE(), 'Pending', NOW()),
('Update all passwords', 'Replace any reused or weak passwords with unique 12+ character ones.', NULL, 'Pending', NOW());
